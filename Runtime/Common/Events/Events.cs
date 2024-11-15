using System;
using System.Collections.Generic;

namespace MMDarkness
{
    public interface IEvent
    {
        bool IsNull { get; }
    }

    public class Events<TKey>
    {
        private readonly Dictionary<TKey, IEvent> events = new();

        public void Subscribe<T>(TKey key, Action<T> handler)
        {
            if (!events.TryGetValue(key, out var evts)) events[key] = evts = new Event<T>();

            ((Event<T>)evts).handler += handler;
        }

        public void Unsubscribe<T>(TKey key, Action<T> handler)
        {
            if (!events.TryGetValue(key, out var evts)) return;

            ((Event<T>)evts).handler -= handler;
        }

        public void Publish<T>(TKey key, T arg)
        {
            if (!events.TryGetValue(key, out var evts)) return;

            ((Event<T>)evts).Handle(arg);
        }

        public void Subscribe(TKey key, Action handler)
        {
            if (!events.TryGetValue(key, out var evts)) events[key] = evts = new Event();

            ((Event)evts).handler += handler;
        }

        public void Unsubscribe(TKey key, Action handler)
        {
            if (!events.TryGetValue(key, out var evts)) return;

            ((Event)evts).handler -= handler;
        }

        public void Publish(TKey key)
        {
            if (!events.TryGetValue(key, out var evts)) return;

            ((Event)evts).Handle();
        }

        public bool ExistsEvent(TKey key)
        {
            return events.TryGetValue(key, out var evts) && !evts.IsNull;
        }

        public void Clear()
        {
            events.Clear();
        }

        private class Event : IEvent
        {
            public bool IsNull => handler == null;
            public event Action handler;

            public void Handle()
            {
                handler?.Invoke();
            }
        }

        private class Event<T> : IEvent
        {
            public bool IsNull => handler == null;
            public event Action<T> handler;

            public void Handle(T arg)
            {
                handler?.Invoke(arg);
            }
        }
    }
}