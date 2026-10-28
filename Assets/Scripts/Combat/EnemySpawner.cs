using System.Collections.Generic;
using RemoteChase.Drone;
using UnityEngine;

namespace RemoteChase.Combat
{
    [AddComponentMenu("Remote Chase/Enemy Spawner")]
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Alvo")]
        [Tooltip("Drone do jogador. Se vazio, procura na cena.")]
        public DroneRailController drone;

        [Header("Spawn de Sentinelas (Alvos Estáticos)")]
        public int maxAlive = 4;
        public float spawnInterval = 1.1f;
        public float spawnDistance = 85f;
        public float lifetime = 7f;
        public Vector2 areaCenter = new Vector2(0f, 14f);
        public Vector2 areaHalfExtents = new Vector2(11f, 6f);

        [Header("Progressao do desafio")]
        public float strafeAtStart = 0f;
        public float strafeAtEnd = 7f;
        public float bobAtEnd = 3f;

        [Header("Drone Enxame (Kamikaze - Estilo Star Fox)")]
        [Range(0f, 1f)] public float swarmStartsAt = 0.1f;
        public int maxSwarmAlive = 6;
        public float swarmInterval = 4f;
        public float swarmSpawnDistance = 120f;
        public float swarmSize = 1.8f;
        
        [Tooltip("Quantos drones kamikazes formam cada lote/onda do enxame.")]
        public int swarmClusterSize = 3;

        public Material swarmMaterial;
        public GameObject swarmPrefab;

        [Header("Visual Sentinela")]
        public float enemySize = 2.6f;
        public Material enemyMaterial;
        public GameObject enemyPrefab;

        public int Kills { get; private set; }

        Game.GameSession _session;
        EnemyBulletPool _bulletPool;
        PlayerHealth _playerHealth;
        readonly Stack<Enemy> _pool = new Stack<Enemy>();
        readonly List<Enemy> _alive = new List<Enemy>();
        readonly Stack<SwarmDrone> _swarmPool = new Stack<SwarmDrone>();
        readonly List<SwarmDrone> _swarmAlive = new List<SwarmDrone>();
        Transform _swarmRoot;
        float _spawnTimer;
        float _swarmTimer;
        float _railZ;

        void Start()
        {
            if (drone == null) drone = FindAnyObjectByType<DroneRailController>();

            _session = FindAnyObjectByType<Game.GameSession>();
            _bulletPool = FindAnyObjectByType<EnemyBulletPool>();
            _playerHealth = FindAnyObjectByType<PlayerHealth>();

            int poolSize = Mathf.Max(maxAlive * 3, 6);
            for (int i = 0; i < poolSize; i++) _pool.Push(Create(i));

            _swarmRoot = new GameObject("Enxame_Root").transform;

            int swarmPoolSize = Mathf.Max(maxSwarmAlive * 2, 4);
            for (int i = 0; i < swarmPoolSize; i++) _swarmPool.Push(CreateSwarm(i));
        }

        void Update()
        {
            FollowDrone();
            RecycleFinished();
            TrySpawn();

            RecycleSwarm();
            TrySpawnSwarm();
        }

        void FollowDrone()
        {
            if (drone == null) return;

            _railZ = drone.transform.position.z;
            transform.position = new Vector3(0f, 0f, _railZ);
        }

        void RecycleFinished()
        {
            for (int i = _alive.Count - 1; i >= 0; i--)
            {
                Enemy enemy = _alive[i];
                if (enemy != null && enemy.gameObject.activeSelf) continue;

                _alive.RemoveAt(i);
                if (enemy != null) _pool.Push(enemy);
            }
        }

        void TrySpawn()
        {
            // === TRAVA DE SEGURANÇA DO CHEFE ===
            // Procura o chefe na cena. Se ele estiver ativo, os sentinelas comuns param de nascer!
            GameObject bossObj = GameObject.Find("Boss_HiveDrone");
            if (bossObj != null && bossObj.activeSelf) return;

            if (drone == null || _alive.Count >= maxAlive) return;

            _spawnTimer -= Time.deltaTime;
            if (_spawnTimer > 0f) return;

            _spawnTimer = spawnInterval;

            Enemy enemy = _pool.Count > 0 ? _pool.Pop() : Create(_alive.Count);

            float progress = drone.SpeedProgress;
            enemy.strafeAmplitude = Mathf.Lerp(strafeAtStart, strafeAtEnd, progress);
            enemy.bobAmplitude = Mathf.Lerp(0f, bobAtEnd, progress);
            enemy.lifetime = lifetime;

            var local = new Vector3(
                areaCenter.x + Random.Range(-areaHalfExtents.x, areaHalfExtents.x),
                areaCenter.y + Random.Range(-areaHalfExtents.y, areaHalfExtents.y),
                spawnDistance);

            enemy.Spawn(local, Random.Range(0f, 6.28f));
            _alive.Add(enemy);
        }

