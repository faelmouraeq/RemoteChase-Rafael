using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

namespace RemoteChase.Game
{
    [AddComponentMenu("Remote Chase/Credits Controller")]
    public class CreditsController : MonoBehaviour
    {
        [Header("Configuração de Cenas")]
        [Tooltip("Nome exato da cena do Menu Principal para onde o jogo retorna.")]
        public string menuSceneName = "MenuScene";

        [Header("Configuração do Scroll")]
        [Tooltip("Velocidade com que o texto sobe na tela.")]
        public float scrollSpeed = 60f;

        [Tooltip("Tempo opcional em segundos para voltar sozinho ao menu no fim (0 desliga).")]
        public float autoReturnTime = 0f;

        [TextArea(20, 30)]
        [Tooltip("Créditos oficiais do Remote Chase atualizados com toda a equipa.")]
        public string creditsText = 
            "REMOTE CHASE\n\n" +
            "UM JOGO DE CORRIDA E COMBATE\n\n\n" +
            "DIREÇÃO DE JOGO\n" +
            "Rafael Moura\n\n\n" +
            "PROGRAMAÇÃO & ARQUITETURA\n" +
            "Rafael Moura\n" +
            "Théo Xavier\n\n\n" +
            "DESIGN DE NÍVEL & MUNDO\n" +
            "Rafael Moura\n" +
            "Théo Xavier\n" +
            "Francisco Casimiro\n\n\n" +
            "INTELIGÊNCIA ARTIFICIAL & COMBATE\n" +
            "Rafael Moura\n" +
            "Théo Xavier\n\n\n" +
            "DIREÇÃO DE ARTE\n" +
            "Francisco Casimiro\n\n\n" +
            "ARTES & MODELOS 3D\n" +
            "Francisco Casimiro\n" +
            "Ramune Bortolin\n" +
            "Pedro Roquette\n\n\n" +
            "TEXTURAS\n" +
            "Francisco Casimiro\n" +
            "Ramune Bortolin\n" +
            "Pedro Roquette\n\n\n" +
            "SPLASH SCREENS\n" +
            "Francisco Casimiro\n" +
            "Ramune Bortolin\n" +
            "Pedro Roquette\n\n\n" +
            "ÁUDIO & SFX\n" +
            "Rafael Moura\n\n\n" +
            "AGRADECIMENTOS ESPECIAIS\n" +
            "PUC Minas\n" +
            "Engine Unity\n\n\n" +
            "OBRIGADO POR JOGAR!";

        private float _textYPosition;
        private float _timer;
        private GUIStyle _creditsStyle;
        private bool _isInitialized = false;

        void Start()
        {
            ResetScrollPosition();
        }

        void ResetScrollPosition()
        {
            // Começa exatamente abaixo da borda inferior da tela
            _textYPosition = Screen.height;
            _isInitialized = true;
        }

        void Update()
        {
            if (!_isInitialized) ResetScrollPosition();

            // Faz o texto subir continuamente
            _textYPosition -= scrollSpeed * Time.deltaTime;

            if (autoReturnTime > 0f)
            {
                _timer += Time.deltaTime;
                if (_timer >= autoReturnTime)
                {
                    ReturnToMenu();
                    return;
                }
            }

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

        void OnGUI()
        {
            // Estilo tipográfico em Branco Puro, centralizado, no estilo de cinema
            _creditsStyle ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
                normal = { textColor = Color.white } 
            };

            // Fundo escuro absoluto para destacar o letreiro
            Color previous = GUI.color;
            GUI.color = Color.black;
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;

            // Área centralizada onde o texto vai subir
            float contentWidth = 650f;
            float contentHeight = 1800f; // Altura ajustada para acomodar todos os blocos de texto
            var rect = new Rect((Screen.width - contentWidth) * 0.5f, _textYPosition, contentWidth, contentHeight);

            GUILayout.BeginArea(rect);
            GUILayout.Label(creditsText, _creditsStyle);
            GUILayout.EndArea();

            // Instrução discreta no rodapé
            var hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.LowerCenter,
                normal = { textColor = new Color(1f, 1f, 1f, 0.4f) }
            };
            GUI.Label(new Rect(0f, Screen.height - 40f, Screen.width, 30f), "Pressione ESC ou Espaço para voltar ao Menu", hintStyle);
        }
    }
}
