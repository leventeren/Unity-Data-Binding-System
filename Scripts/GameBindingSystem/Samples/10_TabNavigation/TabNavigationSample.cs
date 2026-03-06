using UnityEngine;
using UnityEngine.UI;

namespace BindingSystem.Samples
{
    /// <summary>
    /// Sample 10: Tab Navigation System
    /// Demonstrates routing an Active Tab index to multiple CanvasGroups, 
    /// showing only the active tab and hiding the rest cleanly using derived bools.
    /// </summary>
    public class TabNavigationSample : MonoBehaviour
    {
        [Header("State")]
        public Bindable<int> ActiveTabIndex = new(0);

        [Header("Tab Buttons")]
        [SerializeField] private Button _tab0Button;
        [SerializeField] private Button _tab1Button;
        [SerializeField] private Button _tab2Button;

        [Header("Tab Content Panels")]
        [SerializeField] private CanvasGroup _tab0Content;
        [SerializeField] private CanvasGroup _tab1Content;
        [SerializeField] private CanvasGroup _tab2Content;

        void Start()
        {
            // === Bind Buttons ===
            // When clicked, they change the global ActiveTabIndex
            if (_tab0Button != null) _tab0Button.BindClick(() => ActiveTabIndex.Value = 0);
            if (_tab1Button != null) _tab1Button.BindClick(() => ActiveTabIndex.Value = 1);
            if (_tab2Button != null) _tab2Button.BindClick(() => ActiveTabIndex.Value = 2);

            // Optional: Highlight the active button by making it non-interactable when active
            if (_tab0Button != null) _tab0Button.BindInteractable(ActiveTabIndex, current => current != 0);
            if (_tab1Button != null) _tab1Button.BindInteractable(ActiveTabIndex, current => current != 1);
            if (_tab2Button != null) _tab2Button.BindInteractable(ActiveTabIndex, current => current != 2);

            // === Bind Content Visibility ===
            // We use BindActive for CanvasGroups (which controls Alpha + BlocksRaycast + Interactable)
            if (_tab0Content != null) _tab0Content.BindActive(ActiveTabIndex, current => current == 0);
            if (_tab1Content != null) _tab1Content.BindActive(ActiveTabIndex, current => current == 1);
            if (_tab2Content != null) _tab2Content.BindActive(ActiveTabIndex, current => current == 2);

            // Alternatively, if they are GameObjects instead of CanvasGroups:
            // _tab0Content.gameObject.BindActive(ActiveTabIndex, current => current == 0);
        }

        // --- Test Methods ---
        [ContextMenu("Go To Tab 0")]
        public void GoToTab0() => ActiveTabIndex.Value = 0;

        [ContextMenu("Go To Tab 1")]
        public void GoToTab1() => ActiveTabIndex.Value = 1;

        [ContextMenu("Go To Tab 2")]
        public void GoToTab2() => ActiveTabIndex.Value = 2;
    }
}
