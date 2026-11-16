using UnityEngine;

public class PlayerIdelState : PlayerState
{
    public PlayerIdelState(Player player) : base(player) { }

    public override void Enter()
    {
        anim.SetBool("isIdle", true);
        player.rb.linearVelocity = new Vector2(0, player.rb.linearVelocity.y);
    }


    public override void Update()
    {
        base.Update();

        if (JumpPressed)
        {
            JumpPressed = false;
            player.ChangeState(new PlayerJumpState(player));
        }
        else if (Mathf.Abs(player.moveInput.x) > .1f)
        {
            player.runPressed = false;
            player.ChangeState(player.moveState);
        }
        else if (MoveInput.y < -.1f)
        {
            player.ChangeState(player.crouchState);
        }
    }


    public override void Exit()
    {
        anim.SetBool("isIdle", false);
    }

}