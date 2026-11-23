using UnityEngine;

public class SceneAudio : MonoBehaviour
{
    [Header("Áudios da fase")]
    public AudioClip music;
    public AudioClip ambience;

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager não encontrado!");
            return;
        }

        if (music != null)
            AudioManager.Instance.PlayMusic(music);
        else
            AudioManager.Instance.StopMusic();

        if (ambience != null)
            AudioManager.Instance.PlayAmbience(ambience);
        else
            AudioManager.Instance.StopAmbience();
    }
}