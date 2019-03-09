
using System;

namespace TypeInspector
{
    /// <summary>
    ///     Make reference for type and create editor interface
    /// </summary>
    [Serializable]
    public class TypeReference
    {
        public string FullName;

        private Type _typeCache;

        /// <summary>
        ///     Get selected type
        /// </summary>
        /// <returns> if all is ok, returns Type object, or return null </returns>
        public Type Get()
        {
            if (_typeCache == null && !IsValid())
            {
                return null;
            }
            
            if(_typeCache != null)
            {
                return _typeCache;
            }

            _typeCache = Type.GetType(FullName);

            return _typeCache;
        }

        public bool IsValid() => !string.IsNullOrEmpty(FullName) && Type.GetType(FullName) != null;

        public void ClearCache()
        {
            _typeCache = null;
        }
    }
}