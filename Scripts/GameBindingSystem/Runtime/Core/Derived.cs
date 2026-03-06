using System;

namespace BindingSystem
{
    /// <summary>
    /// Creates derived (computed) Bindables that automatically update
    /// when their source Bindables change.
    /// </summary>
    public static class Derived
    {
        /// <summary>
        /// Create a derived Bindable from one source.
        /// </summary>
        public static IBindable<TResult> From<T1, TResult>(
            IBindable<T1> source,
            Func<T1, TResult> transform)
        {
            var result = new Bindable<TResult>(transform(source.Value));
            source.OnChangedWithValue += v => result.Value = transform(v);
            return result;
        }

        /// <summary>
        /// Create a derived Bindable from two sources.
        /// </summary>
        public static IBindable<TResult> From<T1, T2, TResult>(
            IBindable<T1> source1,
            IBindable<T2> source2,
            Func<T1, T2, TResult> transform)
        {
            var result = new Bindable<TResult>(transform(source1.Value, source2.Value));

            void Update() => result.Value = transform(source1.Value, source2.Value);

            source1.OnChanged += Update;
            source2.OnChanged += Update;

            return result;
        }

        /// <summary>
        /// Create a derived Bindable from three sources.
        /// </summary>
        public static IBindable<TResult> From<T1, T2, T3, TResult>(
            IBindable<T1> source1,
            IBindable<T2> source2,
            IBindable<T3> source3,
            Func<T1, T2, T3, TResult> transform)
        {
            var result = new Bindable<TResult>(transform(source1.Value, source2.Value, source3.Value));

            void Update() => result.Value = transform(source1.Value, source2.Value, source3.Value);

            source1.OnChanged += Update;
            source2.OnChanged += Update;
            source3.OnChanged += Update;

            return result;
        }

        /// <summary>
        /// Create a derived Bindable from four sources.
        /// </summary>
        public static IBindable<TResult> From<T1, T2, T3, T4, TResult>(
            IBindable<T1> source1,
            IBindable<T2> source2,
            IBindable<T3> source3,
            IBindable<T4> source4,
            Func<T1, T2, T3, T4, TResult> transform)
        {
            var result = new Bindable<TResult>(transform(source1.Value, source2.Value, source3.Value, source4.Value));

            void Update() => result.Value = transform(source1.Value, source2.Value, source3.Value, source4.Value);

            source1.OnChanged += Update;
            source2.OnChanged += Update;
            source3.OnChanged += Update;
            source4.OnChanged += Update;

            return result;
        }
    }

    /// <summary>
    /// Extension methods for IBindable to create derived values.
    /// </summary>
    public static class BindableDeriveExtensions
    {
        /// <summary>
        /// Create a derived Bindable by transforming this Bindable's value.
        /// </summary>
        public static IBindable<TResult> Derive<T, TResult>(
            this IBindable<T> source,
            Func<T, TResult> transform)
        {
            return Derived.From(source, transform);
        }

        /// <summary>
        /// Extract an inner IBindable from this Bindable.
        /// When the outer Bindable changes, the derived value re-subscribes to the new inner Bindable.
        /// </summary>
        public static IBindable<TInner> Derive<TOuter, TInner>(
            this IBindable<TOuter> source,
            Func<TOuter, IBindable<TInner>> innerSelector)
        {
            var currentInner = source.Value != null ? innerSelector(source.Value) : null;
            var result = new Bindable<TInner>(currentInner != null ? currentInner.Value : default);

            Action innerHandler = null;

            void SubscribeInner()
            {
                // Unsubscribe from previous
                if (currentInner != null && innerHandler != null)
                {
                    currentInner.OnChanged -= innerHandler;
                }

                currentInner = source.Value != null ? innerSelector(source.Value) : null;

                if (currentInner != null)
                {
                    innerHandler = () => result.Value = currentInner.Value;
                    currentInner.OnChanged += innerHandler;
                    result.Value = currentInner.Value;
                }
                else
                {
                    result.Value = default;
                }
            }

            source.OnChanged += SubscribeInner;

            return result;
        }
    }
}
