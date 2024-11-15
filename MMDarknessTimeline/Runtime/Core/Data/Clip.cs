using System;
using Sirenix.OdinInspector;

namespace MMDarkness
{
    [Serializable]
    public class Clip
    {
        [ReadOnly]
        public float startTime;
        [ReadOnly]
        public float length;
    }
}
