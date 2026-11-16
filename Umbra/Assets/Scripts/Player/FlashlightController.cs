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

        if (debugPlatforms)
            Debug.Log("Plataformas encontradas: " + allPlatforms.Length);
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

        Vector2 lightPosition = transform.position;
        Vector2 lightDirection = transform.right;

        foreach (LightPlatform platform in allPlatforms)
        {
            if (platform == null || !platform.gameObject.activeInHierarchy)
                continue;

            // Usa a posição da plataforma, não seu collider.
            // Assim, ela continua detectável quando está intangível.
            Vector2 targetPosition = platform.transform.position;
            Vector2 directionToPlatform = targetPosition - lightPosition;

            float distance = directionToPlatform.magnitude;

            // Verifica o alcance.
            if (distance > range)
                continue;

            // A plataforma está praticamente na posição da lanterna.
            if (distance < 0.001f)
            {
                currentlyLit.Add(platform);
                continue;
            }

            directionToPlatform /= distance;

            // Verifica se a plataforma está dentro do cone de luz.
            float angleToPlatform = Vector2.Angle(
                lightDirection,
                directionToPlatform
            );

            if (angleToPlatform <= angle / 2f)
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
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stunRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, platformLightRange);
    }
}
