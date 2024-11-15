using System;
using UnityEditor;
using UnityEngine;

namespace MMDarkness.Editor
{
    public class G
    {
        public static string SearchString;

        public static float timeInfoInterval = 1000000f;
        public static float timeInfoHighMod = timeInfoInterval;
        public static float timeInfoStart;
        public static float timeInfoEnd;

        public static float SnapTime(float time)
        {
            if (Event.current.control)
            {
                return time;
            }

            return (Mathf.Round(time / Prefs.snapInterval) * Prefs.snapInterval);
        }

        #region Size

        public static float TotalHeight;

        public static float ScreenWidth => Screen.width / EditorGUIUtility.pixelsPerPoint;

        public static float ScreenHeight => Screen.height / EditorGUIUtility.pixelsPerPoint;

        public static Vector2 ScrollPos;

        public static readonly float BottomHeight = Styles.BottomHeight * 2;

        public static Rect TopLeftRect;

        public static Rect LeftRect;

        public static Rect TopMiddleRect;

        public static Rect CenterRect;

        public static void Reset()
        {
            TopLeftRect = new Rect(0, Styles.ToolbarHeight, Styles.LeftMargin, Styles.TopMargin);


            var centerHeight = ScreenHeight - Styles.ToolbarHeight - Styles.TopMargin + ScrollPos.y;
            var centerWidth = ScreenWidth - Styles.LeftMargin - Styles.RightMargin;

            TopMiddleRect = new Rect(Styles.LeftMargin, Styles.ToolbarHeight, centerWidth, Styles.TopMargin);

            LeftRect = new Rect(0, Styles.ToolbarHeight + Styles.TopMargin, Styles.LeftMargin, centerHeight);

            CenterRect = new Rect(Styles.LeftMargin, Styles.TopMargin + Styles.ToolbarHeight, centerWidth,
                centerHeight - BottomHeight);
        }

        #endregion
        
        
        internal static bool IsFilteredOutBySearch(ScriptableObject direct)
        {
            if (string.IsNullOrEmpty(SearchString))
            {
                return false;
            }

            if (string.IsNullOrEmpty(direct.name))
            {
                return true;
            }

            return !direct.name.ToLower().Contains(SearchString.ToLower());
        }
    }
}