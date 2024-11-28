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
        }

        protected override void OnEnter(FrameData frameData, FrameData innerFrameData)
        {
            if(!animator || !animationClip)
                return;
            // 创建 PlayableGraph
            playableGraph = PlayableGraph.Create("AnimationPlayableGraph");
            playableGraph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

            // 创建 AnimationClipPlayable 并设置动画剪辑
            clipPlayable = AnimationClipPlayable.Create(playableGraph, animationClip);

            // 创建 AnimationPlayableOutput 并将其连接到 Animator
            var playableOutput = AnimationPlayableOutput.Create(playableGraph, "AnimationOutput", animator);
            playableOutput.SetSourcePlayable(clipPlayable);

            // 播放 PlayableGraph
            playableGraph.Play();
        }

        protected override void OnUpdate(FrameData frameData, FrameData innerFrameData)
        {
            clipPlayable.SetTime(innerFrameData.currentTime);
        }
    }
}