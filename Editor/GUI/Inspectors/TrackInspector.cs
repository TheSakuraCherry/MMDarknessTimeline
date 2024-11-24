using UnityEditor;

namespace MMDarkness.Editor
{
    public abstract class TrackInspector<T> : TrackInspector where T : TrackAsset
    {
        protected T action => (T)m_target;
    }

    [CustomInspectors(typeof(TrackAsset), true)]
    public class TrackInspector : InspectorsBase
    {
        private TrackAsset action => (TrackAsset)m_target;

        public override void OnInspectorGUI()
        {
            ShowCommonInspector();
        }


        protected void ShowCommonInspector(bool showBaseInspector = true)
        {
            action.Name = EditorGUILayout.TextField("Name", action.Name);
            if (showBaseInspector) base.OnInspectorGUI();
        }
    }
}