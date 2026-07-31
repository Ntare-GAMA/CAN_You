using UnityEngine;
using VaultsOfTheElixir.Interfaces;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.Enemies
{
    /// <summary>Vault 0 guardian. Simple melee + poison sting — the tutorial-tier encounter.</summary>
    public class ScorpionGuardian : Guardian, IVenomousGuardian
    {
        [Header("Scorpion")]
        [SerializeField] private int stingDamage = 12;
        [SerializeField] private AudioClip attackSound;

        public override void Attack()
        {
            animator.SetTrigger("Sting");
            AudioManager.Instance?.PlaySFX(attackSound);
            if (playerTransform != null)
            {
                PoisonSting(playerTransform.gameObject);
            }
        }

        public void PoisonSting(GameObject target)
        {
            int scaledDamage = DifficultyCurve.ScaleDamage(stingDamage, vaultIndex);
            target.GetComponent<IDamageable>()?.TakeDamage(scaledDamage);

            // A fuller build would also start a poison-over-time status
            // effect component on the target here; kept as a direct hit
            // in this scaffold to stay dependency-free.
        }

        public void Burrow()
        {
            // Briefly disables collider + rigidbody movement, plays a
            // "Burrow" animation, then re-emerges near the player for an
            // ambush restart. Wire up rb.position teleport here once the
            // burrow animation clip exists.
            animator.SetTrigger("Burrow");
        }
    }
}