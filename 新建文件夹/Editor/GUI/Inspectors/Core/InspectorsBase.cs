using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MMDarkness.Editor
{
    public class InspectorsBase
    {
        private PropertyTree m_propertyTree;
        protected object m_target;

        public void SetTarget(object t)
        {
            m_target = t;
            m_propertyTree = null;
        }

        public virtual void OnInspectorGUI()
        {
            m_propertyTree ??= PropertyTree.Create(m_target);
            m_propertyTree.Draw(false);
        }

        public void DrawDefaultInspector(Type type)
        {
            var t = type;
            //得到字段的值,只能得到public类型的字典的值
            FieldInfo[] fieldInfos = t.GetFields();
            //排序一下，子类的字段在后，父类的在前
            Array.Sort(fieldInfos, FieldsSprtBy);

            //判断需要过滤不显示的字段
            List<FieldInfo> needShowField = new List<FieldInfo>();
            foreach (var field in fieldInfos)
            {
                var need = true;
                var attributes = field.GetCustomAttributes();
                foreach (var attribute in attributes)
                {
                    if (attribute is OptionRelateParamAttribute option)
                    {
                        var relate = Array.Find(fieldInfos, f1 => f1.Name == option.ArgsName);
                        if (relate != null)
                        {
                            var value = relate.GetValue(m_target);
                            var index = Array.FindIndex(option.ArgsValue, v1 => v1.Equals(value));
                            if (index < 0)
                            {
                                need = false;
                                break;
                            }
                        }
                    }
                }

                if (need)
                {
                    needShowField.Add(field);
                }
            }

            foreach (var field in needShowField)
            {
                FieldDefaultInspector(field, m_target);
            }
        }

        protected void FieldDefaultInspector(FieldInfo field, object target)
        {
            var fieldType = field.FieldType;
            var showType = field.FieldType;
            var value = field.GetValue(target);
            var newValue = value;

            //首字母大写
            var name = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(field.Name);

            List<object> args = new List<object>();

            var attributes = field.GetCustomAttributes();
            foreach (var attribute in attributes)
            {
                var t = attribute.GetType();
                if (attribute is MenuNameAttribute menuNameAttribute)
                {
                    name = menuNameAttribute.ShowName;
                }
                else if (attribute is RangeAttribute rangeAttribute && fieldType == typeof(float))
                {
                    showType = t;
                    args = new List<object> { rangeAttribute.min, rangeAttribute.max };
                }
                else if (attribute is OptionParamAttribute option)
                {
                    showType = t;
                    args = new List<object> { option.ClassType };
                }
                else if (attribute is SelectObjectPathAttribute selectObjectPathAttribute)
                {
                    showType = t;
                    args = new List<object> { selectObjectPathAttribute.Type };
                }
            }

            if (showType == typeof(int))
            {
                newValue = EditorGUILayout.IntField(name, (int)value);
            }
            else if (showType == typeof(float))
            {
                newValue = EditorGUILayout.FloatField(name, (float)value);
            }
            else if (showType == typeof(bool))
            {
                newValue = EditorGUILayout.Toggle(name, (bool)value);
            }
            else if (showType == typeof(string))
            {
                newValue = EditorGUILayout.TextField(name, (string)value);
            }
            else if (showType == typeof(Color))
            {
                newValue = EditorGUILayout.ColorField(name, (Color)value);
            }
            else if (showType.IsSubclassOf(typeof(Object)))
            {
                newValue = EditorGUILayout.ObjectField(name, (Object)value, fieldType, false);
            }
            else if (showType.IsEnum)
            {
                newValue = EditorGUILayout.EnumPopup(name, (Enum)value);
            }
            else if (showType == typeof(AnimationCurve))
            {
                AnimationCurve curve = field.GetValue(target) as AnimationCurve;
                if (curve == null)
                {
                    curve = new AnimationCurve();
                }

                newValue = EditorGUILayout.CurveField(name, curve);
            }
            else if (showType == typeof(Vector2))
            {
                newValue = EditorGUILayout.Vector2Field(name, (Vector2)value);
            }
            else if (showType == typeof(Vector3))
            {
                newValue = EditorGUILayout.Vector3Field(name, (Vector3)value);
            }
            else if (showType == typeof(Vector4))
            {
                newValue = EditorGUILayout.Vector4Field(name, (Vector4)value);
            }
            else if (showType == typeof(Vector2Int))
            {
                newValue = EditorGUILayout.Vector2IntField(name, (Vector2Int)value);
            }
            else if (showType == typeof(Vector3Int))
            {
                newValue = EditorGUILayout.Vector3IntField(name, (Vector3Int)value);
            }
            else if (showType == typeof(Rect))
            {
                newValue = EditorGUILayout.RectField(name, (Rect)value);
            }
            else if (showType == typeof(RectInt))
            {
                newValue = EditorGUILayout.RectIntField(name, (RectInt)value);
            }
            else if (showType == typeof(Bounds))
            {
                newValue = EditorGUILayout.BoundsField(name, (Bounds)value);
            }
            else if (showType == typeof(RangeAttribute))
            {
                if (fieldType == typeof(float))
                {
                    newValue = EditorGUILayout.Slider(name, (float)value, (float)args[0], (float)args[1]);
                }
            }
            else if (showType == typeof(OptionParamAttribute))
            {
                // LookAtType.None
                var t = (Type)args[0];
                var fields = t.GetFields();
                //先对fields排序
                Array.Sort(fields, FieldsSort);

                var title = new List<string>();
                var mask = 0;
                foreach (var f in fields)
                {
                    var menuNameAttr = f.GetCustomAttribute<MenuNameAttribute>();
                    title.Add(menuNameAttr != null ? menuNameAttr.ShowName : f.Name);
                }

                mask = (int)value;

                newValue = EditorGUILayout.Popup(name, mask, title.ToArray());
            }
            else if (showType == typeof(SelectObjectPathAttribute))
            {
                if (args[0] is Type type)
                {
                    Object obj = null;
                    var path = value.ToString();
                    if (!string.IsNullOrEmpty(path))
                    {
                        obj = AssetDatabase.LoadAssetAtPath(path, type);
                    }

                    var newObj = EditorGUILayout.ObjectField(name, obj, type, false);
                    if (newObj != obj)
                    {
                        newValue = AssetDatabase.GetAssetPath(newObj);
                    }
                }
            }
            else if (showType == typeof(Clip))
            {
                m_propertyTree ??= PropertyTree.Create(value);
                m_propertyTree.Draw(false);
            }

            if (value != newValue)
                field.SetValue(target, newValue);
        }

        private int FieldsSprtBy(FieldInfo f1, FieldInfo f2)
        {
            if (f1 == null || f2 == null) return 0;
            //相同则是自身的字段，不相同则是继承的
            var e1 = f1.DeclaringType == f1.ReflectedType;
            var e2 = f2.DeclaringType == f2.ReflectedType;
            if (e1 != e2)
            {
                if (e1)
                {
                    return 1;
                }

                return -1;
            }

            return 0;
        }

        private int FieldsSort(FieldInfo f1, FieldInfo f2)
        {
            var sort1 = f1.GetCustomAttribute<OptionSortAttribute>();
            var sort2 = f2.GetCustomAttribute<OptionSortAttribute>();
            var i1 = 99;
            var i2 = 99;
            if (sort1 != null)
            {
                i1 = sort1.Sort;
            }

            if (sort2 != null)
            {
                i2 = sort2.Sort;
            }

            return i1 - i2;
        }
    }
}