using System;
using System.Reflection;

namespace TypeInspector
{
    /// <summary>
    ///     Make reference to property in selected object
    /// </summary>
    [Serializable]
    public class PropertyReference : PropertyReferenceBase
    {
        public TypeReference TargetType;
        public string PropertyName;

        /// <summary>
        ///     Return type of selected object instance
        /// </summary>
        /// <returns> see <see cref="TypeReference.Get"/> </returns>
        public override Type GetSourceType()
        {
            return TargetType.Get();
        }

        /// <summary>
        ///     Return target property of selected source object 
        /// </summary>
        /// <returns> if all is ok, return PropertyInfo object, or return null </returns>
        public override PropertyInfo GetProperty()
        {
            if (!TargetType.IsValid() || string.IsNullOrEmpty(PropertyName))
            {
                return null;
            }

            if (_propertyCache != null)
            {
                return _propertyCache;
            }

            _propertyCache = TargetType.Get().GetProperty(PropertyName);
            
            return TargetType.Get().GetProperty(PropertyName);
        }

    }
}