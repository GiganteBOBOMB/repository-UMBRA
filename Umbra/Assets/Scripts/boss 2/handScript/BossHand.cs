using System.Collections;
using UnityEngine;

public enum HandSide { Left, Right }

public class BossHand : MonoBehaviour
{
    [SerializeField] private HandSide side;

    [Tooltip("Área de dano desta mão. Se ficar vazio, procura um BossHitbox nesta mão ou nos filhos.")]
    [SerializeField] private BossHitbox hitbox;

    public HandSide Side => side;

    private Vector3 restLocalPosition;

    public Vector3 RestLocalPosition => restLocalPosition;

    public Vector3 RestWorldPosition =>
        transform.parent != null ? transform.parent.TransformPoint(restLocalPosition) : restLocalPosition;

    private void Awake()
    {
        // Local: se o corpo do boss se mover, a mão vai junto
        restLocalPosition = transform.localPosition;

        if (hitbox == null) hitbox = GetComponentInChildren<BossHitbox>();
    }

    // ---------- Movimento ----------

    public IEnumerator MoveTo(Vector3 targetLocalPosition, float duration)
    {
        duration = Mathf.Max(0.01f, duration); // evita divisão por zero
        Vector3 start = transform.localPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            transform.localPosition = Vector3.Lerp(start, targetLocalPosition, t);
            yield return null;
        }

        transform.localPosition = targetLocalPosition;
    }

    // Os ataques pensam em posições do mundo (chão, bordas). Esta versão converte para local.
    public IEnumerator MoveToWorld(Vector3 worldTarget, float duration)
    {
        yield return MoveTo(WorldToLocal(worldTarget), duration);
    }

    public IEnumerator ReturnToRest(float duration)
    {
        yield return MoveTo(restLocalPosition, duration);
    }

    private Vector3 WorldToLocal(Vector3 world)
    {
        return transform.parent != null ? transform.parent.InverseTransformPoint(world) : world;
    }

    // ---------- Dano ----------

    public void SetHitbox(bool value)
    {
        if (hitbox != null) hitbox.SetActive(value);
    }

    // ---------- Cancelamento ----------

    // Coloca a mão no repouso na hora e desliga o dano (usado ao cancelar um ataque)
    public void SnapToRest()
    {
        transform.localPosition = restLocalPosition;
        SetHitbox(false);
    }
}