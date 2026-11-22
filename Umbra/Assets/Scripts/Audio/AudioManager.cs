using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource ambienceSource;
    public AudioSource sfxSource;
    public AudioSource uiSource;

    private void Awake()
    {
        // Impede a duplicação do AudioManager.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Mantém o AudioManager entre as fases.
        DontDestroyOnLoad(gameObject);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null)
            return;

        if (musicSource.clip == clip && musicSource.isPlaying)
            return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlayAmbience(AudioClip clip)
    {
        if (clip == null)
            return;

        if (ambienceSource.clip == clip && ambienceSource.isPlaying)
            return;

        ambienceSource.clip = clip;
        ambienceSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip);
    }

    public void PlayUI(AudioClip clip)
    {
        if (clip != null)
            uiSource.PlayOneShot(clip);
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void StopAmbience()
    {
        ambienceSource.Stop();
    }
}