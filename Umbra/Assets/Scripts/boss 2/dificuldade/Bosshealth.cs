using System;
using UnityEngine;

// Só cuida da vida do Boss. Não sabe nada de ritmo, ataques ou fases:
// quem quiser reagir se inscreve nos eventos.
public class BossHealth : MonoBehaviour
{
    [SerializeField, Min(1)] private int maxHealth = 100;
    [SerializeField] private bool logDamage = false;

    // (vida atual, vida máxima)
    public event Action<int, int> OnHealthChanged;
    public event Action OnDied;

    public int Current { get; private set; }
    public int Max => maxHealth;
    public float Ratio => (float)Current / maxHealth;   // 1 = cheia, 0 = morto
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
        if (IsDead || Invulnerable || amount <= 0) return;

        Current = Mathf.Max(0, Current - amount);

        if (logDamage) Debug.Log($"[BossHealth] {Current}/{maxHealth} ({Ratio:P0})");

        OnHealthChanged?.Invoke(Current, maxHealth);

        if (Current == 0)
        {
            IsDead = true;
            OnDied?.Invoke();
        }
    }
}
