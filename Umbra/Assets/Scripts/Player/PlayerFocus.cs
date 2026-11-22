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
        // Impede o foco caso o Player esteja morto.
        if (playerHealth != null && playerHealth.IsDead)
        {
            isFocusing = false;
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
        }

        if (isFocusing)
        {
            if (!focusHeld)
            {
                StartRecharge();
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
            StartRecharge();
        }
    }

    private void StartRecharge()
    {
        isFocusing = false;
        isRecharging = true;

        regenerationTimer = regenerationDelay;
    }

    private void RegenerateFocus()
    {
        // Aguarda antes de começar a regenerar.
        if (regenerationTimer > 0f)
        {
            regenerationTimer -= Time.deltaTime;
            return;
        }

        currentFocus +=
            regenerationRate * Time.deltaTime;

        currentFocus = Mathf.Clamp(
            currentFocus,
            0f,
            maxFocus
        );

        // Libera o foco somente com a barra cheia.
        if (currentFocus >= maxFocus)
        {
            currentFocus = maxFocus;
            isRecharging = false;
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
    }
}