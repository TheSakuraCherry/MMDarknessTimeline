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

    public abstract class GroupProcessor<T> : GroupProcessor where T : Group
    {
        protected T TData => Data as T;
    }
    
    [ViewModel(typeof(Group))]
    public class GroupProcessor : IDirectable
    {
        private Group data;
        private List<TrackProcessor> tracks;
        private CFloat startTime;
        private CFloat endTime;
        
        public CFloat StartTime => startTime;
        
        public CFloat EndTime => endTime;
        
        public CFloat Length => endTime - startTime;
        public ITimelineGraph Root { get; private set; }
        public IDirectable Parent => null;

        public GameObject Owner => Root.Owner;
        
        protected Group Data => data;
        
        public IEnumerable<IDirectable> Children => tracks != null ? tracks : Array.Empty<IDirectable>();
        
        public bool IsTriggered { get; private set; }
        
        public void SetUp(Group group, ITimelineGraph graph)
        {
            this.data = group;
            this.Root = graph;
            this.startTime = 0;
            this.endTime = Root.Length;
            if (group.tracks != null)
            {
                this.tracks = new List<TrackProcessor>(group.tracks.Count);
                foreach (var track in group.tracks)
                {
                    var trackProcessorType = ViewModelFactory.GetViewModelType(track.GetType());
                    if (ObjectPools.Instance.Spawn(trackProcessorType) is TrackProcessor trackProcessor)
                    {
                        trackProcessor.SetUp(track, this);
                        tracks.Add(trackProcessor);
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
            foreach (var child in Children)
            {
                child.Reset();
            }

            OnReset();
        }
        public void Dispose()
        {
            foreach (var child in Children)
            {
                child.Dispose();
            }

            tracks.Clear();

            OnDispose();

            data = null;
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
