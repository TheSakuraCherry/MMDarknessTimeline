using UnityEngine;

namespace MMDarkness.Editor
{
    internal struct GuideLine
    {
        public readonly float Time;
        public Color Color;

        public GuideLine(float time, Color color)
        {
            this.Time = time;
            this.Color = color;
        }
    }
}
