using UnityEngine;

namespace VaultsOfTheElixir.Interfaces
{
    /// <summary>
    /// Additive interface for venomous guardians (e.g. the Scorpion in
    /// Vault 0). Extends IEnemyBehaviour with a poison sting and a
    /// burrow/ambush-reposition move, giving it a distinct playstyle
    /// from the other archetypes' direct combat approaches.
    /// </summary>
    public interface IVenomousGuardian : IEnemyBehaviour
    {
        /// <summary>Delivers a melee sting that applies immediate damage (and, in a fuller build, a poison DoT).</summary>
        void PoisonSting(GameObject target);

        /// <summary>Burrows underground briefly to reposition for an ambush.</summary>
        void Burrow();
    }
}
