using System.Collections.Generic;
using UnityEngine;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.Enemies
{
    /// <summary>
    /// Coordinator for Vault 4 (the final vault). Unlike every other
    /// vault — which completes when a single guardian's relic is
    /// collected — this vault has three concurrent Dinosaur guardians
    /// and no per-guardian relic drop. Instead, this manager listens for
    /// GameEvents.OnEnemyDefeated, tracks which of its three assigned
    /// Dinosaurs have died, and once all three are down, spawns a single
    /// "Elixir of Life" pickup (a GuardianRelic configured with
    /// vaultIndex = 4) at a designated spawn point. Walking into that
    /// pickup completes the vault the same way every other relic does,
    /// which in turn raises GameEvents.OnElixirFound — the game's true
    /// win condition.
    /// </summary>
    public class DinosaurPackManager : MonoBehaviour
    {
        [Tooltip("Assign all 3 DinosaurGuardian GameObjects for this vault.")]
        [SerializeField] private List<GameObject> dinosaurs = new List<GameObject>();

        [Tooltip("The Elixir of Life pickup prefab (a GuardianRelic with Vault Index = 4).")]
        [SerializeField] private GameObject elixirPickupPrefab;

        [Tooltip("Where the Elixir pickup appears once all 3 Dinosaurs are defeated.")]
        [SerializeField] private Transform elixirSpawnPoint;

        private readonly HashSet<GameObject> _defeated = new HashSet<GameObject>();
        private bool _elixirSpawned;

        private void OnEnable() => GameEvents.OnEnemyDefeated += HandleEnemyDefeated;
        private void OnDisable() => GameEvents.OnEnemyDefeated -= HandleEnemyDefeated;

        private void HandleEnemyDefeated(GameObject enemy)
        {
            if (_elixirSpawned) return;
            if (!dinosaurs.Contains(enemy)) return; // ignore anything that isn't one of this vault's 3 dinosaurs

            _defeated.Add(enemy);

            if (_defeated.Count >= dinosaurs.Count)
            {
                SpawnElixir();
            }
        }

        private void SpawnElixir()
        {
            _elixirSpawned = true;

            if (elixirPickupPrefab != null && elixirSpawnPoint != null)
            {
                Instantiate(elixirPickupPrefab, elixirSpawnPoint.position, Quaternion.identity);
            }
        }
    }
}
