using System;
using System.Reflection;

namespace TypeInspector
{
    /// <summary>
    ///     Make ditable reference to method in selected object
    ///    Support filters: <see cref="TargetFilterAttribute"/> and <see cref="MethodsFilterAttribute"/>
    /// </summary>
    [Serializable]
    public class MonoMethodReference
    {
        public UnityEngine.Object Target;
        public string MethodName;

        public MethodInfo GetMethod()
        {
            if (Target == null)
            {
                return null;
            }

            if (string.IsNullOrEmpty(MethodName))
            {
                return null;
            }

            var method = Target.GetType().GetMethod(MethodName);

            return method;
        }
    }
}