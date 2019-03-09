using System;
using UnityEngine;

namespace TypeInspector.Examples
{
    public class TypeReferenceExample : MonoBehaviour
    {
        public TypeReference type;
        
        [TypeFilter(nameof(FilterTypePredicate))]
        public TypeReference typeFiltered;
        
        public bool FilterTypePredicate(Type type)
        {
            return type.FullName.StartsWith("TypeInspector");
        }

        public void Start()
        {
            Debug.Log(typeFiltered.FullName);
        }
    }
}