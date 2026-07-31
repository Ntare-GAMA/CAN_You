using System;
using System.Collections.Generic;

namespace VaultsOfTheElixir.Data
{
    /// <summary>
    /// Plain serializable snapshot of everything that needs to persist
    /// between sessions. This is the single object JsonUtility
    /// serializes to/from disk — every ISaveable feeds into building
    /// one of these, and reads itself back out of one.
    ///
    /// LevelStatus values: 0 = Locked, 1 = Available, 2 = Completed.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        // Level progression (index-aligned with LevelManager's level list).
        // 5 vaults total:
        //   0 = Scorpion, 1 = Anaconda, 2 = Armed Guard, 3 = Dragon,
        //   4 = 3 Dinosaurs (final vault — the Elixir of Life is found at
        //   the end of THIS vault, there is no separate 6th level).
        public int[] levelStatus = new int[5];

        // Player stats
        public int playerCurrentHealth = 100;
        public int playerMaxHealth = 100;

        // Inventory: simple list of item ids. Sorting/filtering happens
        // in InventoryManager, not here — this is just the raw data.
        public List<string> inventoryItemIds = new List<string>();

        // Guardian Relics collected (indices into levelStatus, one relic
        // per guardian vault). Collecting a vault's Relic is what marks
        // that vault Completed and unlocks the next one — see
        // LevelManager.CollectRelic(). The final vault (index 4) requires
        // all 4 relics from vaults 0-3 before it becomes available.
        public List<int> relicsCollected = new List<int>();

        // Settings
        public float musicVolume = 0.75f;
        public float sfxVolume = 0.75f;
        public bool musicMuted = false;

        // Metadata
        public string lastSavedUtc = "";
    }
}