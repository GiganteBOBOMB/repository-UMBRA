using System.Collections;
using UnityEngine;

public class ArcherEnemy : EnemyBase
{
    [Header("Detecção")]
    public float detectionRange = 8f;

    [Header("Ataque")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float arrowSpeed = 8f;
    public float arrowDamage = 1f;
    public float attackCooldown = 2f;

    private Transform player;
    private bool isAttacking;

    protected override void Awake()
    {
        base.Awake();

        // Procura o jogador na cena.
        GameObject playerObject =
            GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    private void Update()
    {
        // Não ataca enquanto estiver atordoado.
        if (IsStunned || isAttacking || player == null)
            return;

        // Calcula a distância até o jogador.
        float distance =
            Vector2.Distance(transform.position, player.position);

        // Ataca quando o jogador entra no alcance.
        if (distance <= detectionRange)
            StartCoroutine(ShootRoutine());
    }

    private IEnumerator ShootRoutine()
    {
        if (isAttacking || IsStunned)
            yield break;

        isAttacking = true;

        // Vira na direção do jogador.
        float direction =
            Mathf.Sign(player.position.x - transform.position.x);

        if (spriteRenderer != null)
            spriteRenderer.flipX = direction < 0f;

        // Cria e dispara a flecha.
        if (arrowPrefab != null && firePoint != null)
        {
            GameObject arrowObject = Instantiate(
                arrowPrefab,
                firePoint.position,
                Quaternion.identity
            );

            Arrow arrow = arrowObject.GetComponent<Arrow>();

            if (arrow != null)
            {
                Vector2 aimDirection =
                    ((Vector2)player.position -
                    (Vector2)firePoint.position).normalized;

                arrow.Launch(
                    aimDirection,
                    arrowSpeed,
                    arrowDamage
                );
            }
            else
            {
                Debug.LogWarning(
                    "A flecha não possui o componente Arrow!",
                    arrowObject
                );
            }
        }

        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
    }

    // Mostra na Console quando o arqueiro é atordoado.
    public override void Stun(float duration = -1f)
    {
        StopCoroutine(nameof(ShootRoutine));
        isAttacking = false;

        Debug.Log("Arqueiro atordoado: " + gameObject.name);

        base.Stun(duration);
    }

    // Mostra na Console quando o arqueiro recebe dano.
    public override void TakeDamage(int damage)
    {
        Debug.Log(
            "Arqueiro recebeu tentativa de dano: " +
            damage + ". Atordoado: " + IsStunned,
            gameObject
        );

        base.TakeDamage(damage);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(
            transform.position,
            detectionRange
        );
    }
}
