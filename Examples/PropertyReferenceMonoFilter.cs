using UnityEngine;

namespace TypeInspector.Examples
{
    public class PropertyReferenceMonoFilter : MonoBehaviour
    {
        [TargetFilter(nameof(Filter))]
        public MonoPropertyReference P;
        
        [TargetFilter(nameof(FilterStatic))]
        public MonoPropertyReference PStatic;

        public bool Filter(Object o)
        {
            return true;
        }

        public static bool FilterStatic(Object o)
        {
            return false;
        }
    }
}