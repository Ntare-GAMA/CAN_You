using UnityEngine;
using VaultsOfTheElixir.Interfaces;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.Enemies
{
    [RequireComponent(typeof(Collider2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private string poolKey = "dragon_fireball";
        [SerializeField] private float speed = 10f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float lifetime = 4f;

        private float _timer;
        private Vector2 _direction;
        private GameObject _owner;

        /// <summary>Call immediately after Spawn(). owner is excluded from damage so shooters don't hit themselves.</summary>
        public void Launch(Vector2 direction, int damageOverride = -1, GameObject owner = null)
        {
            _direction = direction.normalized;
            _timer = 0f;
            if (damageOverride > 0) damage = damageOverride;
            _owner = owner;
        }

        private void OnEnable() => _timer = 0f;

        private void Update()
        {
            transform.position += (Vector3)(_direction * speed * Time.deltaTime);
            _timer += Time.deltaTime;
            if (_timer >= lifetime) ReturnToPool();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject == _owner) return; // don't hit whoever fired it
            if (other.CompareTag("Environment"))
            {
                ReturnToPool();
                return;
            }

            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                ReturnToPool();
            }
        }

        private void ReturnToPool()
        {
            ObjectPoolManager.Instance.Return(poolKey, gameObject);
        }
    }
}