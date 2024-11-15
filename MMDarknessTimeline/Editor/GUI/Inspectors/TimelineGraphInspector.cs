using UnityEditor;

namespace MMDarkness.Editor
{
    [CustomInspectors(typeof(TimelineGraphAsset), true)]
    public class TimelineGraphInspector : InspectorsBase
    {
        private TimelineGraphAsset m_graph => (TimelineGraphAsset)m_target;

        public override void OnInspectorGUI()
        {
            ShowCommonInspector();
            //base.OnInspectorGUI();
        }

        protected void ShowCommonInspector(bool showBaseInspector = true)
        {
        }
    }
}