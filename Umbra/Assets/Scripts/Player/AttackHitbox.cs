
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class AttackHitbox : MonoBehaviour
{
    [Header("Configuração do dano")]
    public int damage = 1;

    [Header("Depuração")]
    public bool debugAttack = true;

    private BoxCollider2D attackCollider;

    private HashSet<EnemyBase> enemiesHit =
        new HashSet<EnemyBase>();

    private void Awake()
    {
        attackCollider = GetComponent<BoxCollider2D>();
        attackCollider.isTrigger = true;
        attackCollider.enabled = false;
    }

    public void ActivateHitbox()
    {
        enemiesHit.Clear();
        attackCollider.enabled = true;

        if (debugAttack)
            Debug.Log("Hitbox ATIVADA");
    }

    public void DeactivateHitbox()
    {
        if (attackCollider != null)
            attackCollider.enabled = false;

        if (debugAttack)
            Debug.Log("Hitbox DESATIVADA");
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
        if (!attackCollider.enabled)
            return;

        EnemyBase enemy =
            other.GetComponentInParent<EnemyBase>();

        if (enemy == null)
            return;

        if (enemiesHit.Contains(enemy))
            return;

        if (!enemy.IsStunned)
        {
            if (debugAttack)
                Debug.Log(enemy.name + " não está atordoado!");

            return;
        }

        enemiesHit.Add(enemy);

        enemy.TakeDamage(damage);

        if (debugAttack)
            Debug.Log("Ataque acertou: " + enemy.name);
    }

    private void OnDrawGizmos()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();

        if (box == null)
            return;

        Gizmos.matrix = transform.localToWorldMatrix;

        // Vermelho quando ativa.
        // Amarelo quando desativada.
        Gizmos.color = box.enabled
            ? Color.red
            : Color.yellow;

        Vector3 center = new Vector3(
            box.offset.x,
            box.offset.y,
            0f
        );

        Vector3 size = new Vector3(
            box.size.x,
            box.size.y,
            0.1f
        );

        Gizmos.DrawWireCube(center, size);
    }
}
