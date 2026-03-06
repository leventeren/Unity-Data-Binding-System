using System;
using UnityEngine;

namespace BindingSystem
{
    /// <summary>
    /// General-purpose binding extension methods for any Component.
    /// Provides Bind, BindUpdate, BindFixedUpdate, BindEndOfFrame, and BindInterval.
    /// </summary>
    public static class BindableExtensions
    {
        #region Core Bind

        /// <summary>
        /// Bind a callback to a Bindable. The callback is invoked immediately with the current value
        /// and then whenever the value changes. Auto-unbinds when the component is destroyed.
        /// </summary>
        public static void Bind<T>(this Component component, IBindable<T> bindable, Action<T> callback)
        {
            var context = BindContext.Get(component);
            context.Bind(bindable, callback);
        }

        /// <summary>
        /// Bind a callback to a Bindable without receiving the value.
        /// </summary>
        public static void Bind<T>(this Component component, IBindable<T> bindable, Action callback)
        {
            var context = BindContext.Get(component);
            context.Bind(bindable, callback);
        }

        /// <summary>
        /// Bind a callback to two Bindables. Invoked immediately and whenever either changes.
        /// </summary>
        public static void Bind<T1, T2>(this Component component,
            IBindable<T1> bindable1,
            IBindable<T2> bindable2,
            Action<T1, T2> callback)
        {
            var context = BindContext.Get(component);

            void Update() => callback(bindable1.Value, bindable2.Value);

            bindable1.OnChanged += Update;
            bindable2.OnChanged += Update;

            context.AddUnbindAction(() =>
            {
                bindable1.OnChanged -= Update;
                bindable2.OnChanged -= Update;
            });

            Update(); // invoke immediately
        }

        /// <summary>
        /// Bind a callback to three Bindables.
        /// </summary>
        public static void Bind<T1, T2, T3>(this Component component,
            IBindable<T1> bindable1,
            IBindable<T2> bindable2,
            IBindable<T3> bindable3,
            Action<T1, T2, T3> callback)
        {
            var context = BindContext.Get(component);

            void Update() => callback(bindable1.Value, bindable2.Value, bindable3.Value);

            bindable1.OnChanged += Update;
            bindable2.OnChanged += Update;
            bindable3.OnChanged += Update;

            context.AddUnbindAction(() =>
            {
                bindable1.OnChanged -= Update;
                bindable2.OnChanged -= Update;
                bindable3.OnChanged -= Update;
            });

            Update();
        }

        #endregion

        #region Update / FixedUpdate / LateUpdate / EndOfFrame

        /// <summary>
        /// Register a callback to be called every Update. Auto-unregisters when the component is destroyed.
        /// Note: This callback runs even when the component is disabled. Use a custom BindContext for enable/disable control.
        /// </summary>
        public static void BindUpdate(this Component component, Action callback)
        {
            var context = BindContext.Get(component);
            var unregister = BindRunner.RegisterUpdate(callback);
            context.AddUnbindAction(unregister);
        }

        /// <summary>
        /// Register a callback to be called every FixedUpdate. Auto-unregisters when the component is destroyed.
        /// </summary>
        public static void BindFixedUpdate(this Component component, Action callback)
        {
            var context = BindContext.Get(component);
            var unregister = BindRunner.RegisterFixedUpdate(callback);
            context.AddUnbindAction(unregister);
        }

        /// <summary>
        /// Register a callback to be called every LateUpdate. Auto-unregisters when the component is destroyed.
        /// </summary>
        public static void BindLateUpdate(this Component component, Action callback)
        {
            var context = BindContext.Get(component);
            var unregister = BindRunner.RegisterLateUpdate(callback);
            context.AddUnbindAction(unregister);
        }

        /// <summary>
        /// Register a callback to be called at the end of every frame.
        /// </summary>
        public static void BindEndOfFrame(this Component component, Action callback)
        {
            var context = BindContext.Get(component);
            var unregister = BindRunner.RegisterEndOfFrame(callback);
            context.AddUnbindAction(unregister);
        }

        /// <summary>
        /// Register a callback to be called at a custom time interval (in seconds).
        /// </summary>
        public static void BindInterval(this Component component, float intervalSeconds, Action callback)
        {
            var context = BindContext.Get(component);
            var unregister = BindRunner.RegisterInterval(intervalSeconds, callback);
            context.AddUnbindAction(unregister);
        }

        #endregion
    }
}
