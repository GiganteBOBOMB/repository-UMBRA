using UnityEngine;

public class LightPlatform : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private SpriteRenderer platformSprite;
    [SerializeField] private Collider2D platformCollider;

    [Header("Aparência")]
    [Range(0f, 1f)]
    [SerializeField] private float transparentAlpha = 0.15f;

    [SerializeField] private float solidAlpha = 1f;

    // Indica se a plataforma está sendo iluminada.
    private bool isIlluminated;

    private void Awake()
    {
        // Busca os componentes automaticamente, caso não sejam atribuídos.
        if (platformSprite == null)
            platformSprite = GetComponent<SpriteRenderer>();

        if (platformCollider == null)
            platformCollider = GetComponent<Collider2D>();

        // Começa transparente e sem colisão.
        SetIlluminated(false);
    }

    public void SetIlluminated(bool illuminated)
    {
        isIlluminated = illuminated;

        // Ativa a colisão apenas quando a plataforma é iluminada.
        if (platformCollider != null)
            platformCollider.enabled = isIlluminated;

        // Controla a transparência da plataforma.
        if (platformSprite != null)
        {
            Color color = platformSprite.color;

            color.a = isIlluminated
                ? solidAlpha
                : transparentAlpha;

            platformSprite.color = color;
        }
    }
}
