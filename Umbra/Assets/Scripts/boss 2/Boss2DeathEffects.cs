
using System.Collections;
using UnityEngine;

public class Boss2DeathEffect : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private BossHealth health;
    [SerializeField] private Transform corpo;

    [Header("Efeito de Morte")]
    [SerializeField] private float shakeDuration = 2f;
    [SerializeField] private float shakeIntensity = 0.15f;

    private bool isDying = false;
    private Vector3 originalPosition;

    private void Awake()
    {
        if (health == null)
            health = GetComponent<BossHealth>();
    }

    private void OnEnable()
    {
        if (health != null)
            health.OnDied += StartDeathEffect;
    }

    private void OnDisable()
    {
        if (health != null)
            health.OnDied -= StartDeathEffect;
    }

    private void StartDeathEffect()
    {
        if (isDying || corpo == null)
            return;

        isDying = true;
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        originalPosition = corpo.localPosition;

        // Chacoalha o corpo durante a morte.
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;

            Vector2 offset =
                Random.insideUnitCircle * shakeIntensity;

            corpo.localPosition =
                originalPosition +
                new Vector3(offset.x, offset.y, 0f);

            yield return null;
        }

        corpo.localPosition = originalPosition;

        // Desativa o Boss 2 inteiro.
        gameObject.SetActive(false);
    }
}
