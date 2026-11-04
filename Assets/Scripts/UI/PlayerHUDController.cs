using UnityEngine;
using UnityEngine.UI;
using RemoteChase.Combat;

namespace RemoteChase.UI
{
    [AddComponentMenu("Remote Chase/Player HUD Controller")]
    public class PlayerHUDController : MonoBehaviour
    {
        [Header("Referências Visuais (Arraste os objetos Fill)")]
        [SerializeField] private Image vidaFillImage;
        [SerializeField] private Image energiaFillImage;

        [Header("Fontes de dado")]
        [SerializeField] private PlayerHealth health;
        [SerializeField] private DroneEnergy energy;

        void Awake()
        {
            if (health == null) health = FindAnyObjectByType<PlayerHealth>();
            if (energy == null) energy = FindAnyObjectByType<DroneEnergy>();
        }

        void Update()
        {
            if (vidaFillImage != null && health != null)
            {
                vidaFillImage.fillAmount = health.Normalized;
            }

            if (energiaFillImage != null && energy != null)
            {
                energiaFillImage.fillAmount = energy.Normalized;
            }
        }

        public void TakeDamage(float amount)
        {
            if (health != null) health.TakeHit(amount, transform.position);
        }

        public void Heal(float amount)
        {
            if (health != null) health.Heal(amount);
        }

        public void AddEnergy(float amount)
        {
            if (energy != null) energy.Recharge(amount);
        }
    }
}
