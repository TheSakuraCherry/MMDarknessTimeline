using System;
using System.Collections.Generic;

namespace MMDarkness
{
    public interface IGlobalEvent
    {
        public Type EventType { get; }
    }

    public abstract class GlobalEvent<A> : IGlobalEvent where A : struct
    {
        public Type EventType => typeof(A);

        public abstract void Handle(A arg);
    }

    public interface IEvtTask<K>
    {
        public K EvtName { get; }
    }

    public class EvtTask<K> : IEvtTask<K>
    {
        public K evtName;

        public K EvtName => evtName;
    }

    public class EvtTask<K, A> : IEvtTask<K>
    {
        public A arg;
        public K evtName;
        public Type ArgType => typeof(A);
        public A Arg => arg;

        public K EvtName => evtName;
    }

    public partial class GlobalEvents
    {
        private static bool s_Initialized;
        private static Dictionary<Type, List<IGlobalEvent>> s_AllGlobalEvents;

        static GlobalEvents()
        {
            Init();
        }

        public static void Init(bool force = false)
        {
            if (!force && s_Initialized) return;

            if (s_AllGlobalEvents == null)
                s_AllGlobalEvents = new Dictionary<Type, List<IGlobalEvent>>(128);
            else
                s_AllGlobalEvents.Clear();

            foreach (var type in Util_TypeCache.GetTypesDerivedFrom<IGlobalEvent>())
            {
                if (type.IsAbstract) continue;

                if (!type.IsClass) continue;

                var evt = Activator.CreateInstance(type) as IGlobalEvent;
                if (evt == null) continue;

                var evtType = evt.EventType;
                if (!s_AllGlobalEvents.TryGetValue(evtType, out var evts))
                    s_AllGlobalEvents[evtType] = evts = new List<IGlobalEvent>();

                evts.Add(evt);
            }

            s_Initialized = true;
        }

        public static void Publish<A>(A arg) where A : struct
        {
            var evtType = typeof(A);
            if (!s_AllGlobalEvents.TryGetValue(evtType, out var evts)) return;

            foreach (var evt in evts) (evt as GlobalEvent<A>)?.Handle(arg);
        }
    }

    public partial class GlobalEvents : ISingleton
    {
        private Events<string> events;

        public static GlobalEvents Instance { get; private set; }

        public bool IsDisposed { get; private set; }

        public void Register()
        {
            if (Instance != null)
                throw new Exception($"singleton register twice! {typeof(Events<string>).Name}");

            Instance = this;
            events = new Events<string>();
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            Instance = null;
        }

        public void Subscribe<T>(string key, Action<T> handler)
        {
            events.Subscribe(key, handler);
        }

        public void Unsubscribe<T>(string key, Action<T> handler)
        {
            events.Unsubscribe(key, handler);
        }

        public void Publish<T>(string key, T arg)
        {
            events.Publish(key, arg);
        }

        public void Subscribe(string key, Action handler)
        {
            events.Subscribe(key, handler);
        }

        public void Unsubscribe(string key, Action handler)
        {
            events.Unsubscribe(key, handler);
        }

        public void Publish(string key)
        {
            events.Publish(key);
        }

        public bool ExistsEvent(string key)
        {
            return events.ExistsEvent(key);
        }

        public void Clear()
        {
            events.Clear();
        }
    }
}