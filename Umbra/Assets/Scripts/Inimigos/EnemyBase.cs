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

    protected bool isStunned = false;
    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;

    private Coroutine stunCoroutine;

    // Permite que outros scripts verifiquem se o inimigo está atordoado.
    public bool IsStunned => isStunned;

    protected virtual void Awake()
    {
        // Inicializa a vida de acordo com o valor definido no Inspector.
        currentHealth = Mathf.Max(1, maxHealth);

        // Procura o SpriteRenderer no próprio objeto ou em seus filhos.
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning(
                "EnemyBase: nenhum SpriteRenderer encontrado em " + gameObject.name,
                gameObject
            );
        }
    }

    public virtual void TakeDamage(int damage)
    {
        // Ignora dano inválido.
        if (damage <= 0)
            return;

        // Não permite causar dano enquanto o inimigo não estiver atordoado.
        if (!IsStunned)
            return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);

        Debug.Log(
            gameObject.name + " recebeu " + damage +
            " de dano. Vida restante: " + currentHealth + "/" + maxHealth,
            gameObject
        );

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // Atordoa usando a duração configurada no Inspector.
    public virtual void Stun()
    {
        Stun(stunDuration);
    }

    // Atordoa pelo tempo informado.
    public virtual void Stun(float duration)
    {
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        isStunned = true;

        ApplyStunnedColor();

        stunCoroutine = StartCoroutine(
            StunRoutine(Mathf.Max(0f, duration))
        );
    }

    private void ApplyStunnedColor()
    {
        // Tenta localizar o SpriteRenderer novamente, se necessário.
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

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
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;

        stunCoroutine = null;
    }

    protected virtual void Die()
    {
        // Remove o inimigo da cena quando sua vida chega a zero.
        Destroy(gameObject);
    }
}
