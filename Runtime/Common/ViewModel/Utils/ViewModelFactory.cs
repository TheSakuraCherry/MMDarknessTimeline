using System;
using System.Collections.Generic;
using System.Reflection;

namespace MMDarkness
{
    public static class ViewModelFactory
    {
        private static bool s_Initialized;
        private static Dictionary<Type, Type> s_ViewModelTypeCache;

        static ViewModelFactory()
        {
            Init(true);
        }

        public static void Init(bool force)
        {
            if (!force && s_Initialized)
                return;

            if (s_ViewModelTypeCache == null)
                s_ViewModelTypeCache = new Dictionary<Type, Type>();
            else
                s_ViewModelTypeCache.Clear();

            foreach (var type in Util_TypeCache.GetTypesWithAttribute<ViewModelAttribute>())
            {
                if (type.IsAbstract)
                    continue;
                if (type.IsGenericType)
                    continue;

                var attribute = type.GetCustomAttribute<ViewModelAttribute>(true);
                s_ViewModelTypeCache[attribute.modelType] = type;
            }

            s_Initialized = true;
        }

        public static Type GetViewModelType(Type modelType)
        {
            if (modelType == null) return null;

            if (!s_ViewModelTypeCache.TryGetValue(modelType, out var viewModelType))
            {
                var sourceModelType = modelType;
                do
                {
                    modelType = modelType.BaseType;
                } while (modelType != null && !s_ViewModelTypeCache.TryGetValue(modelType, out viewModelType));

                s_ViewModelTypeCache.Add(sourceModelType, viewModelType);
            }

            return viewModelType;
        }

        public static object CreateViewModel(object model)
        {
            var modelType = model.GetType();
            var viewModelType = GetViewModelType(modelType);
            if (viewModelType == null)
                return null;

            return Activator.CreateInstance(viewModelType, model);
        }
    }
}