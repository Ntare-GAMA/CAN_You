using UnityEngine;
using UnityEngine.UI;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.UI
{
    /// <summary>
    /// A small world-space health bar that floats above a character.
    /// Reusable for both the Player and any enemy — attach as a child
    /// of the character, positioned above their head.
    ///
    /// Player usage: leave "Auto-Subscribe To Player" checked. This
    /// bar will listen to GameEvents.OnHealthChanged automatically,
    /// same as every other system that reacts to player health.
    ///
    /// Enemy usage: uncheck "Auto-Subscribe To Player", and have the
    /// enemy's own TakeDamage() call UpdateFill(current, max) directly
    /// after applying damage — enemies don't share a global health
    /// event yet, so this is a direct, explicit call from that enemy's
    /// own script.
    /// </summary>
    public class WorldHealthBar : MonoBehaviour
    {
        [SerializeField] private Image fillImage; // Image Type = Filled, Fill Method = Horizontal
        [SerializeField] private bool autoSubscribeToPlayer = false;

        private void OnEnable()
        {
            if (autoSubscribeToPlayer)
            {
                GameEvents.OnHealthChanged += UpdateFill;
            }
        }

        private void OnDisable()
        {
            if (autoSubscribeToPlayer)
            {
                GameEvents.OnHealthChanged -= UpdateFill;
            }
        }

        /// <summary>Call this directly from an enemy's TakeDamage() to update its bar.</summary>
        public void UpdateFill(int current, int max)
        {
            if (fillImage == null || max <= 0) return;
            fillImage.fillAmount = Mathf.Clamp01((float)current / max);
        }

        private void LateUpdate()
        {
            // Keep the bar facing the camera (billboard) so it doesn't
            // rotate/skew if the character or camera turns.
            if (Camera.main != null)
            {
                transform.rotation = Camera.main.transform.rotation;
            }
        }
    }
}
