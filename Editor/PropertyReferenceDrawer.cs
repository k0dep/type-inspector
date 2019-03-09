using UnityEditor;
using UnityEngine;

namespace TypeInspector.Editor
{
    [CustomPropertyDrawer(typeof(PropertyReference))]
    public class PropertyReferenceDrawer : PropertyReferenceDrawerBase
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var typeProperty = property.FindPropertyRelative("TargetType");
            var propertyNameSP = property.FindPropertyRelative("PropertyName");

            var propertyRect = position;
            propertyRect.height = GetPropertyHeight(property, label);
            EditorGUI.BeginProperty(propertyRect, label, property);

            position.height = base.GetPropertyHeight(typeProperty, new GUIContent(""));
            
            var objectPos = position;
            
            TypeReferenceDrawer.FilterObjectProperty = property;
            
            EditorGUI.PropertyField(position, typeProperty);
            
            TypeReferenceDrawer.FilterObjectProperty = null;

            objectPos.y += objectPos.height + 2;
            objectPos.x += 5;
            objectPos.width -= 5;

            var typeAccessor = (TypeReference) GetTargetObjectOfProperty(typeProperty);

            if (typeAccessor.IsValid())
            {
                DrawPropertySelector(objectPos, propertyNameSP, property, typeAccessor.Get());
            }
            
            typeAccessor.ClearCache();

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var typeAccessor = (TypeReference) GetTargetObjectOfProperty(property.FindPropertyRelative("TargetType"));

            if (typeAccessor != null && typeAccessor.IsValid())
            {
                return base.GetPropertyHeight(property, label) * 2 + 4;
            }
            
            return base.GetPropertyHeight(property, label);
        }
    }
}