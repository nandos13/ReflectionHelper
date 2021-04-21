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
            _ = type ?? throw new ArgumentNullException(nameof(type));

            if (assemblies != null)
            {
                var assignableTypes = new List<Type>();

                foreach (var assembly in assemblies)
                    if (assembly != null)
                        foreach (var t in assembly.GetTypes())
                            if (type.IsAssignableFrom(t))
                                assignableTypes.Add(t);

                if (assignableTypes.Count > 0)
                    return assignableTypes.ToArray();
            }

            return Array.Empty<Type>();
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
