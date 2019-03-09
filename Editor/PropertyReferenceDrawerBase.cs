using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TypeInspector.Editor
{
    public abstract class PropertyReferenceDrawerBase : PropertyDrawer
    {
        protected PropertyFilterAttribute GetAttributeFilter(SerializedProperty property)
        {
            return property.serializedObject.targetObject
                .GetType()
                ?.GetField(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetCustomAttribute<PropertyFilterAttribute>();
        }
        
        protected IEnumerable<PropertyInfo> FilterProperties(Type targetType, SerializedProperty propertySP)
        {
            var allProperties = targetType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var attribute = GetAttributeFilter(propertySP);

            if (attribute == null)
            {
                return allProperties;
            }

            var filter = ReflectionHelpers.CreateFilter<PropertyInfo, PropertyFilterAttribute>(propertySP);
            
            return allProperties.Where(filter);
        }

        public static object GetTargetObjectOfProperty(SerializedProperty prop)
        {
            var path = prop.propertyPath.Replace(".Array.data[", "[");
            object obj = prop.serializedObject.targetObject;
            var elements = path.Split('.');
            foreach (var element in elements)
            {
                if (element.Contains("["))
                {
                    var elementName = element.Substring(0, element.IndexOf("["));
                    var index = System.Convert.ToInt32(element.Substring(element.IndexOf("[")).Replace("[", "").Replace("]", ""));
                    obj = GetValue_Imp(obj, elementName, index);
                }
                else
                {
                    obj = GetValue_Imp(obj, element);
                }
            }
            return obj;
        }
        
        private static object GetValue_Imp(object source, string name)
        {
            if (source == null)
                return null;
            var type = source.GetType();

            while (type != null)
            {
                var f = type.GetField(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (f != null)
                    return f.GetValue(source);

                var p = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p != null)
                    return p.GetValue(source, null);

                type = type.BaseType;
            }
            return null;
        }

        private static object GetValue_Imp(object source, string name, int index)
        {
            var enumerable = GetValue_Imp(source, name) as System.Collections.IEnumerable;
            if (enumerable == null) return null;
            var enm = enumerable.GetEnumerator();

            for (int i = 0; i <= index; i++)
            {
                if (!enm.MoveNext()) return null;
            }
            return enm.Current;
        }

        protected void DrawPropertySelector(Rect objectPos, SerializedProperty propertyNameSP, SerializedProperty root, Type targetType)
        {
            var properties = FilterProperties(targetType, root);

            if(!properties.Any())
            {
                propertyNameSP.stringValue = "";
                GUI.Label(objectPos, "Selected type not contain any public property");
                return;
            }
            
            var selected = string.IsNullOrEmpty(propertyNameSP.stringValue)
                ? 0
                : properties.Select(p => p.Name).ToList().IndexOf(propertyNameSP.stringValue);
            
            var index = EditorGUI.Popup(objectPos, "Property", selected,
                properties.Select(p => p.Name + " : " + p.PropertyType.FullName).ToArray());
            
            propertyNameSP.stringValue = properties.ElementAt(Mathf.Max(index, 0)).Name;
        }
    }
}