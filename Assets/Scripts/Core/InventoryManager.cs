using System.Collections.Generic;
using UnityEngine;

namespace VaultsOfTheElixir.Core
{
    /// <summary>
    /// Singleton. Collects every item id raised via GameEvents.OnItemCollected
    /// (including Guardian Relics, via GuardianRelic.Collect()) into the
    /// player's inventory, persisted through SaveManager.CurrentSave —
    /// the same pattern LevelManager uses for level status, so there's
    /// one consistent way every manager reads/writes persisted data.
    ///
    /// SortInventory() is this project's required Sorting algorithm:
    /// purpose is presenting inventory in a stable, predictable order for
    /// the UI; approach is a simple custom-comparer sort (relics grouped
    /// first, then alphabetical) rather than anything exotic, because
    /// inventory lists here are small (at most ~9 items: 4 relics + misc
    /// pickups) and correctness/readability matter far more than
    /// asymptotic performance at that scale — a good README talking point
    /// for "why this algorithm was selected."
    /// </summary>
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

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

        private void OnEnable() => GameEvents.OnItemCollected += HandleItemCollected;
        private void OnDisable() => GameEvents.OnItemCollected -= HandleItemCollected;

        private void HandleItemCollected(string itemId)
        {
            var save = SaveManager.Instance.CurrentSave;
            if (!save.inventoryItemIds.Contains(itemId))
            {
                save.inventoryItemIds.Add(itemId);
                SaveManager.Instance.Save();
            }
        }

        /// <summary>Returns the current inventory in raw (collection-order) form.</summary>
        public List<string> GetInventory() => SaveManager.Instance.CurrentSave.inventoryItemIds;

        /// <summary>
        /// Returns a sorted copy of the inventory: relic items (ids
        /// starting with "relic_vault_") first, ordered by vault index,
        /// followed by all other items alphabetically. Used by the
        /// inventory UI panel so relics — the progression-critical items
        /// — always appear at the top regardless of pickup order.
        /// </summary>
        public List<string> GetSortedInventory()
        {
            var items = new List<string>(GetInventory());

            items.Sort((a, b) =>
            {
                bool aIsRelic = a.StartsWith("relic_vault_");
                bool bIsRelic = b.StartsWith("relic_vault_");

                if (aIsRelic && bIsRelic)
                {
                    int aIndex = ExtractVaultIndex(a);
                    int bIndex = ExtractVaultIndex(b);
                    return aIndex.CompareTo(bIndex);
                }

                if (aIsRelic != bIsRelic)
                {
                    // Relics sort before non-relics.
                    return aIsRelic ? -1 : 1;
                }

                return string.Compare(a, b, System.StringComparison.OrdinalIgnoreCase);
            });

            return items;
        }

        /// <summary>Simple linear search — the project's required Searching algorithm example for "searching inventory".</summary>
        public bool HasItem(string itemId) => GetInventory().Contains(itemId);

        private int ExtractVaultIndex(string relicItemId)
        {
            var suffix = relicItemId.Replace("relic_vault_", "");
            return int.TryParse(suffix, out int index) ? index : int.MaxValue;
        }
    }
}
