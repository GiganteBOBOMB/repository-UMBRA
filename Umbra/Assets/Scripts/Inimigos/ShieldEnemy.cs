using UnityEngine;

// Os modos do inimigo. Só um está ativo por vez.
public enum ShieldEnemyMode
{
    Idle,    // parado (jogador longe, ou perto demais durante a recarga)
    Chase,   // andando até o jogador
    Charge   // dash com o escudo
}

public class ShieldEnemy : EnemyBase
{
    [Header("Jogador")]

    // Referência ao jogador.
    public Transform player;


    [Header("Alcances")]

    // Ligado: o dash começa quando o jogador está dentro da distância que o próprio dash percorre
    // (calculada com a velocidade, a duração e a curva). Assim o dash sempre alcança o jogador.
    // Desligado: usa o Attack Range manual abaixo.
    public bool attackRangeFollowsDash = true;

    // Alcance do dash manual. Só vale se "Attack Range Follows Dash" estiver desligado.
    public float attackRange = 2.5f;

    // O alcance em que ele SEGUE o jogador é o alcance do dash multiplicado por este valor.
    // 2 = o dobro.
    public float followRangeMultiplier = 2f;


    [Header("Modo 1: Andar até o jogador")]

    // Velocidade de caminhada.
    public float walkSpeed = 2.5f;

    // Quão rápido ele ganha e perde velocidade (unidades por segundo ao quadrado).
    // Maior = arranca e para mais bruscamente. Menor = mais suave.
    public float acceleration = 12f;

    // Diferença horizontal mínima para ele virar de lado.
    // Evita ficar virando de um lado para o outro quando o jogador está bem acima ou abaixo.
    public float turnDeadZone = 0.1f;


    [Header("Modo 2: Dash")]

    // Velocidade MÁXIMA do dash (o pico da curva abaixo).
    public float chargeSpeed = 15f;

    // Duração total do dash.
    public float chargeDuration = 0.6f;

    // Como a velocidade muda durante o dash.
    // Horizontal: tempo do dash (0 = começo, 1 = fim).
    // Vertical: fração da velocidade máxima (1 = chargeSpeed).
    // Padrão: arranca, chega ao pico em 30% do tempo e vai freando até parar.
    public AnimationCurve chargeSpeedCurve = new AnimationCurve(
        new Keyframe(0f, 0.2f),
        new Keyframe(0.3f, 1f),
        new Keyframe(1f, 0f)
    );

    // Tempo de espera entre dashes.
    public float attackCooldown = 2f;


    // Distância que o dash percorre, calculada. Somente leitura (aparece nos gizmos).
    public float DashDistance { get; private set; }

    // Alcance do dash: a distância em que ele começa o dash.
    public float DashRange => attackRangeFollowsDash ? DashDistance : attackRange;

    // Alcance em que ele segue o jogador: o alcance do dash vezes o multiplicador (2 = o dobro).
    public float FollowRange => DashRange * followRangeMultiplier;

    // Modo atual (somente leitura de fora).
    public ShieldEnemyMode Mode { get; private set; } = ShieldEnemyMode.Idle;

    // Informa se o ataque está em tempo de recarga.
    private bool isOnCooldown = false;

    // Tempo restante da recarga.
    private float cooldownTimer = 0f;

    // Direção do dash (travada no começo dele).
    private int chargeDirection = 1;

    // Tempo já gasto no dash.
    private float chargeElapsed = 0f;

    // Lado para onde o inimigo está virado (-1 esquerda, 1 direita).
    private int facingDirection = 1;

    // Velocidade horizontal atual, com sinal. É ela que é suavizada.
    private float moveVelocity = 0f;


