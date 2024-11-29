#if USE_FIXED_POINT
using CMath = Box2DSharp.Common.FMath;
using CFloat = Box2DSharp.Common.FP;
#else
using CFloat = System.Single;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

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
        protected Clip Data { get; private set; }

        public bool IsTriggered { get; private set; }

        public CFloat StartTime => Data.startTime;

        public CFloat EndTime => Data.startTime + Data.length;

        public GameObject Owner => Root.Owner;
        public CFloat Length => Data.length;

        public ITimelineGraph Root { get; private set; }

        public IDirectable Parent { get; private set; }
        public IEnumerable<IDirectable> Children => Array.Empty<IDirectable>();

        public void Init()
        {
            OnInit();
        }

        public void Enter(FrameData frameData, FrameData innerFrameData)
        {
            IsTriggered = true;
            OnEnter(frameData, innerFrameData);
        }

        public void Update(FrameData frameData, FrameData innerFrameData)
        {
            OnUpdate(frameData, innerFrameData);
        }

        public void Exit(FrameData frameData, FrameData innerFrameData)
        {
            IsTriggered = false;
            OnExit(frameData, innerFrameData);
        }

        public void ReverseEnter(FrameData frameData, FrameData innerFrameData)
        {
            IsTriggered = true;
            OnReverseEnter(frameData, innerFrameData);
        }

        public void Reverse(FrameData frameData, FrameData innerFrameData)
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

            Data = null;
            Root = null;
            Parent = null;
            ObjectPools.Instance.Recycle(this);
        }

        public void SetUp(Clip clip, IDirectable track)
        {
            Data = clip;
            Root = track.Root;
            Parent = track;
        }

        protected virtual void OnInit()
        {
            
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
            OnEnter(frameData,innerFrameData);
        }

        protected virtual void OnReverse(FrameData frameData, FrameData innerFrameData)
        {
            OnExit(frameData,innerFrameData);
        }

        protected virtual void OnReset()
        {
        }

        protected virtual void OnDispose()
        {
        }
    }
}