using UnityEngine.Experimental.Rendering;
using UnityEngine;

public class PlayerJumpState : PlayerState
{
    private float jumpCutMultiplier;

    public PlayerJumpState(Player player) : base(player) { }


    public override void Enter()
    {
        base.Enter();
        anim.SetBool("isJump", true);

        player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x, player.jumpForce);

        JumpPressed = false;
        JumpReleased = false;
    }

    public override void Update()
    {
        base.Update();

        if (player.isGrounded && player.rb.linearVelocity.y <= 0)
            player.ChangeState(player.idleState);
        
    }
    public override void FixedUpdate()
    {
        base.FixedUpdate();

        player.ApplyVariableGravity();

        if (JumpReleased && player.rb.linearVelocity.y > 0)
        {
            player.rb.linearVelocity = new Vector2(player.rb.linearVelocity.x, player.rb.linearVelocity.y * jumpCutMultiplier);
            JumpReleased = false;
        }

        float speed = RunPressed ? player.runSpeed : player.WalkSpeed;
        float targetSpeed = speed * player.moveInput.x;
        player.rb.linearVelocity = new Vector2(targetSpeed, player.rb.linearVelocity.y);
    }

    public override void Exit()
    {
        base.Exit();
        anim.SetBool("isJumping", false);
    }
}