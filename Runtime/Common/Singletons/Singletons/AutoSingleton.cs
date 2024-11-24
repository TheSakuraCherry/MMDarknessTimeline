using System;

namespace MMDarkness
{
    public abstract class AutoSingleton<T> : ISingleton where T : AutoSingleton<T>, new()
    {
        public bool IsDisposed { get; private set; }

        public void Register()
        {
            if (instance != null)
                throw new Exception($"singleton register twice! {typeof(T).Name}");

            instance = (T)this;
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            if (this is ISingletonDestory iSingletonDestory)
                iSingletonDestory.Destroy();
            if (this == instance)
                instance = null;
        }

        #region Static

        private static readonly object @lock = new();

        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance == null)
                    lock (@lock)
                    {
                        if (instance == null) Game.AddSingleton(new T());
                    }

                return instance;
            }
        }

        public static bool IsInitialized()
        {
            return instance != null;
        }

        #endregion
    }
}