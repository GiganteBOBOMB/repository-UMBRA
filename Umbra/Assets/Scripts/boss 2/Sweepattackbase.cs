using System.Collections;
using UnityEngine;

// Base das varreduras. Guarda os tempos e a rotina de varrer uma mão de um X até outro.
public abstract class SweepAttackBase : BossAttack
{
    [Header("Varredura")]
    [Tooltip("Tempo para a mão ir do repouso até o ponto de partida.")]
    [SerializeField] protected float windupTime = 0.5f;
    [Tooltip("Tempo parada no ponto de partida antes de varrer (aviso).")]
    [SerializeField] protected float telegraphTime = 0.35f;
    [SerializeField] protected float sweepTime = 0.7f;
    [SerializeField] protected float returnTime = 0.5f;
    [Tooltip("Ajuste fino da altura da varredura em relação ao chão.")]
    [SerializeField] protected float sweepOffsetY = 0f;

    protected IEnumerator Sweep(BossHand hand, float startX, float endX, BossArena arena)
    {
        float y = arena.GroundY + sweepOffsetY;
        float z = hand.transform.position.z;

        // 1. Vai até o ponto de partida e avisa
        yield return hand.MoveToWorld(new Vector3(startX, y, z), windupTime);
        yield return new WaitForSeconds(telegraphTime);

        // 2. Varre com o dano ligado
        hand.SetHitbox(true);
        yield return hand.MoveToWorld(new Vector3(endX, y, z), sweepTime);
        hand.SetHitbox(false);

        // 3. Volta
        yield return hand.ReturnToRest(returnTime);
    }
}
