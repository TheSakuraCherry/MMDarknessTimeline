using System;

namespace MMDarkness
{
    [Serializable]
    public abstract class Singleton<T> : ISingleton where T : Singleton<T>
    {
        public bool IsDisposed { get; private set; }

        public void Register()
        {
            if (Instance != null)
                throw new Exception($"singleton register twice! {typeof(T).Name}");

            Instance = (T)this;
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;
            if (Instance is ISingletonDestory iSingletonDestory)
                iSingletonDestory.Destroy();
            Instance = null;
        }

        #region Static

        public static T Instance { get; private set; }

        public static bool IsInitialized()
        {
            return Instance != null;
        }

        #endregion
    }
}