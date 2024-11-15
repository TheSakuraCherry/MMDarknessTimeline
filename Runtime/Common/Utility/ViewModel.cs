using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace MMDarkness
{
    public delegate ref V RefFunc<V>();

    public interface IViewModel
    {
        int Count { get; }

        IEnumerator<KeyValuePair<string, IBindableProperty>> GetEnumerator();

        bool Contains(string propertyName);

        IBindableProperty GetProperty(string propertyName);

        IBindableProperty<T> GetProperty<T>(string propertyName);

        bool TryGetProperty(string propertyName, out IBindableProperty property);

        bool TryGetProperty<T>(string propertyName, out IBindableProperty<T> property);

        void RegisterProperty(string propertyName, IBindableProperty property);

        void UnregisterProperty(string propertyName);

        T GetPropertyValue<T>(string propertyName);

        void SetPropertyValue<T>(string propertyName, T value);
    }

    public class ViewModel : INotifyPropertyChanged
    {
        private readonly Dictionary<string, IBindableProperty> bindableProperties = new();

        public int Count => bindableProperties.Count;

        public IReadOnlyDictionary<string, IBindableProperty> Properties => bindableProperties;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            GetProperty(propertyName)?.NotifyValueChanged();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Contains(string propertyName)
        {
            return bindableProperties.ContainsKey(propertyName);
        }

        public bool TryGetProperty(string propertyName, out IBindableProperty property)
        {
            if (bindableProperties == null)
            {
                property = null;
                return false;
            }

            return bindableProperties.TryGetValue(propertyName, out property);
        }

        public bool TryGetProperty<T>(string propertyName, out IBindableProperty<T> property)
        {
            if (!bindableProperties.TryGetValue(propertyName, out var tempProperty))
            {
                property = null;
                return false;
            }

            property = tempProperty as IBindableProperty<T>;
            return property != null;
        }

        public IBindableProperty GetProperty(string propertyName)
        {
            if (!bindableProperties.TryGetValue(propertyName, out var property)) return null;

            return property;
        }

        public IBindableProperty<T> GetProperty<T>(string propertyName)
        {
            if (!bindableProperties.TryGetValue(propertyName, out var property)) return null;

            return property as IBindableProperty<T>;
        }

        public void RegisterProperty(string propertyName, IBindableProperty property)
        {
            bindableProperties.Add(propertyName, property);
        }

        public void RegisterProperty<T>(string propertyName, Func<T> getter = null, Action<T> setter = null)
        {
            bindableProperties.Add(propertyName, new BindableProperty<T>(getter, setter));
        }

        public void RegisterProperty<T>(string propertyName, RefFunc<T> getter)
        {
            bindableProperties.Add(propertyName, new BindableProperty<T>(() => getter(), v => getter() = v));
        }

        public void UnregisterProperty(string propertyName)
        {
            if (!bindableProperties.TryGetValue(propertyName, out var property)) return;

            property.Dispose();
            bindableProperties.Remove(propertyName);
        }

        public T GetPropertyValue<T>(string propertyName)
        {
            return GetProperty<T>(propertyName).Value;
        }

        public void SetPropertyValue<T>(string propertyName, T value)
        {
            GetProperty<T>(propertyName).Value = value;
        }

        public void NotifyPropertyChanged(string propertyName)
        {
            GetProperty(propertyName)?.NotifyValueChanged();
            OnPropertyChanged(propertyName);
        }
    }
}