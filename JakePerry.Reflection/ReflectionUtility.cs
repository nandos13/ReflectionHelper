using System;
using System.Collections.Generic;
using System.Reflection;

namespace JakePerry.Reflection
{
    public static class ReflectionUtility
    {
        /// <inheritdoc cref="AppDomain.CurrentDomain"/>
        /// <remarks>
        /// Shortcut for the <see cref="AppDomain.CurrentDomain"/> property.
        /// </remarks>
        public static AppDomain CurrentDomain => AppDomain.CurrentDomain;

        /// <inheritdoc cref="AppDomain.GetAssemblies"/>
        /// <remarks>
        /// Shortcut method which invokes <see cref="AppDomain.GetAssemblies"/> on the current app domain.
        /// </remarks>
        public static Assembly[] GetAssemblies() => CurrentDomain.GetAssemblies();

        /// <inheritdoc cref="AppDomain.ReflectionOnlyGetAssemblies"/>
        /// <remarks>
        /// Shortcut method which invokes <see cref="AppDomain.ReflectionOnlyGetAssemblies"/> on the current app domain.
        /// </remarks>
        public static Assembly[] ReflectionOnlyGetAssemblies() => CurrentDomain.ReflectionOnlyGetAssemblies();

        /// <summary>
        /// Enumerate all types from all assemblies in the current <see cref="AppDomain"/>.
        /// </summary>
        /// <returns>
        /// An enumeration that represents all types from all assemblies in the current <see cref="AppDomain"/>.
        /// </returns>
        public static IEnumerable<Type> EnumerateAllTypes()
        {
            foreach (var assembly in GetAssemblies())
                foreach (var type in assembly.GetTypes())
                    yield return type;
        }

        /// <summary>
        /// Get all types from all assemblies in the current <see cref="AppDomain"/>.
        /// </summary>
        /// <returns>
        /// An array of all types from all assemblies in the current <see cref="AppDomain"/>.
        /// </returns>
        public static Type[] GetAllTypes()
        {
            var assemblies = GetAssemblies();
            int assemblyCount = assemblies.Length;

            var assemblyTypes = new Type[assemblyCount][];

            int totalTypeCount = 0;
            for (int i = 0; i < assemblyCount; i++)
            {
                var types = assemblyTypes[i] = assemblies[i].GetTypes();
                totalTypeCount += types.Length;
            }

            var allTypes = new Type[totalTypeCount];

            for (int i = 0; i < assemblyCount; i++)
            {
                var source = assemblyTypes[i];
                Array.Copy(source, 0, allTypes, 0, source.Length);
            }

            return allTypes;
        }

        /// <summary>
        /// Searches for members of a given type using the specified binding and member flags, as well
        /// as an optional filter predicate.
        /// </summary>
        /// <param name="type">The type to search for members.</param>
        /// <param name="bindingFlags">The <see cref="BindingFlags"/> to search with.</param>
        /// <param name="memberFlags">The <see cref="MemberFlags"/> to search with.</param>
        /// <param name="memberFilter">An optional predicate that can filter members against a custom criteria.</param>
        /// <returns>A collection of members defined for the given type.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="memberFlags"/> is not a valid value.</exception>
        public static MemberInfo[] GetMembers(Type type, BindingFlags bindingFlags, MemberFlags memberFlags, Predicate<MemberInfo> memberFilter)
        {
            _ = type ?? throw new ArgumentNullException(nameof(type));

            if (memberFlags.HasNoFlags())
                throw new ArgumentException(nameof(memberFlags));

            if (memberFlags.HasAllMemberFlags())
            {
                var typeMembers = type.GetMembers(bindingFlags);

                if (memberFilter is null)
                    return typeMembers;

                var members = new List<MemberInfo>(typeMembers);

                foreach (var member in typeMembers)
                    if (memberFilter.Invoke(member))
                        members.Add(member);

                return members.ToArray();
            }
            else
            {
                MemberInfo[] fields = memberFlags.HasFieldFlag()
                    ? type.GetFields(bindingFlags)
                    : Array.Empty<MemberInfo>();

                MemberInfo[] properties = memberFlags.HasPropertyFlag()
                    ? type.GetProperties(bindingFlags)
                    : Array.Empty<MemberInfo>();

                MemberInfo[] methods = memberFlags.HasMethodFlag()
                    ? type.GetMethods(bindingFlags)
                    : Array.Empty<MemberInfo>();

                int length = fields.Length + properties.Length + methods.Length;

                if (length == 0)
                    return Array.Empty<MemberInfo>();

                var members = new List<MemberInfo>(length);

                if (memberFilter is null)
                {
                    members.AddRange(fields);
                    members.AddRange(properties);
                    members.AddRange(methods);
                }
                else
                {
                    foreach (var member in fields) if (memberFilter.Invoke(member)) members.Add(member);
                    foreach (var member in properties) if (memberFilter.Invoke(member)) members.Add(member);
                    foreach (var member in methods) if (memberFilter.Invoke(member)) members.Add(member);
                }

                if (members.Count == 0)
                    return Array.Empty<MemberInfo>();

                return members.ToArray();
            }
        }

