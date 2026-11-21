using System.Collections;
using UnityEngine;

// Ataque 1 (Phantom): golpeia 3 vezes com a arma para a frente, avançando um pouco a cada golpe.
public class AtaqueTresGolpes : KnightAttack
{
    [Header("Golpes")]
    [SerializeField, Min(1)] private int strikeCount = 3;
    [Tooltip("Ligado: antes de cada golpe o boss vira para o player. Desligado: mantém o lado do primeiro golpe.")]
    [SerializeField] private bool reaimEachStrike = true;

    [Header("Preparação")]
    [Tooltip("Tempo da arma indo para trás antes de cada golpe.")]
    [SerializeField, Min(0.01f)] private float windupTime = 0.25f;
    [Tooltip("Espera extra só antes do PRIMEIRO golpe. É a janela de reação do jogador.")]
    [SerializeField, Min(0f)] private float firstStrikeExtraWindup = 0.3f;
    [Tooltip("Quanto a arma recua atrás do boss antes de golpear.")]
    [SerializeField] private float windBackDistance = 1f;

    [Header("Golpe")]
    [Tooltip("Duração do golpe. Menor = mais rápido e mais difícil de desviar.")]
    [SerializeField, Min(0.01f)] private float strikeTime = 0.12f;
    [Tooltip("O leve avanço do boss em cada golpe.")]
    [SerializeField] private float stepDistance = 1.5f;
    [Tooltip("Até onde a ponta da arma chega à frente do boss no fim do golpe.")]
    [SerializeField] private float reach = 1.6f;
    [Tooltip("Altura da arma em relação ao boss durante o ataque.")]
    [SerializeField] private float weaponHeight = 0.2f;

    [Header("Recuperação")]
    [SerializeField, Min(0f)] private float recoverBetweenStrikes = 0.12f;
    [SerializeField, Min(0f)] private float endRecovery = 0.4f;

    public override IEnumerator Execute(KnightContext context)
    {
        KnightMotor motor = context.Motor;
        FloatingWeapon weapon = context.Weapon;

        for (int i = 0; i < strikeCount; i++)
        {
            // Vira para o player (sempre no primeiro golpe, nos outros se Reaim estiver ligado)
            if (i == 0 || reaimEachStrike)
                motor.FaceTowards(context.Target);

            int facing = motor.Facing;

            // Onde o boss está e onde vai parar depois do avanço (sem sair da arena)
            Vector3 bossStart = motor.transform.position;
            float endX = bossStart.x + facing * stepDistance;
            if (context.Arena != null)
                endX = Mathf.Clamp(endX, context.Arena.LeftX, context.Arena.RightX);
            Vector3 bossEnd = new Vector3(endX, bossStart.y, bossStart.z);

            // 1. Preparação: a arma vai para trás do boss, apontando para a frente
            Vector3 windPoint = new Vector3(
                bossStart.x - facing * windBackDistance,
                bossStart.y + weaponHeight,
                bossStart.z);

            weapon.PointToward(new Vector2(facing, 0f));
            yield return weapon.MoveTo(windPoint, windupTime);

            if (i == 0 && firstStrikeExtraWindup > 0f)
                yield return new WaitForSeconds(firstStrikeExtraWindup);

            // 2. Golpe: a arma dispara para a frente enquanto o boss avança, ao mesmo tempo
            Vector3 tipPoint = new Vector3(
                bossEnd.x + facing * reach,
                bossStart.y + weaponHeight,
                bossStart.z);

            weapon.SetHitbox(true);

            Coroutine dash = StartCoroutine(motor.MoveTo(bossEnd, strikeTime, true));
            yield return weapon.MoveTo(tipPoint, strikeTime);
            yield return dash; // garante que o boss também terminou o avanço

            weapon.SetHitbox(false);

            // 3. Recuperação (maior depois do último golpe)
            bool isLast = i == strikeCount - 1;
            yield return new WaitForSeconds(isLast ? endRecovery : recoverBetweenStrikes);
        }

        // A arma volta a seguir o boss quando o cérebro chamar weapon.Follow()
    }
}