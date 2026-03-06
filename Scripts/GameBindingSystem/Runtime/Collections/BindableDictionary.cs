using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BindingSystem
{
    /// <summary>
    /// Event data for element-level dictionary operations.
    /// </summary>
    public struct DictElementEventArgs<TKey, TValue>
    {
        public TKey Key;
        public TValue Value;
        public TValue OldValue; // Only used for OnElementChange
    }

    /// <summary>
    /// A reactive dictionary that notifies observers on element-level changes (add, change, remove)
    /// as well as bulk changes (clear, full dictionary replacement).
    /// </summary>
    [Serializable]
    public class BindableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    {
        [SerializeField] private DictionarySerializable<TKey, TValue> _dict = new DictionarySerializable<TKey, TValue>();

        // Bulk change events
        private event Action _onChanged;
        private event Action<Dictionary<TKey, TValue>> _onChangedWithValue;

        // Element-level events
        public event Action<DictElementEventArgs<TKey, TValue>> OnElementAdd;
        public event Action<DictElementEventArgs<TKey, TValue>> OnElementChange;
        public event Action<DictElementEventArgs<TKey, TValue>> OnElementRemove;

        /// <summary>Invoked on any change.</summary>
        public event Action OnChanged
        {
            add => _onChanged += value;
            remove => _onChanged -= value;
        }

        /// <summary>Invoked on any change, passing the current dictionary.</summary>
        public event Action<Dictionary<TKey, TValue>> OnChangedWithValue
        {
            add => _onChangedWithValue += value;
            remove => _onChangedWithValue -= value;
        }

        /// <summary>The underlying dictionary. Setting replaces it entirely and triggers OnChanged.</summary>
        public Dictionary<TKey, TValue> Value
        {
            get => _dict.Dictionary;
            set
            {
                _dict = new DictionarySerializable<TKey, TValue>(value ?? new Dictionary<TKey, TValue>());
                RaiseBulkChanged();
            }
        }

        public BindableDictionary() { }

        public BindableDictionary(IDictionary<TKey, TValue> dictionary)
        {
            _dict = new DictionarySerializable<TKey, TValue>(new Dictionary<TKey, TValue>(dictionary));
        }

        #region IDictionary<TKey, TValue> Implementation

        public TValue this[TKey key]
        {
            get => _dict.Dictionary[key];
            set
            {
                if (_dict.Dictionary.TryGetValue(key, out var oldValue))
                {
                    if (EqualityComparer<TValue>.Default.Equals(oldValue, value)) return;

                    _dict.Dictionary[key] = value;
                    OnElementChange?.Invoke(new DictElementEventArgs<TKey, TValue>
                    {
                        Key = key,
                        Value = value,
                        OldValue = oldValue
                    });
                }
                else
                {
                    _dict.Dictionary[key] = value;
                    OnElementAdd?.Invoke(new DictElementEventArgs<TKey, TValue>
                    {
                        Key = key,
                        Value = value
                    });
                }
                RaiseBulkChanged();
            }
        }

        public ICollection<TKey> Keys => _dict.Dictionary.Keys;
        public ICollection<TValue> Values => _dict.Dictionary.Values;
        public int Count => _dict.Dictionary.Count;
        public bool IsReadOnly => false;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => _dict.Dictionary.Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => _dict.Dictionary.Values;

        public void Add(TKey key, TValue value)
        {
            _dict.Dictionary.Add(key, value);
            OnElementAdd?.Invoke(new DictElementEventArgs<TKey, TValue>
            {
                Key = key,
                Value = value
            });
            RaiseBulkChanged();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public bool Remove(TKey key)
        {
            if (_dict.Dictionary.TryGetValue(key, out var value))
            {
                _dict.Dictionary.Remove(key);
                OnElementRemove?.Invoke(new DictElementEventArgs<TKey, TValue>
                {
                    Key = key,
                    Value = value
                });
                RaiseBulkChanged();
                return true;
            }
            return false;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        public void Clear()
        {
            _dict.Dictionary.Clear();
            RaiseBulkChanged();
        }

        public bool ContainsKey(TKey key) => _dict.Dictionary.ContainsKey(key);
        public bool Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)_dict.Dictionary).Contains(item);
        public bool TryGetValue(TKey key, out TValue value) => _dict.Dictionary.TryGetValue(key, out value);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)_dict.Dictionary).CopyTo(array, arrayIndex);

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dict.Dictionary.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _dict.Dictionary.GetEnumerator();

        #endregion

        /// <summary>Manually notify that the dictionary has changed externally.</summary>
        public void NotifyChanged()
        {
            RaiseBulkChanged();
        }

        private void RaiseBulkChanged()
        {
            _onChanged?.Invoke();
            _onChangedWithValue?.Invoke(_dict.Dictionary);
        }
    }

    /// <summary>
    /// Serializable wrapper for Dictionary to support Unity serialization.
    /// </summary>
    [Serializable]
    internal class DictionarySerializable<TKey, TValue> : ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> _keys = new List<TKey>();
        [SerializeField] private List<TValue> _values = new List<TValue>();

        [NonSerialized] private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();

        public Dictionary<TKey, TValue> Dictionary => _dictionary;

        public DictionarySerializable()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public DictionarySerializable(Dictionary<TKey, TValue> dictionary)
        {
            _dictionary = dictionary ?? new Dictionary<TKey, TValue>();
        }

        public void OnBeforeSerialize()
        {
            _keys.Clear();
            _values.Clear();
            foreach (var kvp in _dictionary)
            {
                _keys.Add(kvp.Key);
                _values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            _dictionary = new Dictionary<TKey, TValue>();
            int count = Math.Min(_keys.Count, _values.Count);
            for (int i = 0; i < count; i++)
            {
                if (!_dictionary.ContainsKey(_keys[i]))
                    _dictionary[_keys[i]] = _values[i];
            }
        }
    }
}
