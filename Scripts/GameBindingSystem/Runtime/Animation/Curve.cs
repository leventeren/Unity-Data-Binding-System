using System;
using UnityEngine;

namespace BindingSystem
{
    /// <summary>
    /// Defines a curve function that maps time to a value.
    /// Supports cubic bezier, Unity AnimationCurve, built-in easings, and custom functions.
    /// </summary>
    public class Curve
    {
        private readonly Func<float, float> _function;
        private readonly float _duration;

        /// <summary>Duration of the curve in seconds.</summary>
        public float Duration => _duration;

        #region Constructors

        /// <summary>Create a Curve from a custom function and duration.</summary>
        /// <param name="function">Function mapping normalized time [0..duration] to value.</param>
        /// <param name="duration">Total duration in seconds.</param>
        public Curve(Func<float, float> function, float duration)
        {
            _function = function ?? throw new ArgumentNullException(nameof(function));
            _duration = Mathf.Max(duration, 0.001f);
        }

        /// <summary>Create a Curve from a Unity AnimationCurve.</summary>
        public Curve(AnimationCurve animationCurve)
        {
            if (animationCurve == null) throw new ArgumentNullException(nameof(animationCurve));
            _function = t => animationCurve.Evaluate(t);
            _duration = animationCurve.length > 0
                ? animationCurve.keys[animationCurve.length - 1].time
                : 1f;
        }

        #endregion

        /// <summary>Evaluate the curve at a given time.</summary>
        public float Evaluate(float time)
        {
            return _function(time);
        }

        /// <summary>Evaluate the curve at normalized time [0..1], mapping to [0..duration].</summary>
        public float EvaluateNormalized(float normalizedTime)
        {
            return _function(normalizedTime * _duration);
        }

        #region Cubic Bezier

        /// <summary>Create a cubic bezier curve.</summary>
        /// <param name="duration">Duration in seconds.</param>
        /// <param name="x1">First control point X [0,1].</param>
        /// <param name="y1">First control point Y.</param>
        /// <param name="x2">Second control point X [0,1].</param>
        /// <param name="y2">Second control point Y.</param>
        public static Curve Bezier(float duration, float x1, float y1, float x2, float y2)
        {
            return new Curve(t =>
            {
                float nt = Mathf.Clamp01(t / duration);
                float bezierT = FindBezierT(nt, x1, x2);
                return CubicBezierY(bezierT, y1, y2);
            }, duration);
        }

        private static float CubicBezierY(float t, float y1, float y2)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 3f * (1f - t) * (1f - t) * t * y1 + 3f * (1f - t) * t2 * y2 + t3;
        }

        private static float FindBezierT(float x, float x1, float x2, int iterations = 8)
        {
            float t = x;
            for (int i = 0; i < iterations; i++)
            {
                float currentX = CubicBezierX(t, x1, x2);
                float dx = CubicBezierXDerivative(t, x1, x2);
                if (Mathf.Abs(dx) < 1e-6f) break;
                t -= (currentX - x) / dx;
                t = Mathf.Clamp01(t);
            }
            return t;
        }

