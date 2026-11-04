using UnityEngine;
using UnityEngine.UI;

namespace RemoteChase.UI
{
    // Força a Unity a garantir que existe um componente Image neste objeto
    [RequireComponent(typeof(Image))]
    [AddComponentMenu("Remote Chase/UI Health Battery Alert")]
    public class UIHealthBatteryAlert : MonoBehaviour
    {
        [Header("Configuração de Limiar")]
        [Tooltip("Porcentagem crítica (ex: 0.25 para 25%).")]
        [Range(0.01f, 0.99f)]
        public float criticalThreshold = 0.25f;

        [Header("Cores")]
        public Color normalColor = Color.green;
        public Color criticalColor = Color.red;

        [Header("Configuração do Pisca-Pisca")]
        public float blinkSpeed = 10f;

        private Image _barImage;

        void Awake()
        {
            // Pega automaticamente o componente Image que já está no mesmo objeto (FillVida / FillBateria)
            _barImage = GetComponent<Image>();
        }

        void Update()
        {
            // Se não encontrou a imagem, não faz nada
            if (_barImage == null) return;

            // LÊ o valor que o seu sistema original (PlayerHUDController) já atualizou
            if (_barImage.fillAmount <= criticalThreshold)
            {
                // Aplica o pisca-pisca vermelho
                float lerpFactor = (Mathf.Sin(Time.time * blinkSpeed) + 1f) / 2f;
                _barImage.color = Color.Lerp(normalColor, criticalColor, lerpFactor);
            }
            else
            {
                // Mantém a cor normal (verde ou azul)
                _barImage.color = normalColor;
            }
        }
    }
}
