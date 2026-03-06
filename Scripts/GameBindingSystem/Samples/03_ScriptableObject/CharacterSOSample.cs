using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BindingSystem.Samples
{
    /// <summary>
    /// Sample 03: ScriptableObject Binding
    /// Demonstrates binding a CharacterDataSO to UI. Multiple instances of this script
    /// can reference the same SO and will all update together.
    /// 
    /// SETUP:
    /// 1. Create a CharacterDataSO asset (Create → BindingSystem → CharacterData)
    /// 2. Create UI elements (texts, slider, image)
    /// 3. Attach this script and assign the SO + UI references
    /// 4. Play → change values in the SO Inspector → all UIs update instantly
    /// </summary>
    public class CharacterSOSample : MonoBehaviour
    {
        [Header("Data Source")]
        [SerializeField] private CharacterDataSO _characterData;

        [Header("UI References")]
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Image _portraitImage;
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private Slider _manaSlider;
        [SerializeField] private TMP_Text _manaText;
        [SerializeField] private TMP_Text _goldText;

        void Start()
        {
            if (_characterData == null)
            {
                Debug.LogError("CharacterDataSO is not assigned!", this);
                return;
            }

            // Bind all UI to ScriptableObject data
            if (_nameText != null)
                _nameText.BindText(_characterData.CharacterName);

            if (_levelText != null)
                _levelText.BindText(_characterData.Level, lv => $"Lv. {lv}");

            if (_portraitImage != null)
                _portraitImage.BindSprite(_characterData.Portrait);

            if (_healthSlider != null)
                _healthSlider.BindValue(_characterData.Health, _characterData.MaxHealth,
                    (h, max) => max > 0 ? h / max : 0f);

            if (_healthText != null)
                _healthText.BindText(_characterData.Health, _characterData.MaxHealth,
                    (h, max) => $"HP: {h:0}/{max:0}");

            if (_manaSlider != null)
                _manaSlider.BindValue(_characterData.Mana, _characterData.MaxMana,
                    (m, max) => max > 0 ? m / max : 0f);

            if (_manaText != null)
                _manaText.BindText(_characterData.Mana, _characterData.MaxMana,
                    (m, max) => $"MP: {m:0}/{max:0}");

            if (_goldText != null)
                _goldText.BindText(_characterData.Gold, g => $"Gold: {g:N0}");
        }

        // --- Test Methods ---
        [ContextMenu("Level Up")]
        public void LevelUp()
        {
            _characterData.Level.Value++;
            _characterData.MaxHealth.Value += 10f;
            _characterData.Health.Value = _characterData.MaxHealth.Value;
            _characterData.MaxMana.Value += 5f;
            _characterData.Mana.Value = _characterData.MaxMana.Value;
        }

        [ContextMenu("Take Damage")]
        public void TakeDamage()
        {
            _characterData.Health.Value = Mathf.Max(0, _characterData.Health.Value - 15f);
            if (_characterData.Health.Value <= 0)
                _characterData.IsAlive.Value = false;
        }

        [ContextMenu("Add Gold")]
        public void AddGold()
        {
            _characterData.Gold.Value += 100;
        }
    }
}