        private static float CubicBezierX(float t, float x1, float x2)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            return 3f * (1f - t) * (1f - t) * t * x1 + 3f * (1f - t) * t2 * x2 + t3;
        }

        private static float CubicBezierXDerivative(float t, float x1, float x2)
        {
            return 3f * (1f - t) * (1f - t) * x1
                 + 6f * (1f - t) * t * (x2 - x1)
                 + 3f * t * t * (1f - x2);
        }

        #endregion

        #region Built-in Easings

        /// <summary>Linear curve from 0 to 1.</summary>
        public static Curve Linear(float duration) =>
            new Curve(t => t / duration, duration);

        // --- Quad ---
        public static Curve EaseInQuad(float duration) =>
            new Curve(t => { float n = t / duration; return n * n; }, duration);

        public static Curve EaseOutQuad(float duration) =>
            new Curve(t => { float n = t / duration; return 1f - (1f - n) * (1f - n); }, duration);

        public static Curve EaseInOutQuad(float duration) =>
            new Curve(t => { float n = t / duration; return n < 0.5f ? 2f * n * n : 1f - Mathf.Pow(-2f * n + 2f, 2f) / 2f; }, duration);

        // --- Cubic ---
        public static Curve EaseInCubic(float duration) =>
            new Curve(t => { float n = t / duration; return n * n * n; }, duration);

        public static Curve EaseOutCubic(float duration) =>
            new Curve(t => { float n = t / duration; return 1f - Mathf.Pow(1f - n, 3f); }, duration);

        public static Curve EaseInOutCubic(float duration) =>
            new Curve(t => { float n = t / duration; return n < 0.5f ? 4f * n * n * n : 1f - Mathf.Pow(-2f * n + 2f, 3f) / 2f; }, duration);

        // --- Quart ---
        public static Curve EaseInQuart(float duration) =>
            new Curve(t => { float n = t / duration; return n * n * n * n; }, duration);

        public static Curve EaseOutQuart(float duration) =>
            new Curve(t => { float n = t / duration; return 1f - Mathf.Pow(1f - n, 4f); }, duration);

        public static Curve EaseInOutQuart(float duration) =>
            new Curve(t => { float n = t / duration; return n < 0.5f ? 8f * n * n * n * n : 1f - Mathf.Pow(-2f * n + 2f, 4f) / 2f; }, duration);

        // --- Quint ---
        public static Curve EaseInQuint(float duration) =>
            new Curve(t => { float n = t / duration; return n * n * n * n * n; }, duration);

        public static Curve EaseOutQuint(float duration) =>
            new Curve(t => { float n = t / duration; return 1f - Mathf.Pow(1f - n, 5f); }, duration);

        public static Curve EaseInOutQuint(float duration) =>
            new Curve(t => { float n = t / duration; return n < 0.5f ? 16f * n * n * n * n * n : 1f - Mathf.Pow(-2f * n + 2f, 5f) / 2f; }, duration);

        // --- Sine ---
        public static Curve EaseInSine(float duration) =>
            new Curve(t => { float n = t / duration; return 1f - Mathf.Cos(n * Mathf.PI / 2f); }, duration);

        public static Curve EaseOutSine(float duration) =>
            new Curve(t => { float n = t / duration; return Mathf.Sin(n * Mathf.PI / 2f); }, duration);

        public static Curve EaseInOutSine(float duration) =>
            new Curve(t => { float n = t / duration; return -(Mathf.Cos(Mathf.PI * n) - 1f) / 2f; }, duration);

        // --- Expo ---
        public static Curve EaseInExpo(float duration) =>
            new Curve(t => { float n = t / duration; return n <= 0f ? 0f : Mathf.Pow(2f, 10f * n - 10f); }, duration);

        public static Curve EaseOutExpo(float duration) =>
            new Curve(t => { float n = t / duration; return n >= 1f ? 1f : 1f - Mathf.Pow(2f, -10f * n); }, duration);

        public static Curve EaseInOutExpo(float duration) =>
            new Curve(t =>
            {
                float n = t / duration;
                if (n <= 0f) return 0f;
                if (n >= 1f) return 1f;
                return n < 0.5f
                    ? Mathf.Pow(2f, 20f * n - 10f) / 2f
                    : (2f - Mathf.Pow(2f, -20f * n + 10f)) / 2f;
            }, duration);

        // --- Circ ---
        public static Curve EaseInCirc(float duration) =>
            new Curve(t => { float n = t / duration; return 1f - Mathf.Sqrt(1f - Mathf.Pow(n, 2f)); }, duration);

        public static Curve EaseOutCirc(float duration) =>
            new Curve(t => { float n = t / duration; return Mathf.Sqrt(1f - Mathf.Pow(n - 1f, 2f)); }, duration);

        public static Curve EaseInOutCirc(float duration) =>
            new Curve(t =>
            {
                float n = t / duration;
                return n < 0.5f
                    ? (1f - Mathf.Sqrt(1f - Mathf.Pow(2f * n, 2f))) / 2f
                    : (Mathf.Sqrt(1f - Mathf.Pow(-2f * n + 2f, 2f)) + 1f) / 2f;
            }, duration);

        // --- Back ---
        public static Curve EaseInBack(float duration) =>
            new Curve(t =>
            {
                float n = t / duration;
                const float c1 = 1.70158f;
                const float c3 = c1 + 1f;
                return c3 * n * n * n - c1 * n * n;
            }, duration);

        public static Curve EaseOutBack(float duration) =>
            new Curve(t =>
            {
                float n = t / duration;
                const float c1 = 1.70158f;
                const float c3 = c1 + 1f;
                return 1f + c3 * Mathf.Pow(n - 1f, 3f) + c1 * Mathf.Pow(n - 1f, 2f);
            }, duration);

        public static Curve EaseInOutBack(float duration) =>
            new Curve(t =>
            {
                float n = t / duration;
                const float c1 = 1.70158f;
                const float c2 = c1 * 1.525f;
                return n < 0.5f
                    ? (Mathf.Pow(2f * n, 2f) * ((c2 + 1f) * 2f * n - c2)) / 2f
                    : (Mathf.Pow(2f * n - 2f, 2f) * ((c2 + 1f) * (n * 2f - 2f) + c2) + 2f) / 2f;
            }, duration);

        // --- Elastic ---
        public static Curve EaseInElastic(float duration) =>
            new Curve(t =>
            {
                float n = t / duration;
                if (n <= 0f) return 0f;
                if (n >= 1f) return 1f;
                const float c4 = 2f * Mathf.PI / 3f;
                return -Mathf.Pow(2f, 10f * n - 10f) * Mathf.Sin((n * 10f - 10.75f) * c4);
            }, duration);

        public static Curve EaseOutElastic(float duration) =>
            new Curve(t =>
            {
                float n = t / duration;
                if (n <= 0f) return 0f;
                if (n >= 1f) return 1f;
                const float c4 = 2f * Mathf.PI / 3f;
                return Mathf.Pow(2f, -10f * n) * Mathf.Sin((n * 10f - 0.75f) * c4) + 1f;
            }, duration);

        public static Curve EaseInOutElastic(float duration) =>
            new Curve(t =>
            {
                float n = t / duration;
                if (n <= 0f) return 0f;
                if (n >= 1f) return 1f;
                const float c5 = 2f * Mathf.PI / 4.5f;
                return n < 0.5f
                    ? -(Mathf.Pow(2f, 20f * n - 10f) * Mathf.Sin((20f * n - 11.125f) * c5)) / 2f
                    : (Mathf.Pow(2f, -20f * n + 10f) * Mathf.Sin((20f * n - 11.125f) * c5)) / 2f + 1f;
            }, duration);

        // --- Bounce ---
        public static Curve EaseInBounce(float duration) =>
            new Curve(t => { float n = t / duration; return 1f - BounceOut(1f - n); }, duration);

        public static Curve EaseOutBounce(float duration) =>
            new Curve(t => { float n = t / duration; return BounceOut(n); }, duration);

        public static Curve EaseInOutBounce(float duration) =>
            new Curve(t =>
            {
                float n = t / duration;
                return n < 0.5f
                    ? (1f - BounceOut(1f - 2f * n)) / 2f
                    : (1f + BounceOut(2f * n - 1f)) / 2f;
            }, duration);

        private static float BounceOut(float n)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;

            if (n < 1f / d1) return n1 * n * n;
            if (n < 2f / d1) { n -= 1.5f / d1; return n1 * n * n + 0.75f; }
            if (n < 2.5f / d1) { n -= 2.25f / d1; return n1 * n * n + 0.9375f; }
            n -= 2.625f / d1;
            return n1 * n * n + 0.984375f;
        }

        #endregion
    }
}
