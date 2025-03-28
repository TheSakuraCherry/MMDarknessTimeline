using System;
using System.Collections.Generic;

namespace MMDarkness
{
    public class BindableDictionary<TKey, TValue> : BindableProperty<Dictionary<TKey, TValue>>
    {
        public BindableDictionary(Func<Dictionary<TKey, TValue>> getter, Action<Dictionary<TKey, TValue>> setter) :
            base(getter, setter)
        {
        }

        public ICollection<TKey> Keys => Value.Keys;

        public ICollection<TValue> Values => Value.Values;

        public int Count => Value.Count;

        public TValue this[TKey key]
        {
            get => Value[key];
            set
            {
                if (Value.ContainsKey(key))
                {
                    Value[key] = value;
                    OnItemValueChanged?.Invoke(key, value);
                }
                else
                {
                    Value[key] = value;
                    OnAdded?.Invoke(key);
                }
            }
        }

        public bool IsReadOnly => false;
        public event Action<TKey> OnAdded;
        public event Action<TKey, TValue> OnItemValueChanged;
        public event Action<KeyValuePair<TKey, TValue>> OnRemoved;
        public event Action OnClear;

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        public void Add(TKey key, TValue value)
        {
            Value.Add(key, value);
            OnAdded?.Invoke(key);
        }

        public bool ContainsKey(TKey key)
        {
            return Value.ContainsKey(key);
        }

        public bool Remove(TKey key)
        {
            var value = Value[key];
            var result = Value.Remove(key);
            if (result)
                OnRemoved?.Invoke(new KeyValuePair<TKey, TValue>(key, value));
            return result;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return Value.TryGetValue(key, out value);
        }

        public void Clear()
        {
            Value.Clear();
            OnClear?.Invoke();
        }
    }
}