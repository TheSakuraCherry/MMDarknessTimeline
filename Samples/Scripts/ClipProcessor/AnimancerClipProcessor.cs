using Animancer;
using GameLogic;
using MMDarkness;

namespace MMDarknessTimeline
{
    [ViewModel(typeof(AnimancerClip))]
    public class AnimancerClipProcessor : ClipProcessor<AnimancerClip>
    {
        private Player player;
        private AnimancerState _state;
        private AnimancerClip data;

        protected override void OnInit()
        {
            player = Owner.GetComponent<Player>();
            data = TData;
        }

        protected override void OnEnter(FrameData frameData, FrameData innerFrameData)
        {
            _state = player.AnimComponent.Play(data.Clip);
            _state.Weight = 0;
        }

        protected override void OnUpdate(FrameData frameData, FrameData innerFrameData)
        {
            if (innerFrameData.currentTime < data.BlendTime)
            {
                var curweight = innerFrameData.currentTime / data.BlendTime;
                _state.Weight = curweight;
            }

            _state.Time = innerFrameData.currentTime;
            player.AnimComponent.Evaluate();

        }
    }
}