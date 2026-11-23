using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimentação")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.8f;

    [Header("Verificação do chão")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Áudio")]
    [SerializeField] private PlayerAudio playerAudio;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float horizontalInput;
    private bool isGrounded;
    private bool isDashing;
    private bool canDash = true;

    private bool wasGrounded;
    private bool groundInitialized;

    public bool IsGrounded => isGrounded;
    public bool IsDashing => isDashing;

    // Parâmetros do Animator.
    private static readonly int AndandoHash =
        Animator.StringToHash("Andando");

    private static readonly int NoChaoHash =
        Animator.StringToHash("NoChao");

    private static readonly int RolandoHash =
        Animator.StringToHash("Rolando");

    private static readonly int VelocidadeYHash =
        Animator.StringToHash("VelocidadeY");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (playerAudio == null)
            playerAudio = GetComponentInChildren<PlayerAudio>();

        if (groundCheck == null)
            Debug.LogWarning(
                "groundCheck NÃO está atribuído no Inspector!"
            );
    }

    private void Update()
    {
        CheckGround();

        horizontalInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed)
                horizontalInput = -1f;

            if (Keyboard.current.dKey.isPressed)
                horizontalInput = 1f;

            // Pulo.
            if (Keyboard.current.spaceKey.wasPressedThisFrame &&
                isGrounded && !isDashing)
            {
                rb.linearVelocity = new Vector2(
                    rb.linearVelocity.x,
                    jumpForce
                );

                if (playerAudio != null)
                    playerAudio.PlayJump();
            }

            // Dash.
            if (Keyboard.current.leftShiftKey.wasPressedThisFrame &&
                canDash && !isDashing)
            {
                StartCoroutine(PerformDash());
            }
        }

        // Direção visual.
        if (horizontalInput < 0)
            spriteRenderer.flipX = true;
        else if (horizontalInput > 0)
            spriteRenderer.flipX = false;

        UpdateFootsteps();
        UpdateAnimations();
    }

    private void CheckGround()
    {
        if (groundCheck == null)
            return;

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // Evita tocar aterrissagem ao iniciar a fase.
        if (!groundInitialized)
        {
            wasGrounded = isGrounded;
            groundInitialized = true;
            return;
        }

        // Som de aterrissagem.
        if (!wasGrounded && isGrounded)
        {
            if (playerAudio != null)
                playerAudio.PlayLanding();
        }

        wasGrounded = isGrounded;
    }

    private void UpdateFootsteps()
    {
        if (playerAudio == null)
            return;

        bool isWalking =
            isGrounded &&
            !isDashing &&
            Mathf.Abs(horizontalInput) > 0.01f &&
            rb.linearVelocity.y <= 0.1f;

        // Inicia ou interrompe o áudio contínuo.
        if (isWalking)
        {
            playerAudio.StartFootsteps();
        }
        else
        {
            playerAudio.StopFootsteps();
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.linearVelocity = new Vector2(
                horizontalInput * moveSpeed,
                rb.linearVelocity.y
            );
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null)
            return;

        animator.SetBool(
            AndandoHash,
            horizontalInput != 0f && !isDashing
        );

        animator.SetBool(
            NoChaoHash,
            isGrounded
        );

        animator.SetBool(
            RolandoHash,
            isDashing
        );

        animator.SetFloat(
            VelocidadeYHash,
            rb.linearVelocity.y
        );
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;
        canDash = false;

        if (playerAudio != null)
            playerAudio.PlayDash();

        float dashDirection = horizontalInput;

        if (dashDirection == 0f)
            dashDirection =
                spriteRenderer.flipX ? -1f : 1f;

        rb.linearVelocity = new Vector2(
            dashDirection * dashSpeed,
            0f
        );

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    private void OnDisable()
    {
        if (playerAudio != null)
            playerAudio.StopFootsteps();
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;

            Gizmos.DrawWireSphere(
                groundCheck.position,
                groundCheckRadius
            );
        }
    }
}