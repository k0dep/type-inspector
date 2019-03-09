using System;
using UnityEngine;

namespace TypeInspector.Examples
{
    public class PropertyReferenceTypeFilter : MonoBehaviour
    {
        [TypeFilter(nameof(Filter))] public PropertyReference PropFilterNostatic;

        [TypeFilter(nameof(FilterStatic))] public PropertyReference PropFilterStatic;


        private bool Filter(Type type)
        {
            return true;
        }

        private static bool FilterStatic(Type type)
        {
            return true;
        }
    }
}