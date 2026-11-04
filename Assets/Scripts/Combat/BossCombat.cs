using UnityEngine;

namespace RemoteChase.Combat
{
    [AddComponentMenu("Remote Chase/Boss Combat")]
    public class BossCombat : MonoBehaviour
    {
        [Header("Configuração de Rajada de Tiros")]
        public GameObject enemyBulletPrefab;
        
        [Tooltip("Arraste os FirePoints do núcleo e dos canhões aqui.")]
        public Transform[] firePoints; 

        public float burstInterval = 3.0f; 
        public int bulletsPerBurst = 3;   
        public float bulletDelay = 0.15f; 
        private float burstTimer;

        [Header("Lançamento de Kamikazes")]
        public GameObject kamikazePrefab;
        public float spawnInterval = 6.0f;
        private float spawnTimer;

        [Header("Referência ao Alvo")]
        public Transform playerTransform;

        void Start()
        {
            if (playerTransform == null)
            {
                var drone = FindAnyObjectByType<Drone.DroneRailController>();
                if (drone != null) playerTransform = drone.transform;
            }
        }

        void Update()
        {
            if (playerTransform == null) return;

            burstTimer += Time.deltaTime;
            if (burstTimer >= burstInterval)
            {
                StartCoroutine(FireBurstRoutine());
                burstTimer = 0f;
            }

            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                SpawnKamikaze();
                spawnTimer = 0f;
            }
        }

        System.Collections.IEnumerator FireBurstRoutine()
        {
            Debug.Log("🎯 Iniciando rajada de tiros múltiplos do Chefão!");

            for (int i = 0; i < bulletsPerBurst; i++)
            {
                if (enemyBulletPrefab != null && firePoints != null && firePoints.Length > 0)
                {
                    foreach (Transform fp in firePoints)
                    {
                        if (fp != null && fp.gameObject.activeInHierarchy)
                        {
                            // 1. Calcula a direção PRIMEIRO com base na posição do alvo e do ponto de disparo
                            Vector3 direction = (playerTransform.position - fp.position).normalized;

                            // 2. Instancia o tiro já rotacionado para a direção correta
                            GameObject bulletObj = Instantiate(enemyBulletPrefab, fp.position, Quaternion.LookRotation(direction));

                            if (bulletObj.TryGetComponent(out EnemyBullet bulletScript))
                            {
                                var playerHealth = playerTransform.GetComponent<PlayerHealth>();

                                bulletScript.Launch(fp.position, direction, playerHealth, (b) => {
                                    Destroy(b.gameObject); 
                                });
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ Prefab de tiro inimigo ou FirePoints não atribuídos no BossCombat!");
                }

                yield return new WaitForSeconds(bulletDelay);
            }
        }

        void SpawnKamikaze()
        {
            Debug.Log("🛸 O Drone Colmeia liberou um Kamikaze de reforço!");
            if (kamikazePrefab != null)
            {
                Vector3 spawnPos = transform.position + new Vector3(Random.Range(-6f, 6f), 0f, -6f);
                Instantiate(kamikazePrefab, spawnPos, Quaternion.identity);
            }
        }
    }
}
