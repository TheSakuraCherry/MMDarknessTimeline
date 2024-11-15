using System;
using System.Collections;
using System.Collections.Generic;

namespace MMDarkness
{
    public class BindableStack<T> : BindableProperty<Stack<T>>, IEnumerable<T>
    {
        public event Action onPushed;
        public event Action onPoped;
        public event Action onClear;

        public int Count
        {
            get { return Value.Count; }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }

        public BindableStack(Func<Stack<T>> getter, Action<Stack<T>> setter) : base(getter, setter) { }

        public void Push(T item)
        {
            Value.Push(item);
            onPushed?.Invoke();
        }

        public T Pop()
        {
            var t = Value.Pop();
            onPoped?.Invoke();
            return t;
        }

        public T Peek()
        {
            return Value.Peek();
        }

        public void TrimExcess()
        {
            Value.TrimExcess();
        }

        public void Clear()
        {
            Value.Clear();
            onClear?.Invoke();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Value.GetEnumerator();
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
