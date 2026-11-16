using System.Collections;

// Uma mão cruza a arena de um lado ao outro. A mão da vez começa no próprio lado.
public class AtaqueVarreduraUnica : SweepAttackBase
{
    public override IEnumerator Execute(AttackContext context)
    {
        BossHand hand = context.Acting;
        bool startsOnLeft = hand.Side == HandSide.Left;

        float startX = startsOnLeft ? context.Arena.LeftX : context.Arena.RightX;
        float endX = startsOnLeft ? context.Arena.RightX : context.Arena.LeftX;

        yield return Sweep(hand, startX, endX, context.Arena);
    }
}
