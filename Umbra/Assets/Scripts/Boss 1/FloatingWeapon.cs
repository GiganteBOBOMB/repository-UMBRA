using System.Collections;
using UnityEngine;

public enum WeaponMode
{
    Follow,   // segue o boss com atraso (estado normal)
    Free      // um ataque está no controle da arma
}

// Arma flutuante do boss. NÃO deve ser filha do boss: ela segue por código, com atraso.
public class FloatingWeapon : MonoBehaviour
{
    [Header("Seguir o boss")]
    [SerializeField] private Transform owner;
    [Tooltip("Posição da arma em relação ao boss. O X é espelhado conforme o lado para onde o boss olha.")]
    [SerializeField] private Vector2 followOffset = new Vector2(-1.2f, 0.6f);
    [Tooltip("Atraso do seguimento, em segundos. Maior = a arma 'arrasta' mais atrás do boss. 0.05 = quase colada.")]
    [SerializeField, Min(0.01f)] private float followDelay = 0.25f;
    [Tooltip("Limite de velocidade ao alcançar o boss.")]
    [SerializeField, Min(0.1f)] private float maxFollowSpeed = 40f;

    [Header("Flutuação")]
    [SerializeField] private float bobAmplitude = 0.15f;
    [SerializeField] private float bobSpeed = 2f;

    [Header("Rotação")]
    [Tooltip("Ângulo (graus) da arma parada com o boss olhando para a direita. É espelhado para a esquerda.")]
    [SerializeField] private float restAngle = 30f;
    [Tooltip("Velocidade para girar até o ângulo desejado, em graus por segundo.")]
    [SerializeField] private float rotationSpeed = 540f;
    [Tooltip("Para onde o sprite aponta sem rotação. 90 = a ponta aponta para cima, 0 = para a direita.\n" +
             "Usado quando um ataque manda a arma apontar para um alvo.")]
    [SerializeField] private float spriteTipAngle = 90f;

    [Header("Dano")]
    [Tooltip("Área de dano da arma. Se ficar vazio, procura um BossHitbox nesta arma ou nos filhos.")]
    [SerializeField] private BossHitbox hitbox;

    [Header("Lado")]
    [Tooltip("1 = boss olha para a direita, -1 = para a esquerda. Em jogo, o boss atualiza por código.")]
    [SerializeField] private int facing = 1;

    public WeaponMode Mode { get; private set; } = WeaponMode.Follow;
    public int Facing => facing;

    private Vector3 followVelocity;
    private float? manualAngle;       // quando tem valor, um ataque está mandando no ângulo
    private float baseScaleX;

    private void Awake()
    {
        baseScaleX = Mathf.Abs(transform.localScale.x);
        if (hitbox == null) hitbox = GetComponentInChildren<BossHitbox>();

        ApplyFacingScale();
        SnapToFollow(); // já nasce no lugar certo, sem voar do ponto onde você deixou no editor
    }

    // LateUpdate: roda depois de todos os Update, então o boss já se moveu neste frame
    private void LateUpdate()
    {
        if (Mode == WeaponMode.Follow)
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                GetFollowTarget(),
                ref followVelocity,
                followDelay,
                maxFollowSpeed
            );
        }

        UpdateRotation();
    }

    // ---------- Seguir ----------

    private Vector3 GetFollowTarget()
    {
        if (owner == null) return transform.position;

        float bob = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;

        return new Vector3(
            owner.position.x + followOffset.x * facing,
            owner.position.y + followOffset.y + bob,
            transform.position.z
        );
    }

    private void UpdateRotation()
    {
        float desired = manualAngle ?? restAngle * facing;
        float current = transform.eulerAngles.z;
        float next = Mathf.MoveTowardsAngle(current, desired, rotationSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, next);
    }

    // ---------- Controle pelos ataques ----------

    /// Devolve a arma ao seguimento normal. Ela volta suavemente, com o atraso.
    public void Follow()
    {
        Mode = WeaponMode.Follow;
        manualAngle = null;
        followVelocity = Vector3.zero;
    }

    /// O ataque assume o controle: a arma para de seguir o boss.
    public void Release()
    {
        Mode = WeaponMode.Free;
    }

    /// Coloca a arma já no ponto de seguimento, sem atraso (use ao aparecer ou teleportar).
    public void SnapToFollow()
    {
        transform.position = GetFollowTarget();
        followVelocity = Vector3.zero;
    }

    /// Move a arma até um ponto do mundo. Deixa a arma livre.
    public IEnumerator MoveTo(Vector3 worldTarget, float duration)
    {
        Release();
        duration = Mathf.Max(0.01f, duration);

        Vector3 start = transform.position;
        worldTarget.z = start.z;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(start, worldTarget, t);
            yield return null;
        }
        transform.position = worldTarget;
    }

    /// Gira a arma até este ângulo (graus). Use ClearAngle() para voltar ao ângulo de repouso.
    public void SetAngle(float zDegrees) => manualAngle = zDegrees;

    public void ClearAngle() => manualAngle = null;

    /// Faz a ponta da arma apontar na direção dada.
    public void PointToward(Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        manualAngle = angle - spriteTipAngle;
    }

    // ---------- Lado ----------

    public void SetFacing(int direction)
    {
        facing = direction >= 0 ? 1 : -1;
        ApplyFacingScale();
    }

    private void ApplyFacingScale()
    {
        Vector3 scale = transform.localScale;
        scale.x = baseScaleX * facing;
        transform.localScale = scale;
    }

    // ---------- Dano ----------

    public void SetHitbox(bool value)
    {
        if (hitbox != null) hitbox.SetActive(value);
    }

    // Mudou o lado pelo Inspector durante o Play? Atualiza o espelhamento
    private void OnValidate()
    {
        facing = facing >= 0 ? 1 : -1;
        if (Application.isPlaying && baseScaleX > 0f) ApplyFacingScale();
    }

    private void OnDrawGizmosSelected()
    {
        if (owner == null) return;

        // Mostra onde a arma "quer" ficar em relação ao boss
        Vector3 target = new Vector3(
            owner.position.x + followOffset.x * facing,
            owner.position.y + followOffset.y,
            transform.position.z
        );
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(target, 0.2f);
        Gizmos.DrawLine(owner.position, target);
    }
}