using UnityEngine;

// Faz o objeto subir e descer suavemente (efeito de flutuar).
// Coloque no objeto VISUAL (o que tem o sprite), nunca no que tem o BossHand.
public class FloatBob : MonoBehaviour
{
    [Header("Subir e descer")]
    [Tooltip("Distância do movimento para cima e para baixo.")]
    [SerializeField] private float amplitude = 0.25f;
    [Tooltip("Velocidade da oscilação. Maior = balança mais rápido.")]
    [SerializeField] private float speed = 1.5f;

    [Header("Balanço lateral (opcional)")]
    [SerializeField] private float swayAmplitude = 0.08f;
    [SerializeField] private float swaySpeed = 0.9f;

    [Header("Variação")]
    [Tooltip("Deslocamento no tempo. Use valores diferentes em cada objeto para não balançarem juntos.")]
    [SerializeField] private float phaseOffset = 0f;
    [Tooltip("Ligado: sorteia o deslocamento ao iniciar (ignora o campo acima).")]
    [SerializeField] private bool randomPhase = false;

    private Vector3 basePosition;

    private void Awake()
    {
        basePosition = transform.localPosition;
        if (randomPhase) phaseOffset = Random.Range(0f, 10f);
    }

    private void Update()
    {
        float t = Time.time + phaseOffset;

        float y = Mathf.Sin(t * speed) * amplitude;
        float x = Mathf.Sin(t * swaySpeed) * swayAmplitude;

        transform.localPosition = basePosition + new Vector3(x, y, 0f);
    }
}