using UnityEngine;
using UnityEngine.SceneManagement;
using RemoteChase.Combat; 
using UnityEngine.Audio; 
using UnityEngine.UI; // <-- ADICIONADO: Necessário para acessar o componente Slider!

public class PauseMenuController : MonoBehaviour
{
    [Header("Painéis do Menu de Pausa")]
    public GameObject pausePanel;
    public GameObject optionsPanel;

    [Header("Referências de Gameplay")]
    public GameObject gameHUD; 
    public DroneAim droneAimScript; 

    [Header("Áudio / Configurações")]
    public AudioMixer mainAudioMixer; 
    
    // <-- NOVOS CAMPOS: Referências para os Sliders da UI
    [Tooltip("Arraste o Slider de Música (BGM) aqui")]
    public Slider bgmSlider; 
    [Tooltip("Arraste o Slider de Efeitos (SFX) aqui")]
    public Slider sfxSlider;

    private bool isPaused = false;

    void Start()
    {
        // 1. PRIMEIRO: Definimos o volume e atualizamos os sliders (enquanto estão ativos)
        float volumeInicial = 0.5f;

        if (bgmSlider != null) bgmSlider.value = volumeInicial;
        if (sfxSlider != null) sfxSlider.value = volumeInicial;

        // Por segurança, forçamos o Mixer a receber o valor inicial
        SetBGMVolume(volumeInicial);
        SetSFXVolume(volumeInicial);

        // 2. SEGUNDO: Agora sim, com tudo configurado, escondemos os painéis
        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        
        // Liga a HUD e garante que o jogo não está pausado
        if (gameHUD != null) gameHUD.SetActive(true);
        Time.timeScale = 1f;
    }
    void Update()
    {
        var keyboard = UnityEngine.InputSystem.Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
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
        
        if (gameHUD != null) gameHUD.SetActive(false);
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
        
        if (gameHUD != null) gameHUD.SetActive(true);
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

    public void SetBGMVolume(float sliderValue)
    {
        mainAudioMixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f);
    }

    public void SetSFXVolume(float sliderValue)
    {
        mainAudioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f);
    }

    public void ToggleMute(bool isMuted)
    {
        AudioListener.pause = isMuted;
    }
}
