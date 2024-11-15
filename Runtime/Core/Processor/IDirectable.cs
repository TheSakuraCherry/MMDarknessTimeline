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
    public interface IDirectable : IDisposable
    {
        ITimelineGraph Root { get; }

        IDirectable Parent { get; }

        IEnumerable<IDirectable> Children { get; }

        GameObject Owner { get; }

        public CFloat Length { get; }
        public CFloat StartTime { get; }
        public CFloat EndTime { get; }

        void Enter(FrameData frameData, FrameData innerFrameData);

        void Update(FrameData frameData, FrameData innerFrameData);

        void Exit(FrameData frameData, FrameData innerFrameData);

        void ReverseEnter(FrameData frameData, FrameData innerFrameData);

        void Reverse(FrameData frameData, FrameData innerFrameData);

        void Reset();
    }

    public struct FrameData
    {
        public CFloat previousTime;
        public CFloat currentTime;
        public CFloat deltaTime;
    }
}