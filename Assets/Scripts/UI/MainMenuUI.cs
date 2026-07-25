using UnityEngine;
using UnityEngine.UI;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.UI
{
    /// <summary>
    /// The first screen the player sees. Handles the 5 required Main Menu
    /// buttons: Start Game, Continue Game, Level Selection, Settings, Exit.
    ///
    /// Wiring in the Inspector: assign each Button field, then this
    /// script hooks up their onClick listeners in Awake — you don't need
    /// to manually wire onClick() in the Editor UI at all, which keeps
    /// all menu logic in one readable place instead of scattered across
    /// Inspector event lists.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Buttons (assign in Inspector)")]
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button continueGameButton;
        [SerializeField] private Button levelSelectButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button exitButton;

        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;

        [Header("Confirmation (for overwriting an existing save)")]
        [SerializeField] private GameObject newGameConfirmPanel;

        private void Awake()
        {
            startGameButton.onClick.AddListener(OnStartGameClicked);
            continueGameButton.onClick.AddListener(OnContinueClicked);
            levelSelectButton.onClick.AddListener(OnLevelSelectClicked);
            settingsButton.onClick.AddListener(OnSettingsClicked);
            exitButton.onClick.AddListener(OnExitClicked);
        }

        private void OnEnable()
        {
            // Continue is only meaningful if a save already exists —
            // disable it rather than let it silently start a fresh game,
            // which would be confusing.
            continueGameButton.interactable = SaveManager.Instance.HasSaveFile;
        }

        private void OnStartGameClicked()
        {
            if (SaveManager.Instance.HasSaveFile)
            {
                // Existing progress would be wiped — confirm first rather
                // than silently overwriting it.
                if (newGameConfirmPanel != null)
                {
                    newGameConfirmPanel.SetActive(true);
                    return;
                }
            }

            BeginNewGame();
        }

        /// <summary>Wired to the "Yes, overwrite" button on newGameConfirmPanel, if you add one.</summary>
        public void ConfirmNewGame()
        {
            if (newGameConfirmPanel != null) newGameConfirmPanel.SetActive(false);
            BeginNewGame();
        }

        public void CancelNewGameConfirm()
        {
            if (newGameConfirmPanel != null) newGameConfirmPanel.SetActive(false);
        }

        private void BeginNewGame()
        {
            SaveManager.Instance.ResetSave();
            GameManager.Instance.LoadLevel(0); // Vault 0 — Scorpion, the tutorial-tier vault
        }

        private void OnContinueClicked()
        {
            // SaveManager already loaded CurrentSave on Awake — simply
            // move to Playing state. Which vault/hub position the player
            // lands in is a scene-specific detail handled wherever your
            // hub scene reads GameManager.CurrentLevelIndex on load.
            GameManager.Instance.SetState(GameState.Playing);
        }

        private void OnLevelSelectClicked()
        {
            UIManager.Instance.OpenLevelSelect();
        }

        private void OnSettingsClicked()
        {
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        private void OnExitClicked()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
