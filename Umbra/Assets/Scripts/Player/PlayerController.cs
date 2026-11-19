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

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private float horizontalInput;
    private bool isGrounded;
    private bool isDashing;
    private bool canDash = true;

    public bool IsGrounded => isGrounded;
    public bool IsDashing => isDashing;

    // Números dos parâmetros do Animator (mais rápido que usar texto a cada frame).
    // Os nomes precisam ser IDÊNTICOS aos parâmetros Bool criados no Animator.
    private static readonly int AndandoHash = Animator.StringToHash("Andando");
    private static readonly int NoChaoHash = Animator.StringToHash("NoChao");
    private static readonly int RolandoHash = Animator.StringToHash("Rolando");
    private static readonly int VelocidadeYHash = Animator.StringToHash("VelocidadeY");

    private void Awake()
    {
        // Obtém os componentes do jogador.
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Lê as teclas de movimentação do Input System.
        horizontalInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed)
                horizontalInput = -1f;

            if (Keyboard.current.dKey.isPressed)
                horizontalInput = 1f;

            // Pula somente quando o jogador estiver no chão.
            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isDashing)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            }

            // Inicia o dash quando a tecla for pressionada.
            if (Keyboard.current.leftShiftKey.wasPressedThisFrame && canDash && !isDashing)
            {
                StartCoroutine(PerformDash());
            }
        }

        // Atualiza a direção visual do personagem.
        if (horizontalInput < 0)
            spriteRenderer.flipX = true;
        else if (horizontalInput > 0)
            spriteRenderer.flipX = false;

        // Verifica se o jogador está tocando o chão.
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }

        // Precisa vir depois da verificação do chão, para usar o valor atualizado.
        UpdateAnimations();
        if (groundCheck == null)
            Debug.LogWarning("groundCheck NÃO está atribuído no Inspector!");
        else
            Debug.Log("No chão: " + isGrounded);
    }

    private void FixedUpdate()
    {
        // Não altera a velocidade horizontal durante o dash.
        if (!isDashing)
        {
            rb.linearVelocity = new Vector2(
                horizontalInput * moveSpeed,
                rb.linearVelocity.y
            );
        }
    }

    // Cada linha diz ao Animator, com verdadeiro ou falso, qual animação deve tocar.
    // Idle = todos falsos. O Ataque é controlado pelo Trigger no PlayerAttack.
    private void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetBool(AndandoHash, horizontalInput != 0f && !isDashing); // walk
        animator.SetBool(NoChaoHash, isGrounded);                           // jump/falling quando false
        animator.SetBool(RolandoHash, isDashing);                           // Roll
        animator.SetFloat(VelocidadeYHash, rb.linearVelocity.y);            // jump/falling
    }

    private IEnumerator PerformDash()
    {
        // Impede outro dash enquanto este estiver acontecendo.
        isDashing = true;
        canDash = false;

        // Usa a direção do movimento ou a direção para a qual o sprite olha.
        float dashDirection = horizontalInput;

        if (dashDirection == 0f)
            dashDirection = spriteRenderer.flipX ? -1f : 1f;

        // Aplica uma velocidade horizontal rápida durante o dash.
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        // Espera a duração do dash.
        yield return new WaitForSeconds(dashDuration);

        // Devolve o controle normal da movimentação.
        isDashing = false;

        // Aguarda antes de permitir outro dash.
        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    private void OnDrawGizmosSelected()
    {
        // Desenha a área de verificação do chão no editor.
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}