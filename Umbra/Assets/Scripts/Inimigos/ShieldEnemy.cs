using System.Collections;
using UnityEngine;

public class ShieldEnemy : EnemyBase
{
    [Header("Detecção")]
    public float detectionRange = 7f;
    public float attackRange = 2f;

    [Header("Movimentação")]
    public float moveSpeed = 2.5f;

    [Header("Investida")]
    public float chargeSpeed = 12f;
    public float chargeDuration = 0.3f;
    public float preparationTime = 0.4f;
    public float attackCooldown = 2f;

    [Header("Dano")]
    public float contactDamage = 1f;

    [Header("Recuperação do Stun")]
    public float stunRecoveryDelay = 1f;

    private Transform player;
    private Rigidbody2D rb;
    private PlayerHealth playerHealth;

    private float horizontalMovement;
    private float chargeDirection;
    private float nextAttackTime;

    private bool isAttacking;
    private bool isCharging;
    private bool hasDamagedPlayer;

    // Controla o som de detecção.
    private bool hasDetectedPlayer;

    private Coroutine attackCoroutine;

    public bool IsCharging => isCharging;

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            player = playerObject.transform;

            playerHealth =
                playerObject.GetComponent<PlayerHealth>();
        }
    }

    private void Update()
    {
        horizontalMovement = 0f;

        if (IsDead ||
            player == null ||
            (playerHealth != null && playerHealth.IsDead))
        {
            hasDetectedPlayer = false;

            CancelAttack();
            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        bool playerInRange =
            distance <= detectionRange;

        // Permite detectar novamente caso o jogador saia do alcance.
        if (!playerInRange)
        {
            hasDetectedPlayer = false;
        }

        // Não realiza ações enquanto estiver atordoado.
        if (IsStunned)
        {
            StopMovementSound();
            return;
        }

        // Não interfere durante a investida.
        if (isAttacking)
        {
            StopMovementSound();
            return;
        }

        // Não detectou o Player.
        if (!playerInRange)
        {
            StopMovementSound();
            return;
        }

        // Reproduz o som apenas ao detectar o Player.
        if (!hasDetectedPlayer)
        {
            hasDetectedPlayer = true;

            if (enemyAudio != null)
                enemyAudio.PlayDetection();
        }

        float direction = Mathf.Sign(
            player.position.x - transform.position.x
        );

        // Vira o inimigo na direção do Player.
        FlipEnemy(direction);

        // Persegue o Player até alcançar
        // a distância de ataque.
        if (distance > attackRange)
        {
            horizontalMovement = direction * moveSpeed;

            // Inicia o som contínuo de movimentação.
            if (enemyAudio != null &&
                Mathf.Abs(horizontalMovement) > 0.01f)
            {
                enemyAudio.StartMovement();
            }
        }
        else
        {
            horizontalMovement = 0f;

            StopMovementSound();

            // Verifica se pode atacar novamente.
            if (Time.time >= nextAttackTime)
            {
                attackCoroutine = StartCoroutine(
                    AttackRoutine(direction)
                );
            }
        }
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        // Impede movimentação durante o stun ou morte.
        if (IsStunned || IsDead)
        {
            StopMovement();
            return;
        }

        // Movimentação durante a investida.
        if (isCharging)
        {
            rb.linearVelocity = new Vector2(
                chargeDirection * chargeSpeed,
                rb.linearVelocity.y
            );

            return;
        }

        // Fica parado durante a preparação.
        if (isAttacking)
        {
            StopMovement();
            return;
        }

        // Movimentação normal.
        rb.linearVelocity = new Vector2(
            horizontalMovement,
            rb.linearVelocity.y
        );
    }

    private IEnumerator AttackRoutine(float direction)
    {
        isAttacking = true;
        hasDamagedPlayer = false;

        chargeDirection = direction;

        horizontalMovement = 0f;

        StopMovement();

        // Preparação da investida.
        yield return new WaitForSeconds(preparationTime);

        if (IsStunned || IsDead)
        {
            CancelAttack();
            yield break;
        }

        // Inicia a investida.
        isCharging = true;

        // Reproduz o som da investida.
        if (enemyAudio != null)
            enemyAudio.PlayAttack();

        yield return new WaitForSeconds(chargeDuration);

        // Finaliza a investida.
        isCharging = false;
        isAttacking = false;

        StopMovement();

        // Inicia o cooldown do ataque.
        nextAttackTime = Time.time + attackCooldown;

        attackCoroutine = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        TryDamagePlayer(collision.collider);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void TryDamagePlayer(Collider2D other)
    {
        // Só causa dano durante a investida.
        if (!isCharging || IsStunned || IsDead)
            return;

        // Impede múltiplos danos na mesma investida.
        if (hasDamagedPlayer)
            return;

        PlayerHealth targetHealth =
            other.GetComponentInParent<PlayerHealth>();

        if (targetHealth == null)
            return;

        // Verifica se o Player pode receber dano.
        if (targetHealth.IsDead ||
            targetHealth.IsInvulnerable)
            return;

        // Evita causar dano através da hitbox
        // de ataque do próprio Player.
        if (other.GetComponent<AttackHitbox>() != null)
            return;

        hasDamagedPlayer = true;

        targetHealth.TakeDamage(contactDamage);

        Debug.Log(
            "Shield causou " + contactDamage +
            " de dano ao Player."
        );
    }

    private void FlipEnemy(float direction)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction < 0f;
        }
    }

    private void StopMovementSound()
    {
        if (enemyAudio != null)
            enemyAudio.StopMovement();
    }

    private void StopMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );
        }

        StopMovementSound();
    }

    private void CancelAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            attackCoroutine = null;
        }

        isAttacking = false;
        isCharging = false;

        horizontalMovement = 0f;

        StopMovement();
    }

    public override void Stun(float duration)
    {
        if (IsDead)
            return;

        // Cancela a investida ao ser atordoado.
        CancelAttack();

        base.Stun(duration);
    }

    protected override void EndStun()
    {
        base.EndStun();

        // Impede um ataque imediatamente após o stun.
        nextAttackTime = Time.time + stunRecoveryDelay;
    }

    protected override void Die()
    {
        if (IsDead)
            return;

        CancelAttack();

        base.Die();
    }

    private void OnDisable()
    {
        CancelAttack();
    }

    private void OnDrawGizmosSelected()
    {
        // Alcance de detecção.
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        // Alcance de ataque.
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}