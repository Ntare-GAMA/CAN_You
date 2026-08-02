using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.UI
{
    /// <summary>
    /// Shown when the player finds the Elixir (GameEvents.OnElixirFound) —
    /// i.e. completed the whole game. Displays a win message; unlike
    /// GameOverPanel this does not auto-reset, since the player has
    /// finished the game rather than failed a run. A Main Menu button
    /// lets them return manually.
    /// </summary>
    public class GameWinPanel : MonoBehaviour
    {
        [Header("UI refs")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text messageText;
        [SerializeField] private Button mainMenuButton;

        [Header("Message")]
        [SerializeField] private string winMessage = "You Win!";

        [Header("Main Menu scene")]
        [SerializeField] private string mainMenuSceneName = "HOME";

        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(OnMainMenuClicked);
            }
        }

        private void OnEnable()
        {
            GameEvents.OnElixirFound += HandleElixirFound;
        }

        private void OnDisable()
        {
            GameEvents.OnElixirFound -= HandleElixirFound;
        }

        private void HandleElixirFound()
        {
            if (messageText != null) messageText.text = winMessage;
            if (panelRoot != null) panelRoot.SetActive(true);
        }

        private void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
