
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

    private float nextAttackTime;
    private float horizontalMovement;

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
            player = playerObject.transform;

        nextAttackTime = Time.time;
    }

    private void Update()
    {
        horizontalMovement = 0f;

        // Não anda nem atira enquanto estiver atordoado.
        if (IsStunned || player == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            player.position
        );

        // Player fora do raio de detecção.
        if (distance > detectionRange)
            return;

        // Direção do Player.
        float direction = player.position.x >=
            transform.position.x ? 1f : -1f;

        UpdateDirection(direction);

        // Persegue quando estiver fora do raio de ataque.
        if (distance > attackRange)
        {
            horizontalMovement = direction * moveSpeed;
        }
        else
        {
            // Para para realizar o disparo.
            horizontalMovement = 0f;

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
        // Impede movimentação horizontal durante o stun.
        float velocityX = IsStunned
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
        if (IsStunned)
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
        }
        else
        {
            Debug.LogWarning(
                "O prefab da flecha não possui o script Arrow!"
            );

            Destroy(arrowObject);
        }
    }

    public override void Stun(float duration)
    {
        // Interrompe imediatamente a perseguição.
        horizontalMovement = 0f;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                0f,
                rb.linearVelocity.y
            );
        }

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