    protected override void Awake()
    {
        // Inicializa vida e SpriteRenderer da classe base.
        base.Awake();

        DashDistance = CalculateDashDistance();

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


    // Recalcula a distância do dash sempre que um valor muda no Inspector.
    private void OnValidate()
    {
        DashDistance = CalculateDashDistance();
    }


    // Distância total que o dash percorre: velocidade máxima x duração x média da curva.
    // A média é medida somando a curva em vários pontos do tempo.
    private float CalculateDashDistance()
    {
        if (chargeSpeedCurve == null || chargeSpeedCurve.length == 0)
            return chargeSpeed * chargeDuration;

        const int samples = 60;
        float sum = 0f;

        for (int i = 0; i < samples; i++)
        {
            float t = (i + 0.5f) / samples;
            sum += chargeSpeedCurve.Evaluate(t);
        }

        float average = sum / samples;
        return chargeSpeed * chargeDuration * average;
    }


    private void Update()
    {
        // Sem jogador, não há como detectar ou atacar.
        if (player == null)
            return;

        // Durante o atordoamento, o inimigo não faz nada.
        if (IsStunned)
        {
            moveVelocity = 0f;
            return;
        }

        UpdateCooldown();

        // Modo dash: só continua o dash.
        if (Mode == ShieldEnemyMode.Charge)
        {
            UpdateCharge();
            return;
        }

        // Distância e lado do jogador.
        float distance = Vector2.Distance(transform.position, player.position);
        float deltaX = player.position.x - transform.position.x;

        // Dá o dash se o jogador estiver ao alcance do dash e o dash estiver disponível.
        if (!isOnCooldown && distance <= DashRange)
        {
            StartCharge(deltaX);
            return;
        }

        // Decide entre andar até o jogador ou ficar parado.
        // Anda se o jogador está dentro do alcance de seguir e ainda fora do alcance do dash.
        bool shouldChase = distance <= FollowRange && distance > DashRange;
        Mode = shouldChase ? ShieldEnemyMode.Chase : ShieldEnemyMode.Idle;

        // Vira para o jogador sempre que ele estiver à vista (inclusive esperando a recarga).
        if (distance <= FollowRange && Mathf.Abs(deltaX) > turnDeadZone)
        {
            SetFacing(deltaX < 0f ? -1 : 1);
        }

        // Velocidade desejada: caminhada na direção do jogador, ou zero.
        float targetVelocity = shouldChase ? facingDirection * walkSpeed : 0f;
        MoveHorizontally(targetVelocity);
    }


    private void UpdateCooldown()
    {
        if (!isOnCooldown)
            return;

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f)
        {
            isOnCooldown = false;
        }
    }


    // Aproxima a velocidade atual da desejada aos poucos (é isso que deixa o movimento suave)
    // e move o inimigo.
    private void MoveHorizontally(float targetVelocity)
    {
        moveVelocity = Mathf.MoveTowards(moveVelocity, targetVelocity, acceleration * Time.deltaTime);
        transform.position += Vector3.right * moveVelocity * Time.deltaTime;
    }


    private void StartCharge(float deltaX)
    {
        Mode = ShieldEnemyMode.Charge;

        // Determina se o jogador está à esquerda ou à direita e trava essa direção.
        chargeDirection = deltaX < 0f ? -1 : 1;
        SetFacing(chargeDirection);

        // Começa a contagem do dash.
        chargeElapsed = 0f;
    }


    private void UpdateCharge()
    {
        chargeElapsed += Time.deltaTime;

        // Progresso do dash de 0 a 1.
        float t = Mathf.Clamp01(chargeElapsed / Mathf.Max(0.01f, chargeDuration));

        // Fração da velocidade máxima neste instante, vinda da curva.
        float multiplier = (chargeSpeedCurve != null && chargeSpeedCurve.length > 0)
            ? chargeSpeedCurve.Evaluate(t)
            : 1f;

        moveVelocity = chargeDirection * chargeSpeed * multiplier;
        transform.position += Vector3.right * moveVelocity * Time.deltaTime;

        // Finaliza quando o tempo acaba.
        if (chargeElapsed >= chargeDuration)
        {
            StopCharge();
        }
    }


    private void StopCharge()
    {
        // Volta ao modo normal. A velocidade NÃO é zerada aqui:
        // o MoveHorizontally a reduz aos poucos, então o inimigo desliza e para sem tranco.
        Mode = ShieldEnemyMode.Idle;

        // Inicia a recarga do dash.
        isOnCooldown = true;
        cooldownTimer = attackCooldown;
    }


    private void SetFacing(int direction)
    {
        facingDirection = direction;

        // Vira o sprite para a esquerda ou para a direita.
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = direction < 0;
        }
    }


    public override void Stun(float duration)
    {
        // Interrompe o dash e qualquer movimento ao receber o atordoamento.
        Mode = ShieldEnemyMode.Idle;
        moveVelocity = 0f;

        // Aplica o atordoamento definido na classe base.
        base.Stun(duration);
    }


    public override void Stun()
    {
        // Usa a duração padrão definida no Inspector.
        Stun(stunDuration);
    }


    // Mostra os alcances na Scene View quando o inimigo está selecionado.
    private void OnDrawGizmosSelected()
    {
        if (DashDistance <= 0f) DashDistance = CalculateDashDistance();

        // Amarelo: alcance em que ele segue o jogador (o dobro do dash).
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, FollowRange);

        // Vermelho: alcance do dash (onde ele para de andar e investe).
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, DashRange);
    }
}