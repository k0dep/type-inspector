using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TypeInspector.Editor
{
    public abstract class PropertyReferenceDrawerBase : PropertyDrawer
    {
        protected PropertyFilterAttribute GetPropertyAttributeFilter(SerializedProperty property)
        {
            return property.serializedObject.targetObject
                .GetType()
                ?.GetField(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetCustomAttribute<PropertyFilterAttribute>();
        }
        
        protected MethodsFilterAttribute GetMethodAttributeFilter(SerializedProperty property)
        {
            return property.serializedObject.targetObject
                .GetType()
                ?.GetField(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetCustomAttribute<MethodsFilterAttribute>();
        }
        
        protected IEnumerable<PropertyInfo> FilterProperties(Type targetType, SerializedProperty propertySP)
        {
            var allProperties = targetType
                .GetProperties(BindingFlags.Instance | BindingFlags.Public);

            var attribute = GetPropertyAttributeFilter(propertySP);

            if (attribute == null)
            {
                return allProperties;
            }

            var filter = ReflectionHelpers.CreateFilter<PropertyInfo, PropertyFilterAttribute>(propertySP);
            
            return allProperties.Where(filter);
        }
        
        protected IEnumerable<MethodInfo> FilterMethods(Type targetType, SerializedProperty propertySP)
        {
            var allMethods = targetType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName);;

            var attribute = GetMethodAttributeFilter(propertySP);

            if (attribute == null)
            {
                return allMethods;
            }

            var filter = ReflectionHelpers.CreateFilter<MethodInfo, MethodsFilterAttribute>(propertySP);
            
            return allMethods.Where(filter);
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
        
        protected void DrawObjectPicker(Rect padding, SerializedProperty target, SerializedProperty property)
        {
            if (target.objectReferenceValue == null || (!(target.objectReferenceValue is GameObject)
                                                        && !(target.objectReferenceValue is Component)))
            {
                DrawSimpleObjectPicker(padding, target, property);
            }
            else
            {
                DrawSelectableObjectPicker(padding, target, property);
            }
        }

        protected void DrawSelectableObjectPicker(Rect padding, SerializedProperty target, SerializedProperty property)
        {
            var objectRect = padding.DivideHorizontal(0.7f).First();
            var selectorRect = padding.DivideHorizontal(0.7f).Last();

            var value = target.objectReferenceValue;
            
            DrawSimpleObjectPicker(objectRect, target, property);


            List<(string name, Object obj)> variants = null;

            if (value is GameObject refGo)
            {
                variants = new[] {("GameObject", (Object) refGo)}
                    .Union(SelectComponents(refGo))
                    .ToList();
            }
            else if (value is Component comp)
            {
                variants = new[] {("GameObject", (Object) comp.gameObject)}
                    .Union(SelectComponents(comp.gameObject))
                    .ToList();
            }
            
            var index = variants.FindLastIndex(v => v.obj == value);
            var popupValue = EditorGUI.Popup(selectorRect, index, variants.Select(v => v.name).ToArray());
            if (index != popupValue)
            {
                target.objectReferenceValue = variants[popupValue].obj;
            }

            List<(string name, Object obj)> SelectComponents(GameObject go) => go.GetComponents<Component>()
                .Select((c, i) => ($"{i}: {c.GetType().FullName}", c as Object))
                .ToList();
        }

        protected void DrawSimpleObjectPicker(Rect padding, SerializedProperty target, SerializedProperty property)
        {
            var newTarget = EditorGUI.ObjectField(padding, property.displayName, target.objectReferenceValue,
                typeof(Object), true);

            if (newTarget != target.objectReferenceValue && FilterTarget(property, newTarget))
            {
                target.objectReferenceValue = newTarget;
            }
        }
        
        public bool FilterTarget(SerializedProperty property, Object value)
        {
            var filter = ReflectionHelpers.CreateFilter<Object, TargetFilterAttribute>(property);
            return filter(value);
        }
    }
}