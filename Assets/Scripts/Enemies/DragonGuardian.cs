using UnityEngine;
using VaultsOfTheElixir.Interfaces;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.Enemies
{
    /// <summary>Vault 3 guardian. Ranged AoE fire-breath attack, repositions between attacks.</summary>
    public class DragonGuardian : Guardian, IElementalGuardian
    {
        [Header("Dragon")]
        [SerializeField] private int breathDamage = 18;
        [SerializeField] private Transform breathOrigin;
        [SerializeField] private string fireballPoolKey = "dragon_fireball";
        [SerializeField] private AudioClip attackSound;

        public override void Attack()
        {
            animator.SetTrigger("SpitFire");
            BreathAttack();
        }

        public void BreathAttack()
        {
            if (playerTransform == null || breathOrigin == null) return;

            AudioManager.Instance?.PlaySFX(attackSound);

            Vector2 direction = ((Vector2)playerTransform.position - (Vector2)breathOrigin.position).normalized;
            int scaledDamage = DifficultyCurve.ScaleDamage(breathDamage, vaultIndex);

            var fireballObj = ObjectPoolManager.Instance.Spawn(fireballPoolKey, breathOrigin.position, Quaternion.identity);
            fireballObj?.GetComponent<Projectile>()?.Launch(direction, scaledDamage);
        }

        public void TakeFlight()
        {
            // In 2D this repositions the dragon to a different fixed
            // vantage point in the arena between breath attacks rather
            // than a true 3D flight path. Hook up a designated waypoint
            // + rb.MovePosition() here once Vault 3's layout is in place.
            animator.SetTrigger("TakeFlight");
        }
    }
}