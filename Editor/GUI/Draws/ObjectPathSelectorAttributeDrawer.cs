using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace MMDarkness.Editor.Draws
{
    public class ObjectPathSelectorAttributeDrawer : OdinAttributeDrawer<ObjectPathSelectorAttribute, string>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            // 使用 Odin 提供的水平布局
            SirenixEditorGUI.BeginIndentedHorizontal();
            // 绘制原本的字符串字段（使用 Odin 的默认绘制逻辑）
            CallNextDrawer(label);
            // 绘制 ObjectField
            GUILayout.Space(5); //添加间距
            Object selectedObject = SirenixEditorFields.UnityObjectField(null,
                Attribute.ObjectType,
                false,
                GUILayout.Width(60));
            // 如果选择了对象，更新路径
            if (selectedObject != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(selectedObject);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    ValueEntry.SmartValue = assetPath; // 更新字符串字段的值
                }
            }

            SirenixEditorGUI.EndIndentedHorizontal();
        }
    }
}