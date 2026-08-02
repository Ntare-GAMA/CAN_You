using UnityEngine;
using VaultsOfTheElixir.Core;
using VaultsOfTheElixir.Interfaces;

namespace VaultsOfTheElixir.Enemies
{
    public class LungingCrawler : MonoBehaviour, IDamageable
    {
        [SerializeField] private int maxHealth = 30;
        [SerializeField] private float lungespeed = 6f;
        [SerializeField] private float lungeRange = 4f;

        private int _currentHealth;
        private Transform _player;

        public int CurrentHealth => _currentHealth;
        public int MaxHealth => maxHealth;

        private void Awake()
        {
            _currentHealth = maxHealth;
            _player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        private void Update()
        {
            if (_player != null)
            {
                float distance = Vector2.Distance(transform.position, _player.position);
                if (distance <= lungeRange)
                {
                    transform.position = Vector2.MoveTowards(transform.position, _player.position, lungespeed * Time.deltaTime);
                }
            }
        }

        public void TakeDamage(int amount)
        {
            _currentHealth -= amount;
            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            GameEvents.RaiseEnemyDefeated(gameObject);
            Destroy(gameObject);
        }
    }
}