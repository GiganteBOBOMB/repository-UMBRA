using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomAmbience : MonoBehaviour
{
    [Header("Referências")]
    public AudioSource audioSource;

    [Header("Sons aleatórios")]
    public AudioClip[] sounds;

    [Header("Intervalo entre os sons")]
    public float minInterval = 3f;
    public float maxInterval = 10f;

    [Header("Variação de volume")]
    public float minVolume = 0.4f;
    public float maxVolume = 0.8f;

    [Header("Variação de pitch")]
    public float minPitch = 0.9f;
    public float maxPitch = 1.1f;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        if (sounds != null && sounds.Length > 0)
            StartCoroutine(PlayRandomSounds());
    }

    private IEnumerator PlayRandomSounds()
    {
        while (true)
        {
            // Espera um tempo aleatório.
            float interval = Random.Range(
                minInterval, maxInterval
            );

            yield return new WaitForSeconds(interval);

            // Escolhe um som aleatório.
            AudioClip selectedSound =
                sounds[Random.Range(0, sounds.Length)];

            if (selectedSound == null)
                continue;

            // Varia o volume.
            audioSource.volume = Random.Range(
                minVolume, maxVolume
            );

            // Varia levemente a tonalidade.
            audioSource.pitch = Random.Range(
                minPitch, maxPitch
            );

            // Reproduz o som.
            audioSource.clip = selectedSound;
            audioSource.Play();

            // Espera o som terminar.
            yield return new WaitWhile(
                () => audioSource.isPlaying
            );
        }
    }
}