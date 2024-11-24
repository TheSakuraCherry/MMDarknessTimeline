using System.Collections.Generic;
using Animancer;
using UnityEngine;

namespace MMDarkness
{
    [Name("动画轨道")]
    [Description("播放动画片段")]
    [Color(0.0f, 1f, 1f)]
    [Attachable(typeof(ATrack))]
    public class AnimancerClip : Clip
    {
        public AnimationClip Clip;
    }
}