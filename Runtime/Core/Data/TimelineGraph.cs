using System;
using System.Collections.Generic;
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
    public class TimelineGraph
    {
        public CFloat length;
        public WarpCategory warpCategory;
        public List<Group> groups;
    }
}
