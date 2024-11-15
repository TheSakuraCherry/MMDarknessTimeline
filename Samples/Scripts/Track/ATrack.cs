using UnityEngine;

namespace MMDarkness
{
    [Name("ATrack")]
    [Category("Test")]
    [Description("ATrack")]
    [Color(r: 0.0f, 1f, 1f)]
    [Attachable(typeof(Group))]
    [ShowIcon(typeof(Animator))]
    public class ATrack : Track
    {
    }

    [Name("BTrack")]
    [Category("Test")]
    [Description("BTrack")]
    [Color(r: 0.0f, 1f, 1f)]
    [Attachable(typeof(Group))]
    public class BTrack : Track
    {
    }

    [Name("CTrack")]
    [Category("Test")]
    [Description("CTrack")]
    [Color(r: 0.0f, 1f, 1f)]
    [Attachable(typeof(Group))]
    public class CTrack : Track
    {
    }
}
