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

            // 添加间距
            GUILayout.Space(5);

            // 获取当前对象值
            Object currentObject = null;

            // 如果 ValueEntry 有值（非空），则加载对应类型的资源
            if (!string.IsNullOrEmpty(ValueEntry.SmartValue))
            {
                currentObject = AssetDatabase.LoadAssetAtPath<Object>(ValueEntry.SmartValue);
            }

            currentObject = SirenixEditorFields.UnityObjectField(
                currentObject,
                this.Attribute.ObjectType,
                false,
                GUILayout.Width(200)
            );
            // 如果选择了对象，更新路径
            if (currentObject != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(currentObject);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    ValueEntry.SmartValue = assetPath; // 更新字符串字段的值
                }
            }

            // 结束水平布局
            SirenixEditorGUI.EndIndentedHorizontal();
        }
    }
}
