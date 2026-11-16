using UnityEngine;

public struct AttackContext
{
    public BossHand Left;
    public BossHand Right;
    public BossHand Acting;     // a mão da vez (nos ataques de uma mão só)

    public BossArena Arena;     // chão e bordas da arena
    public Transform Target;    // normalmente o Player (pode ser nulo)

    public BossHand Other => Acting == Left ? Right : Left;
}