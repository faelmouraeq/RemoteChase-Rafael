using System;
using UnityEngine;

namespace RemoteChase.Combat
{
    /// <summary>
    /// Tiro inimigo com sistema Homing (Teleguiado) e trava de perseguição frontal.
    /// </summary>
    [AddComponentMenu("Remote Chase/Enemy Bullet")]
    public class EnemyBullet : MonoBehaviour
    {
        [Tooltip("Velocidade em m/s.")]
        public float speed = 55f;

        [Tooltip("Dano causado ao drone.")]
        public float damage = 12f;

        [Tooltip("Tempo de vida em segundos.")]
        public float lifetime = 4f;

        [Tooltip("Distancia do drone em que o tiro conta como acerto.")]
        public float hitRadius = 1.8f;

        [Header("Mecânica Teleguiada")]
        [Tooltip("Velocidade com que o tiro corrige a rota (0 = linha reta, 5 = teleguiado).")]
        public float trackingSpeed = 3f;

        Vector3 _direction;
        float _age;
        Transform _player;
        PlayerHealth _health;
        Action<EnemyBullet> _onDone;

        public void Launch(Vector3 position, Vector3 direction, PlayerHealth target,
                           Action<EnemyBullet> onDone)
        {
            transform.position = position;
            _direction = direction.normalized;
            _health = target;
            _player = target != null ? target.transform : null;
            _age = 0f;
            _onDone = onDone;
            gameObject.SetActive(true);
        }

        void Update()
        {
            float dt = Time.deltaTime;

            if (_player != null && trackingSpeed > 0f)
            {
                // Calcula onde o drone está agora
                Vector3 targetDirection = (_player.position - transform.position).normalized;

                // O Produto Escalar (Dot) verifica se o player ainda está "à frente" da bala.
                if (Vector3.Dot(_direction, targetDirection) > 0f)
                {
                    _direction = Vector3.Slerp(_direction, targetDirection, trackingSpeed * dt).normalized;

                    if (_direction != Vector3.zero)
                    {
                        transform.rotation = Quaternion.LookRotation(_direction);
                    }
                }
            }

            // Move a bala
            transform.position += _direction * (speed * dt);

            if (_player != null
                && (_player.position - transform.position).sqrMagnitude < hitRadius * hitRadius)
            {
                if (_health != null) _health.TakeHit(damage, transform.position);
                Despawn();
                return;
            }

            _age += dt;
            if (_age >= lifetime) Despawn();
        }

        void Despawn()
        {
            gameObject.SetActive(false);
            _onDone?.Invoke(this);
            _onDone = null;
        }
    }
}
