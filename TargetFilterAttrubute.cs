using System;

namespace TypeInspector
{
    /// <summary>
    ///     Make filter of targets for PropertyReference
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class TargetFilterAttribute : Attribute, IFilterMethodName
    {
        public string MethodName { get; set; }

        
        /// <summary>
        ///    Make filter of targets for PropertyReference
        /// </summary>
        /// <param name="methodName"> Method name which will filter targets </param>
        public TargetFilterAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}