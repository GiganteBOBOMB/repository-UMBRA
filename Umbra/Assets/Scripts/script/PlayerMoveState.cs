using UnityEngine;

public class PlayerMoveState : PlayerState
{
    public PlayerMoveState(Player player) : base(player) { }


    public override void Enter()
    {
        base.Enter();
        
    }

    public override void Update()
    {
        base.Update();

        if (JumpPressed)
        {
            player.ChangeState(player.jumpState);
        }
        else if (Mathf.Abs(player.moveInput.x) < .1f)
        {
            player.ChangeState(player.idleState);
        }
        else if (player.isGrounded && RunPressed && MoveInput.y < -.1f)
        {
            player.ChangeState(player.rollState);
        }
        else 
        {
        anim.SetBool("isWalk",!RunPressed);
        anim.SetBool("isRun", RunPressed);
        }

    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        float speed = RunPressed ? player.runSpeed : player.WalkSpeed;
        player.rb.linearVelocity = new Vector2(speed * player.facingDirection, player.rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
        
        anim.SetBool("isWalk", false);
        anim.SetBool("isRun", false);
    }
}
