using UnityEngine;

namespace VaultsOfTheElixir.Interfaces
{
    /// <summary>
    /// The shared contract every guardian enemy implements, regardless of
    /// era (Rex Warden, Drakhorn, Anubis Sentinel, Blacksite Unit,
    /// The Trickster). Enemy spawning, targeting, and the object pool
    /// only ever talk to enemies through this interface, so a brand new
    /// guardian type can be added later without modifying any existing
    /// enemy code — this is the direct answer to the "add a new enemy
    /// without modifying existing systems" presentation question.
    ///
    /// Era-specific abilities (roar, breath attack, summon minions,
    /// call squad, spawn decoy) live in additive interfaces
    /// (IPrehistoricGuardian, IElementalGuardian, IAncientGuardian,
    /// ITacticalGuardian, ITricksterGuardian) that extend this one,
    /// rather than being crammed into this shared interface.
    /// </summary>
    public interface IEnemyBehaviour
    {
        EnemyState CurrentState { get; }

        /// <summary>Move according to this guardian's current state (e.g. chase the player).</summary>
        void Move();

        /// <summary>Perform this guardian's attack.</summary>
        void Attack();

        /// <summary>Called by detection logic (trigger volume / vision cone) when the player enters range.</summary>
        void OnPlayerDetected(Transform player);
    }
}
