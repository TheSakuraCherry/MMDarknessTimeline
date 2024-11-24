using UnityEditor;
using UnityEngine;

namespace MMDarkness.Editor
{
    public class HeaderGUI : ICustomized
    {
        protected TimelineGraphPreviewProcessor player => App.Player;

        public virtual void OnGUI()
        {
            DrawToolbar();
            DrawPlayControl();
        }

        protected void DrawPlayControl()
        {
            GUILayout.BeginArea(G.TopLeftRect);
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            DrawPlayControlLeft();

            GUILayout.FlexibleSpace();

            DrawPlayControlRight();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
            GUI.contentColor = Color.white;
        }

        protected virtual void DrawPlayControlLeft()
        {
            if (GUILayout.Button(new GUIContent(Styles.StepBackwardIcon, Lan.StepBackwardTips),
                    EditorStyles.toolbarButton, GUILayout.Width(26)))
            {
                App.StepBackward();
                Event.current.Use();
            }

            EditorGUI.BeginChangeCheck();

            var playContent = new GUIContent(Styles.PlayIcon, !App.IsPlay ? Lan.PlayTips : Lan.StopTips);
            var isPlaying = GUILayout.Toggle(App.IsPlay, playContent, EditorStyles.toolbarButton, GUILayout.Width(26));
            if (EditorGUI.EndChangeCheck())
            {
                if (isPlaying)
                    App.Play();
                else
                    App.Stop(true);
            }

            EditorGUI.BeginChangeCheck();
            var isPause = GUILayout.Toggle(App.IsPlay && App.IsStop, new GUIContent(Styles.PauseIcon, Lan.PauseTips),
                EditorStyles.toolbarButton, GUILayout.Width(26));
            if (EditorGUI.EndChangeCheck())
            {
                if (isPause)
                    App.Pause();
                else
                    App.Play();
            }

            if (GUILayout.Button(new GUIContent(Styles.StepForwardIcon, Lan.StepForwardTips),
                    EditorStyles.toolbarButton, GUILayout.Width(26)))
            {
                App.StepForward();
                Event.current.Use();
            }

            if (GUILayout.Button(new GUIContent(Styles.PlayForwardIcon, Lan.PlayForwardTips),
                    EditorStyles.toolbarButton, GUILayout.Width(26))) player.CurrentTime = player.Length;
        }

        protected virtual void DrawPlayControlRight()
        {
            EditorGUI.BeginChangeCheck();
            var isLoop = GUILayout.Toggle(App.EditorPlaybackWrapMode == WrapMode.Loop,
                new GUIContent(Styles.PlayLoopIcon, Lan.PlayLoopTips), EditorStyles.toolbarButton, GUILayout.Width(26));
            if (EditorGUI.EndChangeCheck()) App.EditorPlaybackWrapMode = isLoop ? WrapMode.Loop : WrapMode.Once;
        }

        protected virtual void DrawToolbar()
        {
            if (!EditorGUIUtility.isProSkin) GUI.contentColor = Color.black.WithAlpha(0.7f);

            GUI.enabled = player.CurrentTime <= 0; //是否禁用工具条。播放时

            GUI.backgroundColor = Color.white;
            GUI.color = Color.white;
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            DrawToolbarLeft();

            GUILayout.FlexibleSpace();

            DrawToolbarRight();

            GUILayout.EndHorizontal();
            GUI.backgroundColor = Color.white;

            GUI.enabled = true;
            GUI.contentColor = Color.white;
        }

        protected virtual void DrawToolbarLeft()
        {
            if (GUILayout.Button(new GUIContent(Styles.BackIcon, Lan.BackMenuTips), EditorStyles.toolbarButton,
                    GUILayout.Width(26)))
            {
                App.GraphAsset = null;
                GUILayout.EndHorizontal();
                return;
            }

            if (GUILayout.Button(new GUIContent(Styles.PlusIcon, Lan.NewAssetTips), EditorStyles.toolbarButton,
                    GUILayout.Width(26))) CreateAssetWindow.Show();

            Prefs.magnetSnapping = GUILayout.Toggle(Prefs.magnetSnapping,
                new GUIContent(Styles.MagnetIcon, Lan.OpenMagnetSnappingTips), EditorStyles.toolbarButton);

            var gName = App.GraphAsset ? App.GraphAsset.name : string.Empty;

            var con = new GUIContent(string.Format(Lan.HeaderSelectAsset, gName), Lan.SelectAssetTips);
            if (GUILayout.Button(con, EditorStyles.toolbarDropDown, GUILayout.Width(120)))
            {
                App.AutoSave(); //先保存当前的
                ObjectSelectorWindow.ShowObjectPicker<TextAsset>(App.GraphAsset, App.OnObjectPickerConfig,
                    Prefs.savePath);
            }

            App.Owner = EditorGUILayout.ObjectField("", App.Owner, typeof(GameObject), true) as GameObject;
        }

        protected virtual void DrawToolbarRight()
        {
            //显示保持状态
            GUI.color = Color.white.WithAlpha(0.3f);
            GUILayout.Label(
                $"<size=11>{string.Format(Lan.HeaderLastSaveTime, App.LastSaveTime.ToString("HH:mm:ss"))}</size>");
            GUI.color = Color.white;


            if (GUILayout.Button(new GUIContent(Styles.SaveIcon, Lan.Save), EditorStyles.toolbarButton,
                    GUILayout.Width(26))) App.AutoSave(); //先保存当前的

            if (GUILayout.Button(new GUIContent(Styles.SettingsIcon, Lan.OpenPreferencesTips),
                    EditorStyles.toolbarButton, GUILayout.Width(26)))
                PreferencesWindow.Show(new Rect(G.ScreenWidth - 5 - 400, Styles.ToolbarHeight + 5, 400,
                    G.ScreenHeight - Styles.ToolbarHeight - 50));
        }
    }
}