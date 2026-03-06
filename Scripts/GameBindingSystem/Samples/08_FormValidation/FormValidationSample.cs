using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BindingSystem.Samples
{
    /// <summary>
    /// Sample 08: Form Validation
    /// Demonstrates complex Derived states to validate a user registration form.
    /// The Submit button is only interactable if ALL fields are valid.
    /// </summary>
    public class FormValidationSample : MonoBehaviour
    {
        [Header("Inputs")]
        [SerializeField] private TMP_InputField _emailInput;
        [SerializeField] private TMP_InputField _passwordInput;
        [SerializeField] private TMP_InputField _confirmPasswordInput;
        [SerializeField] private Toggle _termsToggle;

        [Header("Outputs & Actions")]
        [SerializeField] private TMP_Text _emailErrorText;
        [SerializeField] private TMP_Text _passwordErrorText;
        [SerializeField] private Button _submitButton;
        [SerializeField] private TMP_Text _successMessageText;

        // Form Data
        public Bindable<string> Email = new("");
        public Bindable<string> Password = new("");
        public Bindable<string> ConfirmPassword = new("");
        public Bindable<bool> AcceptedTerms = new(false);

        void Start()
        {
            // 1. Two-way bind inputs
            if (_emailInput != null) _emailInput.BindTextTwoWay(Email);
            if (_passwordInput != null) _passwordInput.BindTextTwoWay(Password);
            if (_confirmPasswordInput != null) _confirmPasswordInput.BindTextTwoWay(ConfirmPassword);
            if (_termsToggle != null) _termsToggle.BindIsOnTwoWay(AcceptedTerms);

            // 2. Define Validation Logic (Derived States)
            var isEmailValid = Derived.From(Email, email => 
                string.IsNullOrEmpty(email) || Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"));
                
            var hasEmailInput = Derived.From(Email, e => !string.IsNullOrEmpty(e));

            var isPasswordLengthValid = Derived.From(Password, p => 
                string.IsNullOrEmpty(p) || p.Length >= 6);

            var doPasswordsMatch = Derived.From(Password, ConfirmPassword, 
                (p, cp) => string.IsNullOrEmpty(cp) || p == cp);

            // 3. Overall Form Validation
            // Combine all required valid states (Derived from 4 sources)
            var isFormValid = Derived.From(isEmailValid, hasEmailInput, isPasswordLengthValid, doPasswordsMatch, 
                (emailValid, hasEmail, passLengthValid, passMatch) => 
                {
                    return emailValid && hasEmail && passLengthValid && passMatch && AcceptedTerms.Value;
                });

            // Terms toggle is the 5th criteria, we can hook it directly into the Submit button transform, 
            // or nest it. Let's make the Submit Button dependent on isFormValid AND AcceptedTerms
            var canSubmit = Derived.From(isFormValid, AcceptedTerms, (valid, terms) => valid && terms);

            // 4. Bind UI to Validations
            if (_submitButton != null)
            {
                _submitButton.BindInteractable(canSubmit);
                _submitButton.onClick.AddListener(SubmitForm);
            }

            if (_emailErrorText != null)
            {
                // Show error only if email is NOT valid AND it's not empty
                var showEmailError = Derived.From(isEmailValid, hasEmailInput, (valid, hasInput) => !valid && hasInput);
                _emailErrorText.BindText(showEmailError, show => show ? "Invalid email format!" : "");
                _emailErrorText.BindColor(showEmailError, show => show ? Color.red : Color.clear);
            }

            if (_passwordErrorText != null)
            {
                // Show specific errors for password
                var passwordErrorStr = Derived.From(isPasswordLengthValid, doPasswordsMatch, (lenValid, match) =>
                {
                    if (!lenValid) return "Password must be at least 6 characters.";
                    if (!match) return "Passwords do not match.";
                    return "";
                });
                
                _passwordErrorText.BindText(passwordErrorStr);
                _passwordErrorText.BindColor(passwordErrorStr, err => string.IsNullOrEmpty(err) ? Color.clear : Color.red);
            }
            
            if (_successMessageText != null)
                _successMessageText.gameObject.SetActive(false);
        }

        private void SubmitForm()
        {
            if (_successMessageText != null)
            {
                _successMessageText.gameObject.SetActive(true);
                _successMessageText.text = $"Successfully registered: {Email.Value}";
            }
            Debug.Log($"Form Submitted! Email: {Email.Value}");
        }
    }
}
