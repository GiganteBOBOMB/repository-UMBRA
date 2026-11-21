using System;
using UnityEngine;

// Só cuida da vida do Boss. Não sabe nada de ritmo, ataques ou fases:
// quem quiser reagir se inscreve nos eventos.
public class BossHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 100;
    [SerializeField] private bool logDamage = false;

    [Tooltip("Desligado (padrão): ao chegar a 0 de vida o boss morre na hora (OnDied).\n" +
             "Ligado: ao chegar a 0 ele só avisa por OnDepleted e para de tomar dano. Ele só morre\n" +
             "quando alguém chamar ConfirmDeath() (ex.: depois de um ataque final).")]
    [SerializeField] private bool deferDeath = false;

    // (vida atual, vida máxima)
    public event Action<int, int> OnHealthChanged;
    public event Action OnDepleted;   // vida chegou a 0, mas a morte foi adiada (Defer Death)
    public event Action OnDied;

    public int Current { get; private set; }
    public int Max => maxHealth;
    public float Ratio => (float)Current / maxHealth;   // 1 = cheia, 0 = zerada
    public bool IsDepleted { get; private set; }
    public bool IsDead { get; private set; }

    // Ligue para true em cutscenes ou transições de fase para o boss não tomar dano
    public bool Invulnerable { get; set; }

    private void Awake()
    {
        Current = maxHealth;
    }

    private void Start()
    {
        // Avisa o estado inicial (cheia) para quem se inscreveu no OnEnable/Awake
        OnHealthChanged?.Invoke(Current, maxHealth);
    }

    // O ataque do seu player chama este método
    public void TakeDamage(int amount)
    {
        if (IsDead || IsDepleted || Invulnerable || amount <= 0) return;

        Current = Mathf.Max(0, Current - amount);

        if (logDamage) Debug.Log($"[BossHealth] {Current}/{maxHealth} ({Ratio:P0})");

        OnHealthChanged?.Invoke(Current, maxHealth);

        if (Current == 0)
        {
            if (deferDeath)
            {
                IsDepleted = true;
                OnDepleted?.Invoke();
            }
            else
            {
                Die();
            }
        }
    }

    // Conclui a morte adiada (usado depois do ataque final)
    public void ConfirmDeath()
    {
        if (IsDead) return;
        Die();
    }

    private void Die()
    {
        IsDead = true;
        OnDied?.Invoke();
    }
}