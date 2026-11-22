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
    public Transform flashlightPoint;
    public Transform lightPivot;

    [Header("Sistema de Foco")]
    public PlayerFocus playerFocus;

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

    [Header("Tempo de Exposição ao Foco")]
    [Min(0.01f)]
    public float timeToStun = 2f;

    [Header("Plataformas de Luz")]
    public float platformLightRange = 6f;
    public bool platformUsesFocusAngle = true;
    public bool debugPlatforms = false;

    [Header("Botões de Luz e Fantasmas")]
    public float buttonGhostRange = 6f;
    public bool buttonGhostUsesFocusAngle = false;

    private int facingDirection = 1;

    private Vector3 originalFocusScale;
    private Vector3 originalFlashlightPointPosition;

    private float originalLightIntensity = 1f;

    // Inimigos já atordoados durante o uso atual do foco.
    private readonly HashSet<EnemyBase> stunnedEnemies =
        new HashSet<EnemyBase>();

    // Tempo individual de exposição de cada inimigo.
    private readonly Dictionary<EnemyBase, float> stunProgress =
        new Dictionary<EnemyBase, float>();

    // Inimigos iluminados no frame atual.
    private readonly HashSet<EnemyBase> currentlyFocused =
        new HashSet<EnemyBase>();

    private readonly List<EnemyBase> enemiesToReset =
        new List<EnemyBase>();

    private readonly HashSet<LightPlatform> illuminatedPlatforms =
        new HashSet<LightPlatform>();

    private LightPlatform[] allPlatforms;
    private LightButton[] allButtons;
    private GhostVisual[] allGhosts;

    public bool IsFocusing { get; private set; }

    private void Start()
    {
        // Guarda a escala original do cone visual.
        if (focusVisual != null)
        {
            originalFocusScale = focusVisual.localScale;
        }

        // Guarda a posição original da lanterna.
        if (flashlightPoint != null)
        {
            originalFlashlightPointPosition =
                flashlightPoint.localPosition;
        }

        // Localiza o SpriteRenderer do Player.
        if (player != null && playerSprite == null)
        {
            playerSprite =
                player.GetComponentInChildren<SpriteRenderer>();
        }

        // Localiza a luz.
        if (flashlightLight == null)
        {
            flashlightLight =
                GetComponentInChildren<Light2D>();
        }

        // Guarda a intensidade normal da luz.
        if (flashlightLight != null)
        {
            originalLightIntensity =
                flashlightLight.intensity;
        }

        // Localiza automaticamente o PlayerFocus.
        if (playerFocus == null && player != null)
        {
            playerFocus =
                player.GetComponent<PlayerFocus>();
        }

        if (playerFocus == null)
        {
            playerFocus =
                GetComponentInParent<PlayerFocus>();
        }

        if (playerFocus == null)
        {
            Debug.LogWarning(
                "PlayerFocus não encontrado na lanterna."
            );
        }

        if (lightPivot == null)
        {
            Debug.LogWarning(
                "LightPivot não conectado no Inspector."
            );
        }

        // Localiza os objetos que interagem com a luz.
        allPlatforms = FindObjectsOfType<LightPlatform>();
        allButtons = FindObjectsOfType<LightButton>();
        allGhosts = FindObjectsOfType<GhostVisual>();

        if (debugPlatforms)
        {
            Debug.Log(
                "Plataformas encontradas: " +
                allPlatforms.Length
            );

            Debug.Log(
                "Botões encontrados: " +
                allButtons.Length
            );

            Debug.Log(
                "Fantasmas encontrados: " +
                allGhosts.Length
            );
        }
    }

    private void LateUpdate()
    {
        UpdateFacingDirection();

        UpdateFlashlightPosition();

        AimAtMouse();

        // Verifica se o foco está disponível.
        IsFocusing = playerFocus != null &&
                     playerFocus.isActiveAndEnabled &&
                     playerFocus.IsFocusing &&
                     !playerFocus.IsRecharging;

        // Atualiza a aparência da luz.
        UpdateLightMode();

        if (IsFocusing && lightPivot != null)
        {
            StunEnemies();
        }
        else
        {
            // Zera o progresso ao interromper o foco.
            stunnedEnemies.Clear();
            stunProgress.Clear();
            currentlyFocused.Clear();
        }

        // Interações com objetos iluminados.
        UpdateLightPlatforms();
        UpdateLightButtons();
        UpdateGhosts();
    }

    private void UpdateFacingDirection()
    {
        if (playerSprite != null)
        {
            facingDirection =
                playerSprite.flipX ? -1 : 1;
        }
    }

    private void UpdateFlashlightPosition()
    {
        if (player == null)
            return;

        if (flashlightPoint != null)
        {
            Vector3 pointPosition =
                originalFlashlightPointPosition;

            // Espelha o ponto junto com o Player.
            pointPosition.x *= facingDirection;

            flashlightPoint.localPosition =
                pointPosition;

            // Mantém a luz na ponta da lanterna.
            transform.position =
                flashlightPoint.position;
        }
        else
        {
            transform.position =
                player.position + new Vector3(
                    sideOffset * facingDirection,
                    heightOffset,
                    0f
                );
        }
    }

    private void AimAtMouse()
    {
        if (Mouse.current == null || lightPivot == null)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        Vector3 mousePosition =
            Mouse.current.position.ReadValue();

        mousePosition.z = Mathf.Abs(
            lightPivot.position.z -
            mainCamera.transform.position.z
        );

        Vector3 worldMousePosition =
            mainCamera.ScreenToWorldPoint(mousePosition);

        Vector2 direction =
            (Vector2)(worldMousePosition -
            lightPivot.position);

        if (direction.sqrMagnitude < 0.001f)
            return;

        float angle = Mathf.Atan2(
            direction.y,
            direction.x
        ) * Mathf.Rad2Deg;

        // Rotaciona somente o feixe de luz.
        lightPivot.rotation =
            Quaternion.Euler(0f, 0f, angle);
    }

    private void UpdateLightMode()
    {
        if (flashlightLight != null)
        {
            // Mantém o alcance constante.
            flashlightLight.pointLightOuterRadius =
                lightRange;

            // Reduz o ângulo durante o foco.
            flashlightLight.pointLightOuterAngle =
                IsFocusing ? focusAngle : lightAngle;

            // Aumenta a intensidade durante o foco.
            flashlightLight.intensity = IsFocusing
                ? originalLightIntensity *
                  focusIntensityMultiplier
                : originalLightIntensity;
        }

        if (focusVisual != null)
        {
            Vector3 scale = originalFocusScale;

            if (IsFocusing)
            {
                scale.x *= focusVisualWidthMultiplier;
            }

            focusVisual.localScale = scale;
        }
    }

    private void StunEnemies()
    {
        if (!IsFocusing || lightPivot == null)
            return;

        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                lightPivot.position,
                stunRange,
                enemyLayer
            );

        // Limpa os inimigos detectados no frame anterior.
        currentlyFocused.Clear();

        foreach (Collider2D hit in hits)
        {
            EnemyBase enemy =
                hit.GetComponentInParent<EnemyBase>();

            if (enemy == null)
                continue;

            // Verifica se o inimigo está dentro do feixe.
            bool insideBeam = IsInBeam(
                enemy.transform.position,
                stunRange,
                focusAngle
            );

            if (!insideBeam)
                continue;

            // Evita acumular tempo mais de uma vez
            // em inimigos com múltiplos colliders.
            if (!currentlyFocused.Add(enemy))
                continue;

            // Não atordoa novamente durante o mesmo foco.
            if (stunnedEnemies.Contains(enemy) ||
                enemy.IsStunned)
            {
                stunProgress.Remove(enemy);
                continue;
            }

            // Recupera o progresso atual do inimigo.
            if (!stunProgress.TryGetValue(
                enemy, out float progress))
            {
                progress = 0f;
            }

            // Acumula o tempo de exposição contínua.
            progress += Time.deltaTime;

            stunProgress[enemy] = progress;

            // Atordoa somente após o tempo necessário.
            if (progress >= timeToStun)
            {
                enemy.Stun(stunDuration);

                stunnedEnemies.Add(enemy);

                stunProgress.Remove(enemy);

                Debug.Log(
                    enemy.name +
                    " foi atordoado!"
                );
            }
        }

        // Identifica inimigos que saíram do feixe.
        enemiesToReset.Clear();

        foreach (var entry in stunProgress)
        {
            if (entry.Key == null ||
                !currentlyFocused.Contains(entry.Key))
            {
                enemiesToReset.Add(entry.Key);
            }
        }

        // Zera o progresso desses inimigos.
        foreach (EnemyBase enemy in enemiesToReset)
        {
            stunProgress.Remove(enemy);
        }
    }

    private bool IsInBeam(
        Vector2 targetPosition,
        float range,
        float angle)
    {
        if (lightPivot == null)
            return false;

        Vector2 toTarget =
            targetPosition -
            (Vector2)lightPivot.position;

        float distance = toTarget.magnitude;

        if (distance > range)
            return false;

        if (distance < 0.001f)
            return true;

        float angleToTarget = Vector2.Angle(
            lightPivot.right,
            toTarget / distance
        );

        return angleToTarget <= angle / 2f;
    }

    private void UpdateLightButtons()
    {
        if (allButtons == null)
        {
            allButtons =
                FindObjectsOfType<LightButton>();
        }

        float angle =
            IsFocusing && buttonGhostUsesFocusAngle
            ? focusAngle
            : lightAngle;

        foreach (LightButton button in allButtons)
        {
            if (button == null ||
                !button.gameObject.activeInHierarchy)
                continue;

            bool lit = IsInBeam(
                button.transform.position,
                buttonGhostRange,
                angle
            );

            button.SetIlluminated(lit);
        }
    }

    private void UpdateGhosts()
    {
        if (allGhosts == null)
        {
            allGhosts =
                FindObjectsOfType<GhostVisual>();
        }

        float angle =
            IsFocusing && buttonGhostUsesFocusAngle
            ? focusAngle
            : lightAngle;

        foreach (GhostVisual ghost in allGhosts)
        {
            if (ghost == null ||
                !ghost.gameObject.activeInHierarchy)
                continue;

            if (IsInBeam(
                ghost.transform.position,
                buttonGhostRange,
                angle))
            {
                ghost.SetIlluminated(true);
            }
        }
    }

    private void UpdateLightPlatforms()
    {
        if (allPlatforms == null)
        {
            allPlatforms =
                FindObjectsOfType<LightPlatform>();
        }

        float range = platformLightRange;
        float angle = lightAngle;

        if (IsFocusing && platformUsesFocusAngle)
        {
            angle = focusAngle;
        }

        HashSet<LightPlatform> currentlyLit =
            new HashSet<LightPlatform>();

        foreach (LightPlatform platform in allPlatforms)
        {
            if (platform == null ||
                !platform.gameObject.activeInHierarchy)
                continue;

            if (IsInBeam(
                platform.transform.position,
                range,
                angle))
            {
                currentlyLit.Add(platform);
            }
        }

        // Ativa as plataformas iluminadas.
        foreach (LightPlatform platform in currentlyLit)
        {
            if (!illuminatedPlatforms.Contains(platform)
                && debugPlatforms)
            {
                Debug.Log(
                    "Plataforma iluminada: " +
                    platform.name
                );
            }

            platform.SetIlluminated(true);
        }

        // Desativa as plataformas que saíram do feixe.
        foreach (LightPlatform platform
                 in illuminatedPlatforms)
        {
            if (platform != null &&
                !currentlyLit.Contains(platform))
            {
                platform.SetIlluminated(false);

                if (debugPlatforms)
                {
                    Debug.Log(
                        "Plataforma deixou de ser iluminada: " +
                        platform.name
                    );
                }
            }
        }

        illuminatedPlatforms.Clear();

        foreach (LightPlatform platform in currentlyLit)
        {
            illuminatedPlatforms.Add(platform);
        }
    }

    private void OnDisable()
    {
        IsFocusing = false;

        // Limpa o sistema de atordoamento.
        stunnedEnemies.Clear();
        stunProgress.Clear();
        currentlyFocused.Clear();
        enemiesToReset.Clear();

        // Restaura o modo normal da luz.
        UpdateLightMode();

        foreach (LightPlatform platform
                 in illuminatedPlatforms)
        {
            if (platform != null)
            {
                platform.SetIlluminated(false);
            }
        }

        illuminatedPlatforms.Clear();

        if (allButtons != null)
        {
            foreach (LightButton button in allButtons)
            {
                if (button != null)
                {
                    button.SetIlluminated(false);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = lightPivot != null
            ? lightPivot.position
            : transform.position;

        // Alcance do atordoamento.
        Gizmos.color = Color.yellow;

        Gizmos.DrawWireSphere(
            origin,
            stunRange
        );

        // Alcance das plataformas.
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            origin,
            platformLightRange
        );

        // Alcance dos botões e fantasmas.
        Gizmos.color = Color.magenta;

        Gizmos.DrawWireSphere(
            origin,
            buttonGhostRange
        );

        // Ponto de origem da lanterna.
        if (flashlightPoint != null)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawSphere(
                flashlightPoint.position,
                0.06f
            );
        }
    }
}