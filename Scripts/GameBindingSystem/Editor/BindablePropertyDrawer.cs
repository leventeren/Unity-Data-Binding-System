using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace BindingSystem.Editor
{
    /// <summary>
    /// Custom property drawer for Bindable<T>.
    /// Displays only the inner _value field in the Inspector, hiding the wrapper.
    /// In Play mode, modifications via Inspector automatically notify the bindings.
    /// </summary>
    [CustomPropertyDrawer(typeof(Bindable<>), true)]
    public class BindablePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var valueProperty = property.FindPropertyRelative("_value");

            if (valueProperty != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(position, valueProperty, label, true);
                
                if (EditorGUI.EndChangeCheck())
                {
                    property.serializedObject.ApplyModifiedProperties();

                    // If we are in play mode, manually trigger the NotifyChanged to update UI
                    if (Application.isPlaying)
                    {
                        var targetObject = GetTargetObjectOfProperty(property);
                        if (targetObject != null)
                        {
                            var method = targetObject.GetType().GetMethod("NotifyChanged", BindingFlags.Public | BindingFlags.Instance);
                            method?.Invoke(targetObject, null);
                        }
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(position, label.text, "[Bindable] Type not serializable");
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var valueProperty = property.FindPropertyRelative("_value");

            if (valueProperty != null)
            {
                return EditorGUI.GetPropertyHeight(valueProperty, label, true);
            }

            return EditorGUIUtility.singleLineHeight;
        }

        #region Reflection Helpers

        /// <summary>
        /// Gets the actual object instance represented by the SerializedProperty.
        /// Handles nested properties and array/list elements.
        /// </summary>
        private object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            if (prop == null) return null;

            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');

            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }

        private object GetValue_Imp(object source, string name)
        {
            if (source == null) return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null) return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null) return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }

        #endregion
    }
}
