using UnityEngine;

// Reage à morte do Boss: para o ritmo, cancela o ataque em andamento e desliga o sistema.
// Fica separado para BossHealth não precisar conhecer ritmo nem ataques.
public class BossDeathHandler : MonoBehaviour
{
    [SerializeField] private BossHealth health;
    [SerializeField] private BossRhythmController rhythm;
    [SerializeField] private BossAttackController attackController;

    [Tooltip("Objeto opcional para ativar na morte (explosão, animação, tela de vitória...).")]
    [SerializeField] private GameObject activateOnDeath;

    private void Awake()
    {
        if (health == null) health = GetComponent<BossHealth>();
        if (rhythm == null) rhythm = GetComponent<BossRhythmController>();
        if (attackController == null) attackController = GetComponent<BossAttackController>();
    }

    private void OnEnable()
    {
        if (health != null) health.OnDied += HandleDeath;
    }

    private void OnDisable()
    {
        if (health != null) health.OnDied -= HandleDeath;
    }

    private void HandleDeath()
    {
        // Ordem importa: para o relógio primeiro, depois cancela o ataque
        if (rhythm != null) rhythm.Stop();
        if (attackController != null) attackController.CancelCurrentAttack();

        if (activateOnDeath != null) activateOnDeath.SetActive(true);

        Debug.Log("[Boss] Derrotado");
    }
}