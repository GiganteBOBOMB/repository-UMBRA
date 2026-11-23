using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ArcherEnemy : EnemyBase
{
    [Header("Detecção e perseguição")]
    public float detectionRange = 8f;
    public float attackRange = 4f;
    public float moveSpeed = 2f;

    [Header("Configuração do disparo")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float arrowSpeed = 8f;
    public float arrowDamage = 1f;
    public float attackCooldown = 2f;

    [Header("Recuperação do atordoamento")]
    public float stunRecoveryDelay = 1f;

    private Transform player;
    private Rigidbody2D rb;
    private PlayerHealth playerHealth;

    private float nextAttackTime;
    private float horizontalMovement;

    private bool hasDetectedPlayer;

    private Vector3 originalFirePointPosition;

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();

        if (firePoint != null)
        {
            originalFirePointPosition =
                firePoint.localPosition;
        }
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

        nextAttackTime = Time.time;
    }

    private void Update()
    {
        horizontalMovement = 0f;

        // Impede ações caso o inimigo ou o Player
        // estejam mortos.
        if (IsDead ||
            player == null ||
            (playerHealth != null && playerHealth.IsDead))
        {
            hasDetectedPlayer = false;

            StopMovement();

            return;
        }

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        bool playerInRange =
            distance <= detectionRange;

        // Permite reproduzir novamente o som de detecção
        // quando o Player voltar para o alcance.
        if (!playerInRange)
        {
            hasDetectedPlayer = false;
        }

        // Não anda nem atira enquanto estiver atordoado.
        if (IsStunned)
        {
            StopMovement();

            return;
        }

        // Player fora do raio de detecção.
        if (!playerInRange)
        {
            StopMovement();

            return;
        }

        // Reproduz o som de detecção apenas uma vez.
        if (!hasDetectedPlayer)
        {
            hasDetectedPlayer = true;

            if (enemyAudio != null)
                enemyAudio.PlayDetection();
        }

        // Direção do Player.
        float direction = player.position.x >=
            transform.position.x ? 1f : -1f;

        UpdateDirection(direction);

        // Persegue quando estiver fora do raio de ataque.
        if (distance > attackRange)
        {
            horizontalMovement = direction * moveSpeed;

            // Reproduz o som contínuo de movimentação.
            if (enemyAudio != null)
            {
                if (Mathf.Abs(horizontalMovement) > 0.01f)
                    enemyAudio.StartMovement();
                else
                    enemyAudio.StopMovement();
            }
        }
        else
        {
            // Para para realizar o disparo.
            StopMovement();

            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time +
                    Mathf.Max(0.05f, attackCooldown);

                Shoot(direction);
            }
        }
    }

    private void FixedUpdate()
    {
        // Impede movimentação horizontal durante
        // o stun ou após a morte.
        float velocityX = IsStunned || IsDead
            ? 0f
            : horizontalMovement;

        rb.linearVelocity = new Vector2(
            velocityX,
            rb.linearVelocity.y
        );
    }

    private void UpdateDirection(float direction)
    {
        // Vira o sprite para o Player.
        if (spriteRenderer != null)
            spriteRenderer.flipX = direction < 0f;

        // Mantém o ponto de disparo na frente do arqueiro.
        if (firePoint != null)
        {
            firePoint.localPosition = new Vector3(
                Mathf.Abs(originalFirePointPosition.x)
                    * direction,
                originalFirePointPosition.y,
                originalFirePointPosition.z
            );
        }
    }

    private void Shoot(float direction)
    {
        if (IsStunned || IsDead)
            return;

        if (playerHealth != null && playerHealth.IsDead)
            return;

        if (arrowPrefab == null || firePoint == null)
            return;

        // Mantém o disparo horizontal.
        Vector2 shootDirection = new Vector2(
            direction,
            0f
        );

        GameObject arrowObject = Instantiate(
            arrowPrefab,
            firePoint.position,
            Quaternion.identity
        );

        Arrow arrow = arrowObject.GetComponent<Arrow>();

        if (arrow != null)
        {
            arrow.Launch(
                shootDirection,
                arrowSpeed,
                arrowDamage
            );

            // Reproduz o áudio no momento do disparo.
            if (enemyAudio != null)
                enemyAudio.PlayAttack();
        }
        else
        {
            Debug.LogWarning(
                "O prefab da flecha não possui o script Arrow!"
            );

            Destroy(arrowObject);
        }
    }

    private void StopMovement()
    {
        horizontalMovement = 0f;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );
        }

        // Interrompe o som contínuo de movimentação.
        if (enemyAudio != null)
            enemyAudio.StopMovement();
    }

    public override void Stun(float duration)
    {
        if (IsDead)
            return;

        // Interrompe a perseguição e seus sons.
        StopMovement();

        // Mantém o sistema de stun do EnemyBase.
        base.Stun(duration);
    }

    protected override void EndStun()
    {
        base.EndStun();

        // Reinicia o tempo de disparo após o atordoamento.
        nextAttackTime = Time.time +
            Mathf.Max(0f, stunRecoveryDelay);
    }

    protected override void Die()
    {
        if (IsDead)
            return;

        StopMovement();

        // Reproduz o som de morte e destrói o inimigo.
        base.Die();
    }

    private void OnDisable()
    {
        hasDetectedPlayer = false;

        StopMovement();
    }

    private void OnDrawGizmosSelected()
    {
        // Raio de detecção.
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );

        // Raio de ataque.
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}