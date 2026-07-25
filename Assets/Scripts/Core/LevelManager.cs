using UnityEngine;

namespace VaultsOfTheElixir.Core
{
    /// <summary>Status of a single vault/level in the progression chain.</summary>
    public enum LevelStatus
    {
        Locked = 0,
        Available = 1,
        Completed = 2
    }

    /// <summary>
    /// Singleton. Owns level/vault progression rules: which vaults are
    /// locked/available/completed. Reads/writes through SaveManager so
    /// progression survives closing and reopening the app. The Level
    /// Selection UI listens to GameEvents.OnLevelUnlocked / OnLevelCompleted
    /// rather than polling this class every frame.
    ///
    /// Structure: 5 vaults total.
    ///   Vault 0 - Scorpion
    ///   Vault 1 - Anaconda
    ///   Vault 2 - Armed Guard
    ///   Vault 3 - Dragon
    ///   Vault 4 - 3 Dinosaurs (final vault — the Elixir of Life is found
    ///             at the end of THIS vault, there is no separate 6th level)
    ///
    /// Completion model: defeating a vault's guardian is not, by itself,
    /// enough to complete the vault. The guardian drops a Guardian Relic
    /// (GuardianRelic.cs, an ICollectable) on death, and the player must
    /// walk over and collect it — that collection is the real completion
    /// trigger, handled here via CollectRelic(). This gives combat,
    /// inventory, and progression each their own clean event to react to
    /// instead of one system doing everything.
    ///
    /// Vault 4 has a stricter unlock condition than the others: it
    /// requires both Vault 3 completed AND all 4 Relics (from Vaults 0-3)
    /// collected, so reaching the Elixir genuinely depends on having
    /// cleared every other guardian first.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        /// <summary>Total levels: 5 guardian vaults. The final vault (index 4) contains the Elixir.</summary>
        public const int TotalLevels = 5;
        public const int FinalVaultIndex = 4;
        private const int RelicsRequiredForFinalVault = 4;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public LevelStatus GetStatus(int levelIndex)
        {
            var save = SaveManager.Instance.CurrentSave;
            if (levelIndex < 0 || levelIndex >= save.levelStatus.Length)
                return LevelStatus.Locked;

            return (LevelStatus)save.levelStatus[levelIndex];
        }

        public bool CanAccessLevel(int levelIndex) => GetStatus(levelIndex) != LevelStatus.Locked;

        public bool HasCollectedRelic(int vaultIndex) =>
            SaveManager.Instance.CurrentSave.relicsCollected.Contains(vaultIndex);

        /// <summary>
        /// Call this when the player collects a vault's Guardian Relic
        /// (from GuardianRelic.Collect()). This is the real "complete the
        /// vault" trigger — not guardian death alone. Handles marking the
        /// vault Completed, unlocking the next vault in sequence, and
        /// checking whether the final vault's fragment requirement is met.
        /// </summary>
        public void CollectRelic(int vaultIndex)
        {
            var save = SaveManager.Instance.CurrentSave;
            if (save.relicsCollected.Contains(vaultIndex)) return; // already collected, ignore

            save.relicsCollected.Add(vaultIndex);
            GameEvents.RaiseRelicCollected(vaultIndex);

            MarkLevelCompleted(vaultIndex);
        }

        /// <summary>
        /// Marks a level completed and unlocks the next one in sequence.
        /// The final vault (index 4) is never auto-unlocked this way — it
        /// only opens via TryUnlockFinalVault() once the relic requirement
        /// is met.
        /// </summary>
        private void MarkLevelCompleted(int levelIndex)
        {
            var save = SaveManager.Instance.CurrentSave;
            if (levelIndex < 0 || levelIndex >= save.levelStatus.Length) return;

            save.levelStatus[levelIndex] = (int)LevelStatus.Completed;
            GameEvents.RaiseLevelCompleted(levelIndex);

            int next = levelIndex + 1;
            if (next < FinalVaultIndex && GetStatus(next) == LevelStatus.Locked)
            {
                save.levelStatus[next] = (int)LevelStatus.Available;
                GameEvents.RaiseLevelUnlocked(next);
            }

            TryUnlockFinalVault();
            SaveManager.Instance.Save();

            // Completing the final vault itself IS finding the Elixir.
            if (levelIndex == FinalVaultIndex)
            {
                GameEvents.RaiseElixirFound();
            }
        }

        private void TryUnlockFinalVault()
        {
            var save = SaveManager.Instance.CurrentSave;
            bool previousVaultDone = GetStatus(FinalVaultIndex - 1) == LevelStatus.Completed;
            bool hasAllRelics = save.relicsCollected.Count >= RelicsRequiredForFinalVault;

            if (previousVaultDone && hasAllRelics && GetStatus(FinalVaultIndex) == LevelStatus.Locked)
            {
                save.levelStatus[FinalVaultIndex] = (int)LevelStatus.Available;
                GameEvents.RaiseLevelUnlocked(FinalVaultIndex);
            }
        }
    }
}
