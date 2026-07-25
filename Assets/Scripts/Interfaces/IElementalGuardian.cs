namespace VaultsOfTheElixir.Interfaces
{
    /// <summary>
    /// Additive interface for draconic/elemental guardians (e.g. Drakhorn).
    /// Extends IEnemyBehaviour with a ranged AoE breath attack and the
    /// ability to reposition aerially, distinguishing it from the
    /// grounded melee behaviour of IPrehistoricGuardian.
    /// </summary>
    public interface IElementalGuardian : IEnemyBehaviour
    {
        /// <summary>Ranged area-of-effect attack (fire breath) covering a cone in front of the guardian.</summary>
        void BreathAttack();

        /// <summary>Repositions to a new aerial vantage point, used between breath attack cooldowns.</summary>
        void TakeFlight();
    }
}
