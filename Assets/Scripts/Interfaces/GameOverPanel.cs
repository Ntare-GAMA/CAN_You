using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.UI
{
    /// <summary>
    /// Shown when the player's health reaches 0 (GameEvents.OnPlayerDied).
    /// Displays a fail message, then automatically reloads the current
    /// scene after a short delay. A Main Menu button is also available
    /// for the player to leave early instead of waiting for the reset.
    /// </summary>
    public class GameOverPanel : MonoBehaviour
    {
        [Header("UI refs")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text messageText;
        [SerializeField] private Button mainMenuButton;

        [Header("Timing")]
        [SerializeField] private float resetDelaySeconds = 3f;

        [Header("Message")]
        [SerializeField] private string failMessage = "You Failed";

        [Header("Main Menu scene")]
        [SerializeField] private string mainMenuSceneName = "HOME";

        private Coroutine _resetRoutine;

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
            GameEvents.OnPlayerDied += HandlePlayerDied;
        }

        private void OnDisable()
        {
            GameEvents.OnPlayerDied -= HandlePlayerDied;
        }

        private void HandlePlayerDied()
        {
            if (messageText != null) messageText.text = failMessage;
            if (panelRoot != null) panelRoot.SetActive(true);

            _resetRoutine = StartCoroutine(ResetAfterDelay());
        }

        private IEnumerator ResetAfterDelay()
        {
            yield return new WaitForSecondsRealtime(resetDelaySeconds);

            Time.timeScale = 1f; // in case anything paused it
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnMainMenuClicked()
        {
            if (_resetRoutine != null) StopCoroutine(_resetRoutine);

            Time.timeScale = 1f;
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }
}
