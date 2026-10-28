using UnityEngine;

namespace RemoteChase.Combat
{
    [AddComponentMenu("Remote Chase/Boss Part")]
    public class BossPart : MonoBehaviour, IHittable
    {
        [Header("Configuracao da Parte")]
        [Tooltip("Vida máxima desta parte especifica do chefe.")]
        public float maxHealth = 50f;
        private float currentHealth;

        [Tooltip("Se marcado, esta parte impede que o nucleo receba dano enquanto estiver viva.")]
        public bool isShieldingCore = false;
        public BossController bossController;

        void Start()
        {
            currentHealth = maxHealth;
        }

        // Método chamado pelo DroneWeapon.cs quando o tiro acerta esta parte
        public void TakeHit(float damage, Vector3 point)
        {
            // === BLINDAGEM DO NÚCLEO CENTRAL ===
            // Se esta parte for o NÚCLEO (isShieldingCore = true) E o chefe tiver um controlador,
            // verificamos se os canhões ainda estão vivos(!bossController.cannonsDestroyed).
            // Se ainda houver canhões inteiros, o núcleo ignora o dano completamente!
            if (isShieldingCore)
            {
                if (bossController != null && !bossController.cannonsDestroyed)
                {
                    Debug.Log($"{gameObject.name} (Núcleo) está blindado pelos canhões!");
                    return; // Para aqui e não toma dano!
                }
            }

            if (currentHealth <= 0f) return;

            currentHealth -= damage;
            Debug.Log($"{gameObject.name} sofreu dano! Vida restante: {currentHealth}");

            if (currentHealth <= 0f)
            {
                DestroyPart();
            }
        }

        void DestroyPart()
        {
            Debug.Log($"{gameObject.name} foi destruida!");

            // Se esta parte era um canhão protetor, avisa o chefe principal
            if (bossController != null && isShieldingCore == false)
            {
                // Nota: Canhões normais avisam o controlador. O núcleo não é canhão.
                // O BossController verifica o estado dos canhões pelo método abaixo:
            }

            // Se o BossController precisar checar os canhões ao destruir uma parte:
            if (bossController != null)
            {
                bossController.CheckCanonsDestroyed();
            }

            // Desativa a parte destruída
            gameObject.SetActive(false);
        }
    }
}
