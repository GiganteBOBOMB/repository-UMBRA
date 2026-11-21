using System.Collections;
using UnityEngine;

// Movimento do corpo do Knight, controlado pelos ataques. Não decide nada sozinho.
// Move o transform por código (sem física): é o que permite salto, avanço e teleporte exatos.
public class KnightMotor : MonoBehaviour
{
    [Header("Visual")]
    [Tooltip("Sprite do boss. Se ficar vazio, procura nos filhos.")]
    [SerializeField] private SpriteRenderer sprite;
    [Tooltip("O sprite original desenhado olha para a direita?")]
    [SerializeField] private bool spriteFacesRight = true;

    [Header("Corpo")]
    [Tooltip("Colliders do corpo, desligados quando o boss some. Se ficar vazio, usa os do próprio objeto.")]
    [SerializeField] private Collider2D[] bodyColliders;

    [Header("Arma")]
    [Tooltip("Para avisar a arma quando o boss vira de lado.")]
    [SerializeField] private FloatingWeapon weapon;

    public int Facing { get; private set; } = 1;
    public bool IsVisible { get; private set; } = true;

    /// Altura (Y) em que o boss fica de pé no chão. É a posição dele quando a cena começa:
    /// deixe o boss apoiado no chão antes de dar Play.
    public float HomeY { get; private set; }

    private void Awake()
    {
        HomeY = transform.position.y;

        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        if (bodyColliders == null || bodyColliders.Length == 0)
            bodyColliders = GetComponents<Collider2D>();
    }

    // ---------- Lado ----------

    public void SetFacing(int direction)
    {
        Facing = direction >= 0 ? 1 : -1;

        if (sprite != null)
            sprite.flipX = spriteFacesRight ? Facing < 0 : Facing > 0;

        if (weapon != null) weapon.SetFacing(Facing);
    }

    public void FaceTowards(float worldX)
    {
        SetFacing(worldX >= transform.position.x ? 1 : -1);
    }

    public void FaceTowards(Transform target)
    {
        if (target != null) FaceTowards(target.position.x);
    }

    // ---------- Movimento ----------

    /// Vai até um ponto do mundo. easeOut: começa rápido e desacelera (bom para avanços).
    public IEnumerator MoveTo(Vector3 worldTarget, float duration, bool easeOut = false)
    {
        duration = Mathf.Max(0.01f, duration);

        Vector3 start = transform.position;
        worldTarget.z = start.z;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float k = Mathf.Clamp01(t);
            if (easeOut) k = 1f - (1f - k) * (1f - k);

            transform.position = Vector3.Lerp(start, worldTarget, k);
            yield return null;
        }

        transform.position = worldTarget;
    }

    /// Avanço horizontal: dx positivo = direita, negativo = esquerda.
    public IEnumerator DashBy(float dx, float duration)
    {
        Vector3 target = transform.position + new Vector3(dx, 0f, 0f);
        yield return MoveTo(target, duration, true);
    }

    /// Coloca o boss de volta no chão, na mesma posição horizontal (usado ao cancelar um salto).
    public void SnapToGround()
    {
        Vector3 p = transform.position;
        transform.position = new Vector3(p.x, HomeY, p.z);
    }

    // ---------- Aparecer e sumir ----------

    public void SetVisible(bool value)
    {
        IsVisible = value;

        if (sprite != null) sprite.enabled = value;

        foreach (Collider2D c in bodyColliders)
            if (c != null) c.enabled = value;
    }
}