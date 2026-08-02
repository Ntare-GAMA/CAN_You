using UnityEngine;
using VaultsOfTheElixir.Interfaces;

namespace VaultsOfTheElixir.Player
{
    /// <summary>
    /// Basic player melee attack. On input, checks a small area in front
    /// of the player for anything implementing IDamageable and damages
    /// it — this is what lets the player actually fight back against
    /// guardians, separate from PlayerController (which only handles
    /// taking damage, not dealing it) and separate from IAbility
    /// (dash/pulse/ward are special moves on a cooldown, this is the
    /// player's basic recurring attack).
    /// </summary>
    public class PlayerAttack : MonoBehaviour
    {
        [SerializeField] private int attackDamage = 15;
        [SerializeField] private float attackRange = 1.2f;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private KeyCode attackKey = KeyCode.F;
        private float _cooldownTimer;

        private void Update()
        {
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
            }

            if (Input.GetKeyDown(attackKey) && _cooldownTimer <= 0f)
            {
                PerformAttack();
                _cooldownTimer = attackCooldown;
            }
        }

        /// <summary>
        /// Public entry point for a UI attack button's OnClick — same cooldown
        /// gating as the keyboard path. Wire a Button's OnClick to this in the
        /// Inspector for touch/web builds.
        /// </summary>
        public void TryAttack()
        {
            if (_cooldownTimer > 0f) return;
            PerformAttack();
            _cooldownTimer = attackCooldown;
        }

        private void PerformAttack()
        {
            Vector2 facingDirection = new Vector2(Mathf.Sign(transform.localScale.x), 0f);
            Vector2 attackPoint = (Vector2)transform.position + facingDirection * (attackRange * 0.5f);

            var hits = Physics2D.OverlapCircleAll(attackPoint, attackRange * 0.5f);

            foreach (var hit in hits)
            {
                if (hit.gameObject == gameObject) continue;

                var damageable = hit.GetComponent<IDamageable>();
                damageable?.TakeDamage(attackDamage);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Vector2 facingDirection = new Vector2(Mathf.Sign(transform.localScale.x), 0f);
            Vector2 attackPoint = (Vector2)transform.position + facingDirection * (attackRange * 0.5f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint, attackRange * 0.5f);
        }
    }
}
