using System;
using System.Collections.Generic;
using System.Linq;

namespace MMDarkness.Editor
{
    public static class EditorInspectorFactory
    {
        private static bool m_initDic;

        private static readonly Dictionary<Type, Type> s_inspectorsDic = new();

        public static InspectorsBase GetInspector(IData directable)
        {
            InitDic();
            var type = directable.GetType();
            InspectorsBase b = null;
            if (s_inspectorsDic.TryGetValue(type, out var t)) b = Activator.CreateInstance(t) as InspectorsBase;
            b ??= new InspectorsBase();

            b.SetTarget(directable);
            return b;
        }

        public static void InitDic()
        {
            if (m_initDic) return;
            m_initDic = true;
            var type = typeof(InspectorsBase);
            GetTypeLastSubclass(type, s_inspectorsDic);
            GetNotFindType(typeof(IData), s_inspectorsDic);
        }

        public static void GetTypeLastSubclass(Type type, Dictionary<Type, Type> dictionary)
        {
            var children = EditorTools.GetTypeMetaDerivedFrom(type);
            foreach (var t in children)
            {
                var arrs = t.Type.GetCustomAttributes(typeof(CustomInspectors), true);
                foreach (var arr in arrs)
                    if (arr is CustomInspectors c)
                    {
                        var bindT = c.InspectedType;
                        var iT = t.Type;
                        if (!dictionary.ContainsKey(bindT))
                        {
                            if (!iT.IsAbstract) dictionary[bindT] = iT;
                        }
                        else
                        {
                            var old = dictionary[bindT];
                            //如果不是抽象类，且是子类就更新
                            if (!iT.IsAbstract && iT.IsSubclassOf(old)) dictionary[bindT] = iT;
                        }
                    }
            }
        }

        /// <summary>
        ///     找出没有映射关系的对象，并绑定其最近父类的面板对象
        /// </summary>
        /// <param name="type"></param>
        /// <param name="dictionary"></param>
        public static void GetNotFindType(Type type, Dictionary<Type, Type> dictionary)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(type))).ToArray();
            foreach (var t in types)
            {
                if (t.IsAbstract) continue;
                var iT = TryAddDic(t, dictionary);
                if (iT != null) dictionary[t] = iT;
            }
        }

        private static Type TryAddDic(Type type, Dictionary<Type, Type> dictionary)
        {
            if (type != null && !dictionary.ContainsKey(type)) return TryAddDic(type.BaseType, dictionary);

            if (type != null && dictionary.TryGetValue(type, out var add)) return add;

            return null;
        }
    }
}