using UnityEngine;

namespace RemoteChase.Combat
{
    [AddComponentMenu("Remote Chase/Boss Combat")]
    public class BossCombat : MonoBehaviour
    {
        [Header("Configuração de Rajada de Tiros")]
        public GameObject enemyBulletPrefab;
        public Transform firePoint;
        public float burstInterval = 3.0f; // Tempo entre uma rajada e outra
        public int bulletsPerBurst = 3;   // Quantos tiros por rajada
        public float bulletDelay = 0.15f; // Velocidade entre os tiros da mesma rajada
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

            if (firePoint == null)
            {
                firePoint = transform; // Usa o próprio chefão se nenhum ponto foi definido
            }
        }

        void Update()
        {
            if (playerTransform == null) return;

            // Controla o temporizador das rajadas de tiros
            burstTimer += Time.deltaTime;
            if (burstTimer >= burstInterval)
            {
                StartCoroutine(FireBurstRoutine());
                burstTimer = 0f;
            }

            // Controla o temporizador de lançamento de drones kamikazes
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                SpawnKamikaze();
                spawnTimer = 0f;
            }
        }

        System.Collections.IEnumerator FireBurstRoutine()
        {
            Debug.Log("🎯 Iniciando rajada de tiros do Chefão!");

            for (int i = 0; i < bulletsPerBurst; i++)
            {
                if (enemyBulletPrefab != null && firePoint != null)
                {
                    // Instancia o prefab do tiro inimigo
                    GameObject bulletObj = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);

                    // Pega o script EnemyBullet que já existe no seu projeto
                    if (bulletObj.TryGetComponent(out EnemyBullet bulletScript))
                    {
                        Vector3 direction = (playerTransform.position - firePoint.position).normalized;
                        
                        // Busca a vida do player para passar para o tiro
                        var playerHealth = playerTransform.GetComponent<PlayerHealth>();

                        // Dispara usando o método oficial de lançamento do jogo!
                        bulletScript.Launch(firePoint.position, direction, playerHealth, (b) => {
                            // Opcional: o que acontece quando o tiro some ou atinge o alvo
                            Destroy(b.gameObject); 
                        });
                    }
                }
                else
                {
                    Debug.LogWarning("⚠️ Prefab de tiro inimigo ou FirePoint não atribuídos no BossCombat!");
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
