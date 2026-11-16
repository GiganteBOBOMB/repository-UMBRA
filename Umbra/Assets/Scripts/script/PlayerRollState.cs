using UnityEngine;

public class PlayerRollState : PlayerState
{
    private float rollTimer;
    private float rollStopTimer;

    public PlayerRollState(Player player) : base(player) { }

    public override void Enter()
    {
        base.Enter();

        rollTimer = player.rollDuration;
        rollStopTimer = 0;

        player.SetColliderRoll();
        anim.SetBool("isRoll", true);
        
    }

    public override void Update()
    {
        base.Update();


        if (rollTimer > 0 )
                rollTimer -= Time.deltaTime;
        else if (rollStopTimer <= 0)
        {
            rollStopTimer = player.rollStopDuration;
        }
        else
        {
            rollStopTimer -= Time.deltaTime;
            if (rollStopTimer <= 0)
            {
                if (player.CheckForCeiling() || MoveInput.y <= -.1f)
                {
                    player.ChangeState(player.crouchState);

                }
                else
                {
                    player.ChangeState(player.idleState);
                }
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
       
        if (rollTimer > 0)
            player.rb.linearVelocity = new Vector2(player.rollSpeed * player.facingDirection, player.rb.linearVelocity.y);
        else
            player.rb.linearVelocity = new Vector2(0, player.rb.linearVelocity.y);
            player.ChangeState(player.idleState);
    }

    public override void Exit()
    {
        base.Enter();

        player.SetColliderNormal();
        anim.SetBool("isRoll", false);

    }








}
