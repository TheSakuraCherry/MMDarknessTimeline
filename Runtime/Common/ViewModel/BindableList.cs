using System;
using System.Collections;
using System.Collections.Generic;

namespace MMDarkness
{
    public class BindableList<T> : BindableProperty<List<T>>, IList<T>
    {
        public event Action onAdded;
        public event Action<int> onInserted;
        public event Action<T> onRemoved;
        public event Action<int> onItemChanged;
        public event Action onClear;

        public T this[int index]
        {
            get { return Value[index]; }
            set { SetItem(index, value); }
        }
        public int Count
        {
            get { return Value.Count; }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }

        public BindableList(Func<List<T>> getter, Action<List<T>> setter) : base(getter, setter) { }

        public void Add(T item)
        {
            Value.Add(item);
            onAdded?.Invoke();
        }

        public void Insert(int index, T item)
        {
            Value.Insert(index, item);
            onInserted?.Invoke(index);
        }

        public bool Remove(T item)
        {
            if (Value.Remove(item))
            {
                onRemoved?.Invoke(item);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            Value.Clear();
            onClear?.Invoke();
        }

        protected void SetItem(int index, T item)
        {
            Value[index] = item;
            onItemChanged?.Invoke(index);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return Value.IndexOf(item);
        }

        public void RemoveAt(int index)
        {
            T v = Value[index];
            Remove(Value[index]);
            onRemoved?.Invoke(v);
        }

        public bool Contains(T item)
        {
            return Value.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            Value.CopyTo(array, arrayIndex);
        }
    }
}
