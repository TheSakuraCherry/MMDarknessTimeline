using UnityEngine;

namespace MMDarkness
{
    [Name("播放动画")]
    [Attachable(typeof(AnimatorTrack))]
    public class PlayAnimatorClip : Clip
    {
        [ObjectPathSelector(typeof(AnimationClip))]
        public string ClipPath;
    }
}