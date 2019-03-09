using System;
using System.Reflection;

namespace TypeInspector
{
    /// <summary>
    ///     Make property reference for specific MonoBehaviour target object
    /// </summary>
    [Serializable]
    public class MonoPropertyReference : PropertyReferenceBase
    {
        public UnityEngine.Object Target;
        public string PropertyName;


        public void Set(object data)
        {
            Set(Target, data);
        }

        public object Get()
        {
            return Get(Target);
        }

        public override Type GetSourceType()
        {
            return Target.GetType();
        }

        public override PropertyInfo GetProperty()
        {
            if (Target == null || string.IsNullOrEmpty(PropertyName))
            {
                return null;
            }
            
            if (_propertyCache != null)
            {
                return _propertyCache;
            }

            _propertyCache = Target.GetType().GetProperty(PropertyName);

            return _propertyCache;
        }
    }
}