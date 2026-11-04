using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem; // Namespace obrigatório para o novo Input System

namespace RemoteChase.Game
{
    [AddComponentMenu("Remote Chase/Splash Screen Controller")]
    public class SplashScreenController : MonoBehaviour
    {
        [Header("Configuração da Tela")]
        [Tooltip("Nome exato da cena do Menu Principal ou da primeira fase para onde o jogo vai após a logo.")]
        public string nextSceneName = "MenuScene";

        [Tooltip("Tempo em segundos que a logo fica visível antes de trocar de cena.")]
        public float displayDuration = 3.5f;

        private float _timer;

        void Update()
        {
            _timer += Time.deltaTime;

            // NOVA VERIFICAÇÃO DO INPUT SYSTEM:
            // Checa se o tempo acabou OU se qualquer tecla do teclado foi pressionada, 
            // ou se o botão do mouse foi clicado.
            bool anyKeyPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
            bool anyMouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

            if (_timer >= displayDuration || anyKeyPressed || anyMouseClicked)
            {
                LoadNextScene();
            }
        }

        void LoadNextScene()
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}
