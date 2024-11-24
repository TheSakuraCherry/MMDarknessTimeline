using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MMDarkness
{
    [Serializable]
    public class GroupAsset : DirectableAsset
    {
        [SerializeReference] public Group groupModel;

        [SerializeField] [HideInInspector] private List<TrackAsset> trackAssets = new();

        [SerializeField] [HideInInspector] private bool isCollapsed;

        [SerializeField] [HideInInspector] private bool active = true;

        [SerializeField] [HideInInspector] private bool isLocked;

        public override TimelineGraphAsset Root { get; set; }

        public override DirectableAsset Parent
        {
            get => m_parent;
            set { }
        }

        public override float StartTime => 0;
        public override float EndTime => Root.Length;

        public string Name
        {
            get => name;
            set => name = value;
        }

        public List<TrackAsset> Tracks
        {
            get => trackAssets;
            set => trackAssets = value;
        }

        public override bool IsActive
        {
            get => active;
            set => active = value;
        }

        public override bool IsCollapsed
        {
            get => isCollapsed;
            set => isCollapsed = value;
        }

        public override bool IsLocked
        {
            get => isLocked;
            set => isLocked = value;
        }


        #region 增删

        public bool CanAddTrack(TrackAsset trackAsset)
        {
            return trackAsset && CanAddTrackOfType(trackAsset.GetType());
        }

        public bool CanAddTrackOfType(Type type)
        {
            if (type == null || !type.IsSubclassOf(typeof(TrackAsset)) || type.IsAbstract) return false;

            if (type.IsDefined(typeof(UniqueAttribute), true) &&
                Tracks.FirstOrDefault(t => t.GetType() == type) != null) return false;

            var attachAtt = type.RTGetAttribute<AttachableAttribute>(true);
            if (attachAtt == null || attachAtt.Types == null || attachAtt.Types.All(t => t != GetType())) return false;

            return true;
        }

        public TrackAsset AddTrack(Type type, string trackName = null)
        {
            var newTrack = CreateInstance<TrackAsset>();
            newTrack.Name = type.Name;
            newTrack.Parent = this;
            newTrack.trackModel = Activator.CreateInstance(type) as Track;
            Tracks.Add(newTrack);
            CreateUtilities.SaveAssetIntoObject(newTrack, this);
            DirectorUtility.SelectedObject = newTrack;
            return newTrack;
        }

        public void DeleteTrack(TrackAsset trackAsset)
        {
            // Undo.RegisterCompleteObjectUndo(this, "Delete Track");
            Tracks.Remove(trackAsset);
            if (ReferenceEquals(DirectorUtility.SelectedObject, trackAsset)) DirectorUtility.SelectedObject = null;
            // Undo.DestroyObjectImmediate(track);
            // EditorUtility.SetDirty(this);
            // root?.Validate();
            // root?.SaveToAssets();
        }


        public TrackAsset PasteTrack(TrackAsset trackAsset)
        {
            if (!CanAddTrack(trackAsset)) return null;

            var newTrack = Instantiate(trackAsset);
            if (newTrack != null)
            {
                newTrack.Parent = this;
                Tracks.Add(newTrack);
                CreateUtilities.SaveAssetIntoObject(newTrack, this);
                newTrack.Clips.Clear();
                newTrack.trackModel.clips.Clear();
                foreach (var clip in trackAsset.Clips) newTrack.PasteClip(clip);
            }

            return newTrack;
        }

        #endregion
    }
}