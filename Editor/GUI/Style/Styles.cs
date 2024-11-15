using UnityEditor;
using UnityEngine;

namespace MMDarkness.Editor
{
    [InitializeOnLoad]
    public static class Styles
    {
        /// <summary>
        ///     右边边距
        /// </summary>
        public const float RightMargin = 16;

        /// <summary>
        ///     工具栏的高度
        /// </summary>
        public const float ToolbarHeight = 20;

        /// <summary>
        ///     上边距
        /// </summary>
        public const float TopMargin = 20;

        /// <summary>
        ///     组高
        /// </summary>
        public const float GroupHeight = 31;

        /// <summary>
        ///     组右边距
        /// </summary>
        public const float GroupRightMargin = 4;

        /// <summary>
        ///     第一个组上边距
        /// </summary>
        public const float FirstGroupTopMargin = 22;

        /// <summary>
        ///     轨道上下边距
        /// </summary>
        public const float TrackMargins = 4;

        /// <summary>
        ///     轨道右边距
        /// </summary>
        public const float TrackRightMargin = 4;

        /// <summary>
        ///     底部滚动条高度
        /// </summary>
        public const float BottomHeight = 20;

        public static Texture2D Logo;
        public static Texture2D Stripes;
        public static Texture2D MagnetIcon;
        public static Texture2D LockIcon;
        public static Texture2D HiddenIcon;
        public static Texture2D PlayIcon;
        public static Texture2D PlayForwardIcon;
        public static Texture2D PlayBackwardIcon;
        public static Texture2D StepForwardIcon;
        public static Texture2D StepBackwardIcon;
        public static Texture2D PauseIcon;
        public static Texture2D PlayLoopIcon;
        public static Texture2D CarretIcon;
        public static Texture2D CutsceneIconOpen;
        public static Texture2D BackIcon;
        public static Texture2D SaveIcon;
        public static Texture2D SettingsIcon;
        public static Texture2D PlusIcon;
        public static Texture2D MenuIcon;
        private static GUISkin m_styleSheet;
        public static readonly Color ListSelectionColor = new(0.5f, 0.5f, 1, 0.3f);
        public static readonly Color GroupColor = new(0f, 0f, 0f, 0.25f);

        private static GUIStyle m_shadowBorderStyle;
        private static GUIStyle m_clipBoxStyle;
        private static GUIStyle m_clipBoxFooterStyle;
        private static GUIStyle m_clipBoxHorizontalStyle;
        private static GUIStyle m_timeBoxStyle;
        private static GUIStyle m_headerBoxStyle;
        private static GUIStyle m_hollowFrameStyle;
        private static GUIStyle m_hollowFrameHorizontalStyle;
        private static GUIStyle m_centerLabel;

        static Styles()
        {
            Load();
        }

        public static Color HighlightColor =>
            EditorGUIUtility.isProSkin ? new Color(0.65f, 0.65f, 1) : new Color(0.1f, 0.1f, 0.1f);

        public static float LeftMargin
        {
            get => Prefs.trackListLeftMargin;
            set => Prefs.trackListLeftMargin = Mathf.Clamp(value, 230, 400);
        }

        public static Texture2D WhiteTexture => EditorGUIUtility.whiteTexture;
        public static GUIStyle ShadowBorderStyle => m_shadowBorderStyle ??= m_styleSheet.GetStyle("ShadowBorder");
        public static GUIStyle ClipBoxStyle => m_clipBoxStyle ??= m_styleSheet.GetStyle("ClipBox");

        public static GUIStyle ClipBoxHorizontalStyle =>
            m_clipBoxHorizontalStyle ??= m_styleSheet.GetStyle("ClipBoxHorizontal");

        public static GUIStyle TimeBoxStyle => m_timeBoxStyle ??= m_styleSheet.GetStyle("TimeBox");
        public static GUIStyle HeaderBoxStyle => m_headerBoxStyle ??= m_styleSheet.GetStyle("HeaderBox");
        public static GUIStyle HollowFrameStyle => m_hollowFrameStyle ??= m_styleSheet.GetStyle("HollowFrame");

        public static GUIStyle HollowFrameHorizontalStyle =>
            m_hollowFrameHorizontalStyle ??= m_styleSheet.GetStyle("HollowFrameHorizontal");

        public static GUIStyle CenterLabel
        {
            get
            {
                if (m_centerLabel != null) return m_centerLabel;

                m_centerLabel = new GUIStyle("label")
                {
                    alignment = TextAnchor.MiddleCenter
                };
                return m_centerLabel;
            }
        }

        [InitializeOnLoadMethod]
        public static void Load()
        {
            Stripes = (Texture2D)Resources.Load("nbc/Stripes");
            MagnetIcon = (Texture2D)Resources.Load("nbc/magnet");
            LockIcon = (Texture2D)Resources.Load("nbc/LockIcon");
            HiddenIcon = (Texture2D)Resources.Load("nbc/HiddenIcon");
            PlayIcon = (Texture2D)Resources.Load("nbc/play");
            PlayForwardIcon = (Texture2D)Resources.Load("nbc/playForward");
            PlayBackwardIcon = (Texture2D)Resources.Load("nbc/playBackward");
            PlayLoopIcon = (Texture2D)Resources.Load("nbc/loopPlay");
            StepForwardIcon = (Texture2D)Resources.Load("nbc/stepForward");
            StepBackwardIcon = (Texture2D)Resources.Load("nbc/stepBackward");
            PauseIcon = (Texture2D)Resources.Load("nbc/pause");
            CarretIcon = (Texture2D)Resources.Load("nbc/CarretIcon");
            CutsceneIconOpen = (Texture2D)Resources.Load("nbc/CutsceneIconOpen");
            SettingsIcon = (Texture2D)Resources.Load("nbc/settings");
            BackIcon = (Texture2D)Resources.Load("nbc/back");
            SaveIcon = (Texture2D)Resources.Load("nbc/save");
            PlusIcon = (Texture2D)Resources.Load("nbc/plus");
            MenuIcon = (Texture2D)Resources.Load("nbc/menu");

            Logo = (Texture2D)Resources.Load("nbc/Logo");

            m_styleSheet = (GUISkin)Resources.Load("nbc/StyleSheet");
        }
    }
}