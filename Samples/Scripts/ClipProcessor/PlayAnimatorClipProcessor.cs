using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace MMDarkness
{
    [ViewModel(typeof(PlayAnimatorClip))]
    public class PlayAnimatorClipProcessor : ClipProcessor<PlayAnimatorClip>
    {
        private Animator animator;

        private AnimationClipPlayable clipPlayable;

        private AnimationClip animationClip;

        private PlayableGraph playableGraph;
        protected override void OnInit()
        {
            base.OnInit();
            animator = Owner.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("没有找到 Animator 组件！");
                return;
            }

            animationClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(TData.ClipPath);
            if (!animationClip)
            {
                Debug.LogError("指定路径没有找到动画文件");
            }
            playableGraph = PlayableGraph.Create("AnimationPlayableGraph");
        }

        protected override void OnEnter(FrameData frameData, FrameData innerFrameData)
        {
            if(!animator || !animationClip)
                return;
            // 创建 PlayableGraph
            

            // 创建 AnimationClipPlayable 并设置动画剪辑
            clipPlayable = AnimationClipPlayable.Create(playableGraph, animationClip);

            // 创建 AnimationPlayableOutput 并将其连接到 Animator
            var playableOutput = AnimationPlayableOutput.Create(playableGraph, "AnimationOutput", animator);
            playableOutput.SetSourcePlayable(clipPlayable);

            // 播放 PlayableGraph
            playableGraph.Play();
            clipPlayable.SetTime(0);
        }

        protected override void OnUpdate(FrameData frameData, FrameData innerFrameData)
        {
            if(clipPlayable.IsValid())
                clipPlayable.SetTime(innerFrameData.currentTime);
        }

        protected override void OnExit(FrameData frameData, FrameData innerFrameData)
        {
            // 销毁 PlayableGraph 以释放资源
            if (clipPlayable.IsValid())
            {
                clipPlayable.Destroy();
            }
        }
    }
}