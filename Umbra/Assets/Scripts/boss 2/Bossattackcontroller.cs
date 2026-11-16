using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAttackController : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private BossRhythmController rhythm;
    [SerializeField] private BossHand leftHand;
    [SerializeField] private BossHand rightHand;

    [Header("Cenário e alvo")]
    [Tooltip("Marca o chão e as bordas da arena. Os ataques dependem dela.")]
    [SerializeField] private BossArena arena;
    [Tooltip("Normalmente o Player. Usado pelos ataques que miram nele (pode ficar vazio).")]
    [SerializeField] private Transform target;

    [Header("Ataques")]
    [Tooltip("Se ficar vazia, pega automaticamente todos os BossAttack que são filhos deste objeto.")]
    [SerializeField] private List<BossAttack> attacks = new List<BossAttack>();
    [Tooltip("Evita sortear o mesmo ataque duas vezes seguidas (se houver mais de um habilitado).")]
    [SerializeField] private bool avoidRepeatingAttack = true;

    [Header("Comportamento")]
    [Tooltip("Ligado: o compasso para enquanto o ataque roda e volta quando termina.\n" +
             "Desligado: o compasso segue contando e sinais que chegam durante um ataque são ignorados.")]
    [SerializeField] private bool pauseRhythmWhileAttacking = true;
    [SerializeField] private HandSide firstHand = HandSide.Left;
    [SerializeField] private bool logAttacks = false;

    public bool IsAttacking { get; private set; }

    private HandSide nextHand;
    private BossAttack lastAttack;
    private BossAttack currentAttack;
    private Coroutine currentRoutine;
    private readonly List<BossAttack> candidates = new List<BossAttack>();

    private void Awake()
    {
        if (rhythm == null) rhythm = GetComponent<BossRhythmController>();

        if (attacks.Count == 0)
            attacks.AddRange(GetComponentsInChildren<BossAttack>());

        nextHand = firstHand;

        if (arena == null)
            Debug.LogError("[BossAttackController] Arena não atribuída. Nenhum ataque vai rodar.", this);
    }

    private void OnEnable()
    {
        if (rhythm != null) rhythm.OnAttackBeat += HandleAttackBeat;
    }

    private void OnDisable()
    {
        if (rhythm != null) rhythm.OnAttackBeat -= HandleAttackBeat;
        CancelCurrentAttack();
    }

    // Chamado pelo evento do ritmo no 4º tempo
    private void HandleAttackBeat()
    {
        if (IsAttacking) return; // trava: nunca dois ataques ao mesmo tempo
        if (arena == null) return;

        BossAttack attack = PickRandomAttack();
        if (attack == null) return; // nenhum ataque habilitado

        AttackContext context = BuildContext();

        // Alterna a mão só se o ataque usa uma mão
        if (!attack.UsesBothHands)
            nextHand = (nextHand == HandSide.Left) ? HandSide.Right : HandSide.Left;

        if (logAttacks)
            Debug.Log($"[Boss] {attack.AttackName} (mão: {context.Acting.Side})");

        currentAttack = attack;
        currentRoutine = StartCoroutine(RunAttack(attack, context));
    }

    private BossAttack PickRandomAttack()
    {
        candidates.Clear();
        foreach (BossAttack a in attacks)
            if (a != null && a.IsEnabled)
                candidates.Add(a);

        if (candidates.Count == 0) return null;

        if (avoidRepeatingAttack && candidates.Count > 1)
            candidates.Remove(lastAttack);

        BossAttack chosen = candidates[Random.Range(0, candidates.Count)];
        lastAttack = chosen;
        return chosen;
    }

    private AttackContext BuildContext()
    {
        return new AttackContext
        {
            Left = leftHand,
            Right = rightHand,
            Acting = (nextHand == HandSide.Left) ? leftHand : rightHand,
            Arena = arena,
            Target = target
        };
    }

    private IEnumerator RunAttack(BossAttack attack, AttackContext context)
    {
        IsAttacking = true;
        if (pauseRhythmWhileAttacking) rhythm.Pause();

        yield return attack.Execute(context); // espera o ataque terminar por completo

        IsAttacking = false;
        currentAttack = null;
        currentRoutine = null;
        if (pauseRhythmWhileAttacking) rhythm.Resume();
    }

    /// Use quando o boss morrer, mudar de fase, etc.
    /// Atenção: não retoma o ritmo. Se quiser que o boss volte a contar, chame rhythm.Resume().
    public void CancelCurrentAttack()
    {
        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
            currentRoutine = null;
        }

        if (currentAttack != null)
        {
            currentAttack.Cancel(); // para corrotinas internas do ataque
            currentAttack = null;
        }

        IsAttacking = false;

        if (leftHand != null) leftHand.SnapToRest();
        if (rightHand != null) rightHand.SnapToRest();
    }
}