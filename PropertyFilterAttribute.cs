using System;

namespace TypeInspector
{
    /// <summary>
    ///     Make filter of properties for selectes target type
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PropertyFilterAttribute : Attribute, IFilterMethodName
    {
        public string MethodName { get; set; }
        
        public PropertyFilterAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}