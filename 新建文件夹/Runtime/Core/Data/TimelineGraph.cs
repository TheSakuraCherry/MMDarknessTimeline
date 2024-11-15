using System;
using System.Collections.Generic;


namespace MMDarkness
{
    [Serializable]
    public class TimelineGraph
    {
        public float length;
        public WarpCategory warpCategory;
        public List<Group> groups;
    }
}
