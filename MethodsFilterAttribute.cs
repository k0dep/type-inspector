using System;

namespace TypeInspector
{
    /// <summary>
    ///     Make filter of methods for selected target type
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class MethodsFilterAttribute : Attribute, IFilterMethodName
    {
        public string MethodName { get; set; }
        
        public MethodsFilterAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}