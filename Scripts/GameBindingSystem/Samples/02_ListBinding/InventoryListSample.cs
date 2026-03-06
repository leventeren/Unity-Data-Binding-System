using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BindingSystem.Samples
{
    /// <summary>
    /// Sample 02: List Binding — Inventory System
    /// Demonstrates BindableList with prefab instantiation/destruction synced to list changes.
    /// 
    /// SETUP:
    /// 1. Create a Canvas with a vertical ScrollView content area
    /// 2. Create an Item Prefab with TMP_Text (name) and TMP_Text (quantity)
    /// 3. Attach this script, assign ContentParent and ItemPrefab
    /// 4. Use Inspector buttons or code to Add/Remove items and see UI update
    /// </summary>
    public class InventoryListSample : MonoBehaviour
    {
        [Serializable]
        public class ItemData
        {
            public string Name;
            public int Quantity;
            public Sprite Icon;

            public ItemData(string name, int quantity)
            {
                Name = name;
                Quantity = quantity;
            }
        }

        [Header("UI References")]
        [SerializeField] private Transform _contentParent;
        [SerializeField] private GameObject _itemPrefab;
        [SerializeField] private TMP_Text _totalItemsText;

        [Header("Inventory Data")]
        public BindableList<ItemData> Inventory = new();

        void Start()
        {
            // Bind list element events to UI
            Inventory.OnElementAdd += OnItemAdded;
            Inventory.OnElementRemove += OnItemRemoved;

            // Bind total count display
            if (_totalItemsText != null)
            {
                Inventory.OnChanged += () =>
                    _totalItemsText.text = $"Total Items: {Inventory.Count}";
                _totalItemsText.text = $"Total Items: {Inventory.Count}";
            }

            // Add some initial items
            Inventory.Add(new ItemData("Sword", 1));
            Inventory.Add(new ItemData("Health Potion", 5));
            Inventory.Add(new ItemData("Shield", 1));
            Inventory.Add(new ItemData("Arrow", 20));
        }

        private void OnItemAdded(ListElementEventArgs<ItemData> args)
        {
            if (_contentParent == null || _itemPrefab == null) return;

            var go = Instantiate(_itemPrefab, _contentParent);
            go.name = $"Item_{args.Index}_{args.Value.Name}";

            // Find text components in the prefab and set data
            var texts = go.GetComponentsInChildren<TMP_Text>();
            if (texts.Length > 0) texts[0].text = args.Value.Name;
            if (texts.Length > 1) texts[1].text = $"x{args.Value.Quantity}";

            // Set sibling index to match list order
            go.transform.SetSiblingIndex(args.Index);
        }

        private void OnItemRemoved(ListElementEventArgs<ItemData> args)
        {
            if (_contentParent == null) return;

            if (args.Index < _contentParent.childCount)
            {
                Destroy(_contentParent.GetChild(args.Index).gameObject);
            }
        }

        void OnDestroy()
        {
            Inventory.OnElementAdd -= OnItemAdded;
            Inventory.OnElementRemove -= OnItemRemoved;
        }

        // --- Test Methods ---
        [ContextMenu("Add Random Item")]
        public void AddRandomItem()
        {
            string[] names = { "Gem", "Key", "Ring", "Book", "Scroll", "Armor", "Boots" };
            var item = new ItemData(names[UnityEngine.Random.Range(0, names.Length)], UnityEngine.Random.Range(1, 10));
            Inventory.Add(item);
        }

        [ContextMenu("Remove Last Item")]
        public void RemoveLastItem()
        {
            if (Inventory.Count > 0)
                Inventory.RemoveAt(Inventory.Count - 1);
        }

        [ContextMenu("Clear All")]
        public void ClearAll()
        {
            // Remove one by one to trigger individual events, then clear
            while (Inventory.Count > 0)
                Inventory.RemoveAt(Inventory.Count - 1);
        }

        [ContextMenu("Sort By Name")]
        public void SortByName()
        {
            Inventory.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.Ordinal));
            // After sort, rebuild UI
            RebuildUI();
        }

        private void RebuildUI()
        {
            if (_contentParent == null) return;

            // Destroy all children
            for (int i = _contentParent.childCount - 1; i >= 0; i--)
                Destroy(_contentParent.GetChild(i).gameObject);

            // Recreate from list
            for (int i = 0; i < Inventory.Count; i++)
            {
                OnItemAdded(new ListElementEventArgs<ItemData>
                {
                    Index = i,
                    Value = Inventory[i]
                });
            }
        }
    }
}
