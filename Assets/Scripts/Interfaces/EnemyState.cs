namespace VaultsOfTheElixir.Interfaces
{
    /// <summary>
    /// Shared state set for the State pattern FSM driving every guardian
    /// enemy. Individual guardians don't need every state (e.g. a
    /// stationary Guardian may never use Chase), they just ignore the
    /// ones they don't use.
    /// </summary>
    public enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Defend,
        Stagger,
        Dead
    }
}
