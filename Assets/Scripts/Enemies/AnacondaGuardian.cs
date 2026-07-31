using UnityEngine;
using VaultsOfTheElixir.Interfaces;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.Enemies
{
    /// <summary>Vault 1 guardian. Grapples the player with a damage-over-time constrict rather than direct hits.</summary>
    public class AnacondaGuardian : Guardian, IReptilianGuardian
    {
        [Header("Anaconda")]
        [SerializeField] private int constrictDamagePerTick = 4;
        [SerializeField] private float constrictDuration = 3f;
        [SerializeField] private float tickInterval = 0.5f;
        [SerializeField] private AudioClip attackSound;

        private bool _isConstricting;

        public override void Attack()
        {
            if (playerTransform != null && !_isConstricting)
            {
                Constrict(playerTransform.gameObject);
            }
        }

        public void Constrict(GameObject target)
        {
            _isConstricting = true;
            animator.SetTrigger("Constrict");
            AudioManager.Instance?.PlaySFX(attackSound);
            StartCoroutine(ConstrictRoutine(target));
        }

        private System.Collections.IEnumerator ConstrictRoutine(GameObject target)
        {
            float elapsed = 0f;
            var damageable = target.GetComponent<IDamageable>();
            int scaledTick = DifficultyCurve.ScaleDamage(constrictDamagePerTick, vaultIndex);

            while (elapsed < constrictDuration && damageable != null)
            {
                damageable.TakeDamage(scaledTick);
                yield return new WaitForSeconds(tickInterval);
                elapsed += tickInterval;
            }

            _isConstricting = false;
        }

        public void Camouflage()
        {
            // Temporarily reduces detectionRange and fades the sprite's
            // alpha before an ambush strike. Hook up a SpriteRenderer
            // alpha tween here once the camouflage animation/material exists.
            animator.SetTrigger("Camouflage");
        }
    }
}