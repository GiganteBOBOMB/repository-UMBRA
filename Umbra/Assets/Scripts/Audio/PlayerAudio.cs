using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class PlayerAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource footstepsSource;

    [Header("Movimentação")]
    public AudioClip footstepsSound;
    public AudioClip jumpSound;
    public AudioClip landingSound;
    public AudioClip dashSound;

    [Header("Combate")]
    public AudioClip attackSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;

    [Header("Volumes individuais")]

    [Range(0f, 1f)]
    public float jumpVolume = 1f;

    [Range(0f, 1f)]
    public float landingVolume = 0.3f;

    [Range(0f, 1f)]
    public float dashVolume = 1f;

    [Range(0f, 1f)]
    public float attackVolume = 1f;

    [Range(0f, 1f)]
    public float hurtVolume = 1f;

    [Range(0f, 1f)]
    public float deathVolume = 1f;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void StartFootsteps()
    {
        if (footstepsSource == null ||
            footstepsSound == null)
            return;

        if (footstepsSource.isPlaying)
            return;

        footstepsSource.clip = footstepsSound;
        footstepsSource.loop = true;
        footstepsSource.Play();
    }

    public void StopFootsteps()
    {
        if (footstepsSource != null &&
            footstepsSource.isPlaying)
        {
            footstepsSource.Stop();
        }
    }

    public void PlayJump()
    {
        StopFootsteps();
        PlaySound(jumpSound, jumpVolume);
    }

    public void PlayLanding()
    {
        PlaySound(landingSound, landingVolume);
    }

    public void PlayDash()
    {
        StopFootsteps();
        PlaySound(dashSound, dashVolume);
    }

    public void PlayAttack()
    {
        PlaySound(attackSound, attackVolume);
    }

    public void PlayHurt()
    {
        PlaySound(hurtSound, hurtVolume);
    }

    public void PlayDeath()
    {
        StopFootsteps();
        PlaySound(deathSound, deathVolume);
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
        StopFootsteps();
    }
}