using UnityEngine;

namespace VaultsOfTheElixir.Interfaces
{
    /// <summary>
    /// Implemented by any pickup: inventory items, currency, and Vault
    /// Scrolls. Kept separate from IInteractable because not every
    /// collectable requires an explicit interact press (some can be
    /// collected on trigger-enter), and not every interactable is
    /// something you pick up (e.g. a lever).
    /// </summary>
    public interface ICollectable
    {
        string ItemId { get; }

        /// <summary>Called when the player collects this item.</summary>
        void Collect(GameObject collector);
    }
}
