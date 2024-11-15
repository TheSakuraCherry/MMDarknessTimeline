using System;
using System.Collections.Generic;

namespace MMDarkness
{
    public enum NotifyType
    {
        Added,
        Changed,
        Remove
    }

    public struct BBEventArg
    {
        public object value;
        public NotifyType notifyType;
        
        public BBEventArg(object value, NotifyType notifyType)
        {
            this.value = value;
            this.notifyType = notifyType;
        }
    }

    public class BlackboardProcessor<TKey> : IBlackboard<TKey>
    {
        public Blackboard<TKey> blackboard;
        public Events<TKey> events;
        private List<KeyValuePair<TKey, Action<BBEventArg>>> addObservers;
        private List<KeyValuePair<TKey, Action<BBEventArg>>> removeObservers;
        private bool isNotifying;

        public BlackboardProcessor(Blackboard<TKey> blackboard) : this(blackboard, new Events<TKey>())
        {
        }

        public BlackboardProcessor(Blackboard<TKey> blackboard, Events<TKey> events)
        {
            this.blackboard = blackboard;
            this.events = events;
            this.events = new Events<TKey>();
            this.addObservers = new List<KeyValuePair<TKey, Action<BBEventArg>>>();
            this.removeObservers = new List<KeyValuePair<TKey, Action<BBEventArg>>>();
        }

        public bool Contains(TKey key)
        {
            return blackboard.Contains(key);
        }

        public T Get<T>(TKey key)
        {
            return blackboard.Get<T>(key);
        }

        public object Get(TKey key)
        {
            return blackboard.Get(key);
        }

        public bool TryGet<T>(TKey key, out T value)
        {
            return blackboard.TryGet(key, out value);
        }

        public bool TryGet(TKey key, out object value)
        {
            return blackboard.TryGet(key, out value);
        }

        public void Set<T>(TKey key, T value)
        {
            var notifyType = NotifyType.Changed;
            if (!blackboard.Contains(key))
                notifyType = NotifyType.Added;
            blackboard.Set(key, value);
            NotifyObservers(key, value, notifyType);
        }

        public void Remove(TKey key)
        {
            if (blackboard.TryGet(key, out var value))
            {
                NotifyObservers(key, value, NotifyType.Remove);
            }

            blackboard.Remove(key);
        }

        public void Clear()
        {
            blackboard.Clear();
            events.Clear();
            addObservers.Clear();
            removeObservers.Clear();
        }

        private void NotifyObservers(TKey key, object value, NotifyType notifyType)
        {
            if (!events.ExistsEvent(key))
                return;

            addObservers.Clear();
            removeObservers.Clear();

            isNotifying = true;
            try
            {
                events.Publish(key, new BBEventArg(value, notifyType));
            }
            finally
            {
                isNotifying = false;
            }

            foreach (var pair in removeObservers)
            {
                UnregisterObserver(pair.Key, pair.Value);
            }

            foreach (var pair in addObservers)
            {
                RegisterObserver(pair.Key, pair.Value);
            }

            addObservers.Clear();
            removeObservers.Clear();
        }

        public void RegisterObserver(TKey key, Action<BBEventArg> observer)
        {
            if (isNotifying)
            {
                addObservers.Add(new KeyValuePair<TKey, Action<BBEventArg>>(key, observer));
                return;
            }

            events.Subscribe(key, observer);
        }

        public void UnregisterObserver(TKey key, Action<BBEventArg> observer)
        {
            if (isNotifying)
            {
                removeObservers.Add(new KeyValuePair<TKey, Action<BBEventArg>>(key, observer));
                return;
            }

            events.Unsubscribe(key, observer);
        }
    }
}