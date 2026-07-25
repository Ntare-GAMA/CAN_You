using UnityEngine;

namespace VaultsOfTheElixir.Interfaces
{
    /// <summary>
    /// Additive interface for reptilian/jungle-era guardians (e.g. the
    /// Anaconda in Vault 2). Extends IEnemyBehaviour with a constrict
    /// grapple attack and camouflage/ambush behaviour — a damage-over-time,
    /// escape-the-grapple playstyle distinct from every other guardian
    /// archetype's direct-damage approach.
    /// </summary>
    public interface IReptilianGuardian : IEnemyBehaviour
    {
        /// <summary>Grapples the target, dealing damage over time until they break free (e.g. by mashing an input).</summary>
        void Constrict(GameObject target);

        /// <summary>Breaks line of sight and blends into the environment before an ambush strike.</summary>
        void Camouflage();
    }
}
