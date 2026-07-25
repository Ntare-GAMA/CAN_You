using UnityEngine;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.UI
{
    /// <summary>
    /// Singleton. The single source of truth for "what's on screen."
    /// Every UI panel is a GameObject assigned here; UIManager subscribes
    /// to GameEvents.OnGameStateChanged / OnLevelCompleted / OnElixirFound
    /// / OnPlayerDied and shows exactly one panel at a time in response —
    /// no panel script ever reaches into another panel or polls state
    /// itself. This is the Observer pattern applied directly to UI, and
    /// it's what satisfies "UI should react dynamically using events"
    /// rather than screens being manually toggled by scattered calls.
    ///
    /// Note on Level Select and Pause: these can layer OVER the Gameplay
    /// HUD rather than fully replacing it (e.g. Pause is a semi-transparent
    /// overlay). ShowOnly() below assumes exclusive full-screen panels;
    /// if you want an overlay behaviour for Pause specifically, call
    /// SetPanelActive() directly instead of routing it through ShowOnly().
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Full-screen panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private GameObject hudPanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject levelCompletePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject elixirWinPanel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.OnGameStateChanged += HandleGameStateChanged;
            GameEvents.OnLevelCompleted += HandleLevelCompleted;
            GameEvents.OnElixirFound += HandleElixirFound;
            GameEvents.OnPlayerDied += HandlePlayerDied;
        }

        private void OnDisable()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;
            GameEvents.OnLevelCompleted -= HandleLevelCompleted;
            GameEvents.OnElixirFound -= HandleElixirFound;
            GameEvents.OnPlayerDied -= HandlePlayerDied;
        }

        private void Start()
        {
            // First screen the player sees.
            ShowOnly(mainMenuPanel);
        }

        private void HandleGameStateChanged(GameState newState)
        {
            switch (newState)
            {
                case GameState.MainMenu:
                    ShowOnly(mainMenuPanel);
                    break;
                case GameState.Playing:
                    ShowOnly(hudPanel);
                    break;
                case GameState.Paused:
                    // Overlay, not exclusive — HUD stays visible underneath.
                    SetPanelActive(pausePanel, true);
                    break;
                case GameState.LevelComplete:
                    // LevelComplete/ElixirFound panels are shown by their
                    // own dedicated handlers below (HandleLevelCompleted /
                    // HandleElixirFound), since OnElixirFound needs to
                    // override the generic level-complete screen.
                    break;
                case GameState.GameOver:
                    ShowOnly(gameOverPanel);
                    break;
            }

            // Leaving Paused should hide the overlay regardless of which
            // state we're moving TO.
            if (newState != GameState.Paused)
            {
                SetPanelActive(pausePanel, false);
            }
        }

        private void HandleLevelCompleted(int levelIndex)
        {
            // Vault 4's completion also raises OnElixirFound — that
            // handler runs afterward (event order: LevelManager raises
            // OnLevelCompleted first, then OnElixirFound at the end of
            // MarkLevelCompleted), so ElixirWin ends up shown instead,
            // overriding this one for the final vault specifically.
            ShowOnly(levelCompletePanel);
        }

        private void HandleElixirFound()
        {
            ShowOnly(elixirWinPanel);
        }

        private void HandlePlayerDied()
        {
            ShowOnly(gameOverPanel);
        }

        /// <summary>Shows exactly one full-screen panel, hiding all the others in that group.</summary>
        public void ShowOnly(GameObject panelToShow)
        {
            SetPanelActive(mainMenuPanel, panelToShow == mainMenuPanel);
            SetPanelActive(levelSelectPanel, panelToShow == levelSelectPanel);
            SetPanelActive(hudPanel, panelToShow == hudPanel);
            SetPanelActive(levelCompletePanel, panelToShow == levelCompletePanel);
            SetPanelActive(gameOverPanel, panelToShow == gameOverPanel);
            SetPanelActive(elixirWinPanel, panelToShow == elixirWinPanel);
        }

        /// <summary>Opens the Level Select panel as an overlay/navigation step from the Main Menu (not tied to a GameState change).</summary>
        public void OpenLevelSelect() => ShowOnly(levelSelectPanel);

        private void SetPanelActive(GameObject panel, bool active)
        {
            if (panel != null) panel.SetActive(active);
        }
    }
}
