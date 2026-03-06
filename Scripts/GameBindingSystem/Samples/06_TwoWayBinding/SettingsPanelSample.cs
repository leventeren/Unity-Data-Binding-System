using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BindingSystem.Samples
{
    /// <summary>
    /// Sample 06: Two-Way Binding — Settings Panel
    /// Demonstrates bidirectional binding between UI controls and Bindable data.
    /// 
    /// SETUP:
    /// 1. Create UI: Sliders for volume/sensitivity, Toggles for fullscreen/vsync, 
    ///    InputField for player name, Dropdown for quality
    /// 2. Create TMP_Texts to display current values
    /// 3. Attach this script and assign all references
    /// 4. Play → drag sliders, type in input field → data updates → display labels update
    /// </summary>
    public class SettingsPanelSample : MonoBehaviour
    {
        [Header("Settings Data")]
        public Bindable<float> MasterVolume = new(0.8f);
        public Bindable<float> MusicVolume = new(0.7f);
        public Bindable<float> SFXVolume = new(1.0f);
        public Bindable<float> MouseSensitivity = new(0.5f);
        public Bindable<bool> Fullscreen = new(true);
        public Bindable<bool> VSync = new(true);
        public Bindable<string> PlayerName = new("Player");
        public Bindable<int> QualityLevel = new(2);

        [Header("Sliders")]
        [SerializeField] private Slider _masterVolumeSlider;
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Slider _sensitivitySlider;

        [Header("Toggles")]
        [SerializeField] private Toggle _fullscreenToggle;
        [SerializeField] private Toggle _vsyncToggle;

        [Header("Inputs")]
        [SerializeField] private TMP_InputField _nameInput;
        [SerializeField] private TMP_Dropdown _qualityDropdown;

        [Header("Display Labels")]
        [SerializeField] private TMP_Text _masterVolumeLabel;
        [SerializeField] private TMP_Text _musicVolumeLabel;
        [SerializeField] private TMP_Text _sfxVolumeLabel;
        [SerializeField] private TMP_Text _sensitivityLabel;
        [SerializeField] private TMP_Text _statusText;

        void Start()
        {
            // === Two-Way Slider Bindings ===
            // Slider ↔ Bindable<float>: user drags slider → data updates → label updates
            if (_masterVolumeSlider != null) _masterVolumeSlider.BindValueTwoWay(MasterVolume);
            if (_musicVolumeSlider != null) _musicVolumeSlider.BindValueTwoWay(MusicVolume);
            if (_sfxVolumeSlider != null) _sfxVolumeSlider.BindValueTwoWay(SFXVolume);
            if (_sensitivitySlider != null) _sensitivitySlider.BindValueTwoWay(MouseSensitivity);

            // === Two-Way Toggle Bindings ===
            if (_fullscreenToggle != null) _fullscreenToggle.BindIsOnTwoWay(Fullscreen);
            if (_vsyncToggle != null) _vsyncToggle.BindIsOnTwoWay(VSync);

            // === Two-Way InputField Binding ===
            if (_nameInput != null) _nameInput.BindTextTwoWay(PlayerName);

            // === Two-Way Dropdown Binding ===
            if (_qualityDropdown != null) _qualityDropdown.BindValueTwoWay(QualityLevel);

            // === One-Way Display Labels (auto-update from data) ===
            if (_masterVolumeLabel != null)
                _masterVolumeLabel.BindText(MasterVolume, v => $"Master: {v * 100f:0}%");

            if (_musicVolumeLabel != null)
                _musicVolumeLabel.BindText(MusicVolume, v => $"Music: {v * 100f:0}%");

            if (_sfxVolumeLabel != null)
                _sfxVolumeLabel.BindText(SFXVolume, v => $"SFX: {v * 100f:0}%");

            if (_sensitivityLabel != null)
                _sensitivityLabel.BindText(MouseSensitivity, v => $"Sensitivity: {v:0.00}");

            // === Status text combining multiple sources ===
            if (_statusText != null)
            {
                var status = Derived.From(PlayerName, Fullscreen, VSync,
                    (name, fs, vs) => $"{name} | Fullscreen: {(fs ? "ON" : "OFF")} | VSync: {(vs ? "ON" : "OFF")}");
                _statusText.BindText(status, s => s);
            }
        }

        // --- Test Methods ---
        [ContextMenu("Reset Defaults")]
        public void ResetDefaults()
        {
            MasterVolume.Value = 0.8f;
            MusicVolume.Value = 0.7f;
            SFXVolume.Value = 1.0f;
            MouseSensitivity.Value = 0.5f;
            Fullscreen.Value = true;
            VSync.Value = true;
            PlayerName.Value = "Player";
            QualityLevel.Value = 2;
        }

        [ContextMenu("Set Name from Code")]
        public void SetNameFromCode()
        {
            // Setting the Bindable from code also updates the InputField (two-way!)
            PlayerName.Value = "CodeSetName";
        }

        [ContextMenu("Mute All")]
        public void MuteAll()
        {
            MasterVolume.Value = 0f;
            MusicVolume.Value = 0f;
            SFXVolume.Value = 0f;
        }
    }
}
