using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// O ciclo da luta: espera, escolhe um ataque permitido, executa, repete.
// Não sabe COMO cada ataque funciona. Só pergunta CanUse() e chama Execute().
public class KnightBrain : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private BossHealth health;
    [SerializeField] private KnightMotor motor;
    [SerializeField] private FloatingWeapon weapon;
    [SerializeField] private BossArena arena;
    [Tooltip("Normalmente o Player.")]
    [SerializeField] private Transform target;

    [Header("Ataques")]
    [Tooltip("Se ficar vazia, pega automaticamente todos os KnightAttack que são filhos deste objeto.")]
    [SerializeField] private List<KnightAttack> attacks = new List<KnightAttack>();
    [Tooltip("Evita sortear o mesmo ataque duas vezes seguidas (se houver mais de um disponível).")]
    [SerializeField] private bool avoidRepeatingAttack = true;

    [Header("Ritmo da luta")]
    [Tooltip("Espera antes do primeiro ataque, ao começar a luta.")]
    [SerializeField, Min(0f)] private float startDelay = 1.5f;
    [Tooltip("A pausa entre ataques é sorteada entre estes dois valores, em segundos.")]
    [SerializeField, Min(0f)] private float minPause = 0.6f;
    [SerializeField, Min(0f)] private float maxPause = 1.4f;
    [SerializeField] private bool faceTargetBeforeAttack = true;
    [SerializeField] private bool logAttacks = false;

    public bool IsAttacking { get; private set; }
    public bool IsPaused { get; private set; }

    /// Multiplica a pausa entre ataques. 1 = normal, 0.5 = metade (mais difícil). Use nas fases.
    public float PauseMultiplier { get; set; } = 1f;

    private KnightAttack currentAttack;
    private KnightAttack lastAttack;
    private Coroutine loop;
    private readonly List<KnightAttack> candidates = new List<KnightAttack>();

    private void Awake()
    {
        if (health == null) health = GetComponent<BossHealth>();
        if (motor == null) motor = GetComponent<KnightMotor>();

        if (attacks.Count == 0)
            attacks.AddRange(GetComponentsInChildren<KnightAttack>());
    }

    private void OnEnable()
    {
        if (health != null) health.OnDied += HandleDied;
        Begin();
    }

    private void OnDisable()
    {
        if (health != null) health.OnDied -= HandleDied;
        Stop();
    }

    private void HandleDied() => Stop();

    // ---------- API pública ----------

    public void Begin()
    {
        if (loop != null) return;
        loop = StartCoroutine(Loop());
    }

    /// Pausa só a DECISÃO: o ataque em andamento termina, e o próximo só vem ao retomar.
    public void SetPaused(bool value) => IsPaused = value;

    /// Para o ciclo e cancela o ataque em andamento (morte, troca de fase, cutscene).
    public void Stop()
    {
        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }

        if (currentAttack != null)
        {
            currentAttack.Cancel();
            currentAttack = null;
        }

        IsAttacking = false;

        // Desfaz o que um ataque cortado no meio possa ter deixado
        if (weapon != null)
        {
            weapon.SetHitbox(false);
            weapon.Follow();
        }
        if (motor != null) motor.SetVisible(true);
    }

    // ---------- O ciclo ----------

    private IEnumerator Loop()
    {
        yield return WaitSeconds(startDelay);

        while (true)
        {
            if (health != null && health.IsDead) yield break;

            KnightAttack attack = PickAttack();

            if (attack == null)
            {
                // Vida zerada e nenhum ataque final existe: o boss morre, para a luta não travar
                if (health != null && health.IsDepleted)
                {
                    health.ConfirmDeath();
                    yield break;
                }

                // Nenhum ataque liberado agora (todos desabilitados, por exemplo): tenta de novo
                yield return WaitSeconds(0.25f);
                continue;
            }

            yield return RunAttack(attack);

            // Ataque final terminou: a luta acabou
            if (attack.EndsBattleWhenFinished && health != null)
            {
                health.ConfirmDeath();
                yield break;
            }

            float pause = Random.Range(minPause, maxPause) * PauseMultiplier + attack.ExtraRecovery;
            yield return WaitSeconds(pause, true);
        }
    }

    private IEnumerator RunAttack(KnightAttack attack)
    {
        IsAttacking = true;
        currentAttack = attack;

        if (faceTargetBeforeAttack && motor != null)
            motor.FaceTowards(target);

        if (logAttacks) Debug.Log($"[Knight] {attack.AttackName}");

        yield return attack.Execute(BuildContext());

        IsAttacking = false;
        currentAttack = null;

        // A arma volta a seguir o boss, com o atraso suave
        if (weapon != null) weapon.Follow();
    }

    private KnightAttack PickAttack()
    {
        KnightContext context = BuildContext();

        candidates.Clear();
        foreach (KnightAttack a in attacks)
            if (a != null && a.IsEnabled && a.CanUse(context))
                candidates.Add(a);

        if (candidates.Count == 0) return null;

        if (avoidRepeatingAttack && candidates.Count > 1)
            candidates.Remove(lastAttack);

        KnightAttack chosen = candidates[Random.Range(0, candidates.Count)];
        lastAttack = chosen;
        return chosen;
    }

    private KnightContext BuildContext()
    {
        return new KnightContext
        {
            Motor = motor,
            Weapon = weapon,
            Health = health,
            Arena = arena,
            Target = target
        };
    }

    // Espera respeitando a pausa. Se skipWhenDepleted, a vida zerar durante a espera
    // encerra a pausa na hora, para o ataque final não demorar a entrar.
    private IEnumerator WaitSeconds(float seconds, bool skipWhenDepleted = false)
    {
        bool wasDepleted = health != null && health.IsDepleted;
        float t = 0f;

        while (t < seconds)
        {
            if (skipWhenDepleted && health != null && health.IsDepleted && !wasDepleted)
                yield break;

            if (!IsPaused) t += Time.deltaTime;
            yield return null;
        }

        // Pausado entre ataques: fica parado até retomar
        while (IsPaused) yield return null;
    }
}