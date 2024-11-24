#if USE_FIXED_POINT
using CMath = Box2DSharp.Common.FMath;
using CFloat = Box2DSharp.Common.FP;
#else
using CFloat = System.Single;
#endif
using System;
using Sirenix.OdinInspector;

namespace MMDarkness
{
    [Serializable]
    public class Clip
    {
        [ReadOnly] public CFloat startTime;

        [ReadOnly] public CFloat length;
    }
}