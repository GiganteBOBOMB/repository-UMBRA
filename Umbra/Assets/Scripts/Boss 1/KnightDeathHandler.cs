using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public enum WeaponDeathMode
{
    Drop,        // a arma cai no chão e some junto com o boss
    Disappear,   // a arma some na hora
    Stay         // a arma fica onde está
}

// Reage à morte do Knight: para o cérebro, deixa o corpo inofensivo, trata a arma,
// faz o boss sumir devagar e avisa o resto do jogo (porta, tela de vitória...).
// Vai no objeto raiz do boss.
public class KnightDeathHandler : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private BossHealth health;
    [SerializeField] private KnightBrain brain;
    [SerializeField] private FloatingWeapon weapon;
    [Tooltip("Usada para achar o chão quando a arma cai.")]
    [SerializeField] private BossArena arena;
    [Tooltip("Opcional: Animator do boss, para tocar uma animação de morte.")]
    [SerializeField] private Animator animator;

    [Header("Morte")]
    [Tooltip("Nome do Trigger de morte no Animator. Deixe vazio se não houver animação.")]
    [SerializeField] private string deathTrigger = "Morrer";
    [Tooltip("Tempo antes de o boss começar a sumir (deixa a animação de morte tocar).")]
    [SerializeField, Min(0f)] private float fadeDelay = 0.8f;
    [Tooltip("Tempo para o boss sumir. 0 = não some.")]
    [SerializeField, Min(0f)] private float fadeTime = 1.5f;
    [Tooltip("Ligado: depois de sumir, o boss e a arma são destruídos.")]
    [SerializeField] private bool destroyAfterFade = true;

    [Header("Arma")]
    [SerializeField] private WeaponDeathMode weaponMode = WeaponDeathMode.Drop;
    [SerializeField, Min(0.01f)] private float weaponDropTime = 0.6f;
    [Tooltip("Altura do centro da arma acima do chão quando ela cai.")]
    [SerializeField] private float weaponGroundOffset = 0.4f;

    [Header("Tremor da câmera")]
    [Tooltip("0 desliga o tremor.")]
    [SerializeField] private float shakeIntensity = 0.5f;
    [SerializeField] private float shakeDuration = 0.8f;

    [Header("Ao morrer")]
    [Tooltip("Opcional: objeto desativado que é ativado na morte (explosão, tela de vitória...).")]
    [SerializeField] private GameObject activateOnDeath;
    [Tooltip("Eventos extras, configurados no Inspector (abrir uma porta, tocar um som, liberar a saída da fase...).")]
    [SerializeField] private UnityEvent onDeath;

    private void Awake()
    {
        if (health == null) health = GetComponent<BossHealth>();
        if (brain == null) brain = GetComponent<KnightBrain>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (health != null) health.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        if (health != null) health.OnDied -= HandleDied;
    }

    private void HandleDied()
    {
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        // Espera um frame: o KnightBrain também reage à morte (ele para e devolve a arma ao boss).
        // Só depois disso nós assumimos a arma, para um não desfazer o trabalho do outro.
        yield return null;

        if (brain != null) brain.Stop();

        // O corpo e a arma ficam inofensivos
        foreach (Collider2D c in GetComponentsInChildren<Collider2D>())
            c.enabled = false;
        if (weapon != null) weapon.SetHitbox(false);

        if (animator != null && !string.IsNullOrEmpty(deathTrigger))
            animator.SetTrigger(deathTrigger);

        if (shakeIntensity > 0f && CameraShake.Instance != null)
            CameraShake.Instance.Shake(shakeIntensity, shakeDuration);

        if (activateOnDeath != null) activateOnDeath.SetActive(true);
        onDeath?.Invoke();

        // A arma
        if (weapon != null)
        {
            switch (weaponMode)
            {
                case WeaponDeathMode.Drop:
                    Vector3 p = weapon.transform.position;
                    float groundY = arena != null ? arena.GroundY + weaponGroundOffset : p.y;
                    yield return weapon.MoveTo(new Vector3(p.x, groundY, p.z), weaponDropTime);
                    break;

                case WeaponDeathMode.Disappear:
                    weapon.gameObject.SetActive(false);
                    break;

                case WeaponDeathMode.Stay:
                    weapon.Release(); // para de seguir o corpo
                    break;
            }
        }

        // Some devagar
        if (fadeDelay > 0f) yield return new WaitForSeconds(fadeDelay);
        if (fadeTime > 0f) yield return FadeOut(fadeTime);

        if (destroyAfterFade)
        {
            if (weapon != null) Destroy(weapon.gameObject);
            Destroy(gameObject);
        }
    }

    private IEnumerator FadeOut(float time)
    {
        // Boss e arma (se ainda estiver ativa)
        var renderers = new System.Collections.Generic.List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>());
        if (weapon != null && weaponMode != WeaponDeathMode.Disappear)
            renderers.AddRange(weapon.GetComponentsInChildren<SpriteRenderer>());

        Color[] original = new Color[renderers.Count];
        for (int i = 0; i < renderers.Count; i++)
            original[i] = renderers[i].color;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / time;
            float alpha = 1f - Mathf.Clamp01(t);

            for (int i = 0; i < renderers.Count; i++)
            {
                if (renderers[i] == null) continue;
                Color c = original[i];
                c.a = original[i].a * alpha;
                renderers[i].color = c;
            }

            yield return null;
        }
    }
}