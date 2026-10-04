using UnityEngine;
using UnityEngine.SceneManagement;
using RemoteChase.Combat; // Importa o namespace onde o DroneAim está

public class PauseMenuController : MonoBehaviour
{
    [Header("Painéis do Menu de Pausa")]
    public GameObject pausePanel;
    public GameObject optionsPanel;

    [Header("Referências de Gameplay")]
    public GameObject gameHUD; // Objeto visual da HUD/Mira
    public DroneAim droneAimScript; // Arraste o objeto do Drone aqui no Inspetor!

    private bool isPaused = false;

    void Start()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (gameHUD != null) gameHUD.SetActive(true);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsPanel != null && optionsPanel.activeSelf)
            {
                CloseOptions();
            }
            else
            {
                if (isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }
    }

    public void PauseGame()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
        
        // Desliga a HUD visual
        if (gameHUD != null) gameHUD.SetActive(false);

        // DESATIVA O SCRIPT DE MIRA: Isso faz o OnDisable() do DroneAim rodar, 
        // liberando o mouse e escondendo a mira do jogo automaticamente!
        if (droneAimScript != null) droneAimScript.enabled = false;

        Time.timeScale = 0f;
        isPaused = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        
        // Religa a HUD visual
        if (gameHUD != null) gameHUD.SetActive(true);

        // REATIVA O SCRIPT DE MIRA: O OnEnable() do DroneAim vai rodar,
        // travando o mouse de volta e trazendo o retículo de tiro.
        if (droneAimScript != null) droneAimScript.enabled = true;

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void OpenOptions()
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(true);
    }

    public void CloseOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        SceneManager.LoadScene("Menu"); 
    }
}
