using UnityEngine;
using RemoteChase.Combat;

namespace RemoteChase.Visuals 
{
    [AddComponentMenu("Remote Chase/Drone Shield Visual")]
    public class DroneShieldVisual : MonoBehaviour
    {
        [Header("Referências Visuais")]
        public GameObject shieldModel;

        [Header("Referências de Lógica")]
        public PlayerHealth playerHealth;

        [Header("Efeitos (Game Feel)")]
        public float rotationSpeed = 90f;

        void Awake()
        {
            if (playerHealth == null) playerHealth = GetComponentInParent<PlayerHealth>();
            if (shieldModel != null) shieldModel.SetActive(false);
        }

        void Update()
        {
            if (playerHealth == null || shieldModel == null) return;

            // LÊ a nova propriedade exclusiva do power-up, ignorando o dano comum!
            bool isShieldActive = playerHealth.HasShieldPowerUp;
            
            if (shieldModel.activeSelf != isShieldActive)
            {
                shieldModel.SetActive(isShieldActive);
            }

            if (isShieldActive)
            {
                shieldModel.transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);
            }
        }
    }
}
