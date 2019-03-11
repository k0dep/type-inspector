using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TypeInspector.Editor
{
    [CustomPropertyDrawer(typeof(MonoPropertyReference))]
    public class MonoPropertyReferenceDrawer : PropertyReferenceDrawerBase
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var targetSP = property.FindPropertyRelative("Target");
            var propertyNameSP = property.FindPropertyRelative("PropertyName");

            position.height = GetPropertyHeight(targetSP, new GUIContent(""));
            EditorGUI.BeginProperty(position, label, property);

            DrawObjectPicker(
                position.DivideVertical().First().Padding(0, 0, 2, 0),
                targetSP, property);

            if (targetSP.objectReferenceValue != null)
            {
                DrawPropertySelector(
                    position.DivideVertical().Last().Padding(0, 0, 2, 5), 
                    propertyNameSP, property, targetSP.objectReferenceValue.GetType());
            }

            EditorGUI.EndProperty();
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
    }
}