using System;
using System.Collections.Generic;
using UnityEngine;

namespace BindingSystem
{
    /// <summary>
    /// Read-only interface for a reactive value container.
    /// Exposes the current value and change events without allowing modification.
    /// </summary>
    public interface IBindable<T>
    {
        /// <summary>Current value of the bindable.</summary>
        T Value { get; }

        /// <summary>Invoked when the value changes (no parameter).</summary>
        event Action OnChanged;

        /// <summary>Invoked when the value changes, passing the new value.</summary>
        event Action<T> OnChangedWithValue;
    }

    /// <summary>
    /// A reactive container that holds a value and notifies observers whenever that value changes.
    /// Supports Unity serialization and Inspector editing.
    /// </summary>
    [Serializable]
    public class Bindable<T> : IBindable<T>
    {
        [SerializeField] private T _value;

        private event Action _onChanged;
        private event Action<T> _onChangedWithValue;

        // Used to prevent re-entrancy during notification
        private bool _isNotifying;

        // Thread safety: pending value from non-main thread
        private bool _hasPendingValue;
        private T _pendingValue;
        private readonly object _lock = new object();

        /// <summary>Creates a new Bindable with default value.</summary>
        public Bindable() { }

        /// <summary>Creates a new Bindable with an initial value.</summary>
        public Bindable(T initialValue)
        {
            _value = initialValue;
        }

        /// <summary>
        /// Gets or sets the current value. Setting triggers change events only if the value actually changed.
        /// Thread-safe: if set from a non-main thread, the value is queued and applied on the next Update.
        /// </summary>
        public T Value
        {
            get => _value;
            set
            {
                // Thread safety check
                if (!BindRunner.IsMainThread)
                {
                    lock (_lock)
                    {
                        _pendingValue = value;
                        _hasPendingValue = true;
                        BindRunner.EnqueueMainThread(ApplyPendingValue);
                    }
                    return;
                }

                SetValueInternal(value);
            }
        }

        /// <summary>
        /// Applies a pending value that was set from a non-main thread.
        /// Called by BindRunner on the main thread.
        /// </summary>
        private void ApplyPendingValue()
        {
            T val;
            lock (_lock)
            {
                if (!_hasPendingValue) return;
                val = _pendingValue;
                _hasPendingValue = false;
                _pendingValue = default;
            }
            SetValueInternal(val);
        }

        private void SetValueInternal(T value)
        {
            if (EqualityComparer<T>.Default.Equals(_value, value))
                return;

            _value = value;
            RaiseChanged();
        }

        /// <summary>
        /// Manually notify observers that the value has changed.
        /// Use this when modifying internal fields of a reference-type value.
        /// </summary>
        public void NotifyChanged()
        {
            RaiseChanged();
        }

        private void RaiseChanged()
        {
            if (_isNotifying) return;

            _isNotifying = true;
            try
            {
                _onChanged?.Invoke();
                _onChangedWithValue?.Invoke(_value);
            }
            finally
            {
                _isNotifying = false;
            }
        }

        /// <summary>Invoked when the value changes (no parameter).</summary>
        public event Action OnChanged
        {
            add => _onChanged += value;
            remove => _onChanged -= value;
        }

        /// <summary>Invoked when the value changes, passing the new value.</summary>
        public event Action<T> OnChangedWithValue
        {
            add => _onChangedWithValue += value;
            remove => _onChangedWithValue -= value;
        }

        /// <summary>Implicit conversion to the contained value.</summary>
        public static implicit operator T(Bindable<T> bindable) => bindable._value;

        public override string ToString() => _value?.ToString() ?? "null";
    }
}
