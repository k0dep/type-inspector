using System;

namespace TypeInspector
{
    /// <summary>
    ///     Make filter of type targets for PropertyReference
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class TypeFilterAttribute : Attribute, IFilterMethodName
    {
        public string MethodName { get; set; }
        
        /// <summary>
        ///    Make filter of type targets for PropertyReference
        /// </summary>
        /// <param name="methodName"> Method name which will filter type targets </param>
        public TypeFilterAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}