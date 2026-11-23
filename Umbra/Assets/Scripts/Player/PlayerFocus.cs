using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerFocus : MonoBehaviour
{
    [Header("Energia do Foco")]
    public float maxFocus = 100f;
    public float consumptionRate = 20f;

    [Header("Regeneração")]
    public float regenerationRate = 25f;
    public float regenerationDelay = 1f;

    [Header("Interface")]
    public HealthHUD healthHUD;

    [Header("Áudio")]
    [SerializeField] private FlashlightAudio flashlightAudio;

    private float currentFocus;
    private float regenerationTimer;

    private bool isFocusing;
    private bool isRecharging;

    private PlayerHealth playerHealth;

    public bool IsFocusing => isFocusing;
    public bool IsRecharging => isRecharging;
    public float CurrentFocus => currentFocus;

    private void Awake()
    {
        currentFocus = maxFocus;

        playerHealth = GetComponent<PlayerHealth>();

        // Localiza automaticamente o áudio da lanterna.
        if (flashlightAudio == null)
        {
            flashlightAudio =
                GetComponentInChildren<FlashlightAudio>();
        }
    }

    private void Start()
    {
        // Encontra automaticamente o HUD da fase.
        if (healthHUD == null)
        {
            healthHUD =
                FindFirstObjectByType<HealthHUD>();
        }

        if (healthHUD == null)
        {
            Debug.LogWarning(
                "PlayerFocus: HealthHUD não encontrado."
            );
        }

        UpdateHUD();
    }

    private void Update()
    {
        // Interrompe o foco e os sons caso o Player morra.
        if (playerHealth != null && playerHealth.IsDead)
        {
            isFocusing = false;
            isRecharging = false;

            if (flashlightAudio != null)
                flashlightAudio.StopAllSounds();

            return;
        }

        bool focusPressed =
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        bool focusHeld =
            Mouse.current != null &&
            Mouse.current.leftButton.isPressed;

        // Bloqueia completamente o foco durante a recarga.
        if (isRecharging)
        {
            isFocusing = false;

            RegenerateFocus();

            UpdateHUD();

            return;
        }

        // Inicia o foco com um novo clique.
        if (focusPressed && currentFocus > 0f)
        {
            isFocusing = true;

            // Som de ativação e foco contínuo.
            if (flashlightAudio != null)
                flashlightAudio.StartFocus();
        }

        if (isFocusing)
        {
            if (!focusHeld)
            {
                // Interrompe o foco voluntariamente.
                StartRecharge(false);
            }
            else
            {
                ConsumeFocus();
            }
        }

        UpdateHUD();
    }

    private void ConsumeFocus()
    {
        currentFocus -=
            consumptionRate * Time.deltaTime;

        currentFocus = Mathf.Clamp(
            currentFocus,
            0f,
            maxFocus
        );

        // Quando a energia acaba, inicia a recarga.
        if (currentFocus <= 0f)
        {
            StartRecharge(true);
        }
    }

    private void StartRecharge(bool focusEmpty)
    {
        isFocusing = false;
        isRecharging = true;

        regenerationTimer = regenerationDelay;

        if (flashlightAudio != null)
        {
            if (focusEmpty)
            {
                // Som de energia esgotada.
                flashlightAudio.PlayFocusEmpty();
            }
            else
            {
                // Som de desativação voluntária.
                flashlightAudio.StopFocus();
            }
        }
    }

    private void RegenerateFocus()
    {
        // Aguarda antes de começar a regenerar.
        if (regenerationTimer > 0f)
        {
            regenerationTimer -= Time.deltaTime;
            return;
        }

        float previousFocus = currentFocus;

        currentFocus +=
            regenerationRate * Time.deltaTime;

        currentFocus = Mathf.Clamp(
            currentFocus,
            0f,
            maxFocus
        );

        // Reproduz o som somente durante a regeneração.
        if (currentFocus > previousFocus &&
            currentFocus < maxFocus)
        {
            if (flashlightAudio != null)
                flashlightAudio.StartRecharge();
        }

        // Libera o foco somente com a barra cheia.
        if (currentFocus >= maxFocus)
        {
            currentFocus = maxFocus;
            isRecharging = false;

            if (flashlightAudio != null)
            {
                // Interrompe a recarga e toca o som final.
                flashlightAudio.PlayFocusRecovered();
            }
        }
    }

    private void UpdateHUD()
    {
        if (healthHUD != null)
        {
            healthHUD.UpdateFocus(
                currentFocus,
                maxFocus
            );
        }
    }

    private void OnDisable()
    {
        isFocusing = false;

        if (flashlightAudio != null)
            flashlightAudio.StopAllSounds();
    }
}