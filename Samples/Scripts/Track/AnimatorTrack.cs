using UnityEngine;

namespace MMDarkness
{
    [Name("动画轨道")]
    [Category("Test")]
    [Description("动画轨道")]
    [Color(0.0f, 1f, 1f)]
    [Attachable(typeof(Group))]
    [ShowIcon(typeof(Animator))]
    public class AnimatorTrack : Track
    {
    }

    [Name("特效轨道")]
    [Category("Test")]
    [Description("特效轨道")]
    [Color(0.0f, 1f, 1f)]
    [Attachable(typeof(Group))]
    public class VFXTrack : Track
    {
    }

    [Name("CTrack")]
    [Category("Test")]
    [Description("CTrack")]
    [Color(0.0f, 1f, 1f)]
    [Attachable(typeof(Group))]
    public class CTrack : Track
    {
    }
}