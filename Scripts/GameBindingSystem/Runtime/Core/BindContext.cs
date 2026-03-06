using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace BindingSystem
{
    /// <summary>
    /// Manages the lifetime of bindings. When a BindContext is unbound,
    /// all registered callbacks are automatically unsubscribed.
    /// </summary>
    public class BindContext
    {
        private static readonly Dictionary<int, BindContext> _defaultContexts = new Dictionary<int, BindContext>();

        private readonly List<Action> _unbindActions = new List<Action>();
        private bool _isBound = true;

        /// <summary>Whether this context is still active.</summary>
        public bool IsBound => _isBound;

        /// <summary>Creates a standalone BindContext. You must call Unbind() manually.</summary>
        public BindContext() { }

        /// <summary>
        /// Creates a BindContext that is automatically unbound when the CancellationToken is cancelled.
        /// </summary>
        public BindContext(CancellationToken cancellationToken)
        {
            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(Unbind);
            }
        }

        /// <summary>
        /// Gets or creates the default BindContext for a Unity Component.
        /// The context is automatically unbound when the component is destroyed.
        /// </summary>
        public static BindContext Get(Component component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            int id = component.GetInstanceID();

            if (_defaultContexts.TryGetValue(id, out var context) && context._isBound)
                return context;

            context = new BindContext();
            _defaultContexts[id] = context;

            // Use BindRunner to detect destruction
            BindRunner.TrackDestroy(component, () =>
            {
                context.Unbind();
                _defaultContexts.Remove(id);
            });

            return context;
        }

        /// <summary>
        /// Bind to a Bindable's change event. The callback is invoked immediately with the current value
        /// and then whenever the value changes. Automatically unsubscribes when this context is unbound.
        /// </summary>
        public void Bind<T>(IBindable<T> bindable, Action<T> callback)
        {
            if (!_isBound) return;
            if (bindable == null) throw new ArgumentNullException(nameof(bindable));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            // Subscribe
            bindable.OnChangedWithValue += callback;

            // Register unsubscribe action
            _unbindActions.Add(() => bindable.OnChangedWithValue -= callback);

            // Invoke immediately with current value
            callback(bindable.Value);
        }

        /// <summary>
        /// Bind to a Bindable's change event without receiving the value.
        /// The callback is invoked immediately and then whenever the value changes.
        /// </summary>
        public void Bind<T>(IBindable<T> bindable, Action callback)
        {
            if (!_isBound) return;
            if (bindable == null) throw new ArgumentNullException(nameof(bindable));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            bindable.OnChanged += callback;
            _unbindActions.Add(() => bindable.OnChanged -= callback);

            callback();
        }

        /// <summary>
        /// Add a custom action to be invoked when this context is unbound.
        /// Useful for cleaning up external resources.
        /// </summary>
        public void AddUnbindAction(Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (!_isBound) return;
            _unbindActions.Add(action);
        }

        /// <summary>
        /// Unbind all registered callbacks and mark this context as inactive.
        /// </summary>
        public void Unbind()
        {
            if (!_isBound) return;
            _isBound = false;

            for (int i = _unbindActions.Count - 1; i >= 0; i--)
            {
                try
                {
                    _unbindActions[i]?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

            _unbindActions.Clear();
        }
    }
}
