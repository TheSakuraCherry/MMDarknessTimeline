using System;
using System.Collections.Generic;

namespace MMDarkness
{
    public class Blackboard<TKey> : IBlackboard<TKey>
    {
        private readonly Dictionary<TKey, IDataContainer> containerMap = new();

        private readonly DataContainer<object> objectContainer = new();
        private readonly Dictionary<Type, IDataContainer> structContainers = new();

        public object Get(TKey key)
        {
            if (!containerMap.TryGetValue(key, out var dataContainer)) return default;

            return dataContainer.Get(key);
        }

        public bool TryGet(TKey key, out object value)
        {
            if (!containerMap.TryGetValue(key, out var dataContainer))
            {
                value = default;
                return false;
            }

            return dataContainer.TryGet(key, out value);
        }

        public bool Contains(TKey key)
        {
            return containerMap.ContainsKey(key);
        }

        public T Get<T>(TKey key)
        {
            if (!containerMap.TryGetValue(key, out var dataContainer)) return default;

            var type = typeof(T);
            var isValueType = type.IsValueType;
            if (isValueType) return ((DataContainer<T>)dataContainer).Get(key);

            return (T)((DataContainer<object>)dataContainer).Get(key);
        }

        public bool TryGet<T>(TKey key, out T value)
        {
            if (!containerMap.TryGetValue(key, out var dataContainer))
            {
                value = default;
                return false;
            }

            var type = typeof(T);
            var isValueType = type.IsValueType;
            if (isValueType) return ((DataContainer<T>)dataContainer).TryGet(key, out value);

            var result = ((DataContainer<object>)dataContainer).TryGet(key, out var v);
            value = (T)v;
            return result;
        }

        public void Set<T>(TKey key, T value)
        {
            var type = typeof(T);
            var isValueType = type.IsValueType;
            if (!containerMap.TryGetValue(key, out var dataContainer))
            {
                if (isValueType)
                {
                    if (!structContainers.TryGetValue(type, out dataContainer))
                        structContainers[type] = dataContainer = new DataContainer<T>();

                    containerMap[key] = dataContainer;
                }
                else
                {
                    containerMap[key] = dataContainer = objectContainer;
                }
            }

            if (isValueType)
                ((DataContainer<T>)dataContainer).Set(key, value);
            else
                ((DataContainer<object>)dataContainer).Set(key, value);
        }

        public void Remove(TKey key)
        {
            if (!containerMap.TryGetValue(key, out var dataContainer))
                return;

            containerMap.Remove(key);
            dataContainer.Remove(key);
        }

        public void Clear()
        {
            objectContainer.Clear();
            structContainers.Clear();
            containerMap.Clear();
        }

        private interface IDataContainer
        {
            object Get(TKey key);

            bool TryGet(TKey key, out object value);

            void Remove(TKey key);

            void Clear();
        }

        private interface IDataContainer<T>
        {
            T Get(TKey key);

            bool TryGet(TKey key, out T value);

            void Set(TKey key, T value);

            void Remove(TKey key);

            void Clear();
        }

        private class DataContainer<T> : IDataContainer, IDataContainer<T>
        {
            private readonly Dictionary<TKey, T> data = new();

            public T this[TKey key]
            {
                get => Get(key);
                set => Set(key, value);
            }

            object IDataContainer.Get(TKey key)
            {
                if (data.TryGetValue(key, out var value))
                    return value;
                return null;
            }

            bool IDataContainer.TryGet(TKey key, out object value)
            {
                if (data.TryGetValue(key, out var v))
                {
                    value = v;
                    return true;
                }

                value = default;
                return false;
            }

            public void Remove(TKey key)
            {
                data.Remove(key);
            }

            public void Clear()
            {
                data.Clear();
            }

            public T Get(TKey key)
            {
                if (data.TryGetValue(key, out var value)) return value;

                return default;
            }

            public bool TryGet(TKey key, out T value)
            {
                return data.TryGetValue(key, out value);
            }

            public void Set(TKey key, T value)
            {
                data[key] = value;
            }
        }
    }
}