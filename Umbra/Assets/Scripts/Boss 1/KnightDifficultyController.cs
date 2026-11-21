using System;
using System.Collections.Generic;
using UnityEngine;

// Uma fase da luta: a partir de certa vida, muda a pausa entre ataques e libera/bloqueia ataques.
[Serializable]
public class KnightPhase
{
    public string name = "Fase";

    [Tooltip("A fase começa quando a vida cai até esta fração. 1 = vida cheia, 0.5 = metade, 0.25 = 25%.")]
    [Range(0f, 1f)] public float startsAtHealth = 1f;

    [Tooltip("Multiplica a pausa entre ataques. 1 = normal, 0.5 = metade da pausa (mais difícil).")]
    [Min(0.05f)] public float pauseMultiplier = 1f;

    [Tooltip("Ataques que passam a ser sorteados a partir desta fase.")]
    public List<KnightAttack> unlockAttacks = new List<KnightAttack>();

    [Tooltip("Ataques que deixam de ser sorteados a partir desta fase.")]
    public List<KnightAttack> lockAttacks = new List<KnightAttack>();
}

// Aumenta a dificuldade conforme a vida do boss cai. Só conversa com BossHealth e KnightBrain.
public class KnightDifficultyController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private BossHealth health;
    [SerializeField] private KnightBrain brain;

    [Header("Fases")]
    [Tooltip("Ordem: da vida mais alta para a mais baixa. A primeira deve começar em 1 (vida cheia).")]
    [SerializeField]
    private List<KnightPhase> phases = new List<KnightPhase>
    {
        new KnightPhase { name = "Fase 1", startsAtHealth = 1.0f, pauseMultiplier = 1.0f },
        new KnightPhase { name = "Fase 2", startsAtHealth = 0.6f, pauseMultiplier = 0.75f },
        new KnightPhase { name = "Fase 3", startsAtHealth = 0.3f, pauseMultiplier = 0.5f },
    };

    [SerializeField] private bool logPhases = false;

    public int CurrentPhase { get; private set; } = -1;
    public event Action<int, KnightPhase> OnPhaseChanged;

    private void Awake()
    {
        if (health == null) health = GetComponent<BossHealth>();
        if (brain == null) brain = GetComponent<KnightBrain>();
    }

    private void OnEnable()
    {
        if (health != null) health.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        if (health != null) health.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int current, int max)
    {
        float ratio = (float)current / max;

        // Procura a fase mais avançada cuja condição já foi atingida
        int target = CurrentPhase;
        for (int i = 0; i < phases.Count; i++)
        {
            if (ratio <= phases[i].startsAtHealth)
                target = Mathf.Max(target, i);
        }

        // Só avança, nunca volta. Passou de várias fases de uma vez? Aplica todas em ordem.
        while (CurrentPhase < target)
            ApplyPhase(CurrentPhase + 1);
    }

    private void ApplyPhase(int index)
    {
        CurrentPhase = index;
        KnightPhase phase = phases[index];

        if (brain != null) brain.PauseMultiplier = phase.pauseMultiplier;

        foreach (KnightAttack a in phase.unlockAttacks)
            if (a != null) a.SetEnabled(true);

        foreach (KnightAttack a in phase.lockAttacks)
            if (a != null) a.SetEnabled(false);

        if (logPhases)
            Debug.Log($"[Dificuldade] {phase.name}: pausa x{phase.pauseMultiplier}");

        OnPhaseChanged?.Invoke(index, phase);
    }
}