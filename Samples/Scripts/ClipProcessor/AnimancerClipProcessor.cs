using Animancer;
using MMDarkness;

namespace MMDarknessTimeline
{
    [ViewModel(typeof(AnimancerClip))]
    public class AnimancerClipProcessor : ClipProcessor<AnimancerClip>
    {
        //private AnimancerComponent _animancer;
        protected override void OnEnter(FrameData frameData, FrameData innerFrameData)
        {
            // _animancer = Owner.GetComponent<AnimancerComponent>();
            // _animancer.Play(TData.Clip);
        }
    }
}