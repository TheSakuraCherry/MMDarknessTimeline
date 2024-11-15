using System;
using System.Collections.Generic;

namespace MMDarkness
{
    public static class Game
    {
        private static readonly Dictionary<Type, ISingleton> singletonTypes = new Dictionary<Type, ISingleton>();
        private static readonly Queue<ISingleton> singletons = new Queue<ISingleton>();

        public static IReadOnlyDictionary<Type, ISingleton> SingleTypes => singletonTypes;

        private static ISingleton GetSingleton_Internal(Type singletonType)
        {
            if (!singletonTypes.TryGetValue(singletonType, out var singleton))
            {
                foreach (var pair in singletonTypes)
                {
                    if (pair.Value.GetType().IsAssignableFrom(singletonType))
                    {
                        singleton = pair.Value;
                        break;
                    }
                }
            }

            return singleton;
        }

        private static void AddSingleton_Internal(ISingleton singleton, Type singletonType)
        {
            singletonTypes.Add(singletonType, singleton);
            singletons.Enqueue(singleton);

            singleton.Register();

            if (singleton is ISingletonAwake awake)
                awake.Awake();
        }

        public static void FixedUpdate()
        {
            int count = singletons.Count;
            while (count-- > 0)
            {
                var singleton = singletons.Dequeue();

                if (singleton.IsDisposed)
                    continue;

                if (!(singleton is ISingletonFixedUpdate fixedUpdate))
                    continue;

                singletons.Enqueue(singleton);
                fixedUpdate.FixedUpdate();
            }
        }

        public static void Update()
        {
            int count = singletons.Count;
            while (count-- > 0)
            {
                var singleton = singletons.Dequeue();

                if (singleton.IsDisposed)
                    continue;

                if (!(singleton is ISingletonUpdate update))
                    continue;

                singletons.Enqueue(singleton);
                update.Update();
            }
        }

        public static void LateUpdate()
        {
            int count = singletons.Count;
            while (count-- > 0)
            {
                var singleton = singletons.Dequeue();

                if (singleton.IsDisposed)
                    continue;

                if (!(singleton is ISingletonLateUpdate lateUpdate))
                    continue;

                singletons.Enqueue(singleton);
                lateUpdate.LateUpdate();
            }
        }

        public static void Close()
        {
            // 顺序反过来清理
            var singletonStack = new Stack<ISingleton>();
            while (singletons.Count > 0)
            {
                singletonStack.Push(singletons.Dequeue());
            }
            
            while (singletonStack.Count > 0)
            {
                var singleton = singletonStack.Pop();
                if (singleton.IsDisposed)
                    continue;
                singleton.Dispose();
            }

            singletonTypes.Clear();
        }

        public static bool IsInitialized(Type singletonType)
        {
            return singletonTypes.ContainsKey(singletonType);
        }

        public static ISingleton GetSingleton(Type singletonType)
        {
            return GetSingleton_Internal(singletonType);
        }

        public static T GetSingleton<T>()
        {
            return (T)GetSingleton_Internal(typeof(T));
        }

        public static T AddSingleton<T>() where T : Singleton<T>, new()
        {
            T singleton = new T();
            AddSingleton_Internal(singleton, typeof(T));
            return singleton;
        }

        public static void AddSingleton(ISingleton singleton)
        {
            Type singletonType = singleton.GetType();
            AddSingleton_Internal(singleton, singletonType);
        }
    }
}