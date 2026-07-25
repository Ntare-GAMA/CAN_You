using UnityEngine;
using VaultsOfTheElixir.Interfaces;

namespace VaultsOfTheElixir.Player
{
    /// <summary>
    /// Strategy pattern implementation: an AoE knockback pulse, useful
    /// against groups (e.g. the 3-Dinosaur final vault). Swappable with
    /// EmberDashAbility via the same IAbility slot on PlayerController.
    /// </summary>
    public class VaultPulseAbility : MonoBehaviour, IAbility
    {
        [SerializeField] private float pulseRadius = 3f;
        [SerializeField] private float knockbackForce = 8f;
        [SerializeField] private float cooldownDuration = 6f;

        private float _cooldownTimer;

        public string AbilityId => "vault_pulse";
        public float CooldownDuration => cooldownDuration;
        public bool IsReady => _cooldownTimer <= 0f;

        private void Update()
        {
            if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
        }

        public void Activate(GameObject user)
        {
            if (!IsReady) return;

            var hits = Physics2D.OverlapCircleAll(user.transform.position, pulseRadius);
            foreach (var hit in hits)
            {
                if (hit.gameObject == user) continue;

                var enemyRb = hit.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    Vector2 direction = ((Vector2)hit.transform.position - (Vector2)user.transform.position).normalized;
                    enemyRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
                }
            }

            _cooldownTimer = cooldownDuration;
        }
    }
}
