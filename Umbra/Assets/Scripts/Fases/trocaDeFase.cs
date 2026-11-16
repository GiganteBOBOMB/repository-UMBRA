
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [Header("Próxima fase")]
    [SerializeField] private string nextSceneName;

    private bool changingScene = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (changingScene)
            return;

        if (other.CompareTag("Player"))
        {
            changingScene = true;

            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogError(
                    "LevelExit: defina o nome da próxima cena no Inspector!"
                );

                changingScene = false;
            }
        }
    }
}
