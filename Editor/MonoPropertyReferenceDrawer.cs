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

        private void DrawObjectPicker(Rect padding, SerializedProperty target, SerializedProperty property)
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

        private void DrawSelectableObjectPicker(Rect padding, SerializedProperty target, SerializedProperty property)
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

        private void DrawSimpleObjectPicker(Rect padding, SerializedProperty target, SerializedProperty property)
        {
            var newTarget = EditorGUI.ObjectField(padding, property.displayName, target.objectReferenceValue,
                typeof(Object), true);

            if (newTarget != target.objectReferenceValue && FilterTarget(property, newTarget))
            {
                target.objectReferenceValue = newTarget;
            }
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

        public bool FilterTarget(SerializedProperty property, Object value)
        {
            var filter = ReflectionHelpers.CreateFilter<Object, TargetFilterAttribute>(property);
            return filter(value);
        }
    }
}