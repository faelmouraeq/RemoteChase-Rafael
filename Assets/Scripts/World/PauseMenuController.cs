using UnityEngine;
using UnityEngine.SceneManagement;
using RemoteChase.Combat; 
using UnityEngine.Audio; 
using UnityEngine.UI;
using RemoteChase.Audio;

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
    
    public Slider bgmSlider; 
    public Slider sfxSlider;
    public Toggle muteToggle;

    private bool isPaused = false;

    void Start()
    {
        if (AudioManager.Instance != null)
        {
            if (bgmSlider != null) bgmSlider.value = AudioManager.Instance.BGMValue;
            if (sfxSlider != null) sfxSlider.value = AudioManager.Instance.SFXValue;
            if (muteToggle != null) muteToggle.isOn = AudioManager.Instance.IsMuted;
        }
        else
        {
            float volumeInicial = 0.5f;
            if (bgmSlider != null) bgmSlider.value = volumeInicial;
            if (sfxSlider != null) sfxSlider.value = volumeInicial;
            SetBGMVolume(volumeInicial);
            SetSFXVolume(volumeInicial);
        }

        if (pausePanel != null) pausePanel.SetActive(false);
        if (optionsPanel != null) optionsPanel.SetActive(false);
        
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
                if (isPaused) ResumeGame();
                else PauseGame();
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
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetBGMVolume(sliderValue);
        }
        else if (mainAudioMixer != null)
        {
            mainAudioMixer.SetFloat("BGMVolume", Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f);
        }
    }

    public void SetSFXVolume(float sliderValue)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(sliderValue);
        }
        else if (mainAudioMixer != null)
        {
            mainAudioMixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f);
        }
    }

    public void ToggleMute(bool isMuted)
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleMute(isMuted);
        }
        else
        {
            AudioListener.pause = isMuted;
        }
    }
}
