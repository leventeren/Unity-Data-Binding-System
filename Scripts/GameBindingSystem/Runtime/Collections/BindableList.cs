using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BindingSystem
{
    /// <summary>
    /// Event data for element-level list operations.
    /// </summary>
    public struct ListElementEventArgs<T>
    {
        public int Index;
        public T Value;
        public T OldValue; // Only used for OnElementChange
    }

    /// <summary>
    /// A reactive list that notifies observers on element-level changes (add, remove, change, move, swap)
    /// as well as bulk changes (clear, sort, reverse, full list replacement).
    /// </summary>
    [Serializable]
    public class BindableList<T> : IList<T>, IReadOnlyList<T>
    {
        [SerializeField] private List<T> _list = new List<T>();

        // Bulk change events (from Bindable-like behavior)
        private event Action _onChanged;
        private event Action<List<T>> _onChangedWithValue;

        // Element-level events
        public event Action<ListElementEventArgs<T>> OnElementAdd;
        public event Action<ListElementEventArgs<T>> OnElementChange;
        public event Action<ListElementEventArgs<T>> OnElementRemove;
        public event Action<int, int> OnElementMoved; // oldIndex, newIndex
        public event Action<int, int> OnElementSwap;  // index1, index2

        /// <summary>Invoked on any change (bulk or element-level).</summary>
        public event Action OnChanged
        {
            add => _onChanged += value;
            remove => _onChanged -= value;
        }

        /// <summary>Invoked on any change, passing the current list.</summary>
        public event Action<List<T>> OnChangedWithValue
        {
            add => _onChangedWithValue += value;
            remove => _onChangedWithValue -= value;
        }

        /// <summary>The underlying list. Setting replaces the entire list and triggers OnChanged.</summary>
        public List<T> Value
        {
            get => _list;
            set
            {
                _list = value ?? new List<T>();
                RaiseBulkChanged();
            }
        }

        public BindableList() { }

        public BindableList(IEnumerable<T> collection)
        {
            _list = new List<T>(collection);
        }

        #region IList<T> Implementation

        public T this[int index]
        {
            get => _list[index];
            set
            {
                var oldValue = _list[index];
                if (EqualityComparer<T>.Default.Equals(oldValue, value)) return;

                _list[index] = value;
                OnElementChange?.Invoke(new ListElementEventArgs<T>
                {
                    Index = index,
                    Value = value,
                    OldValue = oldValue
                });
                RaiseBulkChanged();
            }
        }

        public int Count => _list.Count;
        public bool IsReadOnly => false;

        public void Add(T item)
        {
            _list.Add(item);
            OnElementAdd?.Invoke(new ListElementEventArgs<T>
            {
                Index = _list.Count - 1,
                Value = item
            });
            RaiseBulkChanged();
        }

        public void AddRange(IEnumerable<T> collection)
        {
            int startIndex = _list.Count;
            _list.AddRange(collection);
            for (int i = startIndex; i < _list.Count; i++)
            {
                OnElementAdd?.Invoke(new ListElementEventArgs<T>
                {
                    Index = i,
                    Value = _list[i]
                });
            }
            RaiseBulkChanged();
        }

        public void Insert(int index, T item)
        {
            _list.Insert(index, item);
            OnElementAdd?.Invoke(new ListElementEventArgs<T>
            {
                Index = index,
                Value = item
            });
            RaiseBulkChanged();
        }

        public void InsertRange(int index, IEnumerable<T> collection)
        {
            var items = new List<T>(collection);
            _list.InsertRange(index, items);
            for (int i = 0; i < items.Count; i++)
            {
                OnElementAdd?.Invoke(new ListElementEventArgs<T>
                {
                    Index = index + i,
                    Value = items[i]
                });
            }
            RaiseBulkChanged();
        }

        public bool Remove(T item)
        {
            int index = _list.IndexOf(item);
            if (index < 0) return false;

            RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            var item = _list[index];
            _list.RemoveAt(index);
            OnElementRemove?.Invoke(new ListElementEventArgs<T>
            {
                Index = index,
                Value = item
            });
            RaiseBulkChanged();
        }

        public void RemoveRange(int index, int count)
        {
            for (int i = index + count - 1; i >= index; i--)
            {
                var item = _list[i];
                _list.RemoveAt(i);
                OnElementRemove?.Invoke(new ListElementEventArgs<T>
                {
                    Index = i,
                    Value = item
                });
            }
            RaiseBulkChanged();
        }

        public void Clear()
        {
            _list.Clear();
            RaiseBulkChanged();
        }

        public bool Contains(T item) => _list.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);
        public int IndexOf(T item) => _list.IndexOf(item);
        public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();

        #endregion

        #region Extended Operations

        /// <summary>Move an element from one index to another.</summary>
        public void Move(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex) return;

            var item = _list[fromIndex];
            _list.RemoveAt(fromIndex);
            _list.Insert(toIndex, item);
            OnElementMoved?.Invoke(fromIndex, toIndex);
            RaiseBulkChanged();
        }

        /// <summary>Swap two elements at the given indices.</summary>
        public void Swap(int index1, int index2)
        {
            if (index1 == index2) return;

            var temp = _list[index1];
            _list[index1] = _list[index2];
            _list[index2] = temp;
            OnElementSwap?.Invoke(index1, index2);
            RaiseBulkChanged();
        }

        /// <summary>Sort the list using the default comparer.</summary>
        public void Sort()
        {
            _list.Sort();
            RaiseBulkChanged();
        }

        /// <summary>Sort the list using a custom comparison.</summary>
        public void Sort(Comparison<T> comparison)
        {
            _list.Sort(comparison);
            RaiseBulkChanged();
        }

        /// <summary>Reverse the list order.</summary>
        public void Reverse()
        {
            _list.Reverse();
            RaiseBulkChanged();
        }

        /// <summary>Manually notify that the list has been changed externally.</summary>
        public void NotifyChanged()
        {
            RaiseBulkChanged();
        }

        #endregion

        #region Derived Lists

        /// <summary>
        /// Create a derived list where each element is transformed.
        /// The derived list stays in sync with this list.
        /// </summary>
        public BindableList<TResult> DeriveSelect<TResult>(Func<T, TResult> selector)
        {
            var derived = new BindableList<TResult>();
            foreach (var item in _list)
                derived._list.Add(selector(item));

            OnElementAdd += args => derived.Insert(args.Index, selector(args.Value));
            OnElementChange += args => derived[args.Index] = selector(args.Value);
            OnElementRemove += args => derived.RemoveAt(args.Index);
            OnElementMoved += (from, to) => derived.Move(from, to);
            OnElementSwap += (i1, i2) => derived.Swap(i1, i2);
            _onChanged += () =>
            {
                derived._list.Clear();
                foreach (var item in _list)
                    derived._list.Add(selector(item));
                derived.RaiseBulkChanged();
            };

            return derived;
        }

        /// <summary>
        /// Create a derived list by concatenating with another BindableList.
        /// </summary>
        public BindableList<T> DeriveConcat(BindableList<T> other)
        {
            var derived = new BindableList<T>();

            void Rebuild()
            {
                derived._list.Clear();
                derived._list.AddRange(_list);
                derived._list.AddRange(other._list);
                derived.RaiseBulkChanged();
            }

            Rebuild();
            _onChanged += Rebuild;
            other._onChanged += Rebuild;

            return derived;
        }

        /// <summary>
        /// Create a derived list by casting each element to a different type.
        /// </summary>
        public BindableList<TResult> DeriveCast<TResult>() where TResult : class
        {
            return DeriveSelect(item => item as TResult);
        }

        #endregion

        private void RaiseBulkChanged()
        {
            _onChanged?.Invoke();
            _onChangedWithValue?.Invoke(_list);
        }
    }
}
