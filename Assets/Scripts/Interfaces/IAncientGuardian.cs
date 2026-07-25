using UnityEngine;

namespace VaultsOfTheElixir.Interfaces
{
    /// <summary>
    /// Additive interface for ancient-era guardians (e.g. Anubis Sentinel).
    /// Extends IEnemyBehaviour with minion summoning and a debuff/curse
    /// mechanic, giving this guardian a support/attrition playstyle
    /// rather than a direct-damage one.
    /// </summary>
    public interface IAncientGuardian : IEnemyBehaviour
    {
        /// <summary>Spawns supporting minions (pulled from the object pool) to overwhelm the player.</summary>
        void SummonMinions();

        /// <summary>Applies a temporary debuff/curse to the target (e.g. reduced damage or slow).</summary>
        void ApplyCurse(GameObject target);
    }
}