        Enemy Create(int index)
        {
            GameObject body = enemyPrefab != null
                ? Instantiate(enemyPrefab, transform)
                : GameObject.CreatePrimitive(PrimitiveType.Sphere);

            body.name = $"Sentinela_{index}";
            if (enemyPrefab == null)
            {
                body.transform.SetParent(transform, false);
                if (enemyMaterial != null)
                {
                    body.GetComponent<MeshRenderer>().sharedMaterial = enemyMaterial;
                }
            }

            body.transform.localScale = Vector3.one * enemySize;

            var enemy = body.AddComponent<Enemy>();
            enemy.BulletPool = _bulletPool;
            enemy.Player = drone != null ? drone.transform : null;
            enemy.OnKilled += HandleKilled;

            body.SetActive(false);
            return enemy;
        }

        void HandleKilled(Enemy enemy, Vector3 point)
        {
            Kills++;
            if (_session != null) _session.RegisterKill();
        }

        void RecycleSwarm()
        {
            for (int i = _swarmAlive.Count - 1; i >= 0; i--)
            {
                SwarmDrone swarm = _swarmAlive[i];
                if (swarm != null && swarm.gameObject.activeSelf) continue;

                _swarmAlive.RemoveAt(i);
                if (swarm != null) _swarmPool.Push(swarm);
            }
        }

        void TrySpawnSwarm()
        {
            // === TRAVA DE SEGURANÇA DO CHEFE PARA O ENXAME ===
            // Se o chefe estiver ativo, os kamikazes comuns também dão uma trégua!
            GameObject bossObj = GameObject.Find("Boss_HiveDrone");
            if (bossObj != null && bossObj.activeSelf) return;

            if (drone == null || _swarmAlive.Count >= maxSwarmAlive) return;

            float progress = drone.SpeedProgress;
            if (progress < swarmStartsAt) return;

            _swarmTimer -= Time.deltaTime;
            if (_swarmTimer > 0f) return;

            _swarmTimer = swarmInterval;

            StartCoroutine(SpawnSwarmClusterRoutine());
        }

        System.Collections.IEnumerator SpawnSwarmClusterRoutine()
        {
            for (int i = 0; i < swarmClusterSize; i++)
            {
                if (drone == null) yield break;

                SwarmDrone swarm = _swarmPool.Count > 0 ? _swarmPool.Pop() : CreateSwarm(_swarmAlive.Count);

                if (swarm != null)
                {
                    float side = Random.value < 0.5f ? -1f : 1f;
                    var worldPos = new Vector3(
                        areaCenter.x + side * (areaHalfExtents.x + Random.Range(4f, 10f)),
                        areaCenter.y + Random.Range(-2f, areaHalfExtents.y),
                        drone.transform.position.z + swarmSpawnDistance);

                    Vector3 targetPos = drone.transform.position + Vector3.forward * 5f;

                    swarm.gameObject.SetActive(true);
                    swarm.Launch(worldPos, targetPos);
                    _swarmAlive.Add(swarm);
                }

                yield return new WaitForSeconds(0.3f);
            }
        }

        SwarmDrone CreateSwarm(int index)
        {
            GameObject body = swarmPrefab != null
                ? Instantiate(swarmPrefab, _swarmRoot)
                : GameObject.CreatePrimitive(PrimitiveType.Capsule);

            body.name = $"Enxame_{index}";
            if (swarmPrefab == null)
            {
                body.transform.SetParent(_swarmRoot, false);
                if (swarmMaterial != null)
                {
                    body.GetComponent<MeshRenderer>().sharedMaterial = swarmMaterial;
                }
            }

            body.transform.localScale = Vector3.one * swarmSize;

            var swarm = body.AddComponent<SwarmDrone>();
            swarm.Target = drone;
            swarm.TargetHealth = _playerHealth;
            swarm.OnKilled += HandleSwarmKilled;

            body.SetActive(false);
            return swarm;
        }

        void HandleSwarmKilled(SwarmDrone swarm, Vector3 point)
        {
            Kills++;
            if (_session != null) _session.RegisterKill();
        }
    }
}
