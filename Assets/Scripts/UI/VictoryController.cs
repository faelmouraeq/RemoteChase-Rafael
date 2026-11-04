using UnityEngine;
using UnityEngine.SceneManagement;

namespace RemoteChase.Game
{
    [AddComponentMenu("Remote Chase/Victory Controller")]
    public class VictoryController : MonoBehaviour
    {
        [Header("Configuração de Cenas")]
        [Tooltip("Nome exato da cena de créditos do jogo.")]
        public string creditsSceneName = "CreditsScene";

        [Tooltip("Nome exato da cena do Menu Principal.")]
        public string menuSceneName = "MenuScene";

        // Método chamado pelo botão "Ver Créditos"
        public void GoToCredits()
        {
            Debug.Log("Indo para a tela de créditos...");
            SceneManager.LoadScene(creditsSceneName);
        }

        // Método chamado pelo botão "Menu Principal"
        public void GoToMenu()
        {
            Debug.Log("Voltando ao Menu Principal...");
            SceneManager.LoadScene(menuSceneName);
        }
    }
}
