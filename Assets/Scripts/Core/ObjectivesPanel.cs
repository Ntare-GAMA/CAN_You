using UnityEngine;
using UnityEngine.UI;

namespace VaultsOfTheElixir.UI
{
    /// <summary>
    /// Shown automatically the moment gameplay starts, before the player
    /// can act — a single briefing screen listing the game's overall
    /// objectives (not per-level). Blocks play until the player presses
    /// Begin. Time is frozen while showing (Time.timeScale = 0) so
    /// nothing can happen underneath it.
    /// </summary>
    public class ObjectivesPanel : MonoBehaviour
    {
        [Header("UI refs")]
        [SerializeField] private GameObject panelRoot;   // the whole panel, toggled on/off
        [SerializeField] private Text objectivesText;    // single text block listing all objectives
        [SerializeField] private Button beginButton;

        [Header("Objectives (shown as one list, whole game)")]
        [TextArea(6, 12)]
        [SerializeField]
        private string objectivesList =
            "- Explore each Vault and collect its Relic\n" +
            "- Defeat the Guardians standing in your way\n" +
            "- Collect all 4 Relics to unlock the Final Vault\n" +
            "- Find the Elixir and complete your journey";

        private void Awake()
        {
            if (beginButton != null)
            {
                beginButton.onClick.AddListener(OnBeginClicked);
            }
        }

        private void Start()
        {
            ShowPanel();
        }

        private void ShowPanel()
        {
            if (objectivesText != null)
            {
                objectivesText.text = objectivesList;
            }

            if (panelRoot != null)
            {
                panelRoot.SetActive(true);
            }

            // Freeze gameplay until the player dismisses this.
            Time.timeScale = 0f;
        }

        private void OnBeginClicked()
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(false);
            }

            Time.timeScale = 1f;
        }
    }
}
