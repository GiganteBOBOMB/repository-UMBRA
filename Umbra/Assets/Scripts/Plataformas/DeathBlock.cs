using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathBlock : MonoBehaviour
{
    private bool playerMorreu = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playerMorreu)
            return;

        if (other.CompareTag("Player"))
        {
            playerMorreu = true;

            // Reinicia a fase atual
            SceneManager.LoadScene(
                SceneManager.GetActiveScene().buildIndex
            );
        }
    }
}
