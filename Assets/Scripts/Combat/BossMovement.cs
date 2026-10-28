using UnityEngine;
using RemoteChase.Drone;

namespace RemoteChase.Combat
{
    [AddComponentMenu("Remote Chase/Boss Rail Follower")]
    public class BossRailFollower : MonoBehaviour
    {
        [Header("Alvo")]
        public DroneRailController drone;

        [Header("Posicionamento à Frente do Jogador")]
        public float forwardOffset = 140f; // Distância fixa à frente do drone na pista
        public float heightOffset = 6f;

        [Header("Flutuação Dinâmica (Vaivém)")]
        public float strafeAmplitude = 6f;
        public float strafeSpeed = 1.0f;
        public float bobAmplitude = 2.5f;
        public float bobSpeed = 1.5f;

        private float _phaseOffset;

        void Start()
        {
            if (drone == null) drone = FindAnyObjectByType<DroneRailController>();
            _phaseOffset = Random.Range(0f, 10f);
        }

        void Update()
        {
            if (drone == null) return;

            // O chefe se move junto com o trilho do drone, mantendo-se sempre à frente dele
            float targetZ = drone.transform.position.z + forwardOffset;

            // Movimento orgânico lateral e vertical (estilo sentinela avançado)
            float targetX = Mathf.Sin(Time.time * strafeSpeed + _phaseOffset) * strafeAmplitude;
            float targetY = heightOffset + Mathf.Sin(Time.time * bobSpeed + _phaseOffset * 1.5f) * bobAmplitude;

            transform.position = new Vector3(targetX, targetY, targetZ);
        }
    }
}
