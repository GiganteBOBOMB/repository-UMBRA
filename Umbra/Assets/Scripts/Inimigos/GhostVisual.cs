using UnityEngine;
using UnityEngine.Rendering.Universal; // Light2D (URP). Em Unity 2019: UnityEngine.Experimental.Rendering.Universal

// Aparência de fantasma: sprite translúcido que pulsa de leve.
// Opcionalmente fica INVISÍVEL até uma das luzes escolhidas iluminá-lo.
// Coloque no objeto do inimigo (ou do sprite). Para flutuar, use também o FloatBob num filho com o sprite.
public class GhostVisual : MonoBehaviour
{
    [Header("Sprites")]
    [Tooltip("Se ficar vazio, usa todos os SpriteRenderer deste objeto e dos filhos.")]
    [SerializeField] private SpriteRenderer[] renderers;

    [Header("Efeito fantasma")]
    [Tooltip("Opacidade do fantasma quando visível. 1 = sólido, 0.5 = bem translúcido.")]
    [SerializeField, Range(0f, 1f)] private float ghostAlpha = 0.65f;
    [Tooltip("Quanto a opacidade oscila para cima e para baixo.")]
    [SerializeField, Range(0f, 0.5f)] private float pulseAmount = 0.12f;
    [SerializeField] private float pulseSpeed = 2f;

    [Header("Visível só com a luz")]
    [Tooltip("Ligado: invisível até a luz iluminar. Desligado: sempre visível (só o efeito fantasma).")]
    [SerializeField] private bool hiddenUntilLit = true;

    [Tooltip("ARRASTE AQUI a(s) luz(es) da cena que revelam este fantasma (ex.: a luz da lanterna).\n" +
             "Aceita Point Light 2D, com ou sem cone (ângulo). Só essas luzes revelam; as outras são ignoradas.")]
    [SerializeField] private Light2D[] revealLights;

    [Tooltip("Folga extra no alcance da luz. Positivo = revela um pouco antes da borda (ajuda com fantasmas grandes).")]
    [SerializeField] private float lightMargin = 0f;

    [Tooltip("Ligado: a lanterna também pode revelar chamando SetIlluminated(true) por script (método antigo).")]
    [SerializeField] private bool alsoAcceptScriptCall = false;

    [Tooltip("Por quanto tempo continua visível depois da luz sair. Evita piscar na borda da luz.")]
    [SerializeField, Min(0f)] private float revealHoldTime = 0.3f;
    [Tooltip("Velocidade para aparecer. Maior = mais rápido.")]
    [SerializeField, Min(0.1f)] private float fadeInSpeed = 8f;
    [Tooltip("Velocidade para sumir. Menor = some devagar, como uma névoa.")]
    [SerializeField, Min(0.1f)] private float fadeOutSpeed = 2.5f;

    /// 0 = totalmente invisível, 1 = totalmente visível (como fantasma).
    public float Visibility { get; private set; }

    /// True quando o fantasma está visível o bastante para ser visto/atingido.
    public bool IsRevealed => Visibility > 0.5f;

    private float[] baseAlpha;
    private float litTimer;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>();

        // Guarda a opacidade original de cada sprite, para multiplicar sem perder
        baseAlpha = new float[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            baseAlpha[i] = renderers[i].color.a;

        Visibility = hiddenUntilLit ? 0f : 1f;
        Apply();
    }

    /// Método opcional: uma lanterna pode chamar isto. Só vale se "Also Accept Script Call" estiver ligado.
    public void SetIlluminated(bool lit)
    {
        if (!alsoAcceptScriptCall) return;
        if (lit) litTimer = revealHoldTime;
    }

    private void Update()
    {
        // Cada quadro que a luz pega o fantasma, o tempo de "visível" é renovado
        if (hiddenUntilLit && IsInsideAnyLight())
            litTimer = revealHoldTime;

        litTimer -= Time.deltaTime;

        bool lit = litTimer > 0f;
        float target = (!hiddenUntilLit || lit) ? 1f : 0f;
        float speed = target > Visibility ? fadeInSpeed : fadeOutSpeed;
        Visibility = Mathf.MoveTowards(Visibility, target, speed * Time.deltaTime);

        Apply();
    }

    // Centro do fantasma: o centro do primeiro sprite (funciona mesmo com o sprite num filho)
    private Vector2 GetCenter()
    {
        if (renderers.Length > 0 && renderers[0] != null)
            return renderers[0].bounds.center;
        return transform.position;
    }

    private bool IsInsideAnyLight()
    {
        if (revealLights == null) return false;

        Vector2 center = GetCenter();

        for (int i = 0; i < revealLights.Length; i++)
        {
            if (IsInsideLight(revealLights[i], center)) return true;
        }
        return false;
    }

    private bool IsInsideLight(Light2D light, Vector2 point)
    {
        // Luz apagada (lanterna desligada) não revela nada
        if (light == null || !light.enabled || !light.gameObject.activeInHierarchy || light.intensity <= 0f)
            return false;

        // Só luzes de ponto/cone têm alcance definido
        if (light.lightType != Light2D.LightType.Point) return false;

        Vector2 toGhost = point - (Vector2)light.transform.position;

        // Distância: dentro do raio externo da luz
        if (toGhost.magnitude > light.pointLightOuterRadius + lightMargin) return false;

        // Cone: se o ângulo externo for menor que 360, só vale dentro dele
        // (a luz aponta para o "up" do objeto, a seta verde)
        float cone = light.pointLightOuterAngle;
        if (cone < 359f)
        {
            float angle = Vector2.Angle(light.transform.up, toGhost);
            if (angle > cone * 0.5f) return false;
        }

        return true;
    }

    private void Apply()
    {
        // Pulso suave de opacidade (seno), somado à opacidade do fantasma
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
        float alpha = Mathf.Clamp01(ghostAlpha + pulse) * Visibility;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;

            Color c = renderers[i].color;
            c.a = baseAlpha[i] * alpha;
            renderers[i].color = c;
        }
    }
}