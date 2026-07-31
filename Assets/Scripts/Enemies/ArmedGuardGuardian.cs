using UnityEngine;
using VaultsOfTheElixir.Interfaces;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.Enemies
{
    /// <summary>Vault 2 guardian. Ranged pooled-projectile attacks, alerts nearby guards when it spots the player.</summary>
    public class ArmedGuardGuardian : Guardian, ITacticalGuardian
    {
        [Header("Armed Guard")]
        [SerializeField] private int shotDamage = 8;
        [SerializeField] private Transform firePoint;
        [SerializeField] private string projectilePoolKey = "guard_bullet";
        [SerializeField] private float squadCallRadius = 10f;
        [SerializeField] private AudioClip attackSound;

        public override void Attack()
        {
            animator.SetTrigger("Shoot");
            FireShot();
        }

        private void FireShot()
        {
            if (playerTransform == null || firePoint == null) return;

            AudioManager.Instance?.PlaySFX(attackSound);

            Vector2 direction = ((Vector2)playerTransform.position - (Vector2)firePoint.position).normalized;
            int scaledDamage = DifficultyCurve.ScaleDamage(shotDamage, vaultIndex);

            var bulletObj = ObjectPoolManager.Instance.Spawn(projectilePoolKey, firePoint.position, Quaternion.identity);
            bulletObj?.GetComponent<Projectile>()?.Launch(direction, scaledDamage);
        }

        public void CallSquad()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, squadCallRadius);
            foreach (var hit in hits)
            {
                var other = hit.GetComponent<ArmedGuardGuardian>();
                if (other != null && other != this && other.CurrentState == EnemyState.Idle)
                {
                    other.OnPlayerDetected(playerTransform);
                }
            }
        }

        public void TakeCover()
        {
            // A fuller build would query pre-placed cover-point markers
            // in the vault and move to the nearest one facing the player
            // before resuming fire.
            animator.SetTrigger("TakeCover");
        }

        public override void OnPlayerDetected(Transform player)
        {
            base.OnPlayerDetected(player);
            CallSquad();
        }
    }
}