
using System.Collections.Generic;
using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [Header("Dano do ataque")]
    public int damage = 1;

    // Guarda os inimigos atingidos durante o golpe atual.
    private HashSet<EnemyBase> hitEnemies = new HashSet<EnemyBase>();

    private void OnEnable()
    {
        // Permite atingir novamente no próximo golpe.
        hitEnemies.Clear();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Procura o inimigo no objeto atingido ou nos pais.
        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();

        if (enemy == null)
            return;

        // Só causa dano se o inimigo estiver atordoado.
        if (!enemy.IsStunned)
            return;

        // Evita dano repetido durante o mesmo golpe.
        if (hitEnemies.Contains(enemy))
            return;

        hitEnemies.Add(enemy);

        Debug.Log("Hitbox aplicando dano em: " + enemy.name);

        enemy.TakeDamage(damage);
    }
}
