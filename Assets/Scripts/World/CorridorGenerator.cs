using System.Collections.Generic;
using UnityEngine;

namespace RemoteChase.World
{
    [AddComponentMenu("Remote Chase/Corridor Generator")]
    public class CorridorGenerator : MonoBehaviour
    {
        [Header("Alvo")]
        public Transform follow;

        [Header("Trilho")]
        public float segmentLength = 50f;
        public int segmentsAhead = 14;
        public int segmentsBehind = 2;
        
        [Tooltip("Largura do corredor: afasta ou aproxima os prédios do centro.")]
        public float corridorHalfWidth = 25f;
        public float ceilingHeight;

        [Header("Predios")]
        public int buildingsPerSide = 3;
        public Vector2 widthRange = new Vector2(10f, 22f);
        public Vector2 heightRange = new Vector2(30f, 110f);
        public float setbackRange = 10f;

        [Header("Obstaculos")]
        [Range(0f, 1f)] public float obstacleChance = 0.25f;

        [Header("Coletaveis")]
        public Vector2 pickupAreaCenter = new Vector2(0f, 14f);
        public Vector2 pickupAreaHalfExtents = new Vector2(11f, 6f);
        public int pickupsPerSegment = 1;
        [Range(0f, 1f)] public float pickupChance = 0.22f;
        public float pickupSize = 1.8f;

        [Header("Arte dos Prédios (Novos Modelos)")]
        [Tooltip("Arraste aqui os prefabs dos seus prédios: [0] Baixo, [1] Médio, [2] Alto.")]
        public GameObject[] buildingPrefabs;

        [Header("Outros Prefabs e Itens")]
        public GameObject repairPrefab;
        public GameObject shieldPrefab;
        public GameObject energyPrefab;

        [Header("Visual")]
        public Material groundMaterial;
        public Material[] buildingMaterials;
        public Material pickupMaterial;
        public Material shieldMaterial;
        public Material energyMaterial;

        Game.GameSession _session;
        Combat.PlayerHealth _playerHealth;
        Combat.DroneEnergy _playerEnergy;

        public int seed = 1337;

        readonly List<Segment> _segments = new List<Segment>();
        int _firstIndex;
        int _nextIndex;

        class Segment
        {
            public Transform Root;
            public Transform Ground;
            public Transform Ceiling;
            public Transform[] Buildings;
            public Transform Obstacle;
            public Combat.Pickup[] Pickups;
            public int Index = -1;
        }

        void Start()
        {
            if (follow == null)
            {
                var drone = FindAnyObjectByType<Drone.DroneRailController>();
                if (drone != null) follow = drone.transform;
            }

            _session = FindAnyObjectByType<Game.GameSession>();
            _playerHealth = FindAnyObjectByType<Combat.PlayerHealth>();
            _playerEnergy = FindAnyObjectByType<Combat.DroneEnergy>();

            for (int i = 0; i < segmentsAhead + segmentsBehind + 1; i++)
            {
                Segment segment = CreateSegment(i);
                Place(segment, _nextIndex++);
                _segments.Add(segment);
            }
        }

        void Update()
        {
            if (follow == null) return;

            int currentIndex = Mathf.FloorToInt(follow.position.z / segmentLength);

            while (_firstIndex < currentIndex - segmentsBehind)
            {
                Segment recycled = _segments[0];
                _segments.RemoveAt(0);
                Place(recycled, _nextIndex++);
                _segments.Add(recycled);
                _firstIndex++;
            }
        }

        public void Retheme(Material newGround, Material[] newBuildings, int newSeed)
        {
            if (newGround != null) groundMaterial = newGround;
            if (newBuildings != null && newBuildings.Length > 0) buildingMaterials = newBuildings;
            seed = newSeed;

            ApplyMaterials();

            int startIndex = follow != null
                ? Mathf.FloorToInt(follow.position.z / segmentLength) - segmentsBehind
                : 0;

            _firstIndex = startIndex;
            _nextIndex = startIndex;

            foreach (Segment segment in _segments)
            {
                Place(segment, _nextIndex++);
            }
        }

