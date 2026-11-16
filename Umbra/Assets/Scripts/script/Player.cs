using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public PlayerState currentState;

    //States
    public PlayerIdelState idleState;
    public PlayerJumpState jumpState;
    public PlayerMoveState moveState;
    public PlayerCrouchState crouchState;
    public PlayerRollState rollState;

    [Header("Componentes")]
    public Rigidbody2D rb;
    public PlayerInput playerInput;
    public Animator anim;
    public CapsuleCollider2D capsuleCollider;

    //lanterna e lampião
    public GameObject lampiao, lanterna;
    public SpriteRenderer render;


    [Header("Movimento variables")]
    public float jumpForce;
    public float WalkSpeed;
    public float runSpeed = 7f;
    public float jumpCutMultiplier = .5f;
    public float normalGravity;
    public float fallGravity;
    public float jumpGravity;


    public int dacinDirection = 1;
    public float facingDirection = 1;

    //inputs
    public Vector2 moveInput;
    public bool runPressed;
    public bool jumpPressed;
    public bool jumpReleased;


    [Header("Ground Check")]


    public Transform groundCheck;
    public float groundCheckRadius;
    public LayerMask groundLayer;
    public bool isGrounded;

    [Header("Crouch Check")]
    public Transform HeadCheck;
    public float headCheckRadius = .2f;
    public LayerMask crouchLayer;
    public bool isCrouching;

    [Header("Roll Settings")]
    public float rollDuration = 6;
    public float rollSpeed = 10f;
    public float rollStopDuration = .15f;

    public float rollHeight;
    public Vector2 rollOffset;
    public float normalHeight;
    public Vector2 normalOffset;

    private bool isRolling;
 
 


    private void Awake()
    {
        idleState = new PlayerIdelState(this);
        jumpState = new PlayerJumpState(this);
        moveState = new PlayerMoveState(this);
        crouchState = new PlayerCrouchState(this);
        rollState = new PlayerRollState(this);
    }

    private void Start()
    {
        rb.gravityScale = normalGravity;

        ChangeState(idleState);
    }

    void HandleAnimation()
    {
        anim.SetBool("isGrounded", isGrounded);

        anim.SetFloat("yVelocity", rb.linearVelocity.y);
    }
    private void Update()
    {
        currentState.Update();
        
        if (!isRolling)
            Flip();
        HandleAnimation();
    }



    void FixedUpdate()
    {
        currentState.FixedUpdate();

        CheckGrounded();

      

        
    }



    public void ChangeState(PlayerState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;
        currentState.Enter();
    }

   



   


    public void SetColliderNormal()
    {
        capsuleCollider.size = new Vector2(capsuleCollider.size.x, normalHeight);
        capsuleCollider.offset = normalOffset;
    }
    public void SetColliderRoll()
    {
        capsuleCollider.size = new Vector2(capsuleCollider.size.x, rollHeight);
        capsuleCollider.offset = rollOffset;
    }

    public void ApplyVariableGravity()
    {
        if (rb.linearVelocity.y < -0.1f) // caindo
        {
            rb.gravityScale = fallGravity;
        }
        else if (rb.linearVelocity.y > 0.1f) // subindo e soltando o botão de pulo
        {
            rb.gravityScale = jumpGravity;
        }
        else // subindo e segurando o botão de pulo
        {
            rb.gravityScale = normalGravity;
        }
    }

    void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public bool CheckForCollision()
    {
        return Physics2D.OverlapCircle(HeadCheck.position, headCheckRadius, groundLayer);
    }

    void Flip()
    {

        if (moveInput.x < 0)
        {
            render.flipX = true;
            lampiao.transform.localPosition = new Vector2(-0.7f, -0.3f);
            Debug.Log("GIRA GIRA");
            lampiao.transform.rotation = new Quaternion(0, 0, 180, 180);
        }
        if (moveInput.x > 0)
        {
            render.flipX = false;
            lampiao.transform.localPosition = new Vector2(0.7f, -0.3f);
            lampiao.transform.localRotation = new Quaternion(0, 0, -180, 180);
        }

    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();

    }

    public void OnRun(InputValue value)
    {
        runPressed = value.isPressed;
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed)
        {
            jumpPressed = true;
            jumpReleased = false;
        }
        else //O botão é liberado
        {
            jumpReleased = true;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(HeadCheck.position, headCheckRadius);
    }

    internal bool CheckForCeiling()
    {
        throw new NotImplementedException();
    }
}