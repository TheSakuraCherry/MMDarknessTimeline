using UnityEngine;

namespace MMDarkness
{
    [Name("ATrack")]
    [Category("Test")]
    [Description("ATrack")]
    [Color(0.0f, 1f, 1f)]
    [Attachable(typeof(Group))]
    [ShowIcon(typeof(Animator))]
    public class ATrack : Track
    {
    }

    [Name("BTrack")]
    [Category("Test")]
    [Description("BTrack")]
    [Color(0.0f, 1f, 1f)]
    [Attachable(typeof(Group))]
    public class BTrack : Track
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