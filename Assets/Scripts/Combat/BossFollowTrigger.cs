using UnityEngine;
using RemoteChase.Game;
using RemoteChase.Drone;

namespace RemoteChase.Combat
{
    [AddComponentMenu("Remote Chase/Boss Follow Trigger")]
    public class BossFollowTrigger : MonoBehaviour
    {
        [Header("Configuração de Aparição")]
        [Tooltip("Distância percorrida DENTRO da Fase 2 para o chefe aparecer.")]
        public float spawnDistanceInPhase2 = 300f;

        [Tooltip("O objeto do Boss_HiveDrone.")]
        public GameObject bossObject;

        private DroneRailController droneController;
        private PhaseDirector phaseDirector;
        private bool bossSpawned = false;

        void Start()
        {
            droneController = FindAnyObjectByType<DroneRailController>();
            phaseDirector = FindAnyObjectByType<PhaseDirector>();

            if (bossObject != null)
            {
                bossObject.SetActive(false); // Começa oculto
            }
        }

        void Update()
        {
            if (bossSpawned || bossObject == null || phaseDirector == null || droneController == null) return;

            // Verifica se estamos na Fase 2 (Índice 1)
            if (phaseDirector.CurrentIndex >= 1)
            {
                // Usa a distância específica da fase atual (PhaseDistance) que reseta na troca de fase!
                if (droneController.PhaseDistance >= spawnDistanceInPhase2)
                {
                    SpawnAndEngage();
                }
            }
        }

        void SpawnAndEngage()
        {
            bossSpawned = true;
            Debug.Log("🚨 CHEFE ATIVADO NA FASE 2! O Drone Colmeia iniciou a perseguição!");
            bossObject.SetActive(true);
        }
    }
}
