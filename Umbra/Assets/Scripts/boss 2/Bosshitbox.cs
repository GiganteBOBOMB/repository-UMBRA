using UnityEngine;

// Área de dano do Boss. Usada nas mãos (ligada só no impacto) e nas ondas (sempre ligada).
// Precisa de um Collider2D no mesmo objeto. Ele é forçado a ser Trigger.
[RequireComponent(typeof(Collider2D))]
public class BossHitbox : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private string targetTag = "Player";

    [Tooltip("Ligado: nasce ativa (use nas ondas).\nDesligado: só ativa quando o ataque mandar (use nas mãos).")]
    [SerializeField] private bool startActive = false;

    private Collider2D area;
    private bool alreadyHit;

    private void Awake()
    {
        area = GetComponent<Collider2D>();
        area.isTrigger = true;
        area.enabled = startActive;
    }

    // Chamado pelos ataques para ligar/desligar a área de dano
    public void SetActive(bool value)
    {
        if (value) alreadyHit = false; // cada ativação só pode acertar uma vez
        area.enabled = value;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (alreadyHit || !other.CompareTag(targetTag)) return;

        alreadyHit = true;
        DealDamage(other);
    }

    private void DealDamage(Collider2D target)
    {
        Debug.Log($"[Boss] Acertou {target.name}: {damage} de dano");

        // TODO: chamar aqui o script de vida do seu player. Exemplo:
        // target.GetComponentInParent<PlayerHealth>()?.TakeDamage(damage);
    }
}