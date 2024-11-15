using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MMDarkness.Editor
{
    public static class EditorTools
    {
        public struct TypeMetaInfo
        {
            public Type Type;
            public string Name;
            public string Category;
            public Type[] AttachableTypes;
            public bool IsUnique;
        }

        public static void BoldSeparator()
        {
            var tex = Styles.WhiteTexture;
            var lastRect = GUILayoutUtility.GetLastRect();
            GUILayout.Space(14);
            GUI.color = new Color(0, 0, 0, 0.25f);
            GUI.DrawTexture(new Rect(0, lastRect.yMax + 6, Screen.width, 4), tex);
            GUI.DrawTexture(new Rect(0, lastRect.yMax + 6, Screen.width, 1), tex);
            GUI.DrawTexture(new Rect(0, lastRect.yMax + 9, Screen.width, 1), tex);
            GUI.color = Color.white;
        }

        public static string GUILayoutGetFilePath(string title, string des, string path, bool textReadonly = false)
        {
            GUILayout.BeginHorizontal();
            GUI.enabled = !textReadonly;
            var str = EditorGUILayout.TextField(new GUIContent(title, des), path);
            GUI.enabled = true;
            if (GUILayout.Button(Lan.Select, GUILayout.Width(40)))
            {
                var selectPath = EditorUtility.OpenFilePanel(Lan.SelectFile, "Assets/", "");
                if (string.IsNullOrEmpty(selectPath)) return str;
                int assetStartIndex = selectPath.IndexOf("Assets", StringComparison.Ordinal);
                if (assetStartIndex > -1)
                {
                    selectPath = selectPath.Substring(assetStartIndex, selectPath.Length - assetStartIndex);
                }

                str = selectPath;
            }

            GUILayout.EndHorizontal();
            return str;
        }

        public static string GUILayoutGetFolderPath(string title, string des, string path, bool textReadonly = false)
        {
            GUILayout.BeginHorizontal();
            GUI.enabled = !textReadonly;
            var str = EditorGUILayout.TextField(new GUIContent(title, des), path);
            GUI.enabled = true;
            if (GUILayout.Button(Lan.Select, GUILayout.Width(40)))
            {
                var selectPath = EditorUtility.OpenFolderPanel(Lan.SelectFolder, "Assets/", "");
                if (string.IsNullOrEmpty(selectPath)) return str;
                int assetStartIndex = selectPath.IndexOf("Assets", StringComparison.Ordinal);
                if (assetStartIndex > -1)
                {
                    selectPath = selectPath.Substring(assetStartIndex, selectPath.Length - assetStartIndex);
                }

                str = selectPath;
            }

            GUILayout.EndHorizontal();
            return str;
        }

        ///Generic Popup for selection of any element within a list
        public static T Popup<T>(string prefix, T selected, List<T> options, params GUILayoutOption[] guiOptions)
        {
            return Popup<T>(null, prefix, selected, options, guiOptions);
        }

        public static T Popup<T>(Rect? rect, string prefix, T selected, List<T> options, params GUILayoutOption[] guiOptions)
        {
            var index = 0;
            if (options.Contains(selected))
            {
                index = options.IndexOf(selected) + 1;
            }

            var stringedOptions = new List<string>();
            if (options.Count == 0)
            {
                stringedOptions.Add("NONE AVAILABLE");
            }
            else
            {
                stringedOptions.Add("NONE");
                stringedOptions.AddRange(options.Select(o => o != null ? o.ToString() : "NONE"));
            }

            GUI.enabled = stringedOptions.Count > 1;
            if (!string.IsNullOrEmpty(prefix))
            {
                index = rect == null ? EditorGUILayout.Popup(prefix, index, stringedOptions.ToArray(), guiOptions) : EditorGUI.Popup(rect.Value, prefix, index, stringedOptions.ToArray());
            }
            else
            {
                index = rect == null ? EditorGUILayout.Popup(index, stringedOptions.ToArray(), guiOptions) : EditorGUI.Popup(rect.Value, index, stringedOptions.ToArray());
            }

            GUI.enabled = true;

            return index == 0 ? default(T) : options[index - 1];
        }

        /// <summary>
        /// 用于选择列表中任何元素而不添加NONE的通用弹出窗口
        /// </summary>
        public static T CleanPopup<T>(string prefix, T selected, List<T> options, params GUILayoutOption[] GUIOptions)
        {
            var index = -1;
            if (options.Contains(selected))
            {
                index = options.IndexOf(selected);
            }

            var stringedOptions = options.Select(o => o != null ? o.ToString() : "NONE");

            GUI.enabled = options.Count > 0;
            index = !string.IsNullOrEmpty(prefix) ? EditorGUILayout.Popup(prefix, index, stringedOptions.ToArray(), GUIOptions) : EditorGUILayout.Popup(index, stringedOptions.ToArray(), GUIOptions);
            GUI.enabled = true;

            return index == -1 ? default(T) : options[index];
        }

        /// <summary>
        /// 获取当前加载的集合中基类型的所有非抽象派生类
        /// </summary>
        /// <param name="baseType"></param>
        /// <returns></returns>
        public static List<TypeMetaInfo> GetTypeMetaDerivedFrom(Type baseType)
        {
            var infos = new List<TypeMetaInfo>();
            foreach (var type in ReflectionTools.GetImplementationsOf(baseType))
            {
                if (type.GetCustomAttributes(typeof(ObsoleteAttribute), true).FirstOrDefault() != null)
                {
                    continue;
                }

                var info = new TypeMetaInfo
                {
                    Type = type,
                    Name = type.GetCustomAttributes(typeof(NameAttribute), true).FirstOrDefault() is NameAttribute nameAtt ? nameAtt.Name : type.Name.SplitCamelCase()
                };

                if (type.GetCustomAttributes(typeof(CategoryAttribute), true).FirstOrDefault() is CategoryAttribute catAtt)
                {
                    info.Category = catAtt.Category;
                }

                if (type.GetCustomAttributes(typeof(AttachableAttribute), true).FirstOrDefault() is AttachableAttribute attachAtt)
                {
                    info.AttachableTypes = attachAtt.Types;
                }

                info.IsUnique = type.IsDefined(typeof(UniqueAttribute), true);

                infos.Add(info);
            }

            infos = infos.OrderBy(i => i.Name).ThenBy(i => i.Category).ToList();
            return infos;
        }


        /// <summary>
        /// 查找类型的最后子类
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dictionary"></param>
        public static void GetTypeLastSubclass(Type type, Dictionary<Type, Type> dictionary)
        {
            var children = GetTypeMetaDerivedFrom(type);
            foreach (var t in children)
            {
                var iT = t.Type;
                //如果不是抽象类就更新
                if (!iT.IsAbstract)
                {
                    dictionary[type] = iT;
                }
            }
        }
    }
}