        /// <inheritdoc cref="GetMembers(Type, BindingFlags, MemberFlags, Predicate{MemberInfo})"/>
        public static MemberInfo[] GetMembers(Type type, BindingFlags bindingFlags, Predicate<MemberInfo> memberFilter)
        {
            return GetMembers(type, bindingFlags, MemberFlags.Field | MemberFlags.Property | MemberFlags.Method, memberFilter);
        }

        /// <inheritdoc cref="GetMembers(Type, BindingFlags, MemberFlags, Predicate{MemberInfo})"/>
        public static MemberInfo[] GetMembers(Type type, BindingFlags bindingFlags, MemberFlags memberFlags)
        {
            return GetMembers(type, bindingFlags, memberFlags, null);
        }

        /// <summary>
        /// Checks if a type <paramref name="c"/> implements one or more interfaces
        /// constructed from a generic type definition.
        /// </summary>
        /// <param name="c">
        /// The type to compare to the generic type definition.
        /// </param>
        /// <param name="genericTypeDefinition">
        /// The generic type definition to be checked for in type <paramref name="c"/>'s implemented interfaces.
        /// </param>
        /// <remarks>Private method only, parameters are not validated.</remarks>
        private static bool CheckTypeImplementsInterfaceOfGenericDefinition(Type c, Type genericTypeDefinition)
        {
            foreach (var @interface in c.GetInterfaces())
                if (@interface.IsGenericType && @interface.GetGenericTypeDefinition() == genericTypeDefinition)
                    return true;

            return false;
        }

        /// <summary>
        /// Checks if a type <paramref name="c"/> is derived from a class that
        /// is constructed from a generic type definition.
        /// </summary>
        /// <param name="c">
        /// The type to compare to the generic type definition.
        /// </param>
        /// <param name="genericTypeDefinition">
        /// The generic type definition to be checked for in type <paramref name="c"/>'s inheritance hierarchy.
        /// </param>
        /// <remarks>Private method only, parameters are not validated.</remarks>
        private static bool CheckTypeDerivesFromClassOfGenericDefinition(Type c, Type genericTypeDefinition)
        {
            while (c != null)
            {
                if (c.IsGenericType && c.GetGenericTypeDefinition() == genericTypeDefinition)
                    return true;

                c = c.BaseType;
            }

            return false;
        }

        /// <remarks></remarks>
        /// <inheritdoc cref="CheckTypeImplementsInterfaceOfGenericDefinition(Type, Type)"/>
        internal static bool TypeImplementsInterfaceOfGenericDefinition(Type c, Type genericTypeDefinition)
        {
            if (c is null ||
                genericTypeDefinition is null ||
                !genericTypeDefinition.IsGenericTypeDefinition ||
                !genericTypeDefinition.IsInterface)
            {
                return false;
            }

            return c == genericTypeDefinition
                || CheckTypeImplementsInterfaceOfGenericDefinition(c, genericTypeDefinition);
        }

        /// <remarks></remarks>
        /// <inheritdoc cref="CheckTypeDerivesFromClassOfGenericDefinition(Type, Type)"/>
        internal static bool TypeDerivesFromClassOfGenericDefinition(Type c, Type genericTypeDefinition)
        {
            if (c is null ||
                genericTypeDefinition is null ||
                !genericTypeDefinition.IsGenericTypeDefinition ||
                genericTypeDefinition.IsInterface)
            {
                return false;
            }

            return c == genericTypeDefinition
                || CheckTypeDerivesFromClassOfGenericDefinition(c, genericTypeDefinition);
        }

