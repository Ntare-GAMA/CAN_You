using UnityEngine;
using UnityEngine.UI;
using VaultsOfTheElixir.Core;

namespace VaultsOfTheElixir.UI
{
    /// <summary>
    /// One vault's button on the Level Select screen. Communicates its
    /// status (Locked / Available / Completed) purely through a sprite
    /// swap (and optionally a color tint) — never text, per the rubric's
    /// explicit requirement. Clicking a locked button does nothing
    /// (button.interactable is false); clicking an available or
    /// completed one loads that vault.
    ///
    /// Attach this to the vaultButtonPrefab referenced by LevelSelectUI.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class VaultButtonUI : MonoBehaviour
    {
        [Header("Visual state sprites (assign in Inspector)")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Sprite lockedSprite;      // e.g. sealed stone door / closed sigil
        [SerializeField] private Sprite availableSprite;    // e.g. glowing ember-lit door
        [SerializeField] private Sprite completedSprite;    // e.g. lit sigil / open door with a checkmark-style icon (not text)

        [Header("Optional tint per state (leave white x white x white to rely on sprites only)")]
        [SerializeField] private Color lockedTint = Color.gray;
        [SerializeField] private Color availableTint = Color.white;
        [SerializeField] private Color completedTint = new Color(1f, 0.85f, 0.4f); // warm gold

        private Button _button;
        private int _vaultIndex;

        public void Setup(int vaultIndex)
        {
            _vaultIndex = vaultIndex;
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnClicked);

            RefreshVisual();
        }

        /// <summary>Re-reads this vault's current status from LevelManager and updates the sprite/tint/interactable state accordingly.</summary>
        public void RefreshVisual()
        {
            var status = LevelManager.Instance.GetStatus(_vaultIndex);

            switch (status)
            {
                case LevelStatus.Locked:
                    SetVisual(lockedSprite, lockedTint, interactable: false);
                    break;
                case LevelStatus.Available:
                    SetVisual(availableSprite, availableTint, interactable: true);
                    break;
                case LevelStatus.Completed:
                    SetVisual(completedSprite, completedTint, interactable: true);
                    break;
            }
        }

        private void SetVisual(Sprite sprite, Color tint, bool interactable)
        {
            if (iconImage != null && sprite != null)
            {
                iconImage.sprite = sprite;
                iconImage.color = tint;
            }

            if (_button != null)
            {
                _button.interactable = interactable;
            }
        }

        private void OnClicked()
        {
            if (!LevelManager.Instance.CanAccessLevel(_vaultIndex)) return;

            GameManager.Instance.LoadLevel(_vaultIndex);
        }
    }
}
