using UnityEngine;
using VaultsOfTheElixir.Interfaces;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.Enemies
{
    /// <summary>
    /// Vault 4 (final) guardian. Three of these run concurrently, making
    /// this the hardest encounter in the game — the Elixir of Life is
    /// found once all three are defeated and their combined relic
    /// conditions are met. Alternates a straightforward bite with a
    /// telegraphed roar-then-charge, so the fight isn't just "stand in
    /// melee range and trade hits."
    /// </summary>
    public class DinosaurGuardian : Guardian, IPrehistoricGuardian
    {
        [Header("Dinosaur")]
        [SerializeField] private int biteDamage = 22;
        [SerializeField] private int chargeDamage = 30;
        [SerializeField] private float chargeSpeed = 6f;
        [SerializeField] private float roarTelegraphDuration = 1f;
        [SerializeField] private float chargeAttackRange = 1.2f;

        [Header("Audio")]
        [SerializeField] private AudioClip biteSound;
        [SerializeField] private AudioClip roarSound;
        [SerializeField] private AudioClip chargeSound;

        private bool _isCharging;

        public override void Attack()
        {
            if (!_isCharging && Random.value < 0.4f)
            {
                Roar();
            }
            else
            {
                animator.SetTrigger("Bite");
                AudioManager.Instance?.PlaySFX(biteSound);
                int scaledBite = DifficultyCurve.ScaleDamage(biteDamage, vaultIndex);
                playerTransform?.GetComponent<IDamageable>()?.TakeDamage(scaledBite);
            }
        }

        public void Roar()
        {
            animator.SetTrigger("Roar");
            AudioManager.Instance?.PlaySFX(roarSound);
            StartCoroutine(ChargeAfterRoar());
        }

        private System.Collections.IEnumerator ChargeAfterRoar()
        {
            _isCharging = true;
            rb.linearVelocity = Vector2.zero; // hold still during telegraph — gives the player a dodge window
            yield return new WaitForSeconds(roarTelegraphDuration);
            ChargeAttack();
            yield return new WaitForSeconds(0.4f); // brief charge-lunge window before returning to normal AI
            _isCharging = false;
        }

        public void ChargeAttack()
        {
            animator.SetTrigger("Charge");
            AudioManager.Instance?.PlaySFX(chargeSound);

            if (playerTransform != null)
            {
                float horizontalDirection = Mathf.Sign(playerTransform.position.x - rb.position.x);

                // Same principle as the base Guardian.Move() fix — never
                // overwrite vertical velocity directly, let gravity own it.
                float verticalVelocity = rb.linearVelocity.y;
                rb.linearVelocity = new Vector2(horizontalDirection * chargeSpeed, verticalVelocity);

                float horizontalDist = Mathf.Abs(transform.position.x - playerTransform.position.x);
                if (horizontalDist <= chargeAttackRange)
                {
                    int scaledCharge = DifficultyCurve.ScaleDamage(chargeDamage, vaultIndex);
                    playerTransform.GetComponent<IDamageable>()?.TakeDamage(scaledCharge);
                }
            }
        }
    }
}