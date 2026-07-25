using UnityEngine;

namespace VaultsOfTheElixir.Interfaces
{
    /// <summary>
    /// Implemented by any object the player can interact with: doors,
    /// treasure chests, switches, Vault Scrolls, NPCs. The interaction
    /// system (a small controller on the player) only ever calls
    /// Interact() through this interface — it never checks "is this a
    /// door, is this a chest" with conditionals, which is what keeps
    /// the interaction system reusable per the assignment brief.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>Called when the player interacts with this object.</summary>
        /// <param name="source">The GameObject that initiated the interaction (usually the player).</param>
        void Interact(GameObject source);

        /// <summary>Whether this object can currently be interacted with (e.g. a locked door returns false).</summary>
        bool CanInteract { get; }
    }
}
