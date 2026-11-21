using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class FlashlightController : MonoBehaviour
{
    [Header("Referências")]
    public Transform player;
    public SpriteRenderer playerSprite;
    public Light2D flashlightLight;
    public Transform focusVisual;

    [Header("Posição da Lanterna")]
    public float sideOffset = 0.5f;
    public float heightOffset = 0f;

    [Header("Ângulo e Alcance")]
    public float lightRange = 10f;

    [Range(1f, 179f)]
    public float lightAngle = 60f;

    [Range(1f, 179f)]
    public float focusAngle = 25f;

    [Header("Foco")]
    public float focusIntensityMultiplier = 1.5f;

    [Range(0.1f, 1f)]
    public float focusVisualWidthMultiplier = 0.6f;

    [Header("Atordoamento")]
    public float stunRange = 6f;
    public float stunDuration = 2f;
    public LayerMask enemyLayer = ~0;

    [Header("Plataformas de Luz")]
    public float platformLightRange = 6f;

    [Tooltip("Se verdadeiro, usa o ângulo reduzido durante o foco.")]
    public bool platformUsesFocusAngle = true;

    [Tooltip("Ativa mensagens de diagnóstico na Console.")]
    public bool debugPlatforms = false;

    [Header("Botões de Luz e Fantasmas")]
    [Tooltip("Alcance da luz para ativar botões e revelar fantasmas.")]
    public float buttonGhostRange = 6f;

    [Tooltip("Se verdadeiro, usa o ângulo reduzido durante o foco (igual às plataformas).")]
    public bool buttonGhostUsesFocusAngle = false;

    private int facingDirection = 1;
    private Vector3 originalFocusScale;
    private float originalLightIntensity = 1f;

    private readonly HashSet<EnemyBase> stunnedEnemies =
        new HashSet<EnemyBase>();

    private readonly HashSet<LightPlatform> illuminatedPlatforms =
        new HashSet<LightPlatform>();

    // Guarda as plataformas da cena, mesmo quando seus
    // colliders físicos estiverem desativados.
    private LightPlatform[] allPlatforms;

    // Botões de luz e fantasmas da cena.
    private LightButton[] allButtons;
    private GhostVisual[] allGhosts;

    public bool IsFocusing { get; private set; }

    private void Start()
    {
        if (focusVisual != null)
            originalFocusScale = focusVisual.localScale;

        if (player != null && playerSprite == null)
            playerSprite = player.GetComponentInChildren<SpriteRenderer>();

        if (flashlightLight == null)
            flashlightLight = GetComponentInChildren<Light2D>();

        if (flashlightLight != null)
            originalLightIntensity = flashlightLight.intensity;

        // Não dependemos do collider físico para localizar
        // as plataformas transparentes.
        allPlatforms = FindObjectsOfType<LightPlatform>();
        allButtons = FindObjectsOfType<LightButton>();
        allGhosts = FindObjectsOfType<GhostVisual>();

        if (debugPlatforms)
        {
            Debug.Log("Plataformas encontradas: " + allPlatforms.Length);
            Debug.Log("Botões encontrados: " + allButtons.Length);
            Debug.Log("Fantasmas encontrados: " + allGhosts.Length);
        }
    }

    private void Update()
    {
        UpdateFacingDirection();
        UpdateFlashlightPosition();
        AimAtMouse();

        IsFocusing = Mouse.current != null &&
                     Mouse.current.leftButton.isPressed;

        UpdateLightMode();

        if (IsFocusing)
            StunEnemies();
        else
            stunnedEnemies.Clear();

        UpdateLightPlatforms();
        UpdateLightButtons();
        UpdateGhosts();
    }

    private void UpdateFacingDirection()
    {
        if (playerSprite != null)
            facingDirection = playerSprite.flipX ? -1 : 1;
    }

    private void UpdateFlashlightPosition()
    {
        if (player == null)
            return;

        transform.position = player.position + new Vector3(
            sideOffset * facingDirection,
            heightOffset,
            0f
        );
    }

    private void AimAtMouse()
    {
        if (Mouse.current == null)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 mousePosition = Mouse.current.position.ReadValue();
        mousePosition.z = Mathf.Abs(mainCamera.transform.position.z);

        Vector3 worldMousePosition =
            mainCamera.ScreenToWorldPoint(mousePosition);

        Vector2 direction =
            (Vector2)(worldMousePosition - transform.position);

        if (direction.sqrMagnitude < 0.001f)
            return;

        float angle =
            Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void UpdateLightMode()
    {
        if (flashlightLight != null)
        {
            flashlightLight.pointLightOuterRadius = lightRange;

            flashlightLight.pointLightOuterAngle =
                IsFocusing ? focusAngle : lightAngle;

            flashlightLight.intensity = IsFocusing
                ? originalLightIntensity * focusIntensityMultiplier
                : originalLightIntensity;
        }

        if (focusVisual != null)
        {
            Vector3 scale = originalFocusScale;

            if (IsFocusing)
                scale.x *= focusVisualWidthMultiplier;

            focusVisual.localScale = scale;
        }
    }

    private void StunEnemies()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            stunRange,
            enemyLayer
        );

        Vector2 flashlightDirection = transform.right;
        float currentAngle = focusAngle / 2f;

        foreach (Collider2D hit in hits)
        {
            EnemyBase enemy = hit.GetComponentInParent<EnemyBase>();

            if (enemy == null || stunnedEnemies.Contains(enemy))
                continue;

            Vector2 directionToEnemy =
                (Vector2)enemy.transform.position -
                (Vector2)transform.position;

            if (directionToEnemy.sqrMagnitude < 0.001f)
                continue;

            directionToEnemy.Normalize();

            float angleToEnemy = Vector2.Angle(
                flashlightDirection,
                directionToEnemy
            );

            if (angleToEnemy <= currentAngle)
            {
                enemy.Stun(stunDuration);
                stunnedEnemies.Add(enemy);
            }
        }
    }

    // Teste único do feixe: o ponto está dentro do alcance e do cone da lanterna?
    private bool IsInBeam(Vector2 targetPosition, float range, float angle)
    {
        Vector2 toTarget = targetPosition - (Vector2)transform.position;
        float distance = toTarget.magnitude;

        if (distance > range)
            return false;

        // Praticamente em cima da lanterna.
        if (distance < 0.001f)
            return true;

        float angleToTarget = Vector2.Angle(transform.right, toTarget / distance);
        return angleToTarget <= angle / 2f;
    }

    private void UpdateLightButtons()
    {
        if (allButtons == null)
            allButtons = FindObjectsOfType<LightButton>();

        float angle = (IsFocusing && buttonGhostUsesFocusAngle) ? focusAngle : lightAngle;

        foreach (LightButton button in allButtons)
        {
            if (button == null || !button.gameObject.activeInHierarchy)
                continue;

            bool lit = IsInBeam(button.transform.position, buttonGhostRange, angle);

            // O LightButton ignora chamadas repetidas com o mesmo estado,
            // então é seguro chamar todo quadro.
            button.SetIlluminated(lit);
        }
    }

    private void UpdateGhosts()
    {
        if (allGhosts == null)
            allGhosts = FindObjectsOfType<GhostVisual>();

        float angle = (IsFocusing && buttonGhostUsesFocusAngle) ? focusAngle : lightAngle;

        foreach (GhostVisual ghost in allGhosts)
        {
            if (ghost == null || !ghost.gameObject.activeInHierarchy)
                continue;

            // Chamado todo quadro enquanto iluminado: renova o tempo de visibilidade.
            if (IsInBeam(ghost.transform.position, buttonGhostRange, angle))
                ghost.SetIlluminated(true);
        }
    }

    private void UpdateLightPlatforms()
    {
        if (allPlatforms == null)
            allPlatforms = FindObjectsOfType<LightPlatform>();

        float range = platformLightRange;

        // Usa o ângulo correspondente ao modo atual da lanterna.
        float angle = lightAngle;

        if (IsFocusing && platformUsesFocusAngle)
            angle = focusAngle;

        HashSet<LightPlatform> currentlyLit =
            new HashSet<LightPlatform>();

        foreach (LightPlatform platform in allPlatforms)
        {
            if (platform == null || !platform.gameObject.activeInHierarchy)
                continue;

            // Usa a posição da plataforma, não seu collider.
            // Assim, ela continua detectável quando está intangível.
            if (IsInBeam(platform.transform.position, range, angle))
                currentlyLit.Add(platform);
        }

        // Torna sólidas as plataformas iluminadas.
        foreach (LightPlatform platform in currentlyLit)
        {
            if (!illuminatedPlatforms.Contains(platform) && debugPlatforms)
                Debug.Log("Plataforma iluminada: " + platform.name);

            platform.SetIlluminated(true);
        }

        // Torna intangíveis as plataformas que saíram do feixe.
        foreach (LightPlatform platform in illuminatedPlatforms)
        {
            if (platform != null && !currentlyLit.Contains(platform))
            {
                platform.SetIlluminated(false);

                if (debugPlatforms)
                    Debug.Log("Plataforma deixou de ser iluminada: " +
                              platform.name);
            }
        }

        illuminatedPlatforms.Clear();

        foreach (LightPlatform platform in currentlyLit)
            illuminatedPlatforms.Add(platform);
    }

    private void OnDisable()
    {
        foreach (LightPlatform platform in illuminatedPlatforms)
        {
            if (platform != null)
                platform.SetIlluminated(false);
        }

        illuminatedPlatforms.Clear();
        stunnedEnemies.Clear();

        if (allButtons != null)
        {
            foreach (LightButton button in allButtons)
            {
                if (button != null)
                    button.SetIlluminated(false);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stunRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, platformLightRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, buttonGhostRange);
    }
}