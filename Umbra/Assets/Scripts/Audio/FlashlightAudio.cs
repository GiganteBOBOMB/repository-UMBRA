using UnityEngine;

public class FlashlightAudio : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource effectsSource;
    public AudioSource focusLoopSource;
    public AudioSource rechargeLoopSource;

    [Header("Efeitos sonoros")]
    public AudioClip focusStartSound;
    public AudioClip focusLoopSound;
    public AudioClip focusEndSound;
    public AudioClip focusEmptySound;
    public AudioClip focusRecoveredSound;
    public AudioClip focusRechargeSound;

    [Header("Volumes individuais")]
    [Range(0f, 1f)]
    public float focusStartVolume = 1f;

    [Range(0f, 1f)]
    public float focusLoopVolume = 0.4f;

    [Range(0f, 1f)]
    public float focusEndVolume = 1f;

    [Range(0f, 1f)]
    public float focusEmptyVolume = 1f;

    [Range(0f, 1f)]
    public float focusRecoveredVolume = 1f;

    [Range(0f, 1f)]
    public float focusRechargeVolume = 0.4f;

    private bool isFocusing;
    private bool isRecharging;

    public void StartFocus()
    {
        if (isFocusing)
            return;

        StopRecharge();

        isFocusing = true;

        PlayEffect(focusStartSound, focusStartVolume);

        if (focusLoopSound != null &&
            focusLoopSource != null)
        {
            focusLoopSource.clip = focusLoopSound;
            focusLoopSource.volume = focusLoopVolume;
            focusLoopSource.loop = true;
            focusLoopSource.Play();
        }
    }

    public void StopFocus()
    {
        if (!isFocusing)
            return;

        isFocusing = false;

        if (focusLoopSource != null)
            focusLoopSource.Stop();

        PlayEffect(focusEndSound, focusEndVolume);
    }

    public void PlayFocusEmpty()
    {
        isFocusing = false;

        if (focusLoopSource != null)
            focusLoopSource.Stop();

        PlayEffect(focusEmptySound, focusEmptyVolume);
    }

    public void StartRecharge()
    {
        if (isRecharging || isFocusing)
            return;

        isRecharging = true;

        if (focusRechargeSound != null &&
            rechargeLoopSource != null)
        {
            rechargeLoopSource.clip =
                focusRechargeSound;

            rechargeLoopSource.volume =
                focusRechargeVolume;

            rechargeLoopSource.loop = true;
            rechargeLoopSource.Play();
        }
    }

    public void StopRecharge()
    {
        isRecharging = false;

        if (rechargeLoopSource != null)
            rechargeLoopSource.Stop();
    }

    public void PlayFocusRecovered()
    {
        StopRecharge();

        PlayEffect(
            focusRecoveredSound,
            focusRecoveredVolume
        );
    }

    private void PlayEffect(AudioClip clip, float volume)
    {
        if (clip != null && effectsSource != null)
        {
            effectsSource.PlayOneShot(clip, volume);
        }
    }

    public void StopAllSounds()
    {
        isFocusing = false;
        isRecharging = false;

        if (focusLoopSource != null)
            focusLoopSource.Stop();

        if (rechargeLoopSource != null)
            rechargeLoopSource.Stop();

        if (effectsSource != null)
            effectsSource.Stop();
    }

    private void OnDisable()
    {
        StopAllSounds();
    }
}