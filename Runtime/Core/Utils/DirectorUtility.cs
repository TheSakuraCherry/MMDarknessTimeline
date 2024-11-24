using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MMDarkness
{
    public static class DirectorUtility
    {
        [NonSerialized] private static InspectorAsset m_currentInspector;

        private static ClipAsset m_copyClipAsset;
        private static Type m_copyClipType;

        [NonSerialized] private static ScriptableObject m_selectedObject;

        public static InspectorAsset CurrentInspector =>
            m_currentInspector ??= ScriptableObject.CreateInstance<InspectorAsset>();

        public static ClipAsset CopyClipAsset
        {
            get => m_copyClipAsset;
            set
            {
                m_copyClipAsset = value;
                m_copyClipType = value ? value.GetType() : default;
            }
        }

        public static ScriptableObject SelectedObject
        {
            get => m_selectedObject;
            set
            {
                m_selectedObject = value;
#if UNITY_EDITOR
                Selection.activeObject = CurrentInspector;
                EditorUtility.SetDirty(CurrentInspector);
#endif
                OnSelectionChange?.Invoke(value);
            }
        }

        public static event Action<ScriptableObject> OnSelectionChange;

        public static Type GetCopyType()
        {
            return m_copyClipType;
        }

        public static void FlushCopyClip()
        {
            m_copyClipType = null;
            m_copyClipAsset = null;
        }

        public static void CutClip(ClipAsset clipAsset)
        {
            m_copyClipAsset = clipAsset;
            m_copyClipType = clipAsset.GetType();
            (clipAsset.Parent as TrackAsset)?.DeleteClip(clipAsset);
        }
    }
}