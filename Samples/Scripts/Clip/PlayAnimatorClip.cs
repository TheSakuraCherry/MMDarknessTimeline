using UnityEngine;

namespace MMDarkness
{
    [Name("播放动画")]
    [Attachable(typeof(ATrack))]
    public class PlayAnimatorClip : Clip
    {
        [ObjectPathSelector(typeof(AnimationClip))]
        public string ClipPath;
    }
}