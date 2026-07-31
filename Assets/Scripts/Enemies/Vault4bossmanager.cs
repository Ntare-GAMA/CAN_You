using UnityEngine;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.Enemies
{
    /// <summary>
    /// Vault 4 (Dinosaurs) is completed by defeating all three Dinosaur
    /// guardians rather than by picking up a relic drop. Place ONE of
    /// these on an empty GameObject in the Vault4_Dinosaurs scene, then
    /// wire each of the three DinosaurGuardian's "On Death" event
    /// (on the Guardian component) to call RegisterDinosaurDefeated()
    /// on this object.
    ///
    /// On the third kill, this calls LevelManager.Instance.CollectRelic
    /// directly — same completion path everything else uses, just
    /// triggered by a kill count instead of a relic trigger.
    /// </summary>
    public class Vault4BossManager : MonoBehaviour
    {
        [Tooltip("This should be 4 (Vault 4 is the final vault, index 4).")]
        [SerializeField] private int vaultIndex = 4;

        [Tooltip("How many Dinosaur guardians must die before the vault completes.")]
        [SerializeField] private int requiredKills = 3;

        private int _killCount;

        public void RegisterDinosaurDefeated()
        {
            _killCount++;
            Debug.Log($"[Vault4BossManager] Dinosaur defeated ({_killCount}/{requiredKills}).");

            if (_killCount >= requiredKills)
            {
                LevelManager.Instance.CollectRelic(vaultIndex);
            }
        }
    }
}