using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// Requer o PlayerController no mesmo objeto (usa IsGrounded e IsDashing).
public class PlayerAttack : MonoBehaviour
{
    [Header("Configuração")]
    public Key attackKey = Key.S;
    public float attackDuration = 0.25f;
    public float attackCooldown = 0.4f;

    [Header("Regras do ataque")]
    [Tooltip("Desmarque para permitir atacar no ar.")]
    [SerializeField] private bool onlyOnGround = true;

    [Header("Ponto de ataque")]
    public Transform attackPoint;

    [Header("Objetos do ataque")]
    public GameObject attackHitbox;
    [Tooltip("Efeito visual opcional. Pode deixar vazio se a animação já mostra o golpe.")]
    public GameObject attackVisual;

    [Header("Posição do ataque")]
    public float attackOffsetX = 0.8f;
    public float attackOffsetY = 0f;

    [Header("Animação")]
    [SerializeField] private Animator animator;
    [SerializeField] private string attackTrigger = "Attack";
    [Tooltip("Ligado: a animação liga/desliga a hitbox e avisa o fim por Animation Events.\n" +
             "Desligado: a hitbox usa o tempo de Attack Duration.")]
    [SerializeField] private bool useAnimationEvents = true;
    [Tooltip("Segurança: se o evento de fim nunca chegar, o ataque é liberado depois deste tempo.")]
    [SerializeField] private float maxAttackTime = 1f;

    // Outros scripts podem consultar (ex.: parar o movimento durante o golpe)
    public bool IsAttacking { get; private set; }

    private bool canAttack = true;
    private bool facingRight = true;
    private bool attackFinished;

    private SpriteRenderer playerSprite;
    private PlayerController controller;
    private Vector3 visualOriginalScale;
    private int attackHash;
    private Coroutine attackRoutine;

    void Awake()
    {
        playerSprite = GetComponent<SpriteRenderer>();
        controller = GetComponent<PlayerController>();
        if (animator == null) animator = GetComponent<Animator>();

        // Converte o nome em número uma vez só (mais rápido que usar texto a cada ataque)
        attackHash = Animator.StringToHash(attackTrigger);

        if (attackVisual != null)
            visualOriginalScale = attackVisual.transform.localScale;

        SetHitbox(false);
        SetVisual(false);
    }

    void Update()
    {
        if (playerSprite != null)
            facingRight = !playerSprite.flipX;

        UpdateAttackPosition();

        // Rolar cancela o golpe em andamento
        if (IsAttacking && controller != null && controller.IsDashing)
        {
            CancelAttack();
            return;
        }

        if (Keyboard.current != null &&
            Keyboard.current[attackKey].wasPressedThisFrame &&
            CanStartAttack())
        {
            attackRoutine = StartCoroutine(AttackRoutine());
        }
    }

    // Todas as regras que impedem o ataque ficam aqui, num lugar só.
    // Bloquear no código (e não só no Animator) evita o trigger ficar pendente.
    private bool CanStartAttack()
    {
        if (!canAttack) return false;

        if (controller != null)
        {
            if (controller.IsDashing) return false;
            if (onlyOnGround && !controller.IsGrounded) return false;
        }

        return true;
    }

    void UpdateAttackPosition()
    {
        if (attackPoint == null) return;

        attackPoint.localPosition = new Vector3(
            Mathf.Abs(attackOffsetX) * (facingRight ? 1f : -1f),
            attackOffsetY,
            0f
        );

        if (attackVisual != null)
        {
            Vector3 scale = visualOriginalScale;
            scale.x = Mathf.Abs(visualOriginalScale.x) * (facingRight ? 1f : -1f);
            attackVisual.transform.localScale = scale;
        }
    }

    IEnumerator AttackRoutine()
    {
        canAttack = false;
        IsAttacking = true;
        attackFinished = false;

        // Dispara a animação
        if (animator != null) animator.SetTrigger(attackHash);

        if (useAnimationEvents)
        {
            // A animação manda: espera o evento de fim (com limite de segurança)
            float timer = 0f;
            while (!attackFinished && timer < maxAttackTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            // Garante que nada fica ligado se o evento de desligar falhou
            SetHitbox(false);
            SetVisual(false);
        }
        else
        {
            // Modo por tempo
            SetHitbox(true);
            SetVisual(true);
            yield return new WaitForSeconds(attackDuration);
            SetHitbox(false);
            SetVisual(false);
        }

        IsAttacking = false;
        attackRoutine = null;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // Interrompe o golpe na hora (rolar, tomar dano, morrer...). Pode ser chamado de fora.
    public void CancelAttack()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        SetHitbox(false);
        SetVisual(false);
        IsAttacking = false;

        // Limpa um trigger que ainda não foi usado, para não disparar o ataque depois
        if (animator != null) animator.ResetTrigger(attackHash);

        // O cooldown ainda vale depois de cancelar
        StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        canAttack = false;
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    // ---------- Chamados pelos Animation Events ----------
    public void AnimEvent_HitboxOn() { SetHitbox(true); SetVisual(true); }
    public void AnimEvent_HitboxOff() { SetHitbox(false); SetVisual(false); }
    public void AnimEvent_AttackEnd() { attackFinished = true; }

    // ---------- Auxiliares ----------
    private void SetHitbox(bool value)
    {
        if (attackHitbox != null) attackHitbox.SetActive(value);
    }

    private void SetVisual(bool value)
    {
        if (attackVisual != null) attackVisual.SetActive(value);
    }
}