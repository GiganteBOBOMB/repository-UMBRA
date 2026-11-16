using UnityEngine;

// Treme a câmera. Coloque na Main Camera, que deve ser FILHA de um objeto "CameraRig".
// O script mexe só na posição LOCAL da câmera, então o rig pode seguir o player normalmente.
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Tooltip("Limite de segurança: nenhum tremor passa deste valor, mesmo que vários se somem.")]
    [SerializeField] private float maxIntensity = 3f;

    private Vector3 originLocalPosition;
    private float intensity;
    private float duration;
    private float timeLeft;

    private void Awake()
    {
        Instance = this;
        originLocalPosition = transform.localPosition;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// intensity: distância máxima do tremor (em unidades do mundo). duration: segundos.
    public void Shake(float intensity, float duration)
    {
        intensity = Mathf.Min(intensity, maxIntensity);

        // Se já está tremendo mais forte, não deixa um tremor fraco sobrescrever
        float currentStrength = this.duration > 0f ? this.intensity * (timeLeft / this.duration) : 0f;
        if (timeLeft > 0f && intensity < currentStrength) return;

        this.intensity = intensity;
        this.duration = Mathf.Max(0.01f, duration);
        timeLeft = this.duration;
    }

    // LateUpdate: roda depois de todos os Update, para o tremor ser a última coisa aplicada
    private void LateUpdate()
    {
        if (timeLeft <= 0f)
        {
            transform.localPosition = originLocalPosition;
            return;
        }

        timeLeft -= Time.deltaTime;

        // O tremor enfraquece até zero no fim
        float fade = Mathf.Clamp01(timeLeft / duration);
        Vector2 offset = Random.insideUnitCircle * intensity * fade;

        transform.localPosition = originLocalPosition + new Vector3(offset.x, offset.y, 0f);
    }
}