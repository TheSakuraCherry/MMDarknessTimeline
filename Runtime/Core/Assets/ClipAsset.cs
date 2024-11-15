using System;
using UnityEngine;

namespace MMDarkness
{
    [Serializable]
    public class ClipAsset : DirectableAsset
    {
        [SerializeReference]
        public Clip clipModel;

        [SerializeField, HideInInspector]
        private float startTime;

        public override TimelineGraphAsset Root
        {
            get => Parent?.Root;
            set { }
        }

        public override DirectableAsset Parent
        {
            get => m_parent;
            set => m_parent = value;
        }

        public string NameClip
        {
            get => name;
            set => name = value;
        }

        public override float StartTime
        {
            get => startTime;
            set
            {
                if (Math.Abs(startTime - value) > 0.0001f)
                {
                    startTime = Mathf.Max(value, 0);
                    clipModel.startTime = startTime;
                    BlendIn = Mathf.Clamp(BlendIn, 0, Length - BlendOut);
                    BlendOut = Mathf.Clamp(BlendOut, 0, Length - BlendIn);
                }
            }
        }


        public override float EndTime
        {
            get => StartTime + Length;
            set
            {
                if (Math.Abs(StartTime + Length - value) > 0.0001f) //if (StartTime + length != value)
                {
                    Length = Mathf.Max(value - StartTime, 0);
                    clipModel.length = Length;
                    BlendOut = Mathf.Clamp(BlendOut, 0, Length - BlendIn);
                    BlendIn = Mathf.Clamp(BlendIn, 0, Length - BlendOut);
                }
            }
        }

        public override bool IsActive => Parent && Parent.IsActive;
        public override bool IsCollapsed => Parent != null && Parent.IsCollapsed;
        public override bool IsLocked => Parent != null && Parent.IsLocked;

        public virtual float Length
        {
            get => clipModel?.length ?? 0;
            set => clipModel.length = value;
        }


        public virtual string Info
        {
            get
            {
                var nameAtt = clipModel.GetType().RTGetAttribute<NameAttribute>(true);
                return nameAtt != null ? nameAtt.Name : GetType().Name.SplitCamelCase();
            }
        }

        public virtual bool IsValid => true;


        public ClipAsset GetNextClip()
        {
            return this.GetNextSibling<ClipAsset>();
        }

        public float GetClipWeight(float time)
        {
            return GetClipWeight(time, this.BlendIn, this.BlendOut);
        }

        public float GetClipWeight(float time, float blendInOut)
        {
            return GetClipWeight(time, blendInOut, blendInOut);
        }

        public float GetClipWeight(float time, float blendIn, float blendOut)
        {
            return this.GetWeight(time, blendIn, blendOut);
        }

        public void TryMatchSubClipLength()
        {
            if (this is ISubClipContainable subClipContainable)
            {
                Length = subClipContainable.SubClipLength / subClipContainable.SubClipSpeed;
            }
        }

        public void TryMatchPreviousSubClipLoop()
        {
            if (this is ISubClipContainable)
            {
                Length = (this as ISubClipContainable).GetPreviousLoopLocalTime();
            }
        }

        public void TryMatchNexSubClipLoop()
        {
            if (this is ISubClipContainable)
            {
                var targetLength = (this as ISubClipContainable).GetNextLoopLocalTime();
                var nextClip = GetNextClip();
                if (nextClip == null || StartTime + targetLength <= nextClip.StartTime)
                {
                    Length = targetLength;
                }
            }
        }

        public void ShowClipGUI(Rect rect)
        {
            OnClipGUI(rect);
        }

        public void ShowClipGUIExternal(Rect left, Rect right)
        {
            OnClipGUIExternal(left, right);
        }

        protected virtual void OnClipGUI(Rect rect)
        {
        }

        protected virtual void OnClipGUIExternal(Rect left, Rect right)
        {
        }
    }
}
