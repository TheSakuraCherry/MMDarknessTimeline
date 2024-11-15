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

    public abstract class TrackProcessor<T> : TrackProcessor where T : Track
    {
        protected T TData => Data as T;
    }
    
    [ViewModel(typeof(Track))]
    public class TrackProcessor : IDirectable
    {
        private Track data;
        private List<ClipProcessor> clips;
        private CFloat startTime;
        private CFloat endTime;

        protected Track Data => data;
        
        public CFloat StartTime => startTime;
        
        public CFloat EndTime => endTime;

        public GameObject Owner => Root.Owner;
        public CFloat Length => endTime - startTime;
        public ITimelineGraph Root { get; private set; }
        
        public IDirectable Parent { get; private set; }
        
        public IEnumerable<IDirectable> Children => clips != null ? clips : Array.Empty<IDirectable>();
        
        public bool IsTriggered { get; private set; }
        
        public void SetUp(Track track, IDirectable group)
        {
            this.data = track;
            this.Parent = group;
            this.Root = group.Root;
            this.startTime = 0;
            this.endTime = Root.Length;
            if (track.clips != null)
            {
                this.clips = new List<ClipProcessor>(track.clips.Count);
                foreach (var clip in track.clips)
                {
                    var clipProcessorType = ViewModelFactory.GetViewModelType(clip.GetType());
                    if (ObjectPools.Instance.Spawn(clipProcessorType) is ClipProcessor clipProcessor)
                    {
                        clipProcessor.SetUp(clip, this);
                        clips.Add(clipProcessor);
                    }
                }
            }
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

        public void Reset()
        {
            foreach (var child in Children)
            {
                child.Reset();
            }

            OnReset();
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
        public void Dispose()
        {
            foreach (var child in Children)
            {
                child.Dispose();
            }

            clips.Clear();

            OnDispose();

            data = null;
            Root = null;
            Parent = null;
            startTime = 0;
            endTime = 0;
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
