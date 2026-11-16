using System.Collections;
using UnityEngine;

// As duas mãos varrem ao mesmo tempo, cada uma saindo da sua borda em direção ao centro.
public class AtaqueVarreduraDupla : SweepAttackBase
{
    [Header("Varredura dupla")]
    [Tooltip("Distância que cada mão para antes do centro. É o espaço seguro no meio. 0 = as mãos se encontram.")]
    [SerializeField, Min(0f)] private float centerGap = 1f;

    // O controlador não alterna a mão depois deste ataque
    public override bool UsesBothHands => true;

    public override IEnumerator Execute(AttackContext context)
    {
        BossArena arena = context.Arena;

        float halfWidth = (arena.RightX - arena.LeftX) * 0.5f;
        float gap = Mathf.Min(centerGap, halfWidth); // evita as mãos se cruzarem

        // Duas corrotinas rodando juntas, uma por mão
        Coroutine left = StartCoroutine(Sweep(context.Left, arena.LeftX, arena.CenterX - gap, arena));
        Coroutine right = StartCoroutine(Sweep(context.Right, arena.RightX, arena.CenterX + gap, arena));

        // Espera as duas terminarem
        yield return left;
        yield return right;
    }
}