using System;
using System.Collections;
using RemoteChase.Combat;
using RemoteChase.Drone;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace RemoteChase.Game
{
    /// <summary>
    /// Regras da fase: pontuacao, condicao de vitoria (chegar ao fim do percurso)
    /// e de derrota (integridade zerada), mais as telas de fim de jogo.
    /// </summary>
    [AddComponentMenu("Remote Chase/Game Session")]
    public class GameSession : MonoBehaviour
    {
        [Header("Objetivo")]
        [Tooltip("Distancia (m) a percorrer para vencer a fase.")]
        public float targetDistance = 2500f;

        [Header("Pontuacao")]
        [Tooltip("Pontos por inimigo abatido.")]
        public int scorePerKill = 100;

        [Tooltip("Pontos por item coletado.")]
        public int scorePerPickup = 50;

        [Tooltip("Pontos por metro percorrido.")]
        public float scorePerMeter = 1f;

        [Header("Telas Finais e Navegacao")]
        [Tooltip("Imagem exibida na tela de derrota.")]
        public Texture2D defeatImage;
        
        [Tooltip("Imagem exibida na tela de Missão Concluída (Fase Limpa).")]
        public Texture2D successImage;
        
        [Tooltip("Nome da cena do Menu Principal para voltar ao concluir a fase.")]
        public string menuSceneName = "MenuScene";

        public int Score => _bonusScore + Mathf.FloorToInt(Distance * scorePerMeter);
        public float Distance => _drone != null ? _drone.PhaseDistance : 0f;
        public float PhaseTarget => _director != null && _director.Current != null ? _director.Current.targetDistance : targetDistance;
        public float Progress => PhaseTarget > 0f ? Mathf.Clamp01(Distance / PhaseTarget) : 0f;
        public string PhaseName => _director != null && _director.Current != null ? _director.Current.displayName : "";
        public int Kills { get; private set; }
        public bool IsOver { get; private set; }
        public bool Won { get; private set; }

        public event Action<bool> OnFinished;

        DroneRailController _drone;
        PlayerHealth _health;
        DroneEnergy _energy;
        PhaseDirector _director;
        DroneAim _droneAim; 
        int _bonusScore;
        string _defeatReason = "Blindagem destruida";
        
        bool _isPhaseClear = false;

        GUIStyle _title;
        GUIStyle _line;
        GUIStyle _buttonStyle;

        void Awake()
        {
            _drone = FindAnyObjectByType<DroneRailController>();
            _health = FindAnyObjectByType<PlayerHealth>();
            _energy = FindAnyObjectByType<DroneEnergy>();
            _director = FindAnyObjectByType<PhaseDirector>();
            _droneAim = FindAnyObjectByType<DroneAim>(); 

            Time.timeScale = 1f;
        }

        void OnEnable()
        {
            if (_health != null) _health.OnDied += HandleDeath;
            if (_energy != null) _energy.OnDepleted += HandleEnergyOut;
        }

        void OnDisable()
        {
            if (_health != null) _health.OnDied -= HandleDeath;
            if (_energy != null) _energy.OnDepleted -= HandleEnergyOut;
        }

        void Update()
        {
            if (IsOver || _isPhaseClear)
            {
                if (IsOver) ReadRestart();
                return;
            }

            if (_director != null && _director.IsLastPhase) return; 

            if (Distance < PhaseTarget) return;

            TriggerPhaseClear();
        }

        public void RegisterKill()
        {
            if (IsOver) return;
            Kills++;
            _bonusScore += scorePerKill;
        }

        public void RegisterPickup()
        {
            if (IsOver) return;
            _bonusScore += scorePerPickup;
        }

        void HandleDeath()
        {
            _defeatReason = "Blindagem destruida";
            Finish(false);
        }

        void HandleEnergyOut()
        {
            _defeatReason = "Bateria esgotada";
            Finish(false);
        }

        void Finish(bool won)
        {
            if (IsOver) return;

            IsOver = true;
            Won = won;
            Time.timeScale = 0f;
            
            if (_droneAim != null) _droneAim.enabled = false;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            OnFinished?.Invoke(won);
        }

        void ReadRestart()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.enterKey.wasPressedThisFrame || keyboard.rKey.wasPressedThisFrame)
            {
                Time.timeScale = 1f;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        void TriggerPhaseClear()
        {
            _isPhaseClear = true;
            Time.timeScale = 0f;
            
            if (_droneAim != null) _droneAim.enabled = false;
            
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void ContinueToNextPhase()
        {
            _isPhaseClear = false;

            // 1. Restaura a Vida e a Energia ao máximo sem mexer na posição do drone no trilho contínuo
            if (_health != null) _health.Heal(9999f);
            if (_energy != null) _energy.Recharge(9999f);
            
            // 2. Aplica as novas configurações de mundo e avança a fase no ponto atual
            if (_director != null) _director.Advance();

            // 3. Religa o tempo
            Time.timeScale = 1f; 
            
            // 4. Inicia a Coroutine de segurança para religar a mira no próximo frame
            StartCoroutine(SafeResumeRoutine());
        }

        IEnumerator SafeResumeRoutine()
        {
            yield return null; 
            
            if (_droneAim != null) _droneAim.enabled = true;
            
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void ReturnToMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(menuSceneName);
        }

        void EnsureStyles()
        {
            _title ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 52,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            _line ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter,
                normal = { textColor = Color.white }
            };

            _buttonStyle ??= new GUIStyle(GUI.skin.button)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true 
            };
        }

        void OnGUI()
        {
            EnsureStyles();

            if (_isPhaseClear && !IsOver)
            {
                DrawPhaseClearScreen();
                return; 
            }

            if (!IsOver) return;

            Color previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.72f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;

            if (!Won && defeatImage != null)
            {
                GUI.DrawTexture(new Rect(0f, Screen.height * 0.1f, Screen.width, Screen.height * 0.45f), defeatImage, ScaleMode.ScaleToFit);
            }

            float areaStartY = (!Won && defeatImage != null) ? (Screen.height * 0.55f) : (Screen.height * 0.5f - 150f);
            var area = new Rect(0f, areaStartY, Screen.width, 350f);
            
            GUILayout.BeginArea(area);

            if (Won || defeatImage == null)
            {
                _title.normal.textColor = Won ? new Color(0.5f, 1f, 0.6f) : new Color(1f, 0.45f, 0.4f);
                GUILayout.Label(Won ? "VITORIA" : "DERROTA", _title);
            }

            GUILayout.Space(8f);
            if (!Won) GUILayout.Label(_defeatReason, _line);

            GUILayout.Space(8f);
            GUILayout.Label($"Pontuacao: {Score}", _line);
            GUILayout.Label($"Distancia: {Distance:0} m de {targetDistance:0} m", _line);
            GUILayout.Label($"Inimigos abatidos: {Kills}", _line);
            GUILayout.Space(20f);
            GUILayout.Label("Enter ou R para jogar de novo", _line);

            GUILayout.EndArea();
        }

        void DrawPhaseClearScreen()
        {
            Color previous = GUI.color;
            GUI.color = new Color(0f, 0f, 0f, 0.90f);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = previous;

            if (successImage != null)
            {
                GUI.DrawTexture(new Rect(0f, Screen.height * 0.05f, Screen.width, Screen.height * 0.4f), successImage, ScaleMode.ScaleToFit);
            }

            float areaStartY = successImage != null ? Screen.height * 0.45f : Screen.height * 0.3f;
            var area = new Rect(Screen.width * 0.5f - 250f, areaStartY, 500f, Screen.height - areaStartY);
            
            GUILayout.BeginArea(area);

            if (successImage == null)
            {
                _title.normal.textColor = new Color(0.3f, 0.8f, 1f);
                GUILayout.Label("FASE CONCLUIDA!", _title);
                GUILayout.Space(15f);
            }

            GUILayout.Label($"Excelente trabalho, piloto.", _line);
            GUILayout.Label($"Pontuacao Acumulada: {Score}", _line);
            GUILayout.Label($"Inimigos abatidos: {Kills}", _line);
            
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("CONTINUAR PARA A FASE 2", _buttonStyle, GUILayout.Height(45)))
            {
                ContinueToNextPhase();
            }

            GUILayout.Space(10f);

            if (GUILayout.Button("VOLTAR AO MENU PRINCIPAL", _buttonStyle, GUILayout.Height(45)))
            {
                ReturnToMenu();
            }
            
            GUILayout.Space(20f); 
            GUILayout.EndArea();
        }
    }
}
