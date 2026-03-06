#if UNITASK_ENABLED
using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace BindingSystem
{
    /// <summary>
    /// Async extension methods for Bindables that require UniTask.
    /// This entire file is conditionally compiled — it is empty when UniTask is not installed.
    /// </summary>
    public static class BindableAsyncExtensions
    {
        /// <summary>
        /// Wait until the Bindable value changes. Returns the new value.
        /// </summary>
        public static async UniTask<T> WaitForChangeAsync<T>(this IBindable<T> bindable, CancellationToken cancellationToken = default)
        {
            var tcs = new UniTaskCompletionSource<T>();

            void Handler(T value) => tcs.TrySetResult(value);

            bindable.OnChangedWithValue += Handler;

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() =>
                {
                    tcs.TrySetCanceled();
                    bindable.OnChangedWithValue -= Handler;
                });
            }

            try
            {
                return await tcs.Task;
            }
            finally
            {
                bindable.OnChangedWithValue -= Handler;
            }
        }

        /// <summary>
        /// Wait until the Bindable value satisfies a predicate. Returns the matching value.
        /// If the current value already satisfies the predicate, returns immediately.
        /// </summary>
        public static async UniTask<T> WaitUntilAsync<T>(this IBindable<T> bindable, Func<T, bool> predicate, CancellationToken cancellationToken = default)
        {
            // Check current value first
            if (predicate(bindable.Value))
                return bindable.Value;

            var tcs = new UniTaskCompletionSource<T>();

            void Handler(T value)
            {
                if (predicate(value))
                    tcs.TrySetResult(value);
            }

            bindable.OnChangedWithValue += Handler;

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() =>
                {
                    tcs.TrySetCanceled();
                    bindable.OnChangedWithValue -= Handler;
                });
            }

            try
            {
                return await tcs.Task;
            }
            finally
            {
                bindable.OnChangedWithValue -= Handler;
            }
        }

        /// <summary>
        /// Wait for a BindableAnimator to complete its animation.
        /// </summary>
        public static async UniTask ToUniTask(this BindableAnimator animator, CancellationToken cancellationToken = default)
        {
            await animator.WaitForCompleteAsync(cancellationToken);
        }
    }
}
#endif
