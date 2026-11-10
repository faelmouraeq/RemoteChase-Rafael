using UnityEngine;
using UnityEngine.Audio;

namespace RemoteChase.Audio
{
    [AddComponentMenu("Remote Chase/Audio Manager")]
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Mixer Global")]
        public AudioMixer mainAudioMixer;

        [Header("Fontes de Áudio")]
        public AudioSource musicSource;

        [Header("Clipes de Áudio")]
        public AudioClip menuMusic;
        public AudioClip gameplayMusic;

        public float BGMValue { get; private set; } = 0.5f;
        public float SFXValue { get; private set; } = 0.5f;
        public bool IsMuted { get; private set; } = false;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
            }
        }

        void Start()
        {
            PlayMenuMusic();
            ApplySavedVolumes();
        }

        public void PlayMenuMusic()
        {
            if (menuMusic != null && musicSource.clip != menuMusic)
            {
                musicSource.clip = menuMusic;
                musicSource.Play();
            }
        }

        public void PlayGameplayMusic()
        {
            if (gameplayMusic != null && musicSource.clip != gameplayMusic)
            {
                musicSource.clip = gameplayMusic;
                musicSource.Play();
            }
        }

        public void SetBGMVolume(float sliderValue)
        {
            BGMValue = Mathf.Clamp(sliderValue, 0.0001f, 1f);
            
            // 1. Tenta atualizar o Mixer com o nome original
            if (mainAudioMixer != null)
            {
                float decibel = Mathf.Log10(BGMValue) * 20f;
                mainAudioMixer.SetFloat("BGMVolume", decibel);
            }
            
            // 2. FORÇA a alteração do volume diretamente na caixa de som (À prova de falhas)
            if (musicSource != null)
            {
                musicSource.volume = BGMValue;
            }
        }

        public void SetSFXVolume(float sliderValue)
        {
            SFXValue = Mathf.Clamp(sliderValue, 0.0001f, 1f);
            
            if (mainAudioMixer != null)
            {
                float decibel = Mathf.Log10(SFXValue) * 20f;
                mainAudioMixer.SetFloat("SFXVolume", decibel);
            }
        }

        public void ToggleMute(bool isMuted)
        {
            IsMuted = isMuted;
            AudioListener.pause = isMuted;
        }

        private void ApplySavedVolumes()
        {
            SetBGMVolume(BGMValue);
            SetSFXVolume(SFXValue);
            ToggleMute(IsMuted);
        }
    }
}
