#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;


namespace MMDarkness
{
    [Serializable]
    public sealed class TimelineGraphAsset : ScriptableObject, IData
    {
        [SerializeField] private float length = 5f;

        [SerializeField] private float viewTimeMin;

        [SerializeField] private float viewTimeMax = 5f;

        [SerializeField] [HideInInspector] public List<GroupAsset> groupAssets = new();

        [NonSerialized] private TrackAsset[] m_cacheOutputTracks;

        public float Length
        {
            get => length;
            set => length = Mathf.Max(value, 0.1f);
        }

        public float ViewTimeMin
        {
            get => viewTimeMin;
            set
            {
                if (ViewTimeMax > 0) viewTimeMin = Mathf.Min(value, ViewTimeMax - 0.25f);
            }
        }

        public float ViewTimeMax
        {
            get => viewTimeMax;
            set => viewTimeMax = Mathf.Max(value, ViewTimeMin + 0.25f, 0);
        }

        public float MaxTime => Mathf.Max(ViewTimeMax, Length);
        public float ViewTime => ViewTimeMax - ViewTimeMin;

        public List<DirectableAsset> Directables { get; private set; }


        public T AddGroup<T>() where T : GroupAsset, new()
        {
            var newGroup = CreateInstance<T>();
            newGroup.Name = "New Group";
            newGroup.Root = this;
            newGroup.groupModel = new Group();
            groupAssets.Add(newGroup);
            CreateUtilities.SaveAssetIntoObject(newGroup, this);
            DirectorUtility.SelectedObject = newGroup;
            return newGroup;
        }

        public void DeleteGroup(GroupAsset groupAsset)
        {
            groupAssets.Remove(groupAsset);
        }

        public GroupAsset PasteGroup(GroupAsset groupAsset)
        {
            var newGroup = Instantiate(groupAsset);
            if (newGroup != null)
            {
                newGroup.Root = this;
                groupAssets.Add(newGroup);
                CreateUtilities.SaveAssetIntoObject(newGroup, this);
                newGroup.Tracks.Clear();
                newGroup.groupModel.tracks.Clear();
                foreach (var track in groupAsset.Tracks) newGroup.PasteTrack(track);
            }

            return newGroup;
        }

        public TimelineGraph GetGraphModel()
        {
            var graphModel = new TimelineGraph
            {
                groups = new List<Group>()
            };
            graphModel.length = length;

            foreach (var groupAsset in groupAssets)
            {
                var groupModel = groupAsset.groupModel;
                groupModel.tracks = new List<Track>();

                foreach (var trackAsset in groupAsset.Tracks)
                {
                    var trackModel = trackAsset.trackModel;
                    trackModel.clips = new List<Clip>();

                    foreach (var clipAsset in trackAsset.Clips)
                    {
                        var clipModel = clipAsset.clipModel;

                        //对Clip属性做同步
                        clipModel.startTime = clipAsset.StartTime;
                        clipModel.length = clipAsset.Length;
                        trackModel.clips.Add(clipModel);
                    }

                    groupModel.tracks.Add(trackModel);
                }

                graphModel.groups.Add(groupModel);
            }

            return graphModel;
        }

        [Button]
        public void SerializeGraphModel()
        {
            var serializedData = SerializationUtility.SerializeValue(GetGraphModel(), DataFormat.Binary);
            using var file = File.Create($"{Prefs.SerializeSavePath}/{name}.bytes");
            file.Write(serializedData, 0, serializedData.Length);
        }

        public void Validate()
        {
            foreach (var groupAsset in groupAssets) groupAsset.Root = this;
        }

        public void SaveToAssets()
        {
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }
    }
}