using UnityEngine;

namespace VaultsOfTheElixir.Core
{
    public enum LevelStatus
    {
        Locked = 0,
        Available = 1,
        Completed = 2
    }

    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

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

        public void CollectRelic(int vaultIndex)
        {
            var save = SaveManager.Instance.CurrentSave;

            Debug.Log($"[LevelManager] CollectRelic({vaultIndex}) called. Already collected? {save.relicsCollected.Contains(vaultIndex)}");

            if (save.relicsCollected.Contains(vaultIndex)) return;

            save.relicsCollected.Add(vaultIndex);
            GameEvents.RaiseRelicCollected(vaultIndex);

            MarkLevelCompleted(vaultIndex);
        }

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

            // Vault 3 -> Vault 4 always unlocks instantly on completion,
            // regardless of relic count.
            if (levelIndex == FinalVaultIndex - 1 && GetStatus(FinalVaultIndex) == LevelStatus.Locked)
            {
                save.levelStatus[FinalVaultIndex] = (int)LevelStatus.Available;
                GameEvents.RaiseLevelUnlocked(FinalVaultIndex);
            }
            else
            {
                TryUnlockFinalVault();
            }

            SaveManager.Instance.Save();

            if (levelIndex == FinalVaultIndex)
            {
                GameEvents.RaiseElixirFound();
                return; // no next scene to load — this is the end
            }

            // Option A: instant cut to the next vault scene.
            // Delegated to GameManager, which also handles player
            // spawn positioning, camera snap, and GameState transition —
            // LevelManager no longer loads scenes directly.
            int nextVaultToLoad = levelIndex + 1;
            if (nextVaultToLoad < TotalLevels)
            {
                GameManager.Instance.LoadLevel(nextVaultToLoad);
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