        /// <summary>
        /// Determines whether the type <paramref name="c"/> is derived from a class - or implements one
        /// or more interfaces - that is constructed from a generic type definition.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if any of these conditions is true:
        /// <para>
        ///  * <paramref name="c"/> and <paramref name="genericTypeDefinition"/> represent the same generic type definition.
        /// </para>
        /// <para>
        ///  * <paramref name="c"/> directly or indirectly derives from a type for which <paramref name="genericTypeDefinition"/>
        ///  is the generic type definition.
        /// </para>
        /// <para>
        ///  * <paramref name="genericTypeDefinition"/> represents a generic interface which <paramref name="c"/>
        ///  implements with one or more combinations of generic arguments.
        /// </para>
        /// <see langword="false"/> if none of these conditions are true, if <paramref name="c"/> is null,
        /// or if <paramref name="genericTypeDefinition"/> is null or is not a generic type definition.
        /// </returns>
        public static bool TypeIsOfGenericDefinition(Type c, Type genericTypeDefinition)
        {
            if (c is null || genericTypeDefinition is null || !genericTypeDefinition.IsGenericTypeDefinition)
                return false;

            if (c == genericTypeDefinition)
                return true;

            if (genericTypeDefinition.IsInterface)
                return CheckTypeImplementsInterfaceOfGenericDefinition(c, genericTypeDefinition);

            return CheckTypeDerivesFromClassOfGenericDefinition(c, genericTypeDefinition);
        }

        /// <summary>
        /// Get all types in the specified assembiles which are assignable to the specified type.
        /// </summary>
        /// <param name="type">The assignee type.</param>
        /// <param name="assemblies">The collection of assemblies to search.</param>
        /// <returns>
        /// A collection of all the types in the specified assemblies that are assignable to <paramref name="type"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="type"/> is null.</exception>
        public static Type[] GetAssignableTypes(Type type, IEnumerable<Assembly> assemblies)
        {
#pragma warning disable HAA0401 // Possible allocation of reference type enumerator

            _ = type ?? throw new ArgumentNullException(nameof(type));

            if (assemblies != null)
            {
                // Sealed types and value types cannot have any other assignable types.
                if (type.IsSealed || type.IsValueType)
                {
                    // If the source type's assembly is included in the searchable assemblies,
                    // return an array with containing only the source type.
                    var typeAssembly = type.Assembly;
                    foreach (var assembly in assemblies)
                        if (assembly == typeAssembly)
                            return new Type[1] { type };

                    return Array.Empty<Type>();
                }

                var assignableTypes = new List<Type>();

                // Generic type definitions are a special case as it doesn't work with the IsAssignableFrom method.
                if (type.IsGenericTypeDefinition)
                {
                    if (type.IsInterface)
                    {
                        foreach (var assembly in assemblies)
                        {
                            if (assembly != null)
                                foreach (var t in assembly.GetTypes())
                                    if (CheckTypeImplementsInterfaceOfGenericDefinition(t, type))
                                        assignableTypes.Add(t);
                        }
                    }
                    else
                    {
                        foreach (var assembly in assemblies)
                        {
                            if (assembly != null)
                                foreach (var t in assembly.GetTypes())
                                    if (CheckTypeDerivesFromClassOfGenericDefinition(t, type))
                                        assignableTypes.Add(t);
                        }
                    }
                }
                else
                {
                    foreach (var assembly in assemblies)
                    {
                        if (assembly != null)
                            foreach (var t in assembly.GetTypes())
                                if (type.IsAssignableFrom(t))
                                    assignableTypes.Add(t);
                    }
                }

                if (assignableTypes.Count > 0)
                    return assignableTypes.ToArray();
            }

            return Array.Empty<Type>();

#pragma warning restore HAA0401
        }

        /// <summary>
        /// Get all types in the current <see cref="AppDomain"/> which are assignable to the specified type.
        /// </summary>
        /// <returns>
        /// A collection of all the types in the current <see cref="AppDomain"/>'s assemblies that
        /// are assignable to <paramref name="type"/>.
        /// </returns>
        /// <inheritdoc cref="GetAssignableTypes(Type, IEnumerable{Assembly})"/>
        public static Type[] GetAssignableTypes(Type type)
        {
            return GetAssignableTypes(type, GetAssemblies());
        }
    }
}
