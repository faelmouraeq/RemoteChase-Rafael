using UnityEngine;
using RemoteChase.Drone;

namespace RemoteChase.Gameplay
{
    [AddComponentMenu("Remote Chase/Power-Up Spawner")]
    public class PowerUpSpawner : MonoBehaviour
    {
        [Header("Prefabs de Power-Ups")]
        [Tooltip("Arraste o Prefab do Power-Up de Energia aqui.")]
        [SerializeField] private GameObject energyPowerUpPrefab;

        [Tooltip("Arraste o Prefab do Power-Up de Vida aqui.")]
        [SerializeField] private GameObject healthPowerUpPrefab;

        [Header("Configuração de Distância e Tempo")]
        [Tooltip("Intervalo de tempo (em segundos) entre o surgimento de cada power-up.")]
        [SerializeField] private float spawnInterval = 6f;

        [Tooltip("Distância MÍNIMA à frente do drone onde o item surge (em metros).")]
        [SerializeField] private float minDistanceAhead = 80f;

        [Tooltip("Distância MÁXIMA à frente do drone onde o item surge (em metros).")]
        [SerializeField] private float maxDistanceAhead = 140f;

        [Header("Dispersão Lateral e Vertical")]
        [Tooltip("Amplitude máxima para os lados (X) em relação ao centro.")]
        [SerializeField] private float lateralSpread = 12f;

        [Tooltip("Amplitude máxima para cima e para baixo (Y).")]
        [SerializeField] private float verticalSpread = 6f;

        private float timer;
        private Transform playerTransform;

        void Start()
        {
            // Localiza o drone na cena através da Tag "Player"
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
                Debug.LogWarning("PowerUpSpawner: Nenhum objeto com a tag 'Player' foi encontrado na cena!");
            }
        }

        void Update()
        {
            if (playerTransform == null) return;

            timer += Time.deltaTime;

            if (timer >= spawnInterval)
            {
                SpawnPowerUp();
                timer = 0f;
            }
        }

        void SpawnPowerUp()
        {
            // Sorteia entre Energia e Vida
            GameObject prefabToSpawn = Random.value > 0.5f ? energyPowerUpPrefab : healthPowerUpPrefab;
            if (prefabToSpawn == null) return;

            // Define uma distância segura lá na frente do drone
            float forwardDistance = Random.Range(minDistanceAhead, maxDistanceAhead);

            // Calcula a posição exata baseada na rotação e direção do drone
            Vector3 spawnPosition = playerTransform.position 
                                  + (playerTransform.forward * forwardDistance)
                                  + (playerTransform.right * Random.Range(-lateralSpread, lateralSpread))
                                  + (playerTransform.up * Random.Range(-verticalSpread, verticalSpread));

            // Instancia o item no mundo
            Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
            
            Debug.Log($"Power-up gerado a {forwardDistance:0} metros à frente do drone.");
        }
    }
}
