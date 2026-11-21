using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StrikeKind
{
    Fall,   // a arma despenca do alto até o chão
    Rise    // a arma sobe de baixo do chão
}

// Um golpe do padrão. A lista de golpes é editada no Inspector.
[System.Serializable]
public class FinalStrikeStep
{
    public StrikeKind kind = StrikeKind.Fall;

    [Tooltip("Onde o golpe acontece na arena: 0 = borda esquerda, 0.5 = centro, 1 = borda direita.")]
    [Range(0f, 1f)] public float arenaPosition = 0.5f;

    [Tooltip("Ligado: ignora a posição acima e usa o X do player no começo do golpe.")]
    public bool aimAtTarget = false;

    [Tooltip("Pausa extra depois deste golpe (além da pausa padrão).")]
    [Min(0f)] public float extraPause = 0f;
}

// Ataque 5 (Jevil): o ataque final. O boss some e a arma, sozinha, cai do alto e sobe do chão
// em pontos diferentes, seguindo um padrão fixo. Só é usado quando a vida do boss zera,
// e a luta termina quando ele acaba.
public class AtaqueFinal : KnightAttack
{
    [Header("Padrão (na ordem em que acontece)")]
    [SerializeField]
    private List<FinalStrikeStep> pattern = new List<FinalStrikeStep>
    {
        new FinalStrikeStep { kind = StrikeKind.Fall, arenaPosition = 0.15f },
        new FinalStrikeStep { kind = StrikeKind.Rise, arenaPosition = 0.85f },
        new FinalStrikeStep { kind = StrikeKind.Fall, arenaPosition = 0.5f },
        new FinalStrikeStep { kind = StrikeKind.Rise, arenaPosition = 0.3f },
        new FinalStrikeStep { kind = StrikeKind.Fall, arenaPosition = 0.7f },
        new FinalStrikeStep { kind = StrikeKind.Rise, arenaPosition = 0.1f },
        new FinalStrikeStep { kind = StrikeKind.Fall, arenaPosition = 0.9f },
        new FinalStrikeStep { kind = StrikeKind.Rise, arenaPosition = 0.5f },
    };

    [Header("Abertura e fim")]
    [Tooltip("Tempo com o boss invisível antes do primeiro golpe.")]
    [SerializeField, Min(0f)] private float introDelay = 0.8f;
    [Tooltip("Ligado: o boss reaparece no lugar onde estava quando o padrão termina.")]
    [SerializeField] private bool reappearAtEnd = true;
    [Tooltip("Tempo com o boss visível antes de a luta terminar.")]
    [SerializeField, Min(0f)] private float endHold = 0.8f;

    [Header("Aviso de cada golpe")]
    [Tooltip("Opcional, mas recomendado: um objeto (marca no chão, seta) criado no ponto do golpe durante o aviso.")]
    [SerializeField] private GameObject warningPrefab;
    [Tooltip("Tempo da arma indo até a posição inicial do golpe.")]
    [SerializeField, Min(0.01f)] private float moveInTime = 0.25f;
    [Tooltip("Tempo de aviso com a arma parada na posição inicial. É a janela de reação.")]
    [SerializeField, Min(0f)] private float warnTime = 0.6f;

    [Header("Golpe")]
    [SerializeField, Min(0.01f)] private float strikeTime = 0.12f;
    [Tooltip("Tempo com a arma parada no ponto final, depois do golpe.")]
    [SerializeField, Min(0f)] private float holdTime = 0.25f;
    [Tooltip("Dos segundos acima, quanto a hitbox continua ligada depois do golpe.")]
    [SerializeField, Min(0f)] private float hitboxHold = 0.1f;
    [SerializeField, Min(0.01f)] private float retreatTime = 0.3f;
    [Tooltip("Pausa padrão entre um golpe e o próximo.")]
    [SerializeField, Min(0f)] private float pauseBetweenStrikes = 0.15f;

    [Header("Posições (em relação ao chão da arena)")]
    [Tooltip("Golpe 'Fall': altura de onde a arma despenca.")]
    [SerializeField] private float spawnHeight = 7f;
    [Tooltip("Golpe 'Fall': altura do centro da arma quando cravada no chão.")]
    [SerializeField] private float fallStopHeight = 0.4f;
    [Tooltip("Golpe 'Rise': profundidade abaixo do chão de onde a arma sai.")]
    [SerializeField] private float riseDepth = 3f;
    [Tooltip("Golpe 'Rise': altura do centro da arma no ponto mais alto.")]
    [SerializeField] private float riseStopHeight = 1.6f;

