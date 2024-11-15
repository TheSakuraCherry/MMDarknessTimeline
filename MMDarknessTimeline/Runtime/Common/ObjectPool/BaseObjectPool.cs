using System.Collections.Generic;

namespace MMDarkness
{
    public abstract class BaseObjectPool<T> : IObjectPool, IObjectPool<T> where T : class
    {
        protected Stack<T> unusedObjects;

        public int UnusedCount
        {
            get { return unusedObjects.Count; }
        }

        public BaseObjectPool()
        {
            this.unusedObjects = new Stack<T>();
        }

        object IObjectPool.Spawn()
        {
            return Spawn();
        }

        /// <summary> 生成 </summary>
        public T Spawn()
        {
            T unit = null;
            if (unusedObjects.Count > 0)
                unit = unusedObjects.Pop();
            else
                unit = Create();
            OnSpawn(unit);
            return unit;
        }

        void IObjectPool.Recycle(object unit)
        {
            Recycle(unit as T);
        }

        /// <summary> 回收 </summary>
        public void Recycle(T unit)
        {
            unusedObjects.Push(unit);
            OnRecycle(unit);
        }

        public void Dispose()
        {
            while (unusedObjects.Count > 0)
            {
                OnDestroy(unusedObjects.Pop());
            }
        }

        protected abstract T Create();

        protected virtual void OnDestroy(T unit)
        {
        }

        protected virtual void OnSpawn(T unit)
        {
        }

        protected virtual void OnRecycle(T unit)
        {
        }
    }
}