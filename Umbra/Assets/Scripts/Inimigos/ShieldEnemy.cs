using UnityEngine;

public class ShieldEnemy : EnemyBase
{
    [Header("Detecção do Jogador")]

    // Distância na qual o inimigo percebe o jogador.
    public float detectionRange = 5f;

    // Referência ao jogador.
    public Transform player;


    [Header("Ataque de Escudo")]

    // Velocidade da investida.
    public float chargeSpeed = 8f;

    // Tempo durante o qual o inimigo investe.
    public float chargeDuration = 0.6f;

    // Tempo de espera entre investidas.
    public float attackCooldown = 2f;

    // Distância mínima para iniciar a investida.
    public float attackRange = 2.5f;


    // Informa se o inimigo está investindo.
    private bool isCharging = false;

    // Informa se o ataque está em tempo de recarga.
    private bool isOnCooldown = false;

    // Direção da investida.
    private int chargeDirection = 1;

    // Tempo restante da investida.
    private float chargeTimer = 0f;

    // Tempo restante da recarga.
    private float cooldownTimer = 0f;


    protected override void Awake()
    {
        // Inicializa vida e SpriteRenderer da classe base.
        base.Awake();

        // Procura o jogador pela tag Player, caso não tenha sido definido.
        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }
    }


    private void Update()
    {
        // Sem jogador, não há como detectar ou atacar.
        if (player == null)
            return;

        // Durante o atordoamento, o inimigo não ataca.
        if (IsStunned)
            return;

        // Atualiza o tempo de recarga.
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;

            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
            }
        }

        // Continua a investida caso esteja atacando.
        if (isCharging)
        {
            UpdateCharge();
            return;
        }

        // Calcula a distância até o jogador.
        float distance = Vector2.Distance(transform.position, player.position);

        // Ataca se o jogador estiver próximo e o ataque estiver disponível.
        if (!isOnCooldown && distance <= attackRange)
        {
            StartCharge();
        }
    }


    private void StartCharge()
    {
        // Define que a investida começou.
        isCharging = true;

        // Determina se o jogador está à esquerda ou à direita.
        chargeDirection = player.position.x < transform.position.x ? -1 : 1;

        // Inicia a contagem da investida.
        chargeTimer = chargeDuration;

        // Vira o sprite para a direção do ataque.
        UpdateFacingDirection();
    }


    private void UpdateCharge()
    {
        // Interrompe a investida se o inimigo for atordoado.
        if (IsStunned)
        {
            StopCharge();
            return;
        }

        // Move o inimigo horizontalmente durante a investida.
        transform.position += Vector3.right * chargeDirection * chargeSpeed * Time.deltaTime;

        // Atualiza a duração restante da investida.
        chargeTimer -= Time.deltaTime;

        // Finaliza a investida quando o tempo acaba.
        if (chargeTimer <= 0f)
        {
            StopCharge();
        }
    }


    private void StopCharge()
    {
        // Encerra a investida.
        isCharging = false;

        // Inicia a recarga do ataque.
        isOnCooldown = true;
        cooldownTimer = attackCooldown;
    }


    private void UpdateFacingDirection()
    {
        // Vira o sprite para a esquerda ou para a direita.
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = chargeDirection < 0;
        }
    }


    public override void Stun(float duration)
    {
        // Interrompe a investida ao receber o atordoamento.
        isCharging = false;

        // Aplica o atordoamento definido na classe base.
        base.Stun(duration);
    }


    public override void Stun()
    {
        // Usa a duração padrão definida no Inspector.
        Stun(stunDuration);
    }
}
