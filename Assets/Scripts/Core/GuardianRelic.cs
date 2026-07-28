using UnityEngine;
using VaultsOfTheElixir.Interfaces;

namespace VaultsOfTheElixir.Core
{
    /// <summary>
    /// The physical relic a guardian drops on defeat (e.g. the Scorpion's
    /// Stinger Shard, the Dragon's Ember Core). Spawn this prefab at the
    /// guardian's position when it dies, then let the player walk over
    /// and collect it — collection is what actually completes the vault,
    /// via LevelManager.CollectRelic(). This keeps "kill the boss" and
    /// "finish the level" as two distinct, separately observable events
    /// rather than one script doing both.
    ///
    /// 2D setup: attach to a prefab with a Collider2D (Is Trigger = true)
    /// and a SpriteRenderer. Auto-collects on contact with the "Player"
    /// tag via OnTriggerEnter2D.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GuardianRelic : MonoBehaviour, ICollectable
    {
        [Tooltip("Which vault (0-4) this relic belongs to.")]
        [SerializeField] private int _vaultIndex;

        [Tooltip("Display id/name for inventory and UI, e.g. 'Stinger Shard'.")]
        [SerializeField] private string _relicName = "Guardian Relic";

        [Tooltip("Optional lore text shown when the relic is collected.")]
        [TextArea]
        [SerializeField] private string _loreText;

        [Tooltip("VFX/sound to play on pickup (optional, assign a prefab or leave null).")]
        [SerializeField] private GameObject _pickupVfx;

        public string ItemId => $"relic_vault_{_vaultIndex}";
        public string RelicName => _relicName;
        public string LoreText => _loreText;

        private bool _collected;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_collected) return;
            if (!other.CompareTag("Player")) return;

            Collect(other.gameObject);
        }

        public void Collect(GameObject collector)
        {
            if (_collected) return;
            _collected = true;

            GameEvents.RaiseItemCollected(ItemId);
            LevelManager.Instance.CollectRelic(_vaultIndex);

            if (_pickupVfx != null)
            {
                Instantiate(_pickupVfx, transform.position, Quaternion.identity);
            }

            gameObject.SetActive(false);
        }
    }
}