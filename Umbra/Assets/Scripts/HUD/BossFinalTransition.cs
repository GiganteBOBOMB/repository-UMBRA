
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BossFinalTransition : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private BossHealth boss2Health;
    [SerializeField] private Image fadeImage;

    [Header("Cena Final")]
    [SerializeField] private string finalSceneName = "Agradecimento";

    [Header("Tempos")]
    [SerializeField] private float deathAnimationDuration = 2f;
    [SerializeField] private float fadeDuration = 1.5f;

    private bool transitioning = false;

    private void Awake()
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;

            fadeImage.raycastTarget = false;
        }
    }

    private void OnEnable()
    {
        if (boss2Health != null)
            boss2Health.OnDied += StartFinalTransition;
    }

    private void OnDisable()
    {
        if (boss2Health != null)
            boss2Health.OnDied -= StartFinalTransition;
    }

    private void StartFinalTransition()
    {
        if (transitioning)
            return;

        transitioning = true;
        StartCoroutine(FinalTransitionRoutine());
    }

    private IEnumerator FinalTransitionRoutine()
    {
        // Espera o Boss 2 terminar sua animação de morte.
        yield return new WaitForSeconds(
            deathAnimationDuration
        );

        // Escurece a tela gradualmente.
        if (fadeImage != null)
        {
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;

                Color color = fadeImage.color;

                color.a = Mathf.Clamp01(
                    elapsed / Mathf.Max(0.01f, fadeDuration)
                );

                fadeImage.color = color;

                yield return null;
            }
        }

        // Carrega a cena de agradecimento.
        SceneManager.LoadScene(finalSceneName);
    }
}
