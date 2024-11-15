using UnityEditor;

namespace MMDarkness.Editor
{
    public static class Menus
    {
        [MenuItem("YAMITool/ActionEditor", false, 0)]
        public static void OpenDirectorWindow()
        {
            ActionEditorWindow.ShowWindow();
        }
    }
}
