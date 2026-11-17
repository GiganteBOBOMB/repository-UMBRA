using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Efeito de chama de vela: a Light2D oscila em intensidade e raio, e a luz balança levemente de posição.
/// Coloque no mesmo objeto da Light2D da vela (ou arraste a luz no campo "luz").
/// </summary>
public class VelaFogo : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Light2D luz;
    [Tooltip("Opcional: sprite da chama, que balança e muda de tamanho junto com a luz.")]
    [SerializeField] private Transform chamaVisual;

    [Header("Intensidade")]
    [SerializeField] private float intensidadeBase = 1f;
    [SerializeField] private float variacaoIntensidade = 0.25f;   // quanto a intensidade sobe/desce

    [Header("Raio da luz (Point Light)")]
    [SerializeField] private bool animarRaio = true;
    [SerializeField] private float variacaoRaio = 0.3f;

    [Header("Movimento da chama")]
    [SerializeField] private float velocidade = 3f;               // rapidez do tremular
    [SerializeField] private float balancoPosicao = 0.04f;        // quanto a luz se desloca de lado
    [SerializeField] private float balancoRotacaoChama = 6f;      // graus que o sprite da chama inclina

    [Header("Rajadas de vento (oscilação ocasional mais forte)")]
    [SerializeField] private bool usarRajadas = true;
    [SerializeField] private float chanceRajadaPorSegundo = 0.4f;
    [SerializeField] private float forcaRajada = 2f;

    private float raioBase;
    private Vector3 posBase;
    private Vector3 escalaChamaBase;
    private float semente;
    private float rajada;   // 0..1, decai com o tempo

    private void Awake()
    {
        if (luz == null) luz = GetComponent<Light2D>();
        semente = Random.Range(0f, 1000f);       // cada vela fica diferente das outras
        posBase = transform.localPosition;
        if (luz != null) raioBase = luz.pointLightOuterRadius;
        if (chamaVisual != null) escalaChamaBase = chamaVisual.localScale;
    }

    private void Update()
    {
        if (luz == null) return;

        float t = Time.time * velocidade;

        // Ruído em duas camadas: um lento (respiração da chama) e um rápido (tremulação)
        float lento = Mathf.PerlinNoise(semente, t * 0.5f);
        float rapido = Mathf.PerlinNoise(semente + 10f, t * 2.5f);
        float ruido = (lento * 0.6f + rapido * 0.4f) * 2f - 1f;   // -1..1

        // Rajada ocasional: a chama quase apaga e volta
        if (usarRajadas)
        {
            if (Random.value < chanceRajadaPorSegundo * Time.deltaTime) rajada = 1f;
            rajada = Mathf.MoveTowards(rajada, 0f, Time.deltaTime * 2.5f);
            ruido -= rajada * 0.5f * forcaRajada;
        }

        luz.intensity = Mathf.Max(0f, intensidadeBase + ruido * variacaoIntensidade);

        if (animarRaio)
            luz.pointLightOuterRadius = raioBase * (1f + ruido * variacaoRaio);

        // Balanço lateral da luz
        float bx = (Mathf.PerlinNoise(semente + 20f, t) - 0.5f) * 2f;
        float by = (Mathf.PerlinNoise(semente + 30f, t) - 0.5f) * 2f;
        transform.localPosition = posBase + new Vector3(bx, by * 0.5f, 0f) * balancoPosicao * (1f + rajada);

        // Sprite da chama (se houver): inclina e estica conforme o ruído
        if (chamaVisual != null)
        {
            chamaVisual.localRotation = Quaternion.Euler(0f, 0f, bx * balancoRotacaoChama * (1f + rajada));
            float esticar = 1f + ruido * 0.12f;
            chamaVisual.localScale = new Vector3(
                escalaChamaBase.x * (1f - ruido * 0.06f),
                escalaChamaBase.y * esticar,
                escalaChamaBase.z);
        }
    }
}