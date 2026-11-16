using System;
using UnityEngine;

public class BossRhythmController : MonoBehaviour
{
    [Header("Compasso")]
    [Tooltip("Duração de cada tempo, em segundos. 2 = lento, 1 = normal, 0.5 = rápido")]
    [SerializeField, Min(0.05f)] private float beatDuration = 1f;

    [Tooltip("Quantos tempos tem o compasso. O ataque acontece no último.")]
    [SerializeField, Min(2)] private int beatsPerMeasure = 4;

    [SerializeField] private bool startOnAwake = true;
    [SerializeField] private bool logBeats = false;

    // Eventos: quem quiser reagir se inscreve neles
    public event Action<int> OnBeat;     // dispara a cada tempo, enviando 1, 2, 3, 4...
    public event Action OnAttackBeat;    // dispara só no último tempo

    public int CurrentBeat { get; private set; }
    public bool IsRunning { get; private set; }

    public float BeatDuration
    {
        get => beatDuration;
        set => beatDuration = Mathf.Max(0.05f, value);
    }

    private float timer;

    private void Start()
    {
        if (startOnAwake) Begin();
    }

    private void Update()
    {
        if (!IsRunning) return;

        timer += Time.deltaTime;

        // while (e não if) para não perder tempos se houver um frame muito longo
        while (timer >= beatDuration)
        {
            timer -= beatDuration;
            AdvanceBeat();
        }
    }

    private void AdvanceBeat()
    {
        CurrentBeat++;
        if (CurrentBeat > beatsPerMeasure) CurrentBeat = 1;

        if (logBeats) Debug.Log($"[Ritmo] Tempo {CurrentBeat}");

        OnBeat?.Invoke(CurrentBeat);

        if (CurrentBeat == beatsPerMeasure)
            OnAttackBeat?.Invoke();
    }

    // ---------- API pública ----------

    /// Começa do zero: o tempo 1 acontece imediatamente.
    public void Begin()
    {
        timer = 0f;
        CurrentBeat = 0;
        IsRunning = true;
        AdvanceBeat();
    }

    public void Pause() => IsRunning = false;   // mantém contagem e timer
    public void Resume() => IsRunning = true;    // continua de onde parou

    public void Stop()
    {
        IsRunning = false;
        CurrentBeat = 0;
        timer = 0f;
    }

    /// Mudar a velocidade durante a batalha (ex.: ao perder vida).
    public void SetBeatDuration(float seconds) => BeatDuration = seconds;
}