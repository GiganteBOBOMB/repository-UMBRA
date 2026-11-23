
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BossBattleManager : MonoBehaviour
{
    [Header("Boss 1")]
    [SerializeField] private BossHealth boss1Health;
    [SerializeField] private GameObject boss1;

    [Header("Arma do Boss 1")]
    [SerializeField] private GameObject boss1Weapon;

    [Header("Boss 2")]
    [SerializeField] private GameObject boss2;
    [SerializeField] private BossHealth boss2Health;
    [SerializeField] private BossRhythmController boss2Rhythm;

    [Header("Transição")]
    [SerializeField] private float boss1DeathDuration = 2f;
    [SerializeField] private float transitionDelay = 1f;

    [Header("Músicas")]
    [SerializeField] private AudioClip boss1Music;
    [SerializeField] private AudioClip boss2Music;

    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.7f;

    [SerializeField] private float fadeDuration = 1f;

    private AudioSource musicSource;
    private Coroutine musicFade;
    private bool transitioning;

    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;

        if (boss2 != null)
            boss2.SetActive(false);
    }

    private void Start()
    {
        if (boss1Music != null)
        {
            musicSource.clip = boss1Music;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    private void OnEnable()
    {
        if (boss1Health != null)
            boss1Health.OnDied += HandleBoss1Death;

        if (boss2Health != null)
            boss2Health.OnDied += HandleBoss2Death;
    }

    private void OnDisable()
    {
        if (boss1Health != null)
            boss1Health.OnDied -= HandleBoss1Death;

        if (boss2Health != null)
            boss2Health.OnDied -= HandleBoss2Death;
    }

    private void HandleBoss1Death()
    {
        if (transitioning)
            return;

        transitioning = true;
        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // Inicia a troca de música quando Boss 1 morre.
        StartMusicFade(boss2Music);

        // Aguarda a animação de morte do Boss 1.
        yield return new WaitForSeconds(boss1DeathDuration);

        if (boss1 != null)
            boss1.SetActive(false);

        if (boss1Weapon != null)
            boss1Weapon.SetActive(false);

        // Pausa antes da segunda batalha.
        yield return new WaitForSeconds(transitionDelay);

        if (boss2 != null)
            boss2.SetActive(true);

        if (boss2Rhythm != null)
            boss2Rhythm.Begin();

        Debug.Log("[BossBattleManager] Boss 2 iniciou!");
    }

    private void HandleBoss2Death()
    {
        // Para a música quando Boss 2 morrer.
        StartMusicFade(null);
    }

    private void StartMusicFade(AudioClip nextClip)
    {
        if (musicFade != null)
            StopCoroutine(musicFade);

        musicFade = StartCoroutine(FadeMusic(nextClip));
    }

    private IEnumerator FadeMusic(AudioClip nextClip)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, fadeDuration);

        // Diminui o volume da música atual.
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            musicSource.volume = Mathf.Lerp(
                startVolume,
                0f,
                Mathf.Clamp01(elapsed / duration)
            );

            yield return null;
        }

        musicSource.Stop();

        // Sem próxima música: apenas encerra.
        if (nextClip == null)
        {
            musicFade = null;
            yield break;
        }

        // Troca a música.
        musicSource.clip = nextClip;
        musicSource.volume = 0f;
        musicSource.Play();

        // Aumenta gradualmente o volume.
        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            musicSource.volume = Mathf.Lerp(
                0f,
                musicVolume,
                Mathf.Clamp01(elapsed / duration)
            );

            yield return null;
        }

        musicSource.volume = musicVolume;
        musicFade = null;
    }
}
