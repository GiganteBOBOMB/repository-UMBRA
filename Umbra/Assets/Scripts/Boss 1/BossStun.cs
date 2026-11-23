
using System.Collections;
using UnityEngine;

public class BossStun : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private BossHealth health;

    [Header("Boss 1")]
    [SerializeField] private KnightBrain brain;

    [Header("Boss 2")]
    [SerializeField] private BossRhythmController rhythm;
    [SerializeField] private BossAttackController attackController;

    [Header("Visual")]
    [SerializeField]
    private Color stunColor =
        new Color(0.4f, 0.9f, 1f, 1f);

    public bool IsStunned { get; private set; }

    private SpriteRenderer[] sprites;
    private Color[] originalColors;

    private Coroutine stunCoroutine;

    private bool rhythmWasRunning;
    private bool brainWasEnabled;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<BossHealth>();

        if (brain == null)
            brain = GetComponent<KnightBrain>();

        if (rhythm == null)
            rhythm = GetComponent<BossRhythmController>();

        if (attackController == null)
            attackController =
                GetComponent<BossAttackController>();

        sprites = GetComponentsInChildren<SpriteRenderer>();

        originalColors = new Color[sprites.Length];

        for (int i = 0; i < sprites.Length; i++)
            originalColors[i] = sprites[i].color;
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDepleted += HandleHealthDepleted;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDepleted -= HandleHealthDepleted;

        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        RestoreColors();
        IsStunned = false;
    }

    public void Stun(float duration)
    {
        if (health != null &&
            (health.IsDead || health.IsDepleted))
            return;

        if (!IsStunned)
        {
            IsStunned = true;

            // Boss 1: interrompe o comportamento.
            if (brain != null)
            {
                brainWasEnabled = brain.enabled;
                brain.enabled = false;
            }

            // Boss 2: interrompe o ritmo.
            if (rhythm != null)
            {
                rhythmWasRunning = true;
                rhythm.Pause();
            }

            if (attackController != null)
                attackController.CancelCurrentAttack();

            ApplyStunColor();
        }

        // Um novo stun renova sua duração.
        if (stunCoroutine != null)
            StopCoroutine(stunCoroutine);

        stunCoroutine =
            StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        yield return new WaitForSeconds(
            Mathf.Max(0f, duration)
        );

        FinishStun();
    }

    private void FinishStun()
    {
        stunCoroutine = null;
        IsStunned = false;

        RestoreColors();

        if (health != null &&
            (health.IsDead || health.IsDepleted))
            return;

        // Boss 1 volta ao comportamento normal.
        if (brain != null && brainWasEnabled)
            brain.enabled = true;

        // Boss 2 retoma os ataques.
        if (rhythm != null && rhythmWasRunning)
        {
            rhythm.Resume();
            rhythmWasRunning = false;
        }
    }

    private void HandleHealthDepleted()
    {
        // A vida chegou a zero.
        // Encerra o stun para liberar a morte.
        if (stunCoroutine != null)
        {
            StopCoroutine(stunCoroutine);
            stunCoroutine = null;
        }

        IsStunned = false;
        RestoreColors();

        // Boss 1 precisa poder executar
        // sua sequência de morte.
        if (brain != null && brainWasEnabled)
            brain.enabled = true;

        // Boss 2 não deve voltar a atacar
        // depois de perder toda a vida.
        if (rhythm != null)
            rhythm.Stop();

        if (attackController != null)
            attackController.CancelCurrentAttack();

        rhythmWasRunning = false;
    }

    private void ApplyStunColor()
    {
        if (sprites == null)
            return;

        foreach (SpriteRenderer sprite in sprites)
        {
            if (sprite != null)
                sprite.color = stunColor;
        }
    }

    private void RestoreColors()
    {
        if (sprites == null || originalColors == null)
            return;

        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
                sprites[i].color = originalColors[i];
        }
    }
}
