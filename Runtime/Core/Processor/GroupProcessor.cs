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
    public abstract class GroupProcessor<T> : GroupProcessor where T : Group
    {
        protected T TData => Data as T;
    }

    [ViewModel(typeof(Group))]
    public class GroupProcessor : IDirectable
    {
        private List<TrackProcessor> tracks;

        protected Group Data { get; private set; }

        public bool IsTriggered { get; private set; }

        public CFloat StartTime { get; private set; }

        public CFloat EndTime { get; private set; }

        public CFloat Length => EndTime - StartTime;
        public ITimelineGraph Root { get; private set; }
        public IDirectable Parent => null;

        public GameObject Owner => Root.Owner;

        public IEnumerable<IDirectable> Children => tracks != null ? tracks : Array.Empty<IDirectable>();

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
            foreach (var child in Children) child.Reset();

            OnReset();
        }

        public void Dispose()
        {
            foreach (var child in Children) child.Dispose();

            tracks.Clear();

            OnDispose();

            Data = null;
            StartTime = 0;
            EndTime = 0;

            ObjectPools.Instance.Recycle(this);
        }

        public void SetUp(Group group, ITimelineGraph graph)
        {
            Data = group;
            Root = graph;
            StartTime = 0;
            EndTime = Root.Length;
            if (group.tracks != null)
            {
                tracks = new List<TrackProcessor>(group.tracks.Count);
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