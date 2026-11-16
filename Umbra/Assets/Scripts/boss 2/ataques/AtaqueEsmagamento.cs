using System.Collections;
using UnityEngine;

public class AtaqueEsmagamento : BossAttack
{
    [Header("Esmagamento")]
    [SerializeField] private float raiseHeight = 4f;
    [SerializeField] private float raiseTime = 0.4f;
    [Tooltip("Tempo parada no alto antes de bater. É a janela de reação do jogador.")]
    [SerializeField] private float hangTime = 0.35f;
    [SerializeField] private float slamTime = 0.12f;
    [Tooltip("Quanto tempo a área de dano fica ligada no impacto. Mantenha acima de 0.1.")]
    [SerializeField] private float hitboxActiveTime = 0.15f;
    [SerializeField] private float recoverTime = 0.3f;
    [SerializeField] private float returnTime = 0.5f;

    [Header("Mira")]
    [Tooltip("Ajuste fino da altura do impacto (ex.: metade da altura do sprite da mão).")]
    [SerializeField] private float impactOffsetY = 0f;
    [Tooltip("Ligado: a mão se alinha ao X do player no começo do ataque e bate ali.")]
    [SerializeField] private bool aimAtTarget = true;

    [Header("Tremor da câmera")]
    [Tooltip("Distância máxima do tremor, em unidades do mundo. 0 desliga o tremor.")]
    [SerializeField] private float shakeIntensity = 0.3f;
    [Tooltip("Duração do tremor, em segundos.")]
    [SerializeField] private float shakeDuration = 0.25f;

    public override IEnumerator Execute(AttackContext context)
    {
        BossHand hand = context.Acting;

        // O X é definido uma vez, no começo. Quem se mexer durante a subida escapa.
        float x = (aimAtTarget && context.Target != null)
            ? context.Target.position.x
            : hand.transform.position.x;
        float z = hand.transform.position.z;

        float topY = hand.RestWorldPosition.y + raiseHeight;
        float groundY = context.Arena.GroundY + impactOffsetY;

        // 1. Sobe e fica parada (aviso)
        yield return hand.MoveToWorld(new Vector3(x, topY, z), raiseTime);
        yield return new WaitForSeconds(hangTime);

        // 2. Bate no chão
        yield return hand.MoveToWorld(new Vector3(x, groundY, z), slamTime);

        // 3. Impacto: liga o dano, treme a câmera e chama o gancho para ataques derivados
        Vector3 impactPoint = new Vector3(x, groundY, z);
        hand.SetHitbox(true);

        if (shakeIntensity > 0f && CameraShake.Instance != null)
            CameraShake.Instance.Shake(shakeIntensity, shakeDuration);

        OnImpact(hand, impactPoint, context);
        yield return new WaitForSeconds(hitboxActiveTime);
        hand.SetHitbox(false);

        // 4. Recupera e volta
        yield return new WaitForSeconds(recoverTime);
        yield return hand.ReturnToRest(returnTime);
    }

    // Gancho: ataques que herdam deste (como a Onda) sobrescrevem para adicionar efeitos no impacto
    protected virtual void OnImpact(BossHand hand, Vector3 impactPoint, AttackContext context) { }
}