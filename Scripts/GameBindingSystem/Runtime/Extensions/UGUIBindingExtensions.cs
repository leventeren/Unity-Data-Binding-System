using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BindingSystem
{
    /// <summary>
    /// UGUI-specific binding extension methods for common Unity UI components.
    /// Covers TextMeshPro, Image, Slider, Toggle, Dropdown, InputField, CanvasGroup, etc.
    /// </summary>
    public static class UGUIBindingExtensions
    {
        #region TMP_Text (TextMeshProUGUI / TextMeshPro)

        /// <summary>Bind a string Bindable directly to a TMP_Text component.</summary>
        public static void BindText(this TMP_Text text, IBindable<string> bindable)
        {
            BindContext.Get(text).Bind(bindable, v => text.text = v);
        }

        /// <summary>Bind a Bindable to TMP_Text with a transform function.</summary>
        public static void BindText<T>(this TMP_Text text, IBindable<T> bindable, Func<T, string> transform)
        {
            BindContext.Get(text).Bind(bindable, v => text.text = transform(v));
        }

        /// <summary>Bind two Bindables to TMP_Text with a transform function.</summary>
        public static void BindText<T1, T2>(this TMP_Text text,
            IBindable<T1> bindable1, IBindable<T2> bindable2,
            Func<T1, T2, string> transform)
        {
            var context = BindContext.Get(text);
            void Update() => text.text = transform(bindable1.Value, bindable2.Value);

            bindable1.OnChanged += Update;
            bindable2.OnChanged += Update;
            context.AddUnbindAction(() =>
            {
                bindable1.OnChanged -= Update;
                bindable2.OnChanged -= Update;
            });
            Update();
        }

        /// <summary>Bind three Bindables to TMP_Text with a transform function.</summary>
        public static void BindText<T1, T2, T3>(this TMP_Text text,
            IBindable<T1> b1, IBindable<T2> b2, IBindable<T3> b3,
            Func<T1, T2, T3, string> transform)
        {
            var context = BindContext.Get(text);
            void Update() => text.text = transform(b1.Value, b2.Value, b3.Value);

            b1.OnChanged += Update;
            b2.OnChanged += Update;
            b3.OnChanged += Update;
            context.AddUnbindAction(() =>
            {
                b1.OnChanged -= Update;
                b2.OnChanged -= Update;
                b3.OnChanged -= Update;
            });
            Update();
        }

        /// <summary>Bind a Bindable color to TMP_Text.</summary>
        public static void BindColor(this TMP_Text text, IBindable<Color> bindable)
        {
            BindContext.Get(text).Bind(bindable, v => text.color = v);
        }

        /// <summary>Bind a Bindable font size to TMP_Text.</summary>
        public static void BindFontSize(this TMP_Text text, IBindable<float> bindable)
        {
            BindContext.Get(text).Bind(bindable, v => text.fontSize = v);
        }

        #endregion

        #region Image

        /// <summary>Bind a Bindable Sprite to an Image.</summary>
        public static void BindSprite(this Image image, IBindable<Sprite> bindable)
        {
            BindContext.Get(image).Bind(bindable, v => image.sprite = v);
        }

        /// <summary>Bind a Bindable color to an Image.</summary>
        public static void BindColor(this Image image, IBindable<Color> bindable)
        {
            BindContext.Get(image).Bind(bindable, v => image.color = v);
        }

        /// <summary>Bind a Bindable color with transform.</summary>
        public static void BindColor<T>(this Image image, IBindable<T> bindable, Func<T, Color> transform)
        {
            BindContext.Get(image).Bind(bindable, v => image.color = transform(v));
        }

        /// <summary>Bind a Bindable fill amount to an Image.</summary>
        public static void BindFillAmount(this Image image, IBindable<float> bindable)
        {
            BindContext.Get(image).Bind(bindable, v => image.fillAmount = v);
        }

        /// <summary>Bind fill amount from a Bindable with transform.</summary>
        public static void BindFillAmount<T>(this Image image, IBindable<T> bindable, Func<T, float> transform)
        {
            BindContext.Get(image).Bind(bindable, v => image.fillAmount = transform(v));
        }

        /// <summary>Bind fill amount from two Bindables with transform.</summary>
        public static void BindFillAmount<T1, T2>(this Image image,
            IBindable<T1> b1, IBindable<T2> b2, Func<T1, T2, float> transform)
        {
            var context = BindContext.Get(image);
            void Update() => image.fillAmount = transform(b1.Value, b2.Value);
            b1.OnChanged += Update;
            b2.OnChanged += Update;
            context.AddUnbindAction(() =>
            {
                b1.OnChanged -= Update;
                b2.OnChanged -= Update;
            });
            Update();
        }

        #endregion

        #region Graphic (Base class for Image, TMP_Text, etc.)

        /// <summary>Bind a Bindable color to any Graphic component.</summary>
        public static void BindColor(this Graphic graphic, IBindable<Color> bindable)
        {
            BindContext.Get(graphic).Bind(bindable, v => graphic.color = v);
        }

        #endregion

        #region Slider

        /// <summary>Bind a float Bindable directly to a Slider value.</summary>
        public static void BindValue(this Slider slider, IBindable<float> bindable)
        {
            BindContext.Get(slider).Bind(bindable, v => slider.value = v);
        }

        /// <summary>Bind a Bindable to Slider value with a transform function.</summary>
        public static void BindValue<T>(this Slider slider, IBindable<T> bindable, Func<T, float> transform)
        {
            BindContext.Get(slider).Bind(bindable, v => slider.value = transform(v));
        }

        /// <summary>Bind two Bindables to Slider value (e.g., health / maxHealth).</summary>
        public static void BindValue<T1, T2>(this Slider slider,
            IBindable<T1> b1, IBindable<T2> b2, Func<T1, T2, float> transform)
        {
            var context = BindContext.Get(slider);
            void Update() => slider.value = transform(b1.Value, b2.Value);
            b1.OnChanged += Update;
            b2.OnChanged += Update;
            context.AddUnbindAction(() =>
            {
                b1.OnChanged -= Update;
                b2.OnChanged -= Update;
            });
            Update();
        }

        /// <summary>Two-way binding: Slider value ↔ Bindable float. Changes in either direction sync automatically.</summary>
        public static void BindValueTwoWay(this Slider slider, Bindable<float> bindable)
        {
            var context = BindContext.Get(slider);

            // Bindable → Slider
            context.Bind(bindable, v => slider.SetValueWithoutNotify(v));

            // Slider → Bindable
            void OnSliderChanged(float v) => bindable.Value = v;
            slider.onValueChanged.AddListener(OnSliderChanged);
            context.AddUnbindAction(() => slider.onValueChanged.RemoveListener(OnSliderChanged));
        }

        #endregion

        #region Toggle

        /// <summary>Bind a bool Bindable to a Toggle.</summary>
        public static void BindIsOn(this Toggle toggle, IBindable<bool> bindable)
        {
            BindContext.Get(toggle).Bind(bindable, v => toggle.isOn = v);
        }

        /// <summary>Two-way binding: Toggle ↔ Bindable bool.</summary>
        public static void BindIsOnTwoWay(this Toggle toggle, Bindable<bool> bindable)
        {
            var context = BindContext.Get(toggle);

            context.Bind(bindable, v => toggle.SetIsOnWithoutNotify(v));

            void OnToggleChanged(bool v) => bindable.Value = v;
            toggle.onValueChanged.AddListener(OnToggleChanged);
            context.AddUnbindAction(() => toggle.onValueChanged.RemoveListener(OnToggleChanged));
        }

        #endregion

        #region TMP_Dropdown

        /// <summary>Bind an int Bindable to a TMP_Dropdown value.</summary>
        public static void BindValue(this TMP_Dropdown dropdown, IBindable<int> bindable)
        {
            BindContext.Get(dropdown).Bind(bindable, v => dropdown.value = v);
        }

        /// <summary>Two-way binding: TMP_Dropdown ↔ Bindable int.</summary>
        public static void BindValueTwoWay(this TMP_Dropdown dropdown, Bindable<int> bindable)
        {
            var context = BindContext.Get(dropdown);

            context.Bind(bindable, v => dropdown.SetValueWithoutNotify(v));

            void OnDropdownChanged(int v) => bindable.Value = v;
            dropdown.onValueChanged.AddListener(OnDropdownChanged);
            context.AddUnbindAction(() => dropdown.onValueChanged.RemoveListener(OnDropdownChanged));
        }

        #endregion

        #region TMP_InputField

        /// <summary>Bind a string Bindable to a TMP_InputField.</summary>
        public static void BindText(this TMP_InputField inputField, IBindable<string> bindable)
        {
            BindContext.Get(inputField).Bind(bindable, v => inputField.text = v);
        }

        /// <summary>Two-way binding: TMP_InputField ↔ Bindable string.</summary>
        public static void BindTextTwoWay(this TMP_InputField inputField, Bindable<string> bindable)
        {
            var context = BindContext.Get(inputField);

            context.Bind(bindable, v => inputField.SetTextWithoutNotify(v));

            void OnTextChanged(string v) => bindable.Value = v;
            inputField.onValueChanged.AddListener(OnTextChanged);
            context.AddUnbindAction(() => inputField.onValueChanged.RemoveListener(OnTextChanged));
        }

        #endregion

        #region Selectable (Button, Slider, Toggle, Dropdown, InputField)

        /// <summary>Bind interactable state to any Selectable UI element.</summary>
        public static void BindInteractable(this Selectable selectable, IBindable<bool> bindable)
        {
            BindContext.Get(selectable).Bind(bindable, v => selectable.interactable = v);
        }

        /// <summary>Bind interactable state with a transform.</summary>
        public static void BindInteractable<T>(this Selectable selectable, IBindable<T> bindable, Func<T, bool> transform)
        {
            BindContext.Get(selectable).Bind(bindable, v => selectable.interactable = transform(v));
        }

        #endregion

        #region CanvasGroup

        /// <summary>Bind alpha of a CanvasGroup.</summary>
        public static void BindAlpha(this CanvasGroup canvasGroup, IBindable<float> bindable)
        {
            BindContext.Get(canvasGroup).Bind(bindable, v => canvasGroup.alpha = v);
        }

        /// <summary>Bind alpha with a transform.</summary>
        public static void BindAlpha<T>(this CanvasGroup canvasGroup, IBindable<T> bindable, Func<T, float> transform)
        {
            BindContext.Get(canvasGroup).Bind(bindable, v => canvasGroup.alpha = transform(v));
        }

        /// <summary>Bind interactable + blocksRaycasts of a CanvasGroup to a bool.</summary>
        public static void BindActive(this CanvasGroup canvasGroup, IBindable<bool> bindable)
        {
            BindContext.Get(canvasGroup).Bind(bindable, v =>
            {
                canvasGroup.alpha = v ? 1f : 0f;
                canvasGroup.interactable = v;
                canvasGroup.blocksRaycasts = v;
            });
        }

        #endregion

        #region GameObject Active State

        /// <summary>Bind the active state of a GameObject to a bool Bindable.</summary>
        public static void BindActive(this Component component, IBindable<bool> bindable)
        {
            BindContext.Get(component).Bind(bindable, v => component.gameObject.SetActive(v));
        }

        /// <summary>Bind the active state with a transform.</summary>
        public static void BindActive<T>(this Component component, IBindable<T> bindable, Func<T, bool> transform)
        {
            BindContext.Get(component).Bind(bindable, v => component.gameObject.SetActive(transform(v)));
        }

        #endregion

        #region Button Click

        /// <summary>Bind a button click to an action. Auto-unbinds when destroyed.</summary>
        public static void BindClick(this Button button, Action onClick)
        {
            var context = BindContext.Get(button);
            void Handler() => onClick();
            button.onClick.AddListener(Handler);
            context.AddUnbindAction(() => button.onClick.RemoveListener(Handler));
        }

        #endregion
    }
}
