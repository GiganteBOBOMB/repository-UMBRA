using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource movementSource;

    [Header("Efeitos sonoros")]
    public AudioClip detectionSound;
    public AudioClip movementSound;
    public AudioClip attackSound;
    public AudioClip stunSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    [Header("Volumes individuais")]

    [Range(0f, 1f)]
    public float detectionVolume = 1f;

    [Range(0f, 1f)]
    public float movementVolume = 0.4f;

    [Range(0f, 1f)]
    public float attackVolume = 1f;

    [Range(0f, 1f)]
    public float stunVolume = 1f;

    [Range(0f, 1f)]
    public float hurtVolume = 1f;

    [Range(0f, 1f)]
    public float deathVolume = 1f;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayDetection()
    {
        PlaySound(detectionSound, detectionVolume);
    }

    public void StartMovement()
    {
        if (movementSource == null ||
            movementSound == null)
            return;

        if (movementSource.isPlaying)
            return;

        movementSource.clip = movementSound;
        movementSource.volume = movementVolume;
        movementSource.loop = true;
        movementSource.Play();
    }

    public void StopMovement()
    {
        if (movementSource != null &&
            movementSource.isPlaying)
        {
            movementSource.Stop();
        }
    }

    public void PlayAttack()
    {
        StopMovement();

        PlaySound(attackSound, attackVolume);
    }

    public void PlayStun()
    {
        StopMovement();

        PlaySound(stunSound, stunVolume);
    }

    public void PlayHurt()
    {
        PlaySound(hurtSound, hurtVolume);
    }

    public void PlayDeath()
    {
        StopMovement();

        if (deathSound == null || audioSource == null)
            return;

        // Cria um objeto independente do inimigo.
        GameObject deathAudioObject =
            new GameObject("EnemyDeathAudio");

        deathAudioObject.transform.position =
            transform.position;

        AudioSource deathSource =
            deathAudioObject.AddComponent<AudioSource>();

        // Mantém as configurações do Audio Source original.
        deathSource.outputAudioMixerGroup =
            audioSource.outputAudioMixerGroup;

        deathSource.spatialBlend =
            audioSource.spatialBlend;

        deathSource.minDistance =
            audioSource.minDistance;

        deathSource.maxDistance =
            audioSource.maxDistance;

        deathSource.rolloffMode =
            audioSource.rolloffMode;

        deathSource.volume =
            audioSource.volume * deathVolume;

        deathSource.pitch = audioSource.pitch;

        deathSource.playOnAwake = false;
        deathSource.loop = false;
        deathSource.clip = deathSound;

        deathSource.Play();

        // Remove o objeto depois que o áudio terminar.
        float duration = deathSound.length /
            Mathf.Max(0.01f, Mathf.Abs(deathSource.pitch));

        Destroy(deathAudioObject, duration + 0.1f);
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    private void OnDisable()
    {
        StopMovement();

        if (audioSource != null)
            audioSource.Stop();
    }
}