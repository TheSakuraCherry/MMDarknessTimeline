using System;

namespace MMDarkness
{
    public interface IObjectPool : IDisposable
    {
        int UnusedCount { get; }

        object Spawn();

        void Recycle(object unit);
    }

    public interface IObjectPool<T> : IDisposable where T : class
    {
        int UnusedCount { get; }

        T Spawn();

        void Recycle(T unit);
    }
}