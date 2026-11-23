using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BossAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource movementSource;

    [Header("Efeitos sonoros")]
    public AudioClip entranceSound;
    public AudioClip movementSound;
    public AudioClip[] attackSounds;
    public AudioClip impactSound;
    public AudioClip stunSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    [Header("Volumes individuais")]

    [Range(0f, 1f)]
    public float entranceVolume = 1f;

    [Range(0f, 1f)]
    public float movementVolume = 0.4f;

    [Range(0f, 1f)]
    public float attackVolume = 1f;

    [Range(0f, 1f)]
    public float impactVolume = 1f;

    [Range(0f, 1f)]
    public float stunVolume = 1f;

    [Range(0f, 1f)]
    public float hurtVolume = 1f;

    [Range(0f, 1f)]
    public float deathVolume = 1f;

    [Header("Volume de cada ataque")]
    [Range(0f, 1f)]
    public float[] individualAttackVolumes;

    private bool deathPlayed;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayEntrance()
    {
        PlaySound(entranceSound, entranceVolume);
    }

    public void StartMovement()
    {
        if (movementSource == null ||
            movementSound == null ||
            deathPlayed)
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
        if (movementSource != null)
            movementSource.Stop();
    }

    public void PlayAttack(int attackIndex)
    {
        if (attackSounds == null)
            return;

        if (attackIndex < 0 ||
            attackIndex >= attackSounds.Length)
            return;

        float volume = attackVolume;

        if (individualAttackVolumes != null &&
            attackIndex < individualAttackVolumes.Length)
        {
            volume *= individualAttackVolumes[attackIndex];
        }

        PlaySound(attackSounds[attackIndex], volume);
    }

    public void PlayImpact()
    {
        PlaySound(impactSound, impactVolume);
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
        if (deathPlayed)
            return;

        deathPlayed = true;

        StopMovement();

        if (audioSource == null)
            return;

        audioSource.Stop();

        if (deathSound == null)
            return;

        GameObject deathAudioObject =
            new GameObject("BossDeathAudio");

        deathAudioObject.transform.position =
            transform.position;

        AudioSource deathSource =
            deathAudioObject.AddComponent<AudioSource>();

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

        float duration = deathSound.length /
            Mathf.Max(0.01f, Mathf.Abs(deathSource.pitch));

        Destroy(deathAudioObject, duration + 0.1f);
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (deathPlayed)
            return;

        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(
                clip,
                Mathf.Clamp01(volume)
            );
        }
    }

    private void OnDisable()
    {
        StopMovement();

        if (audioSource != null)
            audioSource.Stop();
    }
}