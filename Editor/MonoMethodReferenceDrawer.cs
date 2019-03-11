using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TypeInspector.Editor
{
    [CustomPropertyDrawer(typeof(MonoMethodReference))]
    public class MonoMethodReferenceDrawer : PropertyReferenceDrawerBase
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var targetSP = property.FindPropertyRelative("Target");
            var propertyNameSP = property.FindPropertyRelative("MethodName");

            position.height = GetPropertyHeight(targetSP, new GUIContent(""));
            EditorGUI.BeginProperty(position, label, property);

            DrawObjectPicker(
                position.DivideVertical().First().Padding(0, 0, 2, 0),
                targetSP, property);

            if (targetSP.objectReferenceValue != null)
            {
                DrawMethodSelector(
                    position.DivideVertical().Last().Padding(0, 0, 2, 5), 
                    propertyNameSP, property, targetSP.objectReferenceValue.GetType());
            }

            EditorGUI.EndProperty();
        }

        private void DrawMethodSelector(Rect objectPos, SerializedProperty propertyNameSP, SerializedProperty root, Type targetType)
        {
            var methods = FilterMethods(targetType, root);

            if(!methods.Any())
            {
                propertyNameSP.stringValue = "";
                GUI.Label(objectPos, "Selected type not contain any public methods");
                return;
            }
            
            var selected = string.IsNullOrEmpty(propertyNameSP.stringValue)
                ? 0
                : methods.Select(p => p.Name).ToList().IndexOf(propertyNameSP.stringValue);
            
            var methodNames = methods.Select(GetMethodName).ToArray();
            
            var index = EditorGUI.Popup(objectPos, "Method", selected, methodNames);
            
            propertyNameSP.stringValue = methods.ElementAt(Mathf.Max(index, 0)).Name;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var targetSP = property.FindPropertyRelative("Target");
            
            if (targetSP != null && targetSP.objectReferenceValue == null)
            {
                return base.GetPropertyHeight(property, label);
            }
            
            return base.GetPropertyHeight(property, label) * 2 + 4;
        }

        public string GetMethodName(MethodInfo method)
        {
            return $"{method.Name}({ParamString(method.GetParameters())}): {method.ReturnType.FullName}";

            string ParamString(ParameterInfo[] param)
            {
                if (param.Length == 0)
                {
                    return "";
                }

                return param.Select(s => $"{s.Name}: {s.ParameterType.Name}")
                    .Aggregate((acc, seed) => acc += $", {seed}");
            }
        }
    }
}