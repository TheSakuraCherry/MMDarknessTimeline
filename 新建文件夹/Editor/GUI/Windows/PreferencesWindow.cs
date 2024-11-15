using UnityEditor;
using UnityEngine;

namespace MMDarkness.Editor
{
    public class PreferencesWindow : PopupWindowContent
    {
        private static Rect m_myRect;
        private bool m_firstPass = true;

        public static void Show(Rect rect)
        {
            m_myRect = rect;
            PopupWindow.Show(new Rect(rect.x, rect.y, 0, 0), new PreferencesWindow());
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(m_myRect.width, m_myRect.height);
        }

        public override void OnGUI(Rect rect)
        {
            DrawUtils.Draw<PreferencesGUI>();

            if (m_firstPass || Event.current.type == EventType.Repaint)
            {
                m_firstPass = false;
                m_myRect.height = GUILayoutUtility.GetLastRect().yMax + 5;
            }
        }
    }
}
