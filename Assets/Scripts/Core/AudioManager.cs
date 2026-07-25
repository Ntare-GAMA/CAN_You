using UnityEngine;

namespace VaultsOfTheElixir.Core
{
    /// <summary>
    /// Singleton. Owns the music and SFX audio sources. Subscribes to
    /// GameEvents rather than being called directly by gameplay code —
    /// e.g. combat scripts don't call AudioManager.PlaySFX(hitClip)
    /// themselves; they raise GameEvents.OnEnemyDefeated, and
    /// AudioManager reacts. This is the Observer pattern in action for
    /// audio specifically, and it's what lets audio be added/changed
    /// without touching combat code at all.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        [Header("Common clips (assign in Inspector)")]
        [SerializeField] private AudioClip _enemyDefeatedClip;
        [SerializeField] private AudioClip _itemCollectedClip;
        [SerializeField] private AudioClip _relicCollectedClip;
        [SerializeField] private AudioClip _levelCompleteClip;
        [SerializeField] private AudioClip _elixirFoundClip;
        [SerializeField] private AudioClip _playerDamagedClip;

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

        private void OnEnable()
        {
            GameEvents.OnEnemyDefeated += HandleEnemyDefeated;
            GameEvents.OnItemCollected += HandleItemCollected;
            GameEvents.OnRelicCollected += HandleRelicCollected;
            GameEvents.OnLevelCompleted += HandleLevelCompleted;
            GameEvents.OnElixirFound += HandleElixirFound;
            GameEvents.OnHealthChanged += HandleHealthChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyDefeated -= HandleEnemyDefeated;
            GameEvents.OnItemCollected -= HandleItemCollected;
            GameEvents.OnRelicCollected -= HandleRelicCollected;
            GameEvents.OnLevelCompleted -= HandleLevelCompleted;
            GameEvents.OnElixirFound -= HandleElixirFound;
            GameEvents.OnHealthChanged -= HandleHealthChanged;
        }

        private int _lastKnownHealth = -1;

        private void HandleEnemyDefeated(GameObject enemy) => PlaySFX(_enemyDefeatedClip);
        private void HandleItemCollected(string itemId) => PlaySFX(_itemCollectedClip);
        private void HandleRelicCollected(int vaultIndex) => PlaySFX(_relicCollectedClip);
        private void HandleLevelCompleted(int levelIndex) => PlaySFX(_levelCompleteClip);
        private void HandleElixirFound() => PlaySFX(_elixirFoundClip);

        private void HandleHealthChanged(int current, int max)
        {
            // Only play the "hurt" sound on a decrease, not on heals or initial sync.
            if (_lastKnownHealth != -1 && current < _lastKnownHealth)
            {
                PlaySFX(_playerDamagedClip);
            }
            _lastKnownHealth = current;
        }

        public void PlaySFX(AudioClip clip)
        {
            if (clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip);
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null || _musicSource == null) return;
            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.Play();
        }

        public void SetMusicVolume(float volume) => _musicSource.volume = volume;
        public void SetSfxVolume(float volume) => _sfxSource.volume = volume;
    }
}
