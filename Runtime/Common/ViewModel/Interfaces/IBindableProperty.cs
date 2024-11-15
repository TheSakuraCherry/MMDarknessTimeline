using System;

namespace MMDarkness
{
    public interface IBindableProperty : IDisposable
    {
        object BoxedValue { get; set; }
        Type ValueType { get; }
        event ValueChangedEvent<object> onBoxedValueChanged;

        void SetValueWithoutNotify(object value);
        void NotifyValueChanged();
        void ClearValueChangedEvent();
    }

    public interface IBindableProperty<T> : IDisposable
    {
        T Value { get; set; }
        event ValueChangedEvent<T> onValueChanged;

        void SetValueWithoutNotify(T value);
        void RegisterValueChangedEvent(ValueChangedEvent<T> onValueChanged);
        void UnregisterValueChangedEvent(ValueChangedEvent<T> onValueChanged);
    }
}