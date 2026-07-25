using UnityEngine;
using VaultsOfTheElixir.Interfaces;

namespace VaultsOfTheElixir.Player
{
    /// <summary>
    /// Strategy pattern implementation: a short-range dash. PlayerController
    /// holds this behind an IAbility reference, so it can be swapped for
    /// a different ability with zero changes to PlayerController itself.
    /// </summary>
    public class EmberDashAbility : MonoBehaviour, IAbility
    {
        [SerializeField] private float dashDistance = 4f;
        [SerializeField] private float dashDuration = 0.15f;
        [SerializeField] private float cooldownDuration = 3f;

        private float _cooldownTimer;

        public string AbilityId => "ember_dash";
        public float CooldownDuration => cooldownDuration;
        public bool IsReady => _cooldownTimer <= 0f;

        private void Update()
        {
            if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
        }

        public void Activate(GameObject user)
        {
            if (!IsReady) return;

            var rb = user.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 facing = new Vector2(Mathf.Sign(user.transform.localScale.x), 0f);
                StartCoroutine(DashRoutine(rb, facing));
            }

            _cooldownTimer = cooldownDuration;
        }

        private System.Collections.IEnumerator DashRoutine(Rigidbody2D rb, Vector2 direction)
        {
            float elapsed = 0f;
            Vector2 dashVelocity = direction * (dashDistance / dashDuration);

            while (elapsed < dashDuration)
            {
                rb.linearVelocity = dashVelocity;
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }
}
