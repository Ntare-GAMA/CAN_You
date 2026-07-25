namespace VaultsOfTheElixir.Core
{
    /// <summary>
    /// Represents the overall state of the game session.
    /// Consumed by GameManager and broadcast via GameEvents.OnGameStateChanged
    /// so UI, audio, and input systems can react without direct references
    /// to GameManager.
    /// </summary>
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        LevelComplete,
        GameOver
    }
}
