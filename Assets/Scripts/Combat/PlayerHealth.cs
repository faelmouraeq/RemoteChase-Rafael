using System;
using UnityEngine;

namespace RemoteChase.Combat
{
    /// <summary>
    /// Integridade do drone do jogador. Recebe dano de tiro inimigo e de batida
    /// em predio, e e restaurada pelos coletaveis.
    /// </summary>
    [AddComponentMenu("Remote Chase/Player Health")]
    public class PlayerHealth : MonoBehaviour, IHittable
    {
        [Tooltip("Integridade maxima.")]
        public float maxHealth = 100f;

        [Tooltip("Tempo (s) sem receber dano depois de um golpe. Evita morrer "
               + "instantaneamente ao raspar num predio.")]
        public float invulnerabilityTime = 0.7f;

        [Tooltip("Dano por metro por segundo de impacto ao bater em algo solido.")]
        public float crashDamagePerSpeed = 0.7f;

        public float Current { get; private set; }
        public float Normalized => maxHealth > 0f ? Mathf.Clamp01(Current / maxHealth) : 0f;
        public bool IsAlive => Current > 0f;
        public bool IsInvulnerable => _invulnerability > 0f;

        // NOVO: Propriedade exclusiva para saber se o escudo visual deve aparecer
        public bool HasShieldPowerUp { get; private set; }

        public event Action<float> OnDamaged;
        public event Action<float> OnHealed;
        public event Action OnDied;

        float _invulnerability;

        void Awake()
        {
            Current = maxHealth;
        }

        void OnEnable()
        {
            var rail = GetComponent<Drone.DroneRailController>();
            if (rail != null) rail.OnCrash += HandleCrash;
        }

        void OnDisable()
        {
            var rail = GetComponent<Drone.DroneRailController>();
            if (rail != null) rail.OnCrash -= HandleCrash;
        }

        void Update()
        {
            if (_invulnerability > 0f) 
            {
                _invulnerability -= Time.deltaTime;
                
                // NOVO: Desliga o status do escudo quando o tempo de invulnerabilidade acaba
                if (_invulnerability <= 0f) HasShieldPowerUp = false; 
            }
        }

        void HandleCrash(float impactSpeed)
        {
            TakeHit(impactSpeed * crashDamagePerSpeed, transform.position);
        }

        public void TakeHit(float damage, Vector3 point)
        {
            if (!IsAlive || IsInvulnerable || damage <= 0f) return;

            Current = Mathf.Max(Current - damage, 0f);
            _invulnerability = invulnerabilityTime; // Apenas dá o i-frame, NÃO ativa o HasShieldPowerUp!

            OnDamaged?.Invoke(damage);

            if (Current <= 0f) OnDied?.Invoke();
        }

        public void GrantInvulnerability(float seconds)
        {
            _invulnerability = Mathf.Max(_invulnerability, seconds);
            
            // NOVO: Marca que essa invulnerabilidade veio de um power-up!
            HasShieldPowerUp = true; 
        }

        public float InvulnerabilityLeft => Mathf.Max(_invulnerability, 0f);

        public void Heal(float amount)
        {
            if (!IsAlive || amount <= 0f) return;

            float before = Current;
            Current = Mathf.Min(Current + amount, maxHealth);

            float healed = Current - before;
            if (healed > 0f) OnHealed?.Invoke(healed);
        }
    }
}
