using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BindingSystem.Samples
{
    /// <summary>
    /// Sample 01: Basic Binding — Health Bar
    /// Demonstrates binding Bindable data to UI components: text, slider, image color.
    /// 
    /// SETUP:
    /// 1. Create a Canvas with a TMP Text, Slider, and Image
    /// 2. Attach this script to any GameObject
    /// 3. Drag UI references into the Inspector fields
    /// 4. Enter Play mode and change Health/MaxHealth in Inspector to see instant UI updates
    /// </summary>
    public class BasicBindingSample : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private Image _healthBarFill;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private Image _portraitImage;

        [Header("Bindable Data")]
        [SerializeField] private Bindable<string> _playerName = new("Hero");
        [SerializeField] private Bindable<float> _health = new(75f);
        [SerializeField] private Bindable<float> _maxHealth = new(100f);
        [SerializeField] private Bindable<Sprite> _portrait;

        void Start()
        {
            // Direct string binding
            if (_nameText != null)
                _nameText.BindText(_playerName);

            // Direct sprite binding
            if (_portraitImage != null)
                _portraitImage.BindSprite(_portrait);

            // Derived text: combines two bindables into a formatted string
            if (_healthText != null)
                _healthText.BindText(_health, _maxHealth, (h, max) => $"HP: {h:0} / {max:0}");

            // Derived slider value: health / maxHealth
            if (_healthSlider != null)
                _healthSlider.BindValue(_health, _maxHealth, (h, max) => max > 0 ? h / max : 0f);

            // Derived color: green when healthy, red when low
            if (_healthBarFill != null)
            {
                var healthRatio = Derived.From(_health, _maxHealth,
                    (h, max) => max > 0 ? h / max : 0f);

                _healthBarFill.BindColor(healthRatio, ratio =>
                    Color.Lerp(Color.red, Color.green, ratio));
            }
        }

        // Public methods for testing from other scripts or buttons
        public void TakeDamage(float amount) => _health.Value = Mathf.Max(0, _health.Value - amount);
        public void Heal(float amount) => _health.Value = Mathf.Min(_maxHealth.Value, _health.Value + amount);
    }
}
