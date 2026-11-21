using System.Collections;
using UnityEngine;

// Ataque 3 (Phantom): o boss salta, acompanha o player no alto e despenca no chão,
// causando dano em área onde caiu.
public class AtaqueSaltoEsmagador : KnightAttack
{
    [Header("Preparação")]
    [Tooltip("Tempo do boss 'agachando' enquanto a arma sobe. É o aviso do ataque.")]
    [SerializeField, Min(0.01f)] private float crouchTime = 0.3f;

    [Header("Salto")]
    [SerializeField] private float jumpHeight = 4f;
    [SerializeField, Min(0.05f)] private float riseTime = 0.45f;
    [Tooltip("Quanto o boss se desloca na horizontal, no máximo, durante a subida.")]
    [SerializeField] private float maxLeapDistance = 6f;

    [Header("No alto")]
    [Tooltip("Tempo no alto acompanhando o player na horizontal.")]
    [SerializeField, Min(0f)] private float hangTime = 0.35f;
    [Tooltip("Velocidade horizontal com que o boss acompanha o player no alto.")]
    [SerializeField, Min(0f)] private float trackSpeed = 10f;
    [Tooltip("Tempo parado no alto com a posição TRAVADA, antes de cair. É a janela para desviar.")]
    [SerializeField, Min(0f)] private float lockTime = 0.15f;

    [Header("Queda")]
    [SerializeField, Min(0.05f)] private float fallTime = 0.18f;

    [Header("Impacto")]
    [Tooltip("Área de dano do impacto: um filho com Collider2D (trigger) e BossHitbox. O script o posiciona no ponto da queda.")]
    [SerializeField] private BossHitbox impactHitbox;
    [Tooltip("Ajuste da altura da área de dano em relação ao chão.")]
    [SerializeField] private float impactOffsetY = 0f;
    [SerializeField, Min(0.05f)] private float hitboxActiveTime = 0.15f;
    [SerializeField, Min(0f)] private float recoverTime = 0.6f;

    [Header("Arma")]
    [Tooltip("Posição da arma, em relação ao boss, enquanto ele sobe e fica no alto. O X é espelhado pelo lado.")]
    [SerializeField] private Vector2 raisedWeaponOffset = new Vector2(0.5f, 1.3f);
    [Tooltip("Posição da arma, em relação ao boss, durante a queda (abaixo dele, apontando para baixo).")]
    [SerializeField] private Vector2 fallingWeaponOffset = new Vector2(0f, -1.4f);

    [Header("Tremor da câmera no impacto")]
    [Tooltip("0 desliga o tremor.")]
    [SerializeField] private float shakeIntensity = 0.4f;
    [SerializeField] private float shakeDuration = 0.3f;

    private KnightMotor activeMotor;

    public override IEnumerator Execute(KnightContext context)
    {
        KnightMotor motor = context.Motor;
        FloatingWeapon weapon = context.Weapon;
        Transform boss = motor.transform;
        Transform target = context.Target;
        BossArena arena = context.Arena;

        activeMotor = motor;

        motor.FaceTowards(target);
        int facing = motor.Facing;

        // O boss decola do chão. HomeY é a altura dele de pé (gravada no começo da cena)
        float groundY = motor.HomeY;
        float z = boss.position.z;
        Vector3 start = new Vector3(boss.position.x, groundY, z);
        boss.position = start;

        // 1. Agacha: a arma sobe, apontando para cima
        weapon.PointToward(Vector2.up);
        yield return weapon.MoveTo(start + RaisedOffset(facing), crouchTime);

        // 2. Salto: sobe em arco suave rumo ao player
        float targetX = target != null ? target.position.x : start.x;
        float leap = Mathf.Clamp(targetX - start.x, -maxLeapDistance, maxLeapDistance);
        float apexX = ClampX(start.x + leap, arena);
        float apexY = groundY + jumpHeight;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / riseTime;
            float k = Mathf.Clamp01(t);

            // X em linha reta, Y desacelerando até o topo (seno de 0 a 90 graus)
            float x = Mathf.Lerp(start.x, apexX, k);
            float y = groundY + jumpHeight * Mathf.Sin(k * Mathf.PI * 0.5f);
            boss.position = new Vector3(x, y, z);

            ParkWeapon(weapon, boss.position, RaisedOffset(facing));
            yield return null;
        }

        // 3. No alto: acompanha o player na horizontal
        float currentX = apexX;
        t = 0f;
        while (t < hangTime)
        {
            if (target != null)
            {
                float wantedX = ClampX(target.position.x, arena);
                currentX = Mathf.MoveTowards(currentX, wantedX, trackSpeed * Time.deltaTime);
            }

            boss.position = new Vector3(currentX, apexY, z);
            ParkWeapon(weapon, boss.position, RaisedOffset(facing));

            t += Time.deltaTime;
            yield return null;
        }

        // 4. Trava a posição: quem se mexer agora, escapa
        t = 0f;
        while (t < lockTime)
        {
            ParkWeapon(weapon, boss.position, RaisedOffset(facing));
            t += Time.deltaTime;
            yield return null;
        }

        // 5. Queda: acelera até o chão, com a arma apontando para baixo
        weapon.PointToward(Vector2.down);
        weapon.SetHitbox(true);

        Vector3 fallStart = boss.position;
        Vector3 landing = new Vector3(fallStart.x, groundY, z);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / fallTime;
            float k = Mathf.Clamp01(t);
            k *= k; // acelera: começa devagar e termina rápido

            boss.position = Vector3.Lerp(fallStart, landing, k);
            ParkWeapon(weapon, boss.position, FallingOffset());
            yield return null;
        }

        boss.position = landing;
        ParkWeapon(weapon, boss.position, FallingOffset());
        weapon.SetHitbox(false);

        // 6. Impacto: área de dano no ponto da queda e tremor de câmera
        if (impactHitbox != null)
        {
            impactHitbox.transform.position = new Vector3(landing.x, groundY + impactOffsetY, z);
            impactHitbox.SetActive(true);
        }

        if (shakeIntensity > 0f && CameraShake.Instance != null)
            CameraShake.Instance.Shake(shakeIntensity, shakeDuration);

        yield return new WaitForSeconds(hitboxActiveTime);

        if (impactHitbox != null) impactHitbox.SetActive(false);

        // 7. Recuperação (o cérebro devolve a arma ao boss depois)
        yield return new WaitForSeconds(recoverTime);

        activeMotor = null;
    }

    // Cancelado no meio (morte, troca de fase)? Não deixa o boss pairando nem a área de dano ligada.
    public override void Cancel()
    {
        base.Cancel();

        if (impactHitbox != null) impactHitbox.SetActive(false);

        if (activeMotor != null)
        {
            activeMotor.SnapToGround();
            activeMotor = null;
        }
    }

    // ---------- Auxiliares ----------

    private Vector3 RaisedOffset(int facing)
    {
        return new Vector3(raisedWeaponOffset.x * facing, raisedWeaponOffset.y, 0f);
    }

    private Vector3 FallingOffset()
    {
        return new Vector3(fallingWeaponOffset.x, fallingWeaponOffset.y, 0f);
    }

    // A arma acompanha o boss exatamente (sem atraso) durante o salto
    private void ParkWeapon(FloatingWeapon weapon, Vector3 bossPosition, Vector3 offset)
    {
        Vector3 p = bossPosition + offset;
        p.z = weapon.transform.position.z;
        weapon.transform.position = p;
    }

    private float ClampX(float x, BossArena arena)
    {
        return arena != null ? Mathf.Clamp(x, arena.LeftX, arena.RightX) : x;
    }
}