        void ApplyMaterials()
        {
            for (int s = 0; s < _segments.Count; s++)
            {
                Segment segment = _segments[s];

                SetMaterial(segment.Ground, groundMaterial);
                SetMaterial(segment.Ceiling, groundMaterial);
                SetMaterial(segment.Obstacle, PickMaterial(0));

                for (int i = 0; i < segment.Buildings.Length; i++)
                {
                    SetMaterial(segment.Buildings[i], PickMaterial(i));
                }
            }
        }

        static void SetMaterial(Transform target, Material material)
        {
            if (target == null || material == null) return;
            if (target.TryGetComponent(out MeshRenderer renderer)) renderer.sharedMaterial = material;
        }

        Segment CreateSegment(int poolIndex)
        {
            var root = new GameObject($"Segmento_{poolIndex}").transform;
            root.SetParent(transform, false);

            var segment = new Segment
            {
                Root = root,
                Ground = CreateBlock(root, "Chao", groundMaterial),
                Ceiling = CreateBlock(root, "Teto", groundMaterial),
                Buildings = new Transform[buildingsPerSide * 2],
                Obstacle = CreateBlock(root, "Obstaculo", PickMaterial(0)),
                Pickups = new Combat.Pickup[Mathf.Max(pickupsPerSegment, 0)]
            };

            for (int i = 0; i < segment.Buildings.Length; i++)
            {
                segment.Buildings[i] = CreateBuildingTransform(root, i);
            }

            for (int i = 0; i < segment.Pickups.Length; i++)
            {
                segment.Pickups[i] = CreatePickup(root, i);
            }

            return segment;
        }

        Transform CreateBuildingTransform(Transform parent, int index)
        {
            GameObject prefabToUse = null;

            if (buildingPrefabs != null && buildingPrefabs.Length > 0)
            {
                int randomIdx = UnityEngine.Random.Range(0, buildingPrefabs.Length);
                prefabToUse = buildingPrefabs[randomIdx];
            }

            GameObject body = prefabToUse != null
                ? Instantiate(prefabToUse, parent)
                : GameObject.CreatePrimitive(PrimitiveType.Cube);

            body.name = $"Predio_{index}";
            if (prefabToUse == null && groundMaterial != null)
            {
                body.GetComponent<MeshRenderer>().sharedMaterial = groundMaterial;
            }

            return body.transform;
        }

        Combat.Pickup CreatePickup(Transform parent, int index)
        {
            // SORTEIO EQUILIBRADO: Alterna aleatoriamente entre os tipos de power-ups disponíveis
            int randomKind = UnityEngine.Random.Range(0, 3);
            Combat.PickupKind kind = (Combat.PickupKind)randomKind;

            GameObject prefab = kind switch
            {
                Combat.PickupKind.Energia => energyPrefab,
                Combat.PickupKind.Escudo => shieldPrefab,
                _ => repairPrefab
            };

            Material material = kind switch
            {
                Combat.PickupKind.Energia => energyMaterial,
                Combat.PickupKind.Escudo => shieldMaterial,
                _ => pickupMaterial
            };

            GameObject body = CreateVisual(parent, $"{kind}_{index}", prefab,
                                           PrimitiveType.Capsule, material);
            body.transform.localScale = Vector3.one * pickupSize;

            foreach (Collider collider in body.GetComponentsInChildren<Collider>())
            {
                Destroy(collider);
            }

            var pickup = body.AddComponent<Combat.Pickup>();
            pickup.kind = kind;
            pickup.Player = _playerHealth;
            pickup.Energy = _playerEnergy;
            pickup.OnCollected += HandlePickupCollected;
            body.SetActive(false);
            return pickup;
        }

        GameObject CreateVisual(Transform parent, string name, GameObject prefab,
                                PrimitiveType fallback, Material material)
        {
            GameObject body = prefab != null
                ? Instantiate(prefab, parent)
                : GameObject.CreatePrimitive(fallback);

            body.name = name;
            if (prefab == null) body.transform.SetParent(parent, false);

            if (prefab == null && material != null)
            {
                body.GetComponent<MeshRenderer>().sharedMaterial = material;
            }

            return body;
        }

