using UnityEngine;
using UnityEngine.UI;

namespace VaultsOfTheElixir.UI
{
    /// <summary>
    /// Drives the Settings panel: music slider, SFX slider, and a
    /// "mute music" toggle. Reads current values from AudioManager when
    /// the panel opens (so it always reflects reality, not a stale
    /// guess), and writes both to AudioManager (immediate effect) and
    /// SaveManager (persisted to disk) whenever the player changes
    /// something.
    ///
    /// Attach this to the SettingsPanel GameObject and assign the three
    /// UI references in the Inspector.
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [Header("UI references (assign in Inspector)")]
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Toggle _muteMusicToggle;

        private bool _initializing;

        private void OnEnable()
        {
            RefreshFromAudioManager();

            _musicVolumeSlider.onValueChanged.AddListener(HandleMusicVolumeChanged);
            _sfxVolumeSlider.onValueChanged.AddListener(HandleSfxVolumeChanged);
            _muteMusicToggle.onValueChanged.AddListener(HandleMuteToggleChanged);
        }

        private void OnDisable()
        {
            _musicVolumeSlider.onValueChanged.RemoveListener(HandleMusicVolumeChanged);
            _sfxVolumeSlider.onValueChanged.RemoveListener(HandleSfxVolumeChanged);
            _muteMusicToggle.onValueChanged.RemoveListener(HandleMuteToggleChanged);
        }

        /// <summary>Syncs the UI controls to AudioManager's current values
        /// without re-triggering the listeners (which would immediately
        /// write back to AudioManager/SaveManager on open).</summary>
        private void RefreshFromAudioManager()
        {
            if (Core.AudioManager.Instance == null) return;

            _initializing = true;

            _musicVolumeSlider.SetValueWithoutNotify(Core.AudioManager.Instance.MusicVolume);
            _sfxVolumeSlider.SetValueWithoutNotify(Core.AudioManager.Instance.SfxVolume);
            _muteMusicToggle.SetIsOnWithoutNotify(Core.AudioManager.Instance.IsMusicMuted);

            // Music slider is meaningless while muted — grey it out to
            // match, without changing the remembered value underneath.
            _musicVolumeSlider.interactable = !Core.AudioManager.Instance.IsMusicMuted;

            _initializing = false;
        }

        private void HandleMusicVolumeChanged(float value)
        {
            if (_initializing || Core.AudioManager.Instance == null) return;

            Core.AudioManager.Instance.SetMusicVolume(value);
            PersistSettings();
        }

        private void HandleSfxVolumeChanged(float value)
        {
            if (_initializing || Core.AudioManager.Instance == null) return;

            Core.AudioManager.Instance.SetSfxVolume(value);
            PersistSettings();
        }

        private void HandleMuteToggleChanged(bool isMuted)
        {
            if (_initializing || Core.AudioManager.Instance == null) return;

            Core.AudioManager.Instance.SetMusicMuted(isMuted);
            _musicVolumeSlider.interactable = !isMuted;
            PersistSettings();
        }

        private void PersistSettings()
        {
            if (Core.SaveManager.Instance == null) return;

            var save = Core.SaveManager.Instance.CurrentSave;
            save.musicVolume = Core.AudioManager.Instance.MusicVolume;
            save.sfxVolume = Core.AudioManager.Instance.SfxVolume;
            save.musicMuted = Core.AudioManager.Instance.IsMusicMuted;

            Core.SaveManager.Instance.Save();
        }
    }
}