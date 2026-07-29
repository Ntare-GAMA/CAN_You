using UnityEngine;
using UnityEngine.SceneManagement;

namespace VaultsOfTheElixir.Core
{
    /// <summary>
    /// Singleton. Owns the current GameState and the currently-loaded
    /// vault index. Also owns the scene-per-vault loading logic: each
    /// vault index maps to a scene name (added to Build Settings), and
    /// loading a level actually swaps the active scene, then repositions
    /// the persistent Player object to that scene's spawn point.
    ///
    /// Deliberately still kept fairly thin otherwise — it does NOT know
    /// about specific enemies or UI screens directly, it just tracks
    /// state/scene and raises GameEvents.OnGameStateChanged, so anything
    /// that cares (pause menu, HUD, audio) subscribes instead of
    /// GameManager reaching out to each of them.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scene names, index-aligned with vault index (0-4)")]
        [SerializeField]
        private string[] vaultSceneNames = new string[]
        {
            "Vault0_Scorpion",
            "Vault1_Anaconda",
            "Vault2_ArmedGuard",
            "Vault3_Dragon",
            "Vault4_Dinosaurs"
        };

        [Tooltip("Name of an empty GameObject in each vault scene marking where the Player should appear.")]
        [SerializeField] private string playerSpawnPointName = "PlayerSpawnPoint";

        [SerializeField] private GameState _currentState = GameState.MainMenu;
        public GameState CurrentState => _currentState;

        /// <summary>Index of the vault currently loaded (0-4).</summary>
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

        private void OnEnable() => SceneManager.sceneLoaded += HandleSceneLoaded;
        private void OnDisable() => SceneManager.sceneLoaded -= HandleSceneLoaded;

        public void SetState(GameState newState)
        {
            if (_currentState == newState) return;

            _currentState = newState;
            GameEvents.RaiseGameStateChanged(newState);

            // Pause/resume simulation time alongside state changes.
            Time.timeScale = (newState == GameState.Paused) ? 0f : 1f;
        }

        /// <summary>Loads the scene for the given vault index and switches to Playing state.</summary>
        public void LoadLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= vaultSceneNames.Length)
            {
                Debug.LogError($"[GameManager] Invalid vault index {levelIndex} — no scene mapped.");
                return;
            }

            CurrentLevelIndex = levelIndex;
            SceneManager.LoadScene(vaultSceneNames[levelIndex]);
            // Player repositioning happens in HandleSceneLoaded once the
            // new scene has actually finished loading.
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // Only reposition when a vault scene loads, not the Bootstrap/menu scene.
            var spawnPoint = GameObject.Find(playerSpawnPointName);
            var player = GameObject.FindGameObjectWithTag("Player");

            if (spawnPoint != null && player != null)
            {
                player.transform.position = spawnPoint.transform.position;
            }

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