        void HandlePickupCollected(Combat.Pickup pickup)
        {
            if (_session != null) _session.RegisterPickup();
        }

        Transform CreateBlock(Transform parent, string name, Material material)
        {
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = name;
            body.transform.SetParent(parent, false);
            if (material != null) body.GetComponent<MeshRenderer>().sharedMaterial = material;
            return body.transform;
        }

        Material PickMaterial(int index)
        {
            if (buildingMaterials == null || buildingMaterials.Length == 0) return null;
            return buildingMaterials[index % buildingMaterials.Length];
        }

        void Place(Segment segment, int index)
        {
            segment.Index = index;

            float baseZ = index * segmentLength;
            segment.Root.position = new Vector3(0f, 0f, baseZ);

            var random = new System.Random(seed + index);

            segment.Ground.localPosition = new Vector3(0f, -0.5f, segmentLength * 0.5f);
            segment.Ground.localScale = new Vector3(corridorHalfWidth * 4f, 1f, segmentLength);

            bool roofed = ceilingHeight > 0f;
            segment.Ceiling.gameObject.SetActive(roofed);
            if (roofed)
            {
                segment.Ceiling.localPosition =
                    new Vector3(0f, ceilingHeight + 0.5f, segmentLength * 0.5f);
                segment.Ceiling.localScale =
                    new Vector3(corridorHalfWidth * 4f, 1f, segmentLength);
            }

            for (int i = 0; i < segment.Buildings.Length; i++)
            {
                bool rightSide = i % 2 == 0;
                float setback = (float)random.NextDouble() * setbackRange;
                float z = (i / 2 + (float)random.NextDouble()) * (segmentLength / buildingsPerSide);

                float x = corridorHalfWidth + setback;
                if (!rightSide) x = -x;

                segment.Buildings[i].localPosition = new Vector3(x, 0f, z);

                float yRotation = rightSide ? 180f : 0f;
                segment.Buildings[i].localRotation = Quaternion.Euler(0f, yRotation, 0f);
            }

            PlaceObstacle(segment, random);
            PlacePickups(segment, random);
        }

        void PlacePickups(Segment segment, System.Random random)
        {
            if (segment.Pickups == null) return;

            for (int i = 0; i < segment.Pickups.Length; i++)
            {
                Combat.Pickup pickup = segment.Pickups[i];
                if (pickup == null) continue;

                if (random.NextDouble() >= pickupChance)
                {
                    pickup.gameObject.SetActive(false);
                    continue;
                }

                float x = pickupAreaCenter.x
                        + ((float)random.NextDouble() * 2f - 1f) * pickupAreaHalfExtents.x;
                float y = pickupAreaCenter.y
                        + ((float)random.NextDouble() * 2f - 1f) * pickupAreaHalfExtents.y;
                float z = (float)random.NextDouble() * segmentLength;

                pickup.Spawn(new Vector3(x, y, z), (float)random.NextDouble() * 6.28f);
            }
        }

        void PlaceObstacle(Segment segment, System.Random random)
        {
            bool active = random.NextDouble() < obstacleChance;
            segment.Obstacle.gameObject.SetActive(active);
            if (!active) return;

            float width = corridorHalfWidth * (0.5f + (float)random.NextDouble() * 0.5f);
            float height = 6f + (float)random.NextDouble() * 10f;
            float centerY = 8f + (float)random.NextDouble() * 12f;
            float x = (random.NextDouble() < 0.5 ? -1f : 1f) * (corridorHalfWidth - width * 0.5f);

            segment.Obstacle.localPosition = new Vector3(x, centerY, segmentLength * 0.5f);
            segment.Obstacle.localScale = new Vector3(width, height, 4f);
        }

        static float Range(System.Random random, Vector2 range)
        {
            return Mathf.Lerp(range.x, range.y, (float)random.NextDouble());
        }
    }
}
