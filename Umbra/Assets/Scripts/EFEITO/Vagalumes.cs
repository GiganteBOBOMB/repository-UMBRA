using UnityEngine;

/// <summary>
/// Efeito de vagalumes 2D com profundidade (parallax), tamanhos variados e brilho pulsante.
/// Coloque em um objeto vazio no cenário. Não precisa de sprite: ele gera um brilho redondo e suave sozinho.
/// Funciona em qualquer pipeline (Built-in ou URP 2D) porque usa só SpriteRenderer.
/// </summary>
public class Vagalumes : MonoBehaviour
{
    [Header("Quantidade e área")]
    [SerializeField] private int quantidade = 40;
    [SerializeField] private Vector2 tamanhoDaArea = new Vector2(40f, 12f); // largura x altura, centrada neste objeto

    [Header("Profundidade (0 = bem no fundo, 1 = bem na frente)")]
    [SerializeField] private Transform cameraAlvo;                // se vazio, usa Camera.main
    [SerializeField, Range(0f, 1f)] private float parallaxFundo = 0.7f;   // quanto os do fundo acompanham a câmera (parece mais longe)
    [SerializeField] private string sortingLayer = "Default";
    [SerializeField] private int ordemFundo = -10;                // vagalumes distantes (atrás do cenário)
    [SerializeField] private int ordemFrente = 10;                // vagalumes próximos (na frente do cenário)

    [Header("Aparência")]
    [SerializeField] private Color corDoBrilho = new Color(1f, 0.95f, 0.5f, 1f);
    [SerializeField] private Vector2 tamanhoMinMax = new Vector2(0.15f, 0.6f);   // fundo = pequeno, frente = grande
    [SerializeField] private Vector2 brilhoMinMax = new Vector2(0.25f, 0.9f);    // fundo = fraco, frente = forte
    [SerializeField] private Vector2 velocidadePulsoMinMax = new Vector2(0.5f, 1.8f);
    [Tooltip("Opcional: material aditivo (Sprites/Default com Additive, ou Particles/Additive) deixa o brilho mais forte.")]
    [SerializeField] private Material materialOpcional;

    [Header("Movimento")]
    [SerializeField] private Vector2 velocidadeVagarMinMax = new Vector2(0.2f, 0.6f);
    [SerializeField] private float raioDeVagar = 1.5f;            // o quanto cada um flutua em volta do seu ponto

    private class Vagalume
    {
        public Transform t;
        public SpriteRenderer sr;
        public Vector2 casa;           // ponto base no mundo
        public float profundidade;     // 0 fundo .. 1 frente
        public float parallax;         // fator de acompanhamento da câmera
        public float tamanho;
        public float brilho;
        public float velPulso;
        public float velVagar;
        public float semente;          // deslocamento do ruído para cada um ser diferente
    }

    private Vagalume[] lista;
    private Sprite spriteBrilho;
    private Vector3 posInicialCamera;

    private void Start()
    {
        if (cameraAlvo == null && Camera.main != null) cameraAlvo = Camera.main.transform;
        if (cameraAlvo != null) posInicialCamera = cameraAlvo.position;

        spriteBrilho = CriarSpriteBrilho(64);
        lista = new Vagalume[quantidade];

        for (int i = 0; i < quantidade; i++)
        {
            float p = Random.value;              // profundidade sorteada
            var v = new Vagalume
            {
                profundidade = p,
                parallax = Mathf.Lerp(parallaxFundo, 0f, p),
                tamanho = Mathf.Lerp(tamanhoMinMax.x, tamanhoMinMax.y, p) * Random.Range(0.8f, 1.2f),
                brilho = Mathf.Lerp(brilhoMinMax.x, brilhoMinMax.y, p),
                velPulso = Random.Range(velocidadePulsoMinMax.x, velocidadePulsoMinMax.y),
                velVagar = Random.Range(velocidadeVagarMinMax.x, velocidadeVagarMinMax.y),
                semente = Random.Range(0f, 1000f),
                casa = (Vector2)transform.position + new Vector2(
                    Random.Range(-tamanhoDaArea.x / 2f, tamanhoDaArea.x / 2f),
                    Random.Range(-tamanhoDaArea.y / 2f, tamanhoDaArea.y / 2f))
            };

            var go = new GameObject("Vagalume");
            go.transform.SetParent(transform);
            v.t = go.transform;
            v.sr = go.AddComponent<SpriteRenderer>();
            v.sr.sprite = spriteBrilho;
            v.sr.sortingLayerName = sortingLayer;
            v.sr.sortingOrder = Mathf.RoundToInt(Mathf.Lerp(ordemFundo, ordemFrente, p));
            if (materialOpcional != null) v.sr.material = materialOpcional;
            v.t.localScale = Vector3.one * v.tamanho;

            lista[i] = v;
        }
    }

    private void Update()
    {
        if (lista == null) return;

        Vector3 deltaCamera = cameraAlvo != null ? cameraAlvo.position - posInicialCamera : Vector3.zero;
        float t = Time.time;

        foreach (var v in lista)
        {
            // flutuação suave com ruído de Perlin
            float nx = Mathf.PerlinNoise(v.semente, t * v.velVagar) - 0.5f;
            float ny = Mathf.PerlinNoise(v.semente + 50f, t * v.velVagar) - 0.5f;
            Vector2 vagar = new Vector2(nx, ny) * 2f * raioDeVagar;

            // parallax: os do fundo acompanham a câmera e parecem mais distantes
            Vector2 desloc = (Vector2)deltaCamera * v.parallax;
            v.t.position = new Vector3(v.casa.x + vagar.x + desloc.x, v.casa.y + vagar.y + desloc.y, 0f);

            // pulso do brilho (nunca apaga totalmente, só "respira")
            float pulso = 0.5f + 0.5f * Mathf.Sin(t * v.velPulso + v.semente);
            float alfa = v.brilho * Mathf.Lerp(0.15f, 1f, pulso);
            Color c = corDoBrilho;
            c.a = alfa;
            v.sr.color = c;
        }
    }

    // Cria uma textura circular com borda suave (centro brilhante que some para fora)
    private Sprite CriarSpriteBrilho(int tam)
    {
        var tex = new Texture2D(tam, tam, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        Vector2 centro = new Vector2(tam / 2f, tam / 2f);
        for (int y = 0; y < tam; y++)
        {
            for (int x = 0; x < tam; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), centro) / (tam / 2f);
                float a = Mathf.Clamp01(1f - d);
                a = a * a * a;                 // queda suave
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, tam, tam), new Vector2(0.5f, 0.5f), tam);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.9f, 0.3f, 0.6f);
        Gizmos.DrawWireCube(transform.position, tamanhoDaArea);
    }
}