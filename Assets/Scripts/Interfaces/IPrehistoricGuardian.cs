namespace VaultsOfTheElixir.Interfaces
{
    /// <summary>
    /// Additive interface for prehistoric-era guardians (e.g. Rex Warden).
    /// Extends IEnemyBehaviour with behaviour unique to this archetype:
    /// a telegraphed roar (warns the player, builds tension) followed by
    /// a heavy charge attack. Only guardians of this era implement it,
    /// so the shared enemy systems (pooling, targeting) never need to
    /// know it exists.
    /// </summary>
    public interface IPrehistoricGuardian : IEnemyBehaviour
    {
        /// <summary>Telegraphed roar that precedes a charge — gives the player a dodge window.</summary>
        void Roar();

        /// <summary>Heavy, high-damage charge attack along a straight line.</summary>
        void ChargeAttack();
    }
}
