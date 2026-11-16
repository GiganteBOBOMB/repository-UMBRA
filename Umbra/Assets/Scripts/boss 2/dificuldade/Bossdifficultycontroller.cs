using System;
using System.Collections.Generic;
using UnityEngine;

// Uma fase da luta: a partir de certa vida, muda a velocidade e libera/bloqueia ataques.
[Serializable]
public class BossPhase
{
    public string name = "Fase";

    [Tooltip("A fase começa quando a vida cai até esta fração. 1 = vida cheia, 0.5 = metade, 0.25 = 25%.")]
    [Range(0f, 1f)] public float startsAtHealth = 1f;

    [Tooltip("Duração de cada tempo nesta fase, em segundos. Menor = mais rápido.")]
    [Min(0.05f)] public float beatDuration = 1f;

    [Tooltip("Ataques que passam a ser sorteados a partir desta fase.")]
    public List<BossAttack> unlockAttacks = new List<BossAttack>();

    [Tooltip("Ataques que deixam de ser sorteados a partir desta fase.")]
    public List<BossAttack> lockAttacks = new List<BossAttack>();
}

public class BossDifficultyController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private BossHealth health;
    [SerializeField] private BossRhythmController rhythm;

    [Header("Fases")]
    [Tooltip("Ordem: da vida mais alta para a mais baixa. A primeira deve começar em 1 (vida cheia).")]
    [SerializeField]
    private List<BossPhase> phases = new List<BossPhase>
    {
        new BossPhase { name = "Fase 1", startsAtHealth = 1.0f, beatDuration = 1.0f },
        new BossPhase { name = "Fase 2", startsAtHealth = 0.6f, beatDuration = 0.75f },
        new BossPhase { name = "Fase 3", startsAtHealth = 0.3f, beatDuration = 0.5f },
    };

    [SerializeField] private bool logPhases = false;

    // Fase atual (-1 = nenhuma aplicada ainda) e evento para música, efeitos, UI, etc.
    public int CurrentPhase { get; private set; } = -1;
    public event Action<int, BossPhase> OnPhaseChanged;

    private void Awake()
    {
        if (health == null) health = GetComponent<BossHealth>();
        if (rhythm == null) rhythm = GetComponent<BossRhythmController>();
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
        BossPhase phase = phases[index];

        if (rhythm != null) rhythm.SetBeatDuration(phase.beatDuration);

        foreach (BossAttack a in phase.unlockAttacks)
            if (a != null) a.SetEnabled(true);

        foreach (BossAttack a in phase.lockAttacks)
            if (a != null) a.SetEnabled(false);

        if (logPhases)
            Debug.Log($"[Dificuldade] {phase.name}: tempo = {phase.beatDuration}s");

        OnPhaseChanged?.Invoke(index, phase);
    }
}
