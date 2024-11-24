using System;
using System.Collections.Generic;

namespace MMDarkness
{
    [Serializable]
    public class BindableProperty<T> : IBindableProperty<T>, IBindableProperty
    {
        public BindableProperty(Func<T> getter, Action<T> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public event ValueChangedEvent<object> onBoxedValueChanged;

        public object BoxedValue
        {
            get => Value;
            set => Value = (T)value;
        }

        public Type ValueType => typeof(T);

        public void SetValueWithoutNotify(object value)
        {
            setter?.Invoke((T)value);
        }

        public void ClearValueChangedEvent()
        {
            while (onValueChanged != null) onValueChanged -= onValueChanged;
            while (onBoxedValueChanged != null) onBoxedValueChanged -= onBoxedValueChanged;
        }

        public void NotifyValueChanged()
        {
            NotifyValueChanged_Internal(Value, Value);
        }

        public event ValueChangedEvent<T> onValueChanged;

        public T Value
        {
            get
            {
                if (getter == null)
                    throw new NotImplementedException("haven't get method");
                return getter();
            }
            set
            {
                if (setter == null)
                    throw new NotImplementedException("haven't set method");
                if (ValidEquals(Value, value))
                    return;
                var oldValue = Value;
                setter(value);
                NotifyValueChanged_Internal(oldValue, value);
            }
        }

        public void RegisterValueChangedEvent(ValueChangedEvent<T> onValueChanged)
        {
            this.onValueChanged += onValueChanged;
        }

        public void UnregisterValueChangedEvent(ValueChangedEvent<T> onValueChanged)
        {
            this.onValueChanged -= onValueChanged;
        }

        public void SetValueWithoutNotify(T value)
        {
            setter?.Invoke(value);
        }

        public virtual void Dispose()
        {
            getter = null;
            setter = null;
            ClearValueChangedEvent();
        }

        private event Func<T> getter;
        private event Action<T> setter;

        private void NotifyValueChanged_Internal(T oldValue, T newValue)
        {
            onValueChanged?.Invoke(oldValue, newValue);
            onBoxedValueChanged?.Invoke(oldValue, newValue);
        }

        public IBindableProperty<TOut> AsBindableProperty<TOut>()
        {
            return this as BindableProperty<TOut>;
        }

        public override string ToString()
        {
            return Value != null ? Value.ToString() : "null";
        }

        protected virtual bool ValidEquals(T oldValue, T newValue)
        {
            return EqualityComparer<T>.Default.Equals(oldValue, newValue);
        }
    }
}