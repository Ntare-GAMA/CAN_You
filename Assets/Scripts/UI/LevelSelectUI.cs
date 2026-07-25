using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.UI
{
    /// <summary>
    /// The Level Select screen. Dynamically spawns one button per vault
    /// (using LevelManager.TotalLevels, currently 5) from a single
    /// prefab, rather than hand-placing 5 separate buttons in the Editor
    /// — this means adding a 6th vault later only requires bumping
    /// LevelManager.TotalLevels and adding one more sprite set, not
    /// touching this script.
    ///
    /// Status is communicated entirely through sprite/color changes on
    /// each button (locked/available/completed), never text, per the
    /// rubric's explicit note. This is the UI-side counterpart to
    /// VaultGate — same status data, same three visual states, just
    /// rendered as a 2D icon instead of a 3D/2D world gate object.
    ///
    /// Reacts to GameEvents.OnLevelUnlocked / OnLevelCompleted so buttons
    /// refresh automatically the moment progression changes, rather than
    /// only updating when the panel is manually reopened.
    /// </summary>
    public class LevelSelectUI : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private Transform buttonContainer;   // parent with a Layout Group (Grid/Horizontal)
        [SerializeField] private GameObject vaultButtonPrefab; // prefab with a VaultButtonUI component

        [Header("Navigation")]
        [SerializeField] private Button backButton;

        private readonly List<VaultButtonUI> _spawnedButtons = new List<VaultButtonUI>();

        private void Awake()
        {
            if (backButton != null)
            {
                backButton.onClick.AddListener(OnBackClicked);
            }
        }

        private void OnEnable()
        {
            BuildButtons();

            GameEvents.OnLevelUnlocked += HandleProgressionChanged;
            GameEvents.OnLevelCompleted += HandleProgressionChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnLevelUnlocked -= HandleProgressionChanged;
            GameEvents.OnLevelCompleted -= HandleProgressionChanged;
        }

        private void HandleProgressionChanged(int changedVaultIndex)
        {
            RefreshAllButtons();
        }

        private void BuildButtons()
        {
            // Clear any previously spawned buttons first (covers re-opening this panel).
            foreach (Transform child in buttonContainer)
            {
                Destroy(child.gameObject);
            }
            _spawnedButtons.Clear();

            for (int i = 0; i < LevelManager.TotalLevels; i++)
            {
                var buttonObj = Instantiate(vaultButtonPrefab, buttonContainer);
                var vaultButton = buttonObj.GetComponent<VaultButtonUI>();

                if (vaultButton != null)
                {
                    vaultButton.Setup(i);
                    _spawnedButtons.Add(vaultButton);
                }
            }
        }

        private void RefreshAllButtons()
        {
            foreach (var button in _spawnedButtons)
            {
                button.RefreshVisual();
            }
        }

        private void OnBackClicked()
        {
            // Changing state is enough — UIManager already listens for
            // OnGameStateChanged and shows MainMenuPanel in response, so
            // this script never needs to touch panel visibility directly.
            GameManager.Instance.SetState(GameState.MainMenu);
        }
    }
}
