using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MMDarkness.Editor
{
    public delegate void CallbackFunction();

    public delegate void OpenAssetFunction(TimelineGraphAsset timelineGraphAsset);

    public class App
    {
        private static TextAsset m_textAsset;
        public static CallbackFunction OnInitialize;
        public static CallbackFunction OnDisable;
        public static OpenAssetFunction OnOpenAsset;
        public static CallbackFunction OnPlay;
        public static CallbackFunction OnStop;

        public static TimelineGraphAsset GraphAsset { get; set; }

        public static void OnObjectPickerConfig(Object obj)
        {
            if (obj is TimelineGraphAsset a)
            {
                GraphAsset = a;
                GraphAsset.Validate();
                Player = new TimelineGraphPreviewProcessor(GraphAsset);
            }
        }

        public static void SaveAsset()
        {
            if (GraphAsset)
                EditorUtility.SetDirty(GraphAsset);
        }


        #region AutoSave

        public static DateTime LastSaveTime { get; private set; } = DateTime.Now;


        /// <summary>
        ///     尝试自动保存
        /// </summary>
        public static void TryAutoSave()
        {
            var timespan = DateTime.Now - LastSaveTime;
            if (timespan.Seconds > Prefs.autoSaveSeconds) AutoSave();
        }

        public static void AutoSave()
        {
            LastSaveTime = DateTime.Now;
            SaveAsset();
        }

        public static void SerlizeGraph()
        {
            if(GraphAsset)
                GraphAsset.SerializeGraphModel();
        }

        #endregion

        #region 播放相关

        public static TimelineGraphPreviewProcessor Player { get; private set; }


        public static GameObject Owner
        {
            get => Player.Owner;
            set => Player.Owner = value;
        }

        public static bool IsStop => Application.isPlaying
            ? Player.IsPaused || !Player.IsActive
            : EditorPlaybackState == EditorPlaybackState.Stopped;

        internal static EditorPlaybackState EditorPlaybackState = EditorPlaybackState.Stopped;

        public static WrapMode EditorPlaybackWrapMode = WrapMode.Loop;

        public static bool IsPlay => Player.CurrentTime > 0;

        public static void Play(Action callback = null)
        {
            Play(EditorPlaybackState.PlayingForwards, callback);
        }

        private static void Play(EditorPlaybackState playbackState, Action callback = null)
        {
            if (Application.isPlaying) return;

            EditorPlaybackState = playbackState;
            OnPlay?.Invoke();
        }

        public static void Pause()
        {
            EditorPlaybackState = EditorPlaybackState.Stopped;
            OnStop?.Invoke();
        }

        public static void Stop(bool forceRewind)
        {
            if (GraphAsset)
            {
                Player.CurrentTime = 0;
                Player.OnStop();
            }
            EditorPlaybackState = EditorPlaybackState.Stopped;
            OnStop?.Invoke();
        }

        public static void StepForward()
        {
            if (Math.Abs(Player.CurrentTime - Player.Length) < 0.00001f)
            {
                Player.CurrentTime = 0;
                return;
            }

            Player.CurrentTime += Prefs.snapInterval;
        }

        public static void StepBackward()
        {
            if (Player.CurrentTime == 0)
            {
                Player.CurrentTime = Player.Length;
                return;
            }

            Player.CurrentTime -= Prefs.snapInterval;
        }

        #endregion
    }
}