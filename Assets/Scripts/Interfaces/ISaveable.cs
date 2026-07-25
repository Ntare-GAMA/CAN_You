namespace VaultsOfTheElixir.Interfaces
{
    /// <summary>
    /// Implemented by anything whose state must persist across play
    /// sessions (player stats, level progression, settings). SaveManager
    /// collects CaptureState() from every registered ISaveable, bundles
    /// them into SaveData, and serializes to JSON. On load, it calls
    /// RestoreState() back on each one. This means SaveManager never
    /// needs to know the concrete type of what it's saving.
    /// </summary>
    public interface ISaveable
    {
        /// <summary>Return a plain serializable snapshot of this object's current state.</summary>
        object CaptureState();

        /// <summary>Restore this object's state from a previously captured snapshot.</summary>
        void RestoreState(object state);
    }
}
