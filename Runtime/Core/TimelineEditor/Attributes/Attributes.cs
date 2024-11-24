using System;
using UnityEngine;

namespace MMDarkness
{
    /// <summary>
    ///     类排序
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class OrderAttribute : Attribute
    {
        public int Order;

        public OrderAttribute(int order)
        {
            Order = order;
        }
    }

    /// <summary>
    ///     菜单自定义名称
    /// </summary>
    [AttributeUsage(AttributeTargets.All, Inherited = false)]
    public sealed class MenuNameAttribute : Attribute
    {
        public string ShowName;

        public MenuNameAttribute(string name)
        {
            ShowName = name;
        }
    }

    /// <summary>
    ///     关联某个类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class OptionParamAttribute : Attribute
    {
        public Type ClassType;

        public OptionParamAttribute(Type type)
        {
            ClassType = type;
        }
    }

    /// <summary>
    ///     选项排序
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class OptionSortAttribute : Attribute
    {
        public int Sort;

        public OptionSortAttribute(int sort)
        {
            Sort = sort;
        }
    }

    /// <summary>
    ///     关联某个字段的某个值
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class OptionRelateParamAttribute : Attribute
    {
        public string ArgsName;
        public object[] ArgsValue;

        public OptionRelateParamAttribute(string name, params object[] values)
        {
            ArgsName = name;
            ArgsValue = values;
        }
    }

    /// <summary>
    ///     选择对象路径
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class SelectObjectPathAttribute : Attribute
    {
        public Type Type;

        public SelectObjectPathAttribute(Type type)
        {
            Type = type;
        }
    }

    /// <summary>
    ///     自定义检视面板
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomInspectors : Attribute
    {
        public bool EditorForChildClasses;
        public Type InspectedType;

        public CustomInspectors(Type inspectedType)
        {
            InspectedType = inspectedType;
        }

        public CustomInspectors(Type inspectedType, bool editorForChildClasses)
        {
            InspectedType = inspectedType;
            EditorForChildClasses = editorForChildClasses;
        }
    }

    /// <summary>
    ///     自定义名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NameAttribute : Attribute
    {
        public readonly string Name;

        public NameAttribute(string name)
        {
            Name = name;
        }
    }


    /// <summary>
    ///     指定类别
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CategoryAttribute : Attribute
    {
        public readonly string Category;

        public CategoryAttribute(string category)
        {
            Category = category;
        }
    }

    /// <summary>
    ///     指定描述
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class DescriptionAttribute : Attribute
    {
        public readonly string Description;

        public DescriptionAttribute(string description)
        {
            Description = description;
        }
    }

    /// <summary>
    ///     指定类型的图标
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ShowIconAttribute : Attribute
    {
        public readonly Type FromType;
        public readonly string IconPath;
        public readonly Texture Texture;

        public ShowIconAttribute(Texture texture)
        {
            Texture = texture;
        }

        public ShowIconAttribute(string iconPath)
        {
            IconPath = iconPath;
        }

        public ShowIconAttribute(Type fromType)
        {
            FromType = fromType;
        }
    }

    /// <summary>
    ///     指定显示的颜色
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ColorAttribute : Attribute
    {
        public readonly Color Color;

        public ColorAttribute(float r, float g, float b, float a = 1)
        {
            Color = new Color(r, g, b, a);
        }

        public ColorAttribute(Color color)
        {
            Color = color;
        }
    }

    /// <summary>
    ///     指定附加类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AttachableAttribute : Attribute
    {
        public readonly Type[] Types;

        public AttachableAttribute(params Type[] types)
        {
            Types = types;
        }
    }

    /// <summary>
    ///     组内唯一性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class UniqueAttribute : Attribute
    {
    }

    /// <summary>
    ///     自定义片段预览
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomPreviewAttribute : Attribute
    {
        public Type PreviewType;

        public CustomPreviewAttribute(Type type)
        {
            PreviewType = type;
        }
    }

    //属性用于对象或字符串字段，如果未设置，则将其标记为必需(红色)
    [AttributeUsage(AttributeTargets.Field)]
    public class RequiredAttribute : PropertyAttribute
    {
    }
}