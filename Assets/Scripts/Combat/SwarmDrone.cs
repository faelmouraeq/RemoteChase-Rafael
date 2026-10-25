using System;
using RemoteChase.Drone;
using UnityEngine;

namespace RemoteChase.Combat
{
    [RequireComponent(typeof(SphereCollider))]
    [AddComponentMenu("Remote Chase/Swarm Drone")]
    public class SwarmDrone : MonoBehaviour, IHittable
    {
        [Header("Mergulho (Estilo Rail Shooter)")]
        [Tooltip("Velocidade com que avança no bote, em m/s.")]
        public float diveSpeed = 50f;

        [Header("Impacto")]
        [Tooltip("Distancia do drone em que a colisao acontece.")]
        public float hitRadius = 2.2f;

        [Tooltip("Dano causado ao se chocar com o jogador.")]
        public float impactDamage = 18f;

        [Header("Permanencia")]
        [Tooltip("Quantos metros atras do jogador ele desiste e some.")]
        public float despawnBehind = 10f;

        [Tooltip("Tempo maximo em cena, em segundos.")]
        public float lifetime = 12f;

        [Header("Morte")]
        [Tooltip("Duracao (s) do encolhimento ao morrer.")]
        public float deathDuration = 0.12f;

        public event Action<SwarmDrone, Vector3> OnKilled;
        public bool IsAlive { get; private set; }
        public DroneRailController Target { get; set; }
        public PlayerHealth TargetHealth { get; set; }

        Vector3 _baseScale;
        Vector3 _diveDirection; // Direção fixa do torpedo (sem efeito ímã ou meia-volta)
        float _deathTimer;
        float _lifeTimer;
        Collider _collider;

        void Awake()
        {
            _collider = GetComponent<Collider>();
            _baseScale = transform.localScale;
        }

        /// <summary>Lança o kamikaze sincronizado com o trilho e direcionado ao player.</summary>
        public void Launch(Vector3 worldPosition, Vector3 targetPosition)
        {
            transform.position = worldPosition;
            transform.localScale = _baseScale;

            // Trava o vetor exato de direção em linha reta do ponto de spawn até o alvo
            _diveDirection = (targetPosition - worldPosition).normalized;
            if (_diveDirection == Vector3.zero) _diveDirection = Vector3.back;

            transform.rotation = Quaternion.LookRotation(_diveDirection);

            _deathTimer = 0f;
            _lifeTimer = lifetime;
            IsAlive = true;

            if (_collider != null) _collider.enabled = true;
            gameObject.SetActive(true);
        }

        public void TakeHit(float damage, Vector3 point)
        {
            if (!IsAlive) return;

            IsAlive = false;
            _deathTimer = deathDuration;
            if (_collider != null) _collider.enabled = false;

            OnKilled?.Invoke(this, point);

            if (deathDuration <= 0f) gameObject.SetActive(false);
        }

        void Update()
        {
            if (!IsAlive)
            {
                Shrink();
                return;
            }

            if (Target == null)
            {
                gameObject.SetActive(false);
                return;
            }

            MoveAndDive();
            CheckImpact();
            CheckExpiry();
        }

        void MoveAndDive()
        {
            if (Target == null) return;

            // Pega a velocidade atual do trilho do player para acompanhar o avanço da fase
            float railSpeed = Target.ForwardSpeed;

            // Movimento de torpedo cego: avanço do trilho (Z) + vetor fixo do bote nas laterais/frente
            Vector3 railMovement = Vector3.forward * railSpeed;
            Vector3 diveMovement = _diveDirection * diveSpeed;

            Vector3 totalMovement = railMovement + diveMovement;

            // Move o drone no espaço de mundo
            transform.position += totalMovement * Time.deltaTime;

            // Aponta a frente do modelo estritamente para a direção do movimento
            if (totalMovement.sqrMagnitude > 0.01f)
            {
                transform.rotation = Quaternion.LookRotation(totalMovement);
            }
        }

        void CheckImpact()
        {
            if (Target == null) return;
            Vector3 toTarget = Target.transform.position - transform.position;
            if (toTarget.sqrMagnitude > hitRadius * hitRadius) return;

            if (TargetHealth != null)
            {
                TargetHealth.TakeHit(impactDamage, transform.position);
            }

            IsAlive = false;
            gameObject.SetActive(false);
        }

        void CheckExpiry()
        {
            if (Target == null) return;
            _lifeTimer -= Time.deltaTime;

            // Se passou por trás do jogador, despawna imediatamente para nunca dar meia-volta
            bool passed = transform.position.z < Target.transform.position.z - despawnBehind;
            
            if (!passed && _lifeTimer > 0f) return;

            IsAlive = false;
            gameObject.SetActive(false);
        }

        void Shrink()
        {
            if (_deathTimer <= 0f) return;

            _deathTimer -= Time.deltaTime;
            float t = Mathf.Clamp01(_deathTimer / Mathf.Max(deathDuration, 0.0001f));
            transform.localScale = _baseScale * t;

            if (_deathTimer <= 0f) gameObject.SetActive(false);
        }
    }
}
