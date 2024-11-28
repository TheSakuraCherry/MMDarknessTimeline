using Sirenix.OdinInspector;
using UnityEngine;

namespace MMDarkness
{
    [Name("播放动画")]
    [Attachable(typeof(BTrack))]
    public class VFXClip : Clip
    {
        [ObjectPathSelector(typeof(ParticleSystem))]
        public string VFXPath;
        [LabelText("位置偏移")]
        public Vector3 PositionOffset;
        [LabelText("旋转偏移")]
        public Vector3 RotationOffset;

    }
}