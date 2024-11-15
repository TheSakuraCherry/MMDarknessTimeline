using System;
using System.Collections.Generic;

namespace MMDarkness
{
    [Serializable]
    public class BindableProperty<T> : IBindableProperty<T>, IBindableProperty
    {
        private event Func<T> getter;
        private event Action<T> setter;
        
        public event ValueChangedEvent<T> onValueChanged;
        public event ValueChangedEvent<object> onBoxedValueChanged;

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

        public object BoxedValue
        {
            get => Value;
            set => Value = (T)value;
        }

        public Type ValueType => typeof(T);

        public BindableProperty(Func<T> getter, Action<T> setter)
        {
            this.getter = getter;
            this.setter = setter;
        }

        private void NotifyValueChanged_Internal(T oldValue, T newValue)
        {
            onValueChanged?.Invoke(oldValue, newValue);
            onBoxedValueChanged?.Invoke(oldValue, newValue);
        }

        public IBindableProperty<TOut> AsBindableProperty<TOut>()
        {
            return this as BindableProperty<TOut>;
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

        public void SetValueWithoutNotify(object value)
        {
            setter?.Invoke((T)value);
        }

        public void ClearValueChangedEvent()
        {
            while (this.onValueChanged != null)
            {
                this.onValueChanged -= this.onValueChanged;
            }
            while (this.onBoxedValueChanged != null)
            {
                this.onBoxedValueChanged -= this.onBoxedValueChanged;
            }
        }

        public void NotifyValueChanged()
        {
            NotifyValueChanged_Internal(Value, Value);
        }

        public virtual void Dispose()
        {
            getter = null;
            setter = null;
            ClearValueChangedEvent();
        }

        public override string ToString()
        {
            return (Value != null ? Value.ToString() : "null");
        }

        protected virtual bool ValidEquals(T oldValue, T newValue)
        {
            return EqualityComparer<T>.Default.Equals(oldValue, newValue);
        }
    }
}