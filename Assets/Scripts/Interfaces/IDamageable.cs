namespace VaultsOfTheElixir.Interfaces
{
    /// <summary>
    /// Implemented by anything that can take damage and die: the player,
    /// all enemy guardians, and any breakable/destructible objects.
    /// Keeping this separate from IEnemyBehaviour means combat code
    /// (e.g. a weapon's hit detection) only needs to know about
    /// IDamageable, not what kind of object it hit.
    /// </summary>
    public interface IDamageable
    {
        int CurrentHealth { get; }
        int MaxHealth { get; }

        /// <summary>Apply damage. Implementations should clamp health at 0 and call Die() when it hits 0.</summary>
        void TakeDamage(int amount);

        /// <summary>Handle death: play animation/vfx, raise events, disable object, etc.</summary>
        void Die();
    }
}
