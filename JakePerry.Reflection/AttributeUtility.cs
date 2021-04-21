using System;
using System.Collections.Generic;
using System.Reflection;

namespace JakePerry.Reflection
{
    public static class AttributeUtility
    {
        /// <summary>
        /// Helper method that invokes the appropriate overload for the <see cref="Attribute.GetCustomAttribute"/> method
        /// depending on the element object's type.
        /// </summary>
        /// <param name="element">The element object.</param>
        /// <param name="inherit">
        /// If true, specified to also search the ancestors of element for custom attributes,
        /// provided element is a <see cref="MemberInfo"/> or a <see cref="ParameterInfo"/>
        /// </param>
        /// <inheritdoc cref="Attribute.GetCustomAttribute(Assembly, Type, bool)"/>
        public static Attribute GetCustomAttribute(ICustomAttributeProvider element, Type attributeType, bool inherit)
        {
            /// <summary>
            /// Emulate the implementation of <see cref="Attribute.GetCustomAttribute"/> methods
            /// to get the first matching <see cref="Attribute"/>.
            /// </summary>
            static Attribute EmulateGetCustomAttribute(Attribute[] attrib)
            {
                if (attrib is null || attrib.Length == 0)
                    return null;

                if (attrib.Length == 1)
                    return attrib[0];

                throw new AmbiguousMatchException();
            }

            _ = element ?? throw new ArgumentNullException(nameof(element));

            // Check for recognized types first
            switch (element)
            {
                case Module module:
                    return Attribute.GetCustomAttribute(module, attributeType, inherit);

                case MemberInfo member:
                    return Attribute.GetCustomAttribute(member, attributeType, inherit);

                case ParameterInfo param:
                    return Attribute.GetCustomAttribute(param, attributeType, inherit);

                case Assembly assembly:
                    return Attribute.GetCustomAttribute(assembly, attributeType, inherit);

                // Default case: Emulate mscorlib logic
                default:
                    return EmulateGetCustomAttribute(element.GetCustomAttributes(attributeType, inherit) as Attribute[]);
            };
        }

        /// <summary>
        /// Enumerates all attributes of a given type which decorate any of the target attribute providers.
        /// </summary>
        /// <typeparam name="TAttributeType">The attribute type to search for.</typeparam>
        /// <param name="targets">A collection of attribute-providing objects to be checked for attributes of the given type.</param>
        /// <param name="filter">An optional predicate that can filter attribute providers against a custom criteria.</param>
        /// <param name="inherit">When true, look up the hierarchy chain for the inherited custom attribute.</param>
        /// <returns>
        /// An enumeration of found attributes of type <typeparamref name="TAttributeType"/>.
        /// </returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "HAA0401:Possible allocation of reference type enumerator")]
        public static IEnumerable<CustomAttributeDecoration> EnumerateDecorations<TAttributeType>(
            IEnumerable<ICustomAttributeProvider> targets,
            Predicate<ICustomAttributeProvider> filter = null,
            bool inherit = false)
            where TAttributeType : Attribute
        {
            _ = targets ?? throw new ArgumentNullException(nameof(targets));

            foreach (var target in targets)
            {
                if (filter?.Invoke(target) ?? true)
                {
                    if (target.IsDefined(typeof(TAttributeType), inherit))
                    {
                        foreach (Attribute attribute in target.GetCustomAttributes(typeof(TAttributeType), inherit))
                        {
                            if (attribute != null)
                                yield return new CustomAttributeDecoration(attribute, target);
                        }
                    }
                }
            }
        }
    }
}
