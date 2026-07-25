using System;
using UnityEngine;

namespace VaultsOfTheElixir.Core
{
    /// <summary>
    /// Central event hub for the entire game. This is the backbone of the
    /// Observer pattern requirement: gameplay systems (player, enemies,
    /// interactables) raise events here without knowing who is listening.
    /// UI, audio, save system, and analytics all subscribe independently.
    ///
    /// Rule of thumb for this project: if System A needs to *react* to
    /// something System B did, and A and B are not directly related
    /// (e.g. UI reacting to player health, or AudioManager reacting to
    /// enemy defeat), it goes through GameEvents instead of a direct
    /// reference between A and B.
    /// </summary>
    public static class GameEvents
    {
        // --- Player ---
        /// <summary>Fired whenever player health changes. Params: current, max.</summary>
        public static event Action<int, int> OnHealthChanged;

        /// <summary>Fired when the player dies.</summary>
        public static event Action OnPlayerDied;

        /// <summary>Fired when an ability is activated. Param: ability id.</summary>
        public static event Action<string> OnAbilityActivated;

        // --- Enemies ---
        /// <summary>Fired when any enemy is defeated. Param: the enemy GameObject.</summary>
        public static event Action<GameObject> OnEnemyDefeated;

        // --- Items / Interaction ---
        /// <summary>Fired when the player collects a generic item. Param: item id.</summary>
        public static event Action<string> OnItemCollected;

        /// <summary>
        /// Fired when a vault's Guardian Relic is collected. Param: vault
        /// index. This is what LevelManager listens for to mark a vault
        /// Completed and unlock the next one — see LevelManager.CollectRelic().
        /// </summary>
        public static event Action<int> OnRelicCollected;

        // --- Levels / Progression ---
        /// <summary>Fired when a level/vault is completed. Param: level index.</summary>
        public static event Action<int> OnLevelCompleted;

        /// <summary>Fired when a previously locked level becomes available. Param: level index.</summary>
        public static event Action<int> OnLevelUnlocked;

        /// <summary>
        /// Fired when the Elixir of Life is found — the true win condition,
        /// discovered at the end of Vault 4 (the 3-Dinosaur vault) after
        /// all four Guardian Relics and the vault's guardians are defeated.
        /// Distinct from OnLevelCompleted so the win/ending screen can
        /// react separately from a normal level-complete screen.
        /// </summary>
        public static event Action OnElixirFound;

        // --- Game State ---
        /// <summary>Fired whenever the overall game state changes.</summary>
        public static event Action<GameState> OnGameStateChanged;

        // ----- Raise methods -----
        // Systems call these Raise_ methods instead of invoking the event
        // directly, which keeps null-checking in one place.

        public static void RaiseHealthChanged(int current, int max) => OnHealthChanged?.Invoke(current, max);
        public static void RaisePlayerDied() => OnPlayerDied?.Invoke();
        public static void RaiseAbilityActivated(string abilityId) => OnAbilityActivated?.Invoke(abilityId);
        public static void RaiseEnemyDefeated(GameObject enemy) => OnEnemyDefeated?.Invoke(enemy);
        public static void RaiseItemCollected(string itemId) => OnItemCollected?.Invoke(itemId);
        public static void RaiseRelicCollected(int vaultIndex) => OnRelicCollected?.Invoke(vaultIndex);
        public static void RaiseLevelCompleted(int levelIndex) => OnLevelCompleted?.Invoke(levelIndex);
        public static void RaiseLevelUnlocked(int levelIndex) => OnLevelUnlocked?.Invoke(levelIndex);
        public static void RaiseElixirFound() => OnElixirFound?.Invoke();
        public static void RaiseGameStateChanged(GameState newState) => OnGameStateChanged?.Invoke(newState);
    }
}
