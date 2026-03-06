using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BindingSystem.Samples
{
    /// <summary>
    /// Sample 05: Animated UI
    /// Demonstrates BindableAnimator with Curves for smooth UI transitions.
    /// 
    /// SETUP:
    /// 1. Create UI with a health slider, score text, and a panel (RectTransform)
    /// 2. Attach this script and assign references
    /// 3. Play → use ContextMenu methods to trigger animations
    /// </summary>
    public class AnimatedUISample : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Slider _healthSlider;
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private RectTransform _slidePanel;

        [Header("Data")]
        [SerializeField] private Bindable<float> _health = new(1f);
        [SerializeField] private Bindable<int> _score = new(0);

        [Header("Animation Settings")]
        [SerializeField] private float _healthAnimDuration = 0.5f;
        [SerializeField] private float _scoreAnimDuration = 0.8f;
        [SerializeField] private float _slideAnimDuration = 0.6f;

        // Display values (Animated)
        private Bindable<float> _displayHealth = new();
        private Bindable<int> _displayScore = new();
        private Bindable<Vector3> _slidePosition = new(); // Used for localPosition

        // Active Animators
        private BindableAnimator _healthAnimator;
        private BindableAnimator _scoreAnimator;
        private BindableAnimator _slideAnimator;

        void Start()
        {
            // === INITIALIZE BINDINGS ONE TIME ===
            if (_healthSlider != null)
                _healthSlider.BindValue(_displayHealth);
                
            if (_healthText != null)
                _healthText.BindText(_displayHealth, v => $"HP: {v * 100f:0}%");

            if (_scoreText != null)
                _scoreText.BindText(_displayScore, v => $"Score: {v:N0}");

            if (_slidePanel != null)
            {
                var pos = _slidePanel.anchoredPosition;
                _slidePosition.Value = new Vector3(-500f, pos.y, 0f);
                _slidePanel.BindLocalPosition(_slidePosition);

                // Initialize persistent animator for sliding
                _slideAnimator = new BindableAnimator(Curve.EaseOutBack(_slideAnimDuration), autoPlay: false);
                _slideAnimator.OnChangedWithValue += t => 
                {
                    _slidePosition.Value = new Vector3(Mathf.Lerp(-500f, pos.x, t), pos.y, 0f);
                };
                
                // Play entrance animation
                _slideAnimator.Play();
            }

            // === REACT TO DATA CHANGES ===
            
            // Initial Data
            _displayHealth.Value = _health.Value;
            _displayScore.Value = _score.Value;

            // Health changes
            _health.OnChangedWithValue += newHealth =>
            {
                if (_healthAnimator != null && _healthAnimator.IsPlaying)
                    _healthAnimator.Stop();

                float startVal = _displayHealth.Value;
                _healthAnimator = new BindableAnimator(Curve.EaseOutCubic(_healthAnimDuration), autoPlay: false);
                _healthAnimator.OnChangedWithValue += t => 
                {
                    _displayHealth.Value = Mathf.Lerp(startVal, newHealth, t);
                };
                _healthAnimator.Play();
            };

            // Score changes
            _score.OnChangedWithValue += newScore =>
            {
                if (_scoreAnimator != null && _scoreAnimator.IsPlaying)
                    _scoreAnimator.Stop();

                float startVal = _displayScore.Value;
                _scoreAnimator = new BindableAnimator(Curve.EaseOutQuart(_scoreAnimDuration), autoPlay: false);
                _scoreAnimator.OnChangedWithValue += t => 
                {
                    _displayScore.Value = Mathf.RoundToInt(Mathf.Lerp(startVal, newScore, t));
                };
                _scoreAnimator.Play();
            };
        }

        // --- Test Methods ---
        [ContextMenu("Take Damage (-20%)")]
        public void TakeDamage() => _health.Value = Mathf.Max(0f, _health.Value - 0.2f);

        [ContextMenu("Heal (+30%)")]
        public void Heal() => _health.Value = Mathf.Min(1f, _health.Value + 0.3f);

        [ContextMenu("Add Score (+1000)")]
        public void AddScore() => _score.Value += 1000;

        [ContextMenu("Replay Slide Animation")]
        public void ReplaySlide()
        {
            if (_slideAnimator != null)
                _slideAnimator.Play(fromBeginning: true);
        }
    }
}
