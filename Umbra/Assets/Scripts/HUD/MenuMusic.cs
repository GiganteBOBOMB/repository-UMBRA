
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuMusic : MonoBehaviour
{
    [Header("Música")]
    [SerializeField] private AudioClip music;

    [Header("Configurações")]
    [Range(0f, 1f)]
    [SerializeField] private float volume = 0.7f;

    [SerializeField] private bool loop = true;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = loop;
        audioSource.spatialBlend = 0f;
        audioSource.volume = volume;
    }

    private void Start()
    {
        if (music == null)
        {
            Debug.LogWarning(
                "[MenuMusic] Nenhuma música configurada!"
            );
            return;
        }

        audioSource.clip = music;
        audioSource.Play();
    }
}
