using System;

namespace MMDarkness
{
    [Serializable]
    public abstract class Singleton<T> : ISingleton where T : Singleton<T>
    {
        #region Static

        private static T instance;

        public static T Instance
        {
            get { return instance; }
        }

        public static bool IsInitialized()
        {
            return instance != null;
        }

        #endregion

        private bool isDisposed;

        public bool IsDisposed => this.isDisposed;

        public void Register()
        {
            if (instance != null)
                throw new Exception($"singleton register twice! {typeof(T).Name}");

            instance = (T)this;
        }

        public void Dispose()
        {
            if (this.isDisposed)
                return;

            this.isDisposed = true;
            if (instance is ISingletonDestory iSingletonDestory)
                iSingletonDestory.Destroy();
            instance = null;
        }
    }
}