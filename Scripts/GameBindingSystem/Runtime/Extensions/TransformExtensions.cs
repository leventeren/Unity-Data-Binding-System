using System;
using UnityEngine;

namespace BindingSystem
{
    /// <summary>
    /// Transform binding extension methods for position, rotation, and scale.
    /// </summary>
    public static class TransformExtensions
    {
        #region Position

        /// <summary>Bind world position to a Vector3 Bindable.</summary>
        public static void BindPosition(this Transform transform, IBindable<Vector3> bindable)
        {
            BindContext.Get(transform).Bind(bindable, v => transform.position = v);
        }

        /// <summary>Bind world position with a transform function.</summary>
        public static void BindPosition<T>(this Transform transform, IBindable<T> bindable, Func<T, Vector3> func)
        {
            BindContext.Get(transform).Bind(bindable, v => transform.position = func(v));
        }

        /// <summary>Bind local position to a Vector3 Bindable.</summary>
        public static void BindLocalPosition(this Transform transform, IBindable<Vector3> bindable)
        {
            BindContext.Get(transform).Bind(bindable, v => transform.localPosition = v);
        }

        /// <summary>Bind local position with a transform function.</summary>
        public static void BindLocalPosition<T>(this Transform transform, IBindable<T> bindable, Func<T, Vector3> func)
        {
            BindContext.Get(transform).Bind(bindable, v => transform.localPosition = func(v));
        }

        #endregion

        #region Rotation

        /// <summary>Bind world rotation to a Quaternion Bindable.</summary>
        public static void BindRotation(this Transform transform, IBindable<Quaternion> bindable)
        {
            BindContext.Get(transform).Bind(bindable, v => transform.rotation = v);
        }

        /// <summary>Bind local rotation to a Quaternion Bindable.</summary>
        public static void BindLocalRotation(this Transform transform, IBindable<Quaternion> bindable)
        {
            BindContext.Get(transform).Bind(bindable, v => transform.localRotation = v);
        }

        /// <summary>Bind euler angles to a Vector3 Bindable.</summary>
        public static void BindEulerAngles(this Transform transform, IBindable<Vector3> bindable)
        {
            BindContext.Get(transform).Bind(bindable, v => transform.eulerAngles = v);
        }

        /// <summary>Bind local euler angles to a Vector3 Bindable.</summary>
        public static void BindLocalEulerAngles(this Transform transform, IBindable<Vector3> bindable)
        {
            BindContext.Get(transform).Bind(bindable, v => transform.localEulerAngles = v);
        }

        /// <summary>Bind euler angles with a transform function.</summary>
        public static void BindEulerAngles<T>(this Transform transform, IBindable<T> bindable, Func<T, Vector3> func)
        {
            BindContext.Get(transform).Bind(bindable, v => transform.eulerAngles = func(v));
        }

        #endregion

        #region Scale

        /// <summary>Bind local scale to a Vector3 Bindable.</summary>
        public static void BindLocalScale(this Transform transform, IBindable<Vector3> bindable)
        {
            BindContext.Get(transform).Bind(bindable, v => transform.localScale = v);
        }

        /// <summary>Bind local scale with a transform function.</summary>
        public static void BindLocalScale<T>(this Transform transform, IBindable<T> bindable, Func<T, Vector3> func)
        {
            BindContext.Get(transform).Bind(bindable, v => transform.localScale = func(v));
        }

        /// <summary>Bind uniform local scale to a float Bindable.</summary>
        public static void BindLocalScale(this Transform transform, IBindable<float> bindable)
        {
            BindContext.Get(transform).Bind(bindable, v => transform.localScale = Vector3.one * v);
        }

        #endregion
    }
}
