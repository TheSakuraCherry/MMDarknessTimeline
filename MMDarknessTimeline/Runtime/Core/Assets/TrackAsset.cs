using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

namespace MMDarkness
{
    [Serializable]
    public class TrackAsset : DirectableAsset
    {
        [SerializeReference, HideInInspector]
        public Track trackModel;

        [SerializeField, HideInInspector]
        private List<ClipAsset> clipAssets = new List<ClipAsset>();

        [SerializeField]
        [HideInInspector]
        private bool active = true;

        [SerializeField]
        [HideInInspector]
        private bool isLocked = false;

        [SerializeField]
        private Color color = Color.white;

        public Color Color => color.a > 0.1f ? color : Color.white;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public virtual string info => string.Empty;

        public override TimelineGraphAsset Root
        {
            get => Parent?.Root;
            set { }
        }

        public override DirectableAsset Parent
        {
            get => (GroupAsset)m_parent;
            set => m_parent = value;
        }

        public override bool IsCollapsed => Parent && Parent.IsCollapsed;

        public override bool IsActive
        {
            get => Parent && Parent.IsActive && active;
            set => active = value;
        }

        public override bool IsLocked
        {
            get => Parent && (Parent.IsLocked || isLocked);
            set => isLocked = value;
        }


        public List<ClipAsset> Clips
        {
            get => clipAssets;
            set => clipAssets = value;
        }

        public override float StartTime => 0;

        public override float EndTime => Parent != null ? Parent.EndTime : 0;

        public override bool CanCrossBlend => false;

        public virtual float ShowHeight => 30f;


        #region 增删
        public ClipAsset AddClip<T>(float time) where T : Clip
        {
            return AddClip(typeof(T), time);
        }

        public ClipAsset AddClip(Type type, float time)
        {
            if (type.GetCustomAttributes(typeof(CategoryAttribute), true).FirstOrDefault() is CategoryAttribute catAtt && Clips.Count == 0)
            {
                Name = catAtt.Category + " Track";
            }

            var newClip = CreateInstance<ClipAsset>();

            if (newClip != null)
            {
                newClip.Parent = this;
                newClip.clipModel = Activator.CreateInstance(type) as Clip;
                newClip.StartTime = time;
                Clips.Add(newClip);
                var nextAction = Clips.FirstOrDefault(a => a.StartTime > newClip.StartTime);
                if (nextAction != null)
                {
                    newClip.EndTime = Mathf.Min(newClip.EndTime, nextAction.StartTime);
                }
            }
            CreateUtilities.SaveAssetIntoObject(newClip, this);
            DirectorUtility.SelectedObject = newClip;
            return newClip;
        }

        public void DeleteClip(ClipAsset action)
        {
            Clips.Remove(action);
            if (ReferenceEquals(DirectorUtility.SelectedObject, action))
            {
                DirectorUtility.SelectedObject = null;
            }
        }

        public ClipAsset PasteClip(ClipAsset clipAsset, float time = 0)
        {
            var newClip = Instantiate(clipAsset);
            if (newClip != null)
            {
                if (time > 0)
                {
                    newClip.StartTime = time;
                    var nextClip = Clips.FirstOrDefault(a => a.StartTime > newClip.StartTime);
                    if (nextClip != null && newClip.EndTime > nextClip.StartTime)
                    {
                        newClip.EndTime = nextClip.StartTime;
                    }
                }

                newClip.Parent = this;
                Clips.Add(newClip);
                CreateUtilities.SaveAssetIntoObject(newClip, this);
            }

            return newClip;
        }
        #endregion

        internal bool IsCompilable()
        {
            return true;
        }

        bool m_cacheSorted;

        public void SortClips()
        {
            if (!m_cacheSorted)
            {
                Clips.Sort((clip1, clip2) => clip1.StartTime.CompareTo(clip2.StartTime));
                foreach (var clip in Clips)
                {
                    clip.clipModel.startTime = clip.StartTime;
                }
                m_cacheSorted = true;
            }
        }
    }
}
