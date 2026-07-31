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

        // Backing values for the settings panel. _musicSource.volume can
        // momentarily be 0 while muted, so we keep the "real" slider
        // value here separately and re-apply it on unmute.
        private float _musicVolume = 0.75f;
        private float _sfxVolume = 0.75f;
        private bool _isMusicMuted = false;

        public float MusicVolume => _musicVolume;
        public float SfxVolume => _sfxVolume;
        public bool IsMusicMuted => _isMusicMuted;

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

        private void Start()
        {
            // Pull saved settings in on boot, before any music starts
            // playing elsewhere (e.g. VaultAmbience.Start()), so the
            // first note already respects the player's saved volume.
            if (SaveManager.Instance != null)
            {
                var save = SaveManager.Instance.CurrentSave;
                SetSfxVolume(save.sfxVolume);
                SetMusicVolume(save.musicVolume);
                SetMusicMuted(save.musicMuted);
            }
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
            // Respect current mute state even when a new track starts.
            _musicSource.volume = _isMusicMuted ? 0f : _musicVolume;
            _musicSource.Play();
        }

        /// <summary>Sets the music slider value. Actual audible volume stays
        /// 0 while muted, but this is remembered so unmuting restores it.</summary>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = volume;
            if (_musicSource != null && !_isMusicMuted)
            {
                _musicSource.volume = volume;
            }
        }

        public void SetSfxVolume(float volume)
        {
            _sfxVolume = volume;
            if (_sfxSource != null)
            {
                _sfxSource.volume = volume;
            }
        }

        /// <summary>Toggling this silences/restores music without touching
        /// the remembered slider value, so unmuting comes back at the same level.</summary>
        public void SetMusicMuted(bool muted)
        {
            _isMusicMuted = muted;
            if (_musicSource != null)
            {
                _musicSource.volume = muted ? 0f : _musicVolume;
            }
        }
    }
}