using System;
using System.Threading.Tasks;
using UnityEngine;

namespace BindingSystem
{
    /// <summary>
    /// Loop behavior for BindableAnimator.
    /// </summary>
    public enum LoopMode
    {
        /// <summary>No looping — animation plays once and stops.</summary>
        None,
        /// <summary>Restart from beginning when complete.</summary>
        Wrap,
        /// <summary>Reverse direction when complete.</summary>
        PingPong
    }

    /// <summary>
    /// Animates a float value along a Curve, implementing IBindable so it can be used
    /// anywhere a reactive float is expected.
    /// </summary>
    public class BindableAnimator : IBindable<float>
    {
        private Curve _curve;
        private float _currentTime;
        private float _value;
        private bool _isPlaying;
        private bool _isReverse;
        private LoopMode _loop = LoopMode.None;
        private Action _unregisterUpdate;

        private event Action _onChanged;
        private event Action<float> _onChangedWithValue;

        // Completion tracking
        private TaskCompletionSource<bool> _completionSource;

        /// <summary>Current animation curve.</summary>
        public Curve Curve
        {
            get => _curve;
            set => _curve = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <summary>Current animation time.</summary>
        public float CurrentTime
        {
            get => _currentTime;
            set
            {
                _currentTime = Mathf.Clamp(value, 0f, _curve.Duration);
                UpdateValue();
            }
        }

        /// <summary>Current animated value.</summary>
        public float Value => _value;

        /// <summary>Whether the animation is currently playing.</summary>
        public bool IsPlaying => _isPlaying;

        /// <summary>Whether the animation is playing in reverse.</summary>
        public bool IsReverse => _isReverse;

        /// <summary>Loop behavior.</summary>
        public LoopMode Loop
        {
            get => _loop;
            set => _loop = value;
        }

        public event Action OnChanged
        {
            add => _onChanged += value;
            remove => _onChanged -= value;
        }

        public event Action<float> OnChangedWithValue
        {
            add => _onChangedWithValue += value;
            remove => _onChangedWithValue -= value;
        }

        /// <summary>Create a BindableAnimator with a curve.</summary>
        /// <param name="curve">The animation curve to follow.</param>
        /// <param name="autoPlay">If true, starts playing immediately.</param>
        public BindableAnimator(Curve curve, bool autoPlay = false)
        {
            _curve = curve ?? throw new ArgumentNullException(nameof(curve));
            _value = curve.Evaluate(0f);

            if (autoPlay)
            {
                Play();
            }
        }

        /// <summary>Play the animation forward.</summary>
        /// <param name="fromBeginning">If true, restart from time 0.</param>
        public void Play(bool fromBeginning = true)
        {
            _isReverse = false;
            if (fromBeginning) CurrentTime = 0f;
            StartPlaying();
        }

        /// <summary>Play the animation in reverse.</summary>
        /// <param name="fromEnd">If true, start from the end of the curve.</param>
        public void PlayReverse(bool fromEnd = true)
        {
            _isReverse = true;
            if (fromEnd) CurrentTime = _curve.Duration;
            StartPlaying();
        }

        /// <summary>Pause the animation at the current time.</summary>
        public void Pause()
        {
            _isPlaying = false;
            StopPlaying();
        }

        /// <summary>Stop the animation and reset to time 0.</summary>
        public void Stop()
        {
            _isPlaying = false;
            StopPlaying();
            CurrentTime = 0f;
        }

        /// <summary>
        /// Wait for the animation to complete. Returns a Task (or UniTask if UNITASK_ENABLED).
        /// </summary>
#if UNITASK_ENABLED
        public Cysharp.Threading.Tasks.UniTask WaitForCompleteAsync(System.Threading.CancellationToken cancellationToken = default)
        {
            if (!_isPlaying) return Cysharp.Threading.Tasks.UniTask.CompletedTask;

            var utcs = new Cysharp.Threading.Tasks.UniTaskCompletionSource();
            void OnComplete()
            {
                utcs.TrySetResult();
                _onChanged -= OnComplete;
            }

            // We'll signal via a special completion callback
            _completionSource = null; // Not used for UniTask path
            Action completionAction = null;
            completionAction = () =>
            {
                if (!_isPlaying)
                {
                    utcs.TrySetResult();
                    _onChanged -= completionAction;
                }
            };
            _onChanged += completionAction;

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() =>
                {
                    utcs.TrySetCanceled();
                    _onChanged -= completionAction;
                });
            }

            return utcs.Task;
        }
#else
        public Task WaitForCompleteAsync()
        {
            if (!_isPlaying) return Task.CompletedTask;

            _completionSource = new TaskCompletionSource<bool>();
            return _completionSource.Task;
        }
#endif

        private void StartPlaying()
        {
            if (_isPlaying) return;
            _isPlaying = true;

            _unregisterUpdate = BindRunner.RegisterUpdate(OnUpdate);
        }

        private void StopPlaying()
        {
            _unregisterUpdate?.Invoke();
            _unregisterUpdate = null;
        }

        private void OnUpdate()
        {
            if (!_isPlaying) return;

            float dt = Time.deltaTime;
            if (_isReverse)
                _currentTime -= dt;
            else
                _currentTime += dt;

            // Check completion
            bool completed = false;
            if (!_isReverse && _currentTime >= _curve.Duration)
            {
                HandleCompletion(ref completed);
            }
            else if (_isReverse && _currentTime <= 0f)
            {
                HandleCompletion(ref completed);
            }

            UpdateValue();

            if (completed)
            {
                _completionSource?.TrySetResult(true);
                _completionSource = null;
            }
        }

        private void HandleCompletion(ref bool completed)
        {
            switch (_loop)
            {
                case LoopMode.None:
                    _currentTime = _isReverse ? 0f : _curve.Duration;
                    _isPlaying = false;
                    StopPlaying();
                    completed = true;
                    break;

                case LoopMode.Wrap:
                    if (_isReverse)
                        _currentTime += _curve.Duration;
                    else
                        _currentTime -= _curve.Duration;
                    break;

                case LoopMode.PingPong:
                    _isReverse = !_isReverse;
                    if (_isReverse)
                        _currentTime = _curve.Duration * 2f - _currentTime;
                    else
                        _currentTime = -_currentTime;
                    break;
            }
        }

        private void UpdateValue()
        {
            float newValue = _curve.Evaluate(_currentTime);
            if (newValue != _value)
            {
                _value = newValue;
                _onChanged?.Invoke();
                _onChangedWithValue?.Invoke(_value);
            }
        }
    }
}
