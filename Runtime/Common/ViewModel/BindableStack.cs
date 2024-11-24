using System;
using System.Collections;
using System.Collections.Generic;

namespace MMDarkness
{
    public class BindableStack<T> : BindableProperty<Stack<T>>, IEnumerable<T>
    {
        public BindableStack(Func<Stack<T>> getter, Action<Stack<T>> setter) : base(getter, setter)
        {
        }

        public int Count => Value.Count;

        public bool IsReadOnly => false;

        public IEnumerator<T> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        public event Action onPushed;
        public event Action onPoped;
        public event Action onClear;

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