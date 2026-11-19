using UnityEngine;
using UnityEngine.SceneManagement;  

public class Menu : MonoBehaviour
{
    [SerializeField] private string nomeDoLevelDeJogo;

    public void jogar()
    {
        SceneManager.LoadScene(nomeDoLevelDeJogo);
    }

    public void sair()
    {
        Debug.Log("Sair do jogo");
        Application.Quit();
    }
}
