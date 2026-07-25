using UnityEngine;
using VaultsOfTheElixir.Interfaces;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.Enemies
{
    /// <summary>
    /// Pooled 2D projectile used by any ranged guardian (Armed Guard's
    /// bullets, Dragon's fireballs). Always retrieved via
    /// ObjectPoolManager.Spawn() and returned via ReturnToPool() rather
    /// than Instantiate/Destroy — this is the Object Pooling pattern in
    /// action for frequently created/destroyed objects.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private string poolKey = "dragon_fireball";
        [SerializeField] private float speed = 10f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float lifetime = 4f;

        private float _timer;
        private Vector2 _direction;

        /// <summary>Call immediately after Spawn() to set travel direction and (optionally) an already-difficulty-scaled damage value.</summary>
        public void Launch(Vector2 direction, int damageOverride = -1)
        {
            _direction = direction.normalized;
            _timer = 0f;
            if (damageOverride > 0) damage = damageOverride;
        }

        private void OnEnable() => _timer = 0f;

        private void Update()
        {
            transform.position += (Vector3)(_direction * speed * Time.deltaTime);

            _timer += Time.deltaTime;
            if (_timer >= lifetime)
            {
                ReturnToPool();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                other.GetComponent<IDamageable>()?.TakeDamage(damage);
                ReturnToPool();
            }
            else if (other.CompareTag("Environment"))
            {
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            ObjectPoolManager.Instance.Return(poolKey, gameObject);
        }
    }
}
