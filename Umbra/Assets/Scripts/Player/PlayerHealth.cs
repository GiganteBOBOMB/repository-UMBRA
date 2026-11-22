using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 5f;

    [Header("Morte")]
    public float restartDelay = 1f;

    [Header("Invulnerabilidade")]
    public float invulnerabilityDuration = 1f;
    public float blinkInterval = 0.1f;

    [Header("Interface")]
    public HealthHUD healthHUD;

    private float currentHealth;

    private bool isDead;
    private bool isInvulnerable;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    public float CurrentHealth => currentHealth;
    public bool IsDead => isDead;
    public bool IsInvulnerable => isInvulnerable;

    private void Awake()
    {
        currentHealth = maxHealth;

        spriteRenderer =
            GetComponentInChildren<SpriteRenderer>();

        rb = GetComponent<Rigidbody2D>();
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
                "PlayerHealth: HealthHUD não encontrado."
            );
        }

        UpdateHUD();
    }

    public void TakeDamage(float damage)
    {
        if (isDead || isInvulnerable || damage <= 0f)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Clamp(
            currentHealth,
            0f,
            maxHealth
        );

        UpdateHUD();

        Debug.Log("Vida do Player: " + currentHealth);

        if (currentHealth <= 0f)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvulnerabilityRoutine());
        }
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        isInvulnerable = true;

        float elapsedTime = 0f;

        float interval = Mathf.Max(
            0.02f,
            blinkInterval
        );

        while (elapsedTime < invulnerabilityDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled =
                    !spriteRenderer.enabled;
            }

            yield return new WaitForSeconds(interval);

            elapsedTime += interval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        isInvulnerable = false;
    }

    public void Heal(float amount)
    {
        if (isDead || amount <= 0f)
            return;

        currentHealth = Mathf.Clamp(
            currentHealth + amount,
            0f,
            maxHealth
        );

        UpdateHUD();
    }

    private void UpdateHUD()
    {
        if (healthHUD != null)
        {
            healthHUD.UpdateHealth(
                currentHealth,
                maxHealth
            );
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        StopAllCoroutines();

        isInvulnerable = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        PlayerController playerController =
            GetComponent<PlayerController>();

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        PlayerAttack playerAttack =
            GetComponent<PlayerAttack>();

        if (playerAttack != null)
        {
            playerAttack.enabled = false;
        }

        Debug.Log("Player morreu.");

        StartCoroutine(RestartScene());
    }

    private IEnumerator RestartScene()
    {
        yield return new WaitForSeconds(restartDelay);

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        isInvulnerable = false;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }
}