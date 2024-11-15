using System;

namespace MMDarkness
{
    public interface ISingleton : IDisposable
    {
        bool IsDisposed { get; }
        
        void Register();
    }

    public interface ISingletonAwake
    {
        void Awake();
    }

    public interface ISingletonFixedUpdate
    {
        void FixedUpdate();
    }

    public interface ISingletonUpdate
    {
        void Update();
    }

    public interface ISingletonLateUpdate
    {
        void LateUpdate();
    }

    public interface ISingletonDestory
    {
        void Destroy();
    }
}