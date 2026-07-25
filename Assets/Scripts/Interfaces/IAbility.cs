using UnityEngine;

namespace VaultsOfTheElixir.Interfaces
{
    /// <summary>
    /// The Strategy pattern contract for player abilities (e.g. Ember Dash,
    /// Vault Pulse). PlayerController holds a reference typed as IAbility
    /// for each equipped ability slot, so swapping which concrete ability
    /// occupies a slot requires zero changes to PlayerController itself —
    /// that's the whole point of the pattern here.
    /// </summary>
    public interface IAbility
    {
        string AbilityId { get; }
        float CooldownDuration { get; }
        bool IsReady { get; }

        /// <summary>Execute the ability's effect on/around the user.</summary>
        void Activate(GameObject user);
    }
}
