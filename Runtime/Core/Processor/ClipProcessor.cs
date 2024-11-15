using System;
using System.Collections.Generic;
using UnityEngine;

#if USE_FIXED_POINT
using CMath = Box2DSharp.Common.FMath;
using CFloat = Box2DSharp.Common.FP;

#else
using CMath = System.Math;
using CFloat = System.Single;
#endif

namespace MMDarkness
{

    //要继承的类
    public abstract class ClipProcessor<T> : ClipProcessor where T : Clip
    {
        protected T TData => Data as T;
    }
    
    
    [ViewModel(typeof(Clip))]
    public class ClipProcessor : IDirectable
    {

        private Clip data;

        public CFloat StartTime => data.startTime;

        public CFloat EndTime => data.startTime + data.length;
        
        protected Clip Data => data;

        public GameObject Owner => Root.Owner;
        public CFloat Length => data.length;
        
        public ITimelineGraph Root { get; private set; }
        
        public IDirectable Parent { get; private set; }
        public IEnumerable<IDirectable> Children => Array.Empty<IDirectable>();
        
        public bool IsTriggered { get; private set; }
        
        public void SetUp(Clip clip, IDirectable track)
        {
            this.data = clip;
            this.Root = track.Root;
            this.Parent = track;
        }
        
        public void Enter(FrameData frameData,FrameData innerFrameData)
        {
            IsTriggered = true;
            OnEnter(frameData, innerFrameData);
        }

        public void Update(FrameData frameData,FrameData innerFrameData)
        {
            OnUpdate(frameData, innerFrameData);
        }

        public void Exit(FrameData frameData,FrameData innerFrameData)
        {
            IsTriggered = false;
            OnExit(frameData, innerFrameData);
        }

        public void ReverseEnter(FrameData frameData,FrameData innerFrameData)
        {
            IsTriggered = true;
            OnReverseEnter(frameData, innerFrameData);
        }

        public void Reverse(FrameData frameData,FrameData innerFrameData)
        {
            IsTriggered = false;
            OnReverse(frameData, innerFrameData);
        }

        public void Reset()
        {
            OnReset();
        }

        public void Dispose()
        {
            OnDispose();

            data = null;
            Root = null;
            Parent = null;
            ObjectPools.Instance.Recycle(this);
        }

        protected virtual void OnEnter(FrameData frameData, FrameData innerFrameData)
        {
        }

        protected virtual void OnExit(FrameData frameData, FrameData innerFrameData)
        {
        }

        protected virtual void OnUpdate(FrameData frameData, FrameData innerFrameData)
        {
        }

        protected virtual void OnReverseEnter(FrameData frameData, FrameData innerFrameData)
        {
        }
        
        protected virtual void OnReverse(FrameData frameData, FrameData innerFrameData)
        {
        }

        protected virtual void OnReset()
        {
        }

        protected virtual void OnDispose()
        {
        }
        
    }
}
