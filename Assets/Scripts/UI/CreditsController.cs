using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace RemoteChase.Game
{
    [AddComponentMenu("Remote Chase/Credits Controller")]
    public class CreditsController : MonoBehaviour
    {
        [Header("Configuração")]
        [Tooltip("Nome exato da cena do Menu Principal para onde o jogo retorna.")]
        public string menuSceneName = "MenuScene";

        [Tooltip("Tempo opcional em segundos para voltar sozinho ao menu (0 desliga).")]
        public float autoReturnTime = 0f;

        private float _timer;

        void Update()
        {
            // Se houver tempo de auto retorno configurado
            if (autoReturnTime > 0f)
            {
                _timer += Time.deltaTime;
                if (_timer >= autoReturnTime)
                {
                    ReturnToMenu();
                    return;
                }
            }

            // Pula/Retorna ao menu ao apertar ESC, Espaço ou clicar com o mouse
            bool keyPressed = Keyboard.current != null && (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame);
            bool mouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

            if (keyPressed || mouseClicked)
            {
                ReturnToMenu();
            }
        }

        void ReturnToMenu()
        {
            SceneManager.LoadScene(menuSceneName);
        }
    }
}
