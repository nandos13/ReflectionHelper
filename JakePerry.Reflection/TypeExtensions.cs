using System;

namespace JakePerry.Reflection
{
    public static class TypeExtensions
    {
        /// <returns>
        /// An object representing the inheritance hierarchy of the current type.
        /// </returns>
        public static TypeHierarchy GetTypeHierarchy(this Type type)
        {
            return new TypeHierarchy(type);
        }
    }
}
