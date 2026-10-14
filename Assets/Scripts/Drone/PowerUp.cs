using UnityEngine;
using RemoteChase.UI;
using RemoteChase.Drone;

namespace RemoteChase.Gameplay
{
    public enum PowerUpType
    {
        Energy,
        Health
    }

    [AddComponentMenu("Remote Chase/Power-Up")]
    [RequireComponent(typeof(SphereCollider))]
    public class PowerUp : MonoBehaviour
    {
        [Header("Configuração do Power-Up")]
        [Tooltip("Escolha se este item restaura Energia (Bateria) ou Vida.")]
        [SerializeField] private PowerUpType powerUpType;

        [Tooltip("Quantidade restaurada ao coletar o item.")]
        [SerializeField] private float restoreAmount = 25f;

        [Header("Hitbox de Coleta")]
        [Tooltip("Raio da esfera de colisão (deve ser grande o suficiente para facilitar a coleta em alta velocidade).")]
        [SerializeField] private float hitboxRadius = 3.5f;

        [Header("Efeitos")]
        [Tooltip("Som reproduzido ao coletar o item.")]
        [SerializeField] private AudioClip collectSound;

        void Awake()
        {
            // Configura o SphereCollider obrigatoriamente como Trigger para o drone atravessar sem bater
            SphereCollider col = GetComponent<SphereCollider>();
            if (col != null)
            {
                col.isTrigger = true;
                col.radius = hitboxRadius;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            // Verifica se quem entrou no trigger é o drone (verificando a Tag "Player" ou se possui movimento de drone)
            bool isPlayer = other.CompareTag("Player") 
                            || other.GetComponent<IDroneMotion>() != null 
                            || other.GetComponentInParent<IDroneMotion>() != null;

            if (isPlayer)
            {
                // Como o PlayerHUDController está no HUD_Manager na cena, buscamos ele de forma segura
                PlayerHUDController hud = FindAnyObjectByType<PlayerHUDController>();

                if (hud != null)
                {
                    if (powerUpType == PowerUpType.Energy)
                    {
                        hud.AddEnergy(restoreAmount);
                        Debug.Log($"[PowerUp] Bateria coletada com sucesso! +{restoreAmount} de Energia.");
                    }
                    else if (powerUpType == PowerUpType.Health)
                    {
                        hud.Heal(restoreAmount);
                        Debug.Log($"[PowerUp] Kit de Vida coletado com sucesso! +{restoreAmount} de Vida.");
                    }

                    // Toca o som de coleta (se houver)
                    if (collectSound != null)
                    {
                        AudioSource.PlayClipAtPoint(collectSound, transform.position);
                    }

                    // Destrói o power-up para sumir da fase
                    Destroy(gameObject);
                }
                else
                {
                    Debug.LogWarning("[PowerUp] O drone passou pelo item, mas nenhum PlayerHUDController foi encontrado na cena!");
                }
            }
        }
    }
}
