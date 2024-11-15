using System;
using System.Collections.Generic;
using UnityEngine;

namespace MMDarkness
{
    
    [Name("动画轨道")]
    [Description("播放动画片段")]
    [Color(r: 0.0f, 1f, 1f)]
    [Attachable(typeof(ATrack))]
    public class AnimancerClip : Clip
    {
        public List<AnimationClip> clips;
    }

    
    [Name("子弹")]
    [Description("生成子弹")]
    [Color(r: 0.0f, 1f, 1f)]
    [Attachable(typeof(BTrack))]
    public class BulletClip : Clip
    {
    }

    
    [Name("伤害")]
    [Description("造成伤害")]
    [Color(r: 0.0f, 1f, 1f)]
    [Attachable(typeof(CTrack))]
    public class DamageClip : Clip
    {
    }
}