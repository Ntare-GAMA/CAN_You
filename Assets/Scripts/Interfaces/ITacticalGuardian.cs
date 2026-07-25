namespace VaultsOfTheElixir.Interfaces
{
    /// <summary>
    /// Additive interface for modern tactical guardians (e.g. Blacksite
    /// Unit). Extends IEnemyBehaviour with squad coordination and
    /// cover-seeking behaviour, distinguishing it as the only guardian
    /// archetype that fights as a coordinated group rather than solo.
    /// </summary>
    public interface ITacticalGuardian : IEnemyBehaviour
    {
        /// <summary>Alerts nearby squad members, coordinating a group response.</summary>
        void CallSquad();

        /// <summary>Moves to the nearest valid cover point and takes a defensive firing position.</summary>
        void TakeCover();
    }
}
