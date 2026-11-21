using System.Collections;
using UnityEngine;

// Ataque 2 (Phantom): o boss arremessa a arma na direção do player e depois vai até ela.
public class AtaqueArremesso : KnightAttack
{
    [Header("Mira")]
    [Tooltip("Para onde a arma vai antes de ser jogada: atrás do boss (X) e acima dele (Y).")]
    [SerializeField] private Vector2 aimOffset = new Vector2(-0.8f, 1.6f);
    [Tooltip("Tempo da arma indo até a posição de mira.")]
    [SerializeField, Min(0.01f)] private float aimMoveTime = 0.25f;
    [Tooltip("Tempo que a arma fica acompanhando o player antes de travar a mira.")]
    [SerializeField, Min(0f)] private float trackTime = 0.45f;
    [Tooltip("Tempo com a mira TRAVADA antes de jogar. É a janela para o jogador desviar.")]
    [SerializeField, Min(0f)] private float lockTime = 0.2f;

    [Header("Arremesso")]
    [SerializeField, Min(1f)] private float throwSpeed = 28f;
    [Tooltip("Quanto a arma passa além do ponto onde o player estava quando a mira travou.")]
    [SerializeField, Min(0f)] private float overshoot = 1.5f;
    [Tooltip("Distância usada se o ataque não tiver um alvo (Target vazio no KnightBrain).")]
    [SerializeField] private float fallbackDistance = 8f;
    [Tooltip("Altura mínima da arma cravada, acima do chão da arena (se houver Arena).")]
    [SerializeField] private float stuckHeightAboveGround = 0.3f;

    [Header("Arma cravada")]
    [SerializeField, Min(0f)] private float stuckTime = 0.35f;

    [Header("Boss vai até a arma")]
    [Tooltip("Pausa antes de o boss sair correndo.")]
    [SerializeField, Min(0f)] private float dashDelay = 0.15f;
    [SerializeField, Min(1f)] private float dashSpeed = 30f;
    [Tooltip("Opcional: área de dano do corpo do boss, ligada durante a corrida. Pode ficar vazio.")]
    [SerializeField] private BossHitbox bodyHitbox;
    [SerializeField, Min(0f)] private float endRecovery = 0.4f;

    [Header("Tremor da câmera no impacto da arma")]
    [Tooltip("0 desliga o tremor.")]
    [SerializeField] private float shakeIntensity = 0.15f;
    [SerializeField] private float shakeDuration = 0.15f;

    public override IEnumerator Execute(KnightContext context)
    {
        KnightMotor motor = context.Motor;
        FloatingWeapon weapon = context.Weapon;

        motor.FaceTowards(context.Target);
        int facing = motor.Facing;

        // 1. A arma sobe atrás do boss
        Vector3 bossPos = motor.transform.position;
        Vector3 aimPoint = new Vector3(
            bossPos.x + aimOffset.x * facing,
            bossPos.y + aimOffset.y,
            bossPos.z);

        yield return weapon.MoveTo(aimPoint, aimMoveTime);

        // 2. Acompanha o player (a mira gira junto com ele)
        float t = 0f;
        while (t < trackTime)
        {
            AimAtTarget(weapon, context.Target, facing);
            t += Time.deltaTime;
            yield return null;
        }

        // 3. Trava a mira: guarda onde o player ESTAVA. Quem se mexer agora, escapa.
        Vector3 lockedPoint = context.Target != null
            ? context.Target.position
            : weapon.transform.position + Vector3.right * facing * fallbackDistance;

        Vector3 direction = (lockedPoint - weapon.transform.position).normalized;
        weapon.PointToward(direction);

        if (lockTime > 0f) yield return new WaitForSeconds(lockTime);

        // 4. Arremessa: passa do ponto travado e cravada no chão
        Vector3 endPoint = lockedPoint + direction * overshoot;

        if (context.Arena != null)
        {
            endPoint.x = Mathf.Clamp(endPoint.x, context.Arena.LeftX, context.Arena.RightX);
            endPoint.y = Mathf.Max(endPoint.y, context.Arena.GroundY + stuckHeightAboveGround);
        }

        float distance = Vector3.Distance(weapon.transform.position, endPoint);
        float flightTime = Mathf.Max(0.06f, distance / throwSpeed);

        weapon.SetHitbox(true);
        yield return weapon.MoveTo(endPoint, flightTime);
        weapon.SetHitbox(false);

        if (shakeIntensity > 0f && CameraShake.Instance != null)
            CameraShake.Instance.Shake(shakeIntensity, shakeDuration);

        // 5. A arma fica cravada um instante
        if (stuckTime > 0f) yield return new WaitForSeconds(stuckTime);

        // 6. O boss vai até a arma
        motor.FaceTowards(weapon.transform.position.x);
        if (dashDelay > 0f) yield return new WaitForSeconds(dashDelay);

        Vector3 bossStart = motor.transform.position;
        float targetX = weapon.transform.position.x;
        if (context.Arena != null)
            targetX = Mathf.Clamp(targetX, context.Arena.LeftX, context.Arena.RightX);

        Vector3 bossEnd = new Vector3(targetX, bossStart.y, bossStart.z);
        float dashTime = Mathf.Max(0.1f, Mathf.Abs(bossEnd.x - bossStart.x) / dashSpeed);

        if (bodyHitbox != null) bodyHitbox.SetActive(true);
        yield return motor.MoveTo(bossEnd, dashTime, true);
        if (bodyHitbox != null) bodyHitbox.SetActive(false);

        // 7. Recuperação. O cérebro devolve a arma ao boss (weapon.Follow()) e ela volta com o atraso.
        if (endRecovery > 0f) yield return new WaitForSeconds(endRecovery);
    }

    private void AimAtTarget(FloatingWeapon weapon, Transform target, int facing)
    {
        Vector2 dir = target != null
            ? (Vector2)(target.position - weapon.transform.position)
            : new Vector2(facing, 0f);

        weapon.PointToward(dir);
    }

    // Se o ataque for cortado no meio da corrida, o corpo não pode ficar causando dano
    public override void Cancel()
    {
        base.Cancel();
        if (bodyHitbox != null) bodyHitbox.SetActive(false);
    }
}