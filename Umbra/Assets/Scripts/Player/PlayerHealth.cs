using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 5f;

    [Header("Reinício da fase")]
    public float restartDelay = 1f;

    private float currentHealth;
    private bool isDead;

    private void Awake()
    {
        // Começa com a vida máxima.
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        // Impede receber dano após a morte.
        if (isDead)
            return;

        // Reduz a vida e impede valores negativos.
        currentHealth = Mathf.Max(0f, currentHealth - damage);

        Debug.Log("Vida do jogador: " + currentHealth + "/" + maxHealth);

        // Inicia a morte quando a vida chega a zero.
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        // Garante que a rotina de morte só aconteça uma vez.
        if (isDead)
            return;

        isDead = true;

        // Interrompe a movimentação.
        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // Desativa os controles do jogador.
        PlayerController controller = GetComponent<PlayerController>();

        if (controller != null)
            controller.enabled = false;

        PlayerAttack attack = GetComponent<PlayerAttack>();

        if (attack != null)
            attack.enabled = false;

        // Aguarda um instante antes de reiniciar a fase.
        StartCoroutine(RestartCurrentScene());
    }

    private IEnumerator RestartCurrentScene()
    {
        yield return new WaitForSeconds(restartDelay);

        // Recarrega a cena que está sendo jogada atualmente.
        Scene currentScene = SceneManager.GetActiveScene();

        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
