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
                int scaledBite = DifficultyCurve.ScaleDamage(biteDamage, vaultIndex);
                playerTransform?.GetComponent<IDamageable>()?.TakeDamage(scaledBite);
            }
        }

        public void Roar()
        {
            animator.SetTrigger("Roar");
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

            if (playerTransform != null)
            {
                Vector2 direction = ((Vector2)playerTransform.position - rb.position).normalized;
                rb.linearVelocity = direction * chargeSpeed;

                float dist = Vector2.Distance(transform.position, playerTransform.position);
                if (dist <= chargeAttackRange)
                {
                    int scaledCharge = DifficultyCurve.ScaleDamage(chargeDamage, vaultIndex);
                    playerTransform.GetComponent<IDamageable>()?.TakeDamage(scaledCharge);
                }
            }
        }
    }
}
