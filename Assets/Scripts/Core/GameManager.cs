using UnityEngine;

namespace VaultsOfTheElixir.Core
{
    /// <summary>
    /// Singleton. Owns the current GameState and the currently-loaded
    /// vault index. Deliberately kept thin: it does NOT know about
    /// specific enemies, UI screens, or save data directly — it just
    /// tracks state and raises GameEvents.OnGameStateChanged, so
    /// anything that cares (pause menu, input system, audio) subscribes
    /// instead of GameManager reaching out to each of them.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameState _currentState = GameState.MainMenu;
        public GameState CurrentState => _currentState;

        /// <summary>Index of the vault currently loaded (0-4 = the five guardian vaults, 5 = Elixir Sanctum).</summary>
        public int CurrentLevelIndex { get; private set; } = 0;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;

            _currentState = newState;
            GameEvents.RaiseGameStateChanged(newState);

            // Pause/resume simulation time alongside state changes.
            Time.timeScale = (newState == GameState.Paused) ? 0f : 1f;
        }

        public void LoadLevel(int levelIndex)
        {
            CurrentLevelIndex = levelIndex;
            // Actual scene load call (SceneManager.LoadScene) is added once
            // level scenes exist — kept out for now so this compiles standalone.
            SetState(GameState.Playing);
        }

        public void CompleteCurrentLevel()
        {
            GameEvents.RaiseLevelCompleted(CurrentLevelIndex);
            SetState(GameState.LevelComplete);
        }

        public void TriggerGameOver()
        {
            SetState(GameState.GameOver);
        }
    }
}
