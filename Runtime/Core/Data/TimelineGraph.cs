#if USE_FIXED_POINT
using CMath = Box2DSharp.Common.FMath;
using CFloat = Box2DSharp.Common.FP;
#else
using CFloat = System.Single;
#endif
using System;
using System.Collections.Generic;


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