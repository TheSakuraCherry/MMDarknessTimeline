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
    
    public interface ITimelineGraph : IDisposable
    {
        IEnumerable<IDirectable> Children { get; }
        BlackboardProcessor<string> Context { get; }
        Events<string> Events { get; }

        bool Active { get; }

        CFloat Length { get; }
        CFloat CurrentTime { get; set; }
        CFloat PreviousTime { get; }

        GameObject Owner { get; set; }

        void Reset();
    }
}