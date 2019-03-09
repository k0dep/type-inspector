using System.Reflection;
using UnityEngine;

namespace TypeInspector.Examples
{
    public class PropertyReferencePropertyFilter : MonoBehaviour
    {
        [PropertyFilter(nameof(Filter))] public PropertyReference PropFilterNostatic;

        [PropertyFilter(nameof(FilterStatic))] public PropertyReference PropFilterStatic;

        [PropertyFilter(nameof(FilterFalse))] public PropertyReference PropFilterNoneProp;

        private bool Filter(PropertyInfo property)
        {
            return true;
        }

        private bool FilterFalse(PropertyInfo property)
        {
            return false;
        }

        private static bool FilterStatic(PropertyInfo type)
        {
            return true;
        }
    }
}