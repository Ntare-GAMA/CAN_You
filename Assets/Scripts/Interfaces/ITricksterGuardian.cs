namespace VaultsOfTheElixir.Interfaces
{
    /// <summary>
    /// Additive interface for the Trickster guardian archetype — an
    /// original rogue/mythic character built for this project. Extends
    /// IEnemyBehaviour with illusion-based decoys and short-range
    /// teleportation, making it a misdirection/hit-and-run fighter
    /// rather than a direct damage-soak like the other guardians.
    /// </summary>
    public interface ITricksterGuardian : IEnemyBehaviour
    {
        /// <summary>Spawns a decoy clone to draw player attention/attacks.</summary>
        void SpawnDecoy();

        /// <summary>Short-range teleport used to reposition or escape a flank.</summary>
        void Teleport();
    }
}
