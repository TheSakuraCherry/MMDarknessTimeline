using System;
using Sirenix.OdinInspector;

#if USE_FIXED_POINT
using CMath = Box2DSharp.Common.FMath;
using CFloat = Box2DSharp.Common.FP;

#else
using CMath = System.Math;
using CFloat = System.Single;
#endif

namespace MMDarkness
{
    [Serializable]
    public class Clip
    {
        [ReadOnly]
        public CFloat startTime;
        [ReadOnly]
        public CFloat length;
    }
}
