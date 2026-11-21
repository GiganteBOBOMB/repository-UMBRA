using System.Collections;
using UnityEngine;

// Ataque 4 (Jevil): o boss some, reaparece em outro ponto da arena e dispara projéteis em cone
// na direção do player. Repete várias vezes e, no fim, volta ao chão.
public class AtaqueTeleporteCone : KnightAttack
{
    [Header("Teleportes")]
    [SerializeField, Min(1)] private int teleportCount = 10;
    [Tooltip("Tempo invisível entre sumir e reaparecer.")]
    [SerializeField, Min(0f)] private float invisibleTime = 0.15f;

    [Header("Onde reaparece")]
    [Tooltip("Altura máxima acima do chão para os pontos sorteados.")]
    [SerializeField] private float maxHeight = 4f;
    [Tooltip("Distância mínima das bordas da arena.")]
    [SerializeField] private float edgeMargin = 1.5f;
    [Tooltip("O boss nunca reaparece mais perto que isso do player.")]
    [SerializeField] private float minDistanceFromTarget = 3f;
    [Tooltip("O novo ponto fica pelo menos isso longe do ponto anterior (evita 'teleportar' no mesmo lugar).")]
    [SerializeField] private float minDistanceFromPrevious = 2.5f;

    [Header("Disparo em cone")]
    [SerializeField] private KnightProjectile projectilePrefab;
    [SerializeField, Min(1)] private int projectilesPerVolley = 5;
    [Tooltip("Abertura total do cone, em graus. Os projéteis se espalham de um lado ao outro.")]
    [SerializeField, Range(0f, 180f)] private float coneAngle = 50f;
    [SerializeField, Min(0.1f)] private float projectileSpeed = 9f;
    [SerializeField, Min(0.1f)] private float projectileLifetime = 4f;
    [Tooltip("De onde saem os projéteis, em relação ao boss. O X é espelhado pelo lado.")]
    [SerializeField] private Vector2 muzzleOffset = new Vector2(0.5f, 0.3f);

    [Header("Tempos")]
    [Tooltip("Pausa entre reaparecer e disparar. É o aviso de cada tiro.")]
    [SerializeField, Min(0f)] private float aimPause = 0.2f;
    [Tooltip("Pausa depois do disparo, antes de sumir de novo.")]
    [SerializeField, Min(0f)] private float holdAfterFire = 0.15f;
    [SerializeField, Min(0f)] private float endRecovery = 0.5f;

    private KnightMotor activeMotor;
    private FloatingWeapon activeWeapon;

    public override IEnumerator Execute(KnightContext context)
    {
        KnightMotor motor = context.Motor;
        FloatingWeapon weapon = context.Weapon;

        activeMotor = motor;
        activeWeapon = weapon;

        if (projectilePrefab == null)
            Debug.LogWarning($"[{AttackName}] Projectile Prefab não atribuído: o boss vai teleportar sem atirar.", this);

        // A arma acompanha o boss durante todo o ataque
        weapon.Follow();

        for (int i = 0; i < teleportCount; i++)
        {
            Vector3 point = PickPoint(context, motor.transform.position, false);
            yield return TeleportTo(context, point);

            // Aviso: o boss aparece, se vira para o player e só então atira
            if (aimPause > 0f) yield return new WaitForSeconds(aimPause);

            motor.FaceTowards(context.Target);
            FireCone(context);

            if (holdAfterFire > 0f) yield return new WaitForSeconds(holdAfterFire);
        }

        // Último teleporte: volta para o chão, para os próximos ataques partirem dele
        Vector3 groundPoint = PickPoint(context, motor.transform.position, true);
        yield return TeleportTo(context, groundPoint);

        if (endRecovery > 0f) yield return new WaitForSeconds(endRecovery);

        activeMotor = null;
        activeWeapon = null;
    }

    // ---------- Teleporte ----------

    private IEnumerator TeleportTo(KnightContext context, Vector3 point)
    {
        KnightMotor motor = context.Motor;
        FloatingWeapon weapon = context.Weapon;

        // Some: o boss e a arma
        weapon.SetHitbox(false);
        motor.SetVisible(false);
        weapon.gameObject.SetActive(false);

        if (invisibleTime > 0f) yield return new WaitForSeconds(invisibleTime);

        // Reaparece: primeiro posiciona e vira, depois cola a arma ao lado dele, sem atraso
        motor.transform.position = point;
        motor.FaceTowards(context.Target);

        weapon.gameObject.SetActive(true);
        weapon.SnapToFollow();
        motor.SetVisible(true);
    }

    // Sorteia um ponto válido: longe do player e do ponto anterior, dentro da arena
    private Vector3 PickPoint(KnightContext context, Vector3 previous, bool groundOnly)
    {
        float groundY = context.Motor.HomeY;
        float z = context.Motor.transform.position.z;

        float minX = context.Arena != null ? context.Arena.LeftX + edgeMargin : previous.x - 8f;
        float maxX = context.Arena != null ? context.Arena.RightX - edgeMargin : previous.x + 8f;

        Vector3 best = previous;
        float bestScore = float.MinValue;

        for (int attempt = 0; attempt < 15; attempt++)
        {
            float x = Random.Range(minX, maxX);
            float y = groundOnly ? groundY : Random.Range(groundY, groundY + maxHeight);
            Vector3 candidate = new Vector3(x, y, z);

            float fromTarget = context.Target != null
                ? Vector2.Distance(candidate, context.Target.position)
                : float.MaxValue;
            float fromPrevious = Vector2.Distance(candidate, previous);

            if (fromTarget >= minDistanceFromTarget && fromPrevious >= minDistanceFromPrevious)
                return candidate;

            // Nenhum ponto passou nas regras? Guarda o que chegou mais perto de passar
            float score = Mathf.Min(fromTarget - minDistanceFromTarget, fromPrevious - minDistanceFromPrevious);
            if (score > bestScore)
            {
                bestScore = score;
                best = candidate;
            }
        }

        return best;
    }

    // ---------- Disparo ----------

    private void FireCone(KnightContext context)
    {
        if (projectilePrefab == null) return;

        Transform boss = context.Motor.transform;
        int facing = context.Motor.Facing;

        Vector3 origin = boss.position + new Vector3(muzzleOffset.x * facing, muzzleOffset.y, 0f);

        // Direção central: do boss até o player
        Vector2 center = context.Target != null
            ? (Vector2)(context.Target.position - origin)
            : new Vector2(facing, 0f);
        center.Normalize();

        float half = coneAngle * 0.5f;

        for (int i = 0; i < projectilesPerVolley; i++)
        {
            // Espalha os projéteis igualmente de -half até +half (um só vai reto no centro)
            float offset = projectilesPerVolley == 1
                ? 0f
                : Mathf.Lerp(-half, half, i / (float)(projectilesPerVolley - 1));

            Vector2 dir = Quaternion.Euler(0f, 0f, offset) * center;

            KnightProjectile p = Instantiate(projectilePrefab, origin, Quaternion.identity);
            p.Launch(dir, projectileSpeed, projectileLifetime);
        }
    }

    // Cortado no meio (morte, troca de fase)? Não deixa o boss ou a arma invisíveis nem no ar.
    public override void Cancel()
    {
        base.Cancel();

        if (activeWeapon != null)
        {
            activeWeapon.gameObject.SetActive(true);
            activeWeapon = null;
        }

        if (activeMotor != null)
        {
            activeMotor.SetVisible(true);
            activeMotor.SnapToGround();
            activeMotor = null;
        }
    }
}