    [Header("Tremor da câmera em cada impacto")]
    [Tooltip("0 desliga o tremor.")]
    [SerializeField] private float shakeIntensity = 0.25f;
    [SerializeField] private float shakeDuration = 0.2f;

    private KnightMotor activeMotor;
    private Vector3 bossStartPosition;
    private GameObject currentMarker;

    // Só existe depois que a vida zerou (os ataques comuns respondem o contrário)
    public override bool CanUse(KnightContext context)
    {
        return context.Health != null && context.Health.IsDepleted;
    }

    // Quando este ataque termina, o cérebro conclui a morte do boss
    public override bool EndsBattleWhenFinished => true;

    public override IEnumerator Execute(KnightContext context)
    {
        BossArena arena = context.Arena;
        if (arena == null)
        {
            Debug.LogError($"[{AttackName}] O KnightBrain precisa de uma Arena. Ataque final cancelado.", this);
            yield break;
        }

        KnightMotor motor = context.Motor;
        FloatingWeapon weapon = context.Weapon;

        activeMotor = motor;
        bossStartPosition = motor.transform.position;

        // 1. O boss some. A arma fica no lugar, solta
        weapon.SetHitbox(false);
        weapon.Release();
        motor.SetVisible(false);

        if (introDelay > 0f) yield return new WaitForSeconds(introDelay);

        // 2. O padrão, golpe por golpe
        foreach (FinalStrikeStep step in pattern)
        {
            yield return PlayStrike(context, step);

            float pause = pauseBetweenStrikes + step.extraPause;
            if (pause > 0f) yield return new WaitForSeconds(pause);
        }

        // 3. O boss reaparece (e a arma volta a segui-lo, pelo cérebro)
        if (reappearAtEnd)
        {
            motor.transform.position = bossStartPosition;
            motor.SetVisible(true);
        }

        weapon.Follow();

        if (endHold > 0f) yield return new WaitForSeconds(endHold);

        activeMotor = null;
    }

    // Um golpe: a arma vai ao ponto inicial, avisa, golpeia, fica parada e recua
    private IEnumerator PlayStrike(KnightContext context, FinalStrikeStep step)
    {
        BossArena arena = context.Arena;
        FloatingWeapon weapon = context.Weapon;

        float groundY = arena.GroundY;
        float z = weapon.transform.position.z;
        bool isFall = step.kind == StrikeKind.Fall;

        // Onde acontece
        float x = (step.aimAtTarget && context.Target != null)
            ? Mathf.Clamp(context.Target.position.x, arena.LeftX, arena.RightX)
            : Mathf.Lerp(arena.LeftX, arena.RightX, step.arenaPosition);

        Vector3 startPoint = isFall
            ? new Vector3(x, groundY + spawnHeight, z)
            : new Vector3(x, groundY - riseDepth, z);

        Vector3 strikePoint = isFall
            ? new Vector3(x, groundY + fallStopHeight, z)
            : new Vector3(x, groundY + riseStopHeight, z);

        weapon.PointToward(isFall ? Vector2.down : Vector2.up);

        // Aviso: marca no chão (se houver) e a arma indo à posição inicial
        if (warningPrefab != null)
            currentMarker = Instantiate(warningPrefab, new Vector3(x, groundY, z), Quaternion.identity);

        yield return weapon.MoveTo(startPoint, moveInTime);
        if (warnTime > 0f) yield return new WaitForSeconds(warnTime);

        // Golpe
        weapon.SetHitbox(true);
        yield return weapon.MoveTo(strikePoint, strikeTime);

        if (shakeIntensity > 0f && CameraShake.Instance != null)
            CameraShake.Instance.Shake(shakeIntensity, shakeDuration);

        // A hitbox continua um instante e a arma fica parada
        float hitboxTime = Mathf.Min(hitboxHold, holdTime);
        if (hitboxTime > 0f) yield return new WaitForSeconds(hitboxTime);
        weapon.SetHitbox(false);

        float rest = holdTime - hitboxTime;
        if (rest > 0f) yield return new WaitForSeconds(rest);

        ClearMarker();

        // Recua para o ponto inicial
        yield return weapon.MoveTo(startPoint, retreatTime);
    }

    private void ClearMarker()
    {
        if (currentMarker != null)
        {
            Destroy(currentMarker);
            currentMarker = null;
        }
    }

    // Cortado no meio? Remove a marca e não deixa o boss invisível
    public override void Cancel()
    {
        base.Cancel();
        ClearMarker();

        if (activeMotor != null)
        {
            activeMotor.transform.position = bossStartPosition;
            activeMotor.SetVisible(true);
            activeMotor = null;
        }
    }
}