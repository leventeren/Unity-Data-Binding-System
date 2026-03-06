using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BindingSystem.Samples
{
    /// <summary>
    /// Sample 07: Derived Chain — Shop System
    /// Demonstrates complex derived state chains:
    ///   PlayerGold + ItemPrice → CanAfford (bool)
    ///   CanAfford → Button.interactable
    ///   ItemName + ItemPrice → DisplayText
    ///   PlayerGold → GoldText
    ///   After purchase: Gold updates → all bindings refresh automatically
    /// 
    /// SETUP:
    /// 1. Create UI: Gold display text, 3 shop item rows (each with name text, price text, buy button)
    /// 2. Attach this script and assign references
    /// 3. Play → try buying items → see gold update → button interactability changes
    /// </summary>
    public class ShopSample : MonoBehaviour
    {
        [Serializable]
        public class ShopItem
        {
            public string Name;
            public int Price;
            public Sprite Icon;
            public string Description;
        }

        [Header("Player Data")]
        public Bindable<int> PlayerGold = new(500);

        [Header("Shop Items")]
        [SerializeField] private ShopItem[] _shopItems = new[]
        {
            new ShopItem { Name = "Health Potion", Price = 50, Description = "Restores 50 HP" },
            new ShopItem { Name = "Mana Potion", Price = 75, Description = "Restores 30 MP" },
            new ShopItem { Name = "Legendary Sword", Price = 300, Description = "+50 Attack" },
            new ShopItem { Name = "Dragon Shield", Price = 450, Description = "+80 Defense" },
        };

        [Header("UI References")]
        [SerializeField] private TMP_Text _goldText;
        [SerializeField] private Transform _shopItemsParent;
        [SerializeField] private GameObject _shopItemPrefab; // Prefab with: TMP_Text(name), TMP_Text(price), Button(buy)

        [Header("Purchase Log")]
        [SerializeField] private TMP_Text _logText;
        private Bindable<string> _lastPurchaseLog = new("Welcome to the Shop!");

        void Start()
        {
            // === Gold Display ===
            if (_goldText != null)
                _goldText.BindText(PlayerGold, gold => $"💰 {gold:N0} Gold");

            // === Purchase Log ===
            if (_logText != null)
                _logText.BindText(_lastPurchaseLog);

            // === Create Shop Item UI ===
            if (_shopItemsParent != null && _shopItemPrefab != null)
            {
                foreach (var item in _shopItems)
                {
                    CreateShopItemUI(item);
                }
            }
        }

        private void CreateShopItemUI(ShopItem item)
        {
            var go = Instantiate(_shopItemPrefab, _shopItemsParent);
            go.name = $"ShopItem_{item.Name}";

            // Find components in prefab
            var texts = go.GetComponentsInChildren<TMP_Text>();
            var button = go.GetComponentInChildren<Button>();

            // === Derived: CanAfford ===
            var canAfford = Derived.From(PlayerGold, gold => gold >= item.Price);

            // === Derived: Display Text ===
            if (texts.Length > 0)
                texts[0].BindText(PlayerGold, _ => item.Name); // Name doesn't change, but we bind for consistency

            if (texts.Length > 1)
            {
                // Price text with affordability color
                texts[1].BindText(canAfford, affordable =>
                    affordable ? $"<color=green>{item.Price}G</color>" : $"<color=red>{item.Price}G</color>");
            }

            if (texts.Length > 2)
                texts[2].text = item.Description;

            // === Button interactability bound to CanAfford ===
            if (button != null)
            {
                button.BindInteractable(canAfford);

                // Purchase action
                button.BindClick(() =>
                {
                    if (PlayerGold.Value >= item.Price)
                    {
                        PlayerGold.Value -= item.Price;
                        _lastPurchaseLog.Value = $"Purchased {item.Name} for {item.Price}G! Remaining: {PlayerGold.Value}G";
                        Debug.Log($"[Shop] Bought {item.Name} for {item.Price}G");
                    }
                });
            }
        }

        // --- Test Methods ---
        [ContextMenu("Add 100 Gold")]
        public void AddGold() => PlayerGold.Value += 100;

        [ContextMenu("Add 500 Gold")]
        public void AddLotsOfGold() => PlayerGold.Value += 500;

        [ContextMenu("Reset Gold to 500")]
        public void ResetGold() => PlayerGold.Value = 500;

        [ContextMenu("Spend All Gold")]
        public void SpendAll() => PlayerGold.Value = 0;
    }
}
