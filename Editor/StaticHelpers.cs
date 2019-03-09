using System;
using System.Reflection;
using UnityEditor;

namespace TypeInspector.Editor
{
    public static class ReflectionHelpers
    {
        public static Func<T, bool> CreateFilter<T,TAttr>(SerializedProperty filterObjectProperty)
            where TAttr : Attribute, IFilterMethodName
        {
            var targetType = filterObjectProperty.serializedObject.targetObject.GetType();
            var targetField = targetType.GetField(filterObjectProperty.name, BindingFlags.Instance
                                                                             | BindingFlags.Public
                                                                             | BindingFlags.NonPublic);
            
            if (targetField == null)
            {
                return t => true;
            }
            
            var attribute = targetField.GetCustomAttribute<TAttr>();
            
            if (attribute == null)
            {
                return t => true;
            }

            var method = targetType.GetMethod(attribute.MethodName, BindingFlags.NonPublic
                                                                    | BindingFlags.Public
                                                                    | BindingFlags.Instance
                                                                    | BindingFlags.Static);

            if (method == null)
            {
                return t => true;
            }
            
            Func<T, bool> filterDelegate = null;
            
            if (method.IsStatic)
            {
                filterDelegate = (Func<T, bool>) Delegate.CreateDelegate(typeof(Func<T, bool>), method);
            }
            else
            {
                filterDelegate = (Func<T, bool>) Delegate.CreateDelegate(typeof(Func<T, bool>), filterObjectProperty.serializedObject.targetObject, method);
            }

            return filterDelegate;
        }
    }
}