using UnityEngine;

public class PlayerCrouchState : PlayerState
{
    public PlayerCrouchState(Player player) : base(player) { }
    public override void Enter()
    {
        base.Enter();
        anim.SetBool("isCrouch", true);
        player.SetColliderRoll();
    }

 public override void Update()
    {
        base.Update();
        if (JumpPressed)
        {
            player.ChangeState(player.jumpState);
        }
        else if (MoveInput.y > -.1f && !player.CheckForCollision())//sem limite máximo
        {
            player.ChangeState(player.idleState);

        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (Mathf.Abs(MoveInput.x) > .1f)
        {
            player.rb.linearVelocity = new Vector2(player.facingDirection * player.WalkSpeed, player.rb.linearVelocity.y);
        }
        else
        {
            player.rb.linearVelocity = new Vector2(0, player.rb.linearVelocity.y);
        }

    }
   

    public override void Exit()
    {
        base.Exit();
        anim.SetBool("isCrouch", false);
        player.SetColliderNormal();
    }
}
