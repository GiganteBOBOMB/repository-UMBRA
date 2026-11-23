using System.Collections;
using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Vida do Inimigo")]
    [Tooltip("Quantidade máxima de vida deste inimigo.")]
    public int maxHealth = 3;

    protected int currentHealth;

    [Header("Atordoamento")]
    [Tooltip("Tempo padrão que o inimigo fica atordoado.")]
    public float stunDuration = 2f;

    [Tooltip("Cor do inimigo enquanto estiver atordoado.")]
    public Color stunnedColor = Color.yellow;

    [Header("Áudio")]
    [SerializeField] protected EnemyAudio enemyAudio;

    protected bool isStunned = false;
    protected bool isDead = false;

    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;

    private Coroutine stunCoroutine;

    public bool IsStunned => isStunned;
    public bool IsDead => isDead;

    protected virtual void Awake()
    {
        currentHealth = Mathf.Max(1, maxHealth);

        spriteRenderer =
            GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning(
                "EnemyBase: nenhum SpriteRenderer encontrado em "
                + gameObject.name,
                gameObject
            );
        }

        // Localiza automaticamente o áudio do inimigo.
        if (enemyAudio == null)
        {
            enemyAudio =
                GetComponentInChildren<EnemyAudio>();
        }
    }

    public virtual void TakeDamage(int damage)
    {
        // Ignora dano inválido ou inimigos mortos.
        if (damage <= 0 || isDead)
            return;

        // Só recebe dano enquanto estiver atordoado.
        if (!IsStunned)
            return;

        currentHealth -= damage;

        currentHealth = Mathf.Max(
            0,
            currentHealth
        );

        Debug.Log(
            gameObject.name + " recebeu " + damage +
            " de dano. Vida restante: " +
            currentHealth + "/" + maxHealth,
            gameObject
        );

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Som de dano válido.
            if (enemyAudio != null)
                enemyAudio.PlayHurt();
        }
    }

    public virtual void Stun()
    {
        Stun(stunDuration);
    }

    public virtual void Stun(float duration)
    {
        if (isDead)
            return;

        // Verifica se já estava atordoado.
        bool wasStunned = isStunned;

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        isStunned = true;

        ApplyStunnedColor();

        // Toca somente ao iniciar um novo atordoamento.
        if (!wasStunned && enemyAudio != null)
        {
            enemyAudio.PlayStun();
        }

        stunCoroutine = StartCoroutine(
            StunRoutine(Mathf.Max(0f, duration))
        );
    }

    private void ApplyStunnedColor()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
            spriteRenderer.color = stunnedColor;
    }

    private IEnumerator StunRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        EndStun();
    }

    protected virtual void EndStun()
    {
        isStunned = false;

        if (spriteRenderer == null)
        {
            spriteRenderer =
                GetComponentInChildren<SpriteRenderer>();
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        stunCoroutine = null;
    }

    protected virtual void Die()
    {
        // Impede que a morte seja executada mais de uma vez.
        if (isDead)
            return;

        isDead = true;

        StopAllCoroutines();

        // Reproduz o som de morte independentemente do inimigo.
        if (enemyAudio != null)
        {
            enemyAudio.StopMovement();
            enemyAudio.PlayDeath();
        }

        // Remove imediatamente o inimigo da cena.
        Destroy(gameObject);
    }
}