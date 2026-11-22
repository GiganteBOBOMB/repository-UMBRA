
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Referências")]
    public AttackHitbox attackHitbox;

    [Header("Posição da Hitbox")]
    public float horizontalOffset = 0.8f;
    public float verticalOffset = 0f;

    [Header("Configuração do ataque")]
    public float attackCooldown = 0.5f;
    public float attackDuration = 0.2f;
    public float attackDelay = 0f;

    private Animator animator;
    private SpriteRenderer spriteRenderer;

    private float nextAttackTime;
    private bool isAttacking;

    private static readonly int AttackHash =
        Animator.StringToHash("Attack");

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.qKey.wasPressedThisFrame)
        {
            Attack();
        }
    }

    private void LateUpdate()
    {
        UpdateHitboxPosition();
    }

    private void UpdateHitboxPosition()
    {
        if (attackHitbox == null)
            return;

        float direction = spriteRenderer.flipX ? -1f : 1f;

        attackHitbox.transform.localPosition = new Vector3(
            horizontalOffset * direction,
            verticalOffset,
            0f
        );
    }

    private void Attack()
    {
        if (isAttacking || Time.time < nextAttackTime)
            return;

        if (attackHitbox == null)
        {
            Debug.LogWarning("AttackHitbox não atribuída!");
            return;
        }

        nextAttackTime = Time.time + attackCooldown;

        // Executa a animação de ataque.
        if (animator != null)
            animator.SetTrigger(AttackHash);

        // Controla a hitbox independentemente da animação.
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        // Tempo de espera antes de ativar o dano.
        if (attackDelay > 0f)
            yield return new WaitForSeconds(attackDelay);

        // Atualiza a posição da hitbox.
        UpdateHitboxPosition();

        // Ativa o dano.
        attackHitbox.ActivateHitbox();

        // Mantém a hitbox ativa durante o golpe.
        yield return new WaitForSeconds(attackDuration);

        // Desativa o dano.
        attackHitbox.DeactivateHitbox();

        isAttacking = false;
    }

    private void OnDisable()
    {
        StopAllCoroutines();

        if (attackHitbox != null)
            attackHitbox.DeactivateHitbox();

        isAttacking = false;
    }
}
