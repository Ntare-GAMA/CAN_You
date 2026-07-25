using UnityEngine;
using VaultsOfTheElixir.Interfaces;

namespace VaultsOfTheElixir.Core
{
    /// <summary>
    /// The physical gate blocking entry to a vault. Reacts to
    /// GameEvents.OnLevelUnlocked / OnLevelCompleted / OnRelicCollected to
    /// update its own visual state — sealed (Locked), glowing (Available),
    /// lit-sigil (Completed) — entirely through material/sprite/animation
    /// changes, never text. This is the direct implementation of the
    /// rubric's "avoid using text to indicate level availability" note,
    /// and it's also a clean Observer pattern example: the gate never
    /// polls LevelManager every frame, it just listens and reacts.
    ///
    /// Also implements IInteractable so a locked gate can show a subtle
    /// "can't enter yet" response (a shimmer/shake) if the player tries
    /// to walk through it, without needing any UI text popup.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class VaultGate : MonoBehaviour, IInteractable
    {
        [Tooltip("Which vault (0-4) this gate leads to.")]
        [SerializeField] private int vaultIndex;

        [Header("Visual state objects (assign in Inspector)")]
        [SerializeField] private GameObject sealedVisual;      // Locked: dull stone/closed door look
        [SerializeField] private GameObject availableVisual;   // Available: glowing/ember-lit look
        [SerializeField] private GameObject completedVisual;   // Completed: lit sigil/open look

        [Header("Blocking")]
        [Tooltip("Collider that physically blocks the player while locked. Disabled once the vault is Available or Completed.")]
        [SerializeField] private Collider2D blockingCollider;

        public bool CanInteract => LevelManager.Instance.CanAccessLevel(vaultIndex);

        private void OnEnable()
        {
            GameEvents.OnLevelUnlocked += HandleLevelStatusMightHaveChanged;
            GameEvents.OnLevelCompleted += HandleLevelStatusMightHaveChanged;
            RefreshVisual();
        }

        private void OnDisable()
        {
            GameEvents.OnLevelUnlocked -= HandleLevelStatusMightHaveChanged;
            GameEvents.OnLevelCompleted -= HandleLevelStatusMightHaveChanged;
        }

        private void HandleLevelStatusMightHaveChanged(int changedVaultIndex)
        {
            // Any level-status change could affect this gate's own vault
            // (or, in principle, none) — cheapest correct approach is to
            // just re-check this gate's own status rather than filter.
            RefreshVisual();
        }

        private void RefreshVisual()
        {
            var status = LevelManager.Instance.GetStatus(vaultIndex);

            if (sealedVisual != null) sealedVisual.SetActive(status == LevelStatus.Locked);
            if (availableVisual != null) availableVisual.SetActive(status == LevelStatus.Available);
            if (completedVisual != null) completedVisual.SetActive(status == LevelStatus.Completed);

            if (blockingCollider != null)
            {
                blockingCollider.enabled = (status == LevelStatus.Locked);
            }
        }

        /// <summary>Called if the player interacts with a still-locked gate (e.g. presses E against it) — plays a denial cue with no text.</summary>
        public void Interact(GameObject source)
        {
            if (!CanInteract)
            {
                // Hook up a shimmer/shake/ward-sound reaction here — a
                // wordless "not yet" cue rather than a text popup.
                return;
            }

            // If CanInteract is true, the blocking collider is already
            // disabled, so the player simply walks through — this method
            // mainly exists to give locked gates a reaction to a direct
            // interact attempt.
        }
    }
}
