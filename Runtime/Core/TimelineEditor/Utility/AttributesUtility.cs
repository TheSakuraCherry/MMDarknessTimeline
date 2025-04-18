﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace MMDarkness
{
    public static class AttributesUtility
    {
        private static readonly Dictionary<Type, Dictionary<int, string>> CacheMenuName = new();

        public static string GetMenuName(int index, Type type)
        {
            // type.GetCustomAttributes();
            if (type != null)
            {
                if (CacheMenuName.TryGetValue(type, out var dictionary))
                {
                    if (dictionary.TryGetValue(index, out var name)) return name;
                }
                else
                {
                    CacheMenuName[type] = new Dictionary<int, string>();
                }

                var fieldInfos = type.GetFields();
                foreach (var field in fieldInfos)
                {
                    var fieldType = field.FieldType;
                    var value = field.GetValue(type);
                    if (value is int i && i == index)
                    {
                        var attributes = field.GetCustomAttributes();
                        foreach (var attribute in attributes)
                        {
                            var t = attribute.GetType();
                            if (attribute is MenuNameAttribute menuNameAttribute)
                            {
                                CacheMenuName[type][index] = menuNameAttribute.ShowName;
                                return menuNameAttribute.ShowName;
                            }
                        }
                    }
                }
            }

            return index.ToString();
        }
    }
}