
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [Header("Configurações do Ataque")]
    [SerializeField] private int damage = 1;

    [Header("Referências")]
    [SerializeField] private Collider2D attackCollider;

    // Inimigos atingidos durante o ataque atual.
    private HashSet<Component> hitTargets =
        new HashSet<Component>();

    private void Awake()
    {
        if (attackCollider == null)
            attackCollider = GetComponent<Collider2D>();

        if (attackCollider != null)
        {
            attackCollider.isTrigger = true;
            attackCollider.enabled = false;
        }
    }

    // Chamado pelo PlayerAttack quando o golpe começa.
    public void ActivateHitbox()
    {
        hitTargets.Clear();

        if (attackCollider != null)
            attackCollider.enabled = true;
    }

    // Chamado pelo PlayerAttack quando o golpe termina.
    public void DeactivateHitbox()
    {
        if (attackCollider != null)
            attackCollider.enabled = false;

        hitTargets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (attackCollider == null ||
            !attackCollider.enabled)
        {
            return;
        }

        // Verifica primeiro se atingiu um boss.
        BossHealth bossHealth =
            other.GetComponentInParent<BossHealth>();

        if (bossHealth != null)
        {
            BossStun bossStun =
                bossHealth.GetComponent<BossStun>();

            // Não permite dano sem atordoamento.
            if (bossStun == null ||
                !bossStun.IsStunned)
            {
                return;
            }

            // Evita acertar um boss morto.
            if (bossHealth.IsDead ||
                bossHealth.IsDepleted)
            {
                return;
            }

            // Apenas um acerto por ativação.
            if (hitTargets.Contains(bossHealth))
                return;

            hitTargets.Add(bossHealth);

            bossHealth.TakeDamage(damage);

            return;
        }

        // Verifica inimigos comuns.
        EnemyBase enemy =
            other.GetComponentInParent<EnemyBase>();

        if (enemy == null)
            return;

        // Inimigos comuns também precisam
        // estar atordoados para receber dano.
        if (!enemy.IsStunned)
            return;

        if (hitTargets.Contains(enemy))
            return;

        hitTargets.Add(enemy);

        enemy.TakeDamage(damage);
    }

    private void OnDisable()
    {
        if (attackCollider != null)
            attackCollider.enabled = false;

        hitTargets.Clear();
    }
}
