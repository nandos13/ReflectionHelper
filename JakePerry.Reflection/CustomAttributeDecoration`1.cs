using System;
using System.Collections.Generic;
using System.Reflection;

namespace JakePerry.Reflection
{
    /// <summary>
    /// A data structure containing an attribute and it's target.
    /// </summary>
    public readonly struct CustomAttributeDecoration<T> : IEquatable<CustomAttributeDecoration<T>>
        where T : Attribute
    {
        private readonly T m_attribute;
        private readonly ICustomAttributeProvider m_target;

        public T CustomAttribute => m_attribute;

        public ICustomAttributeProvider Target => m_target;

        public CustomAttributeDecoration(T attribute, ICustomAttributeProvider target)
        {
            this.m_attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            this.m_target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public bool Equals(CustomAttributeDecoration<T> other)
        {
            return EqualityComparer<T>.Default.Equals(m_attribute, other.m_attribute)
                && EqualityComparer<ICustomAttributeProvider>.Default.Equals(m_target, other.m_target);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            // Avoid attempting a cast to value types if the object is a reference type (avoids boxing)
            if (obj.GetType().IsValueType)
            {
                if (obj is CustomAttributeDecoration<T> other)
                    return this.Equals(other);

                if (obj is CustomAttributeDecoration other2)
                    return ((CustomAttributeDecoration)this).Equals(other2);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(m_attribute, m_target);
        }

        public static bool operator ==(CustomAttributeDecoration<T> left, CustomAttributeDecoration<T> right) => left.Equals(right);

        public static bool operator !=(CustomAttributeDecoration<T> left, CustomAttributeDecoration<T> right) => !(left == right);

        public static implicit operator CustomAttributeDecoration(CustomAttributeDecoration<T> source)
        {
            return source.m_attribute is null
                ? default
                : new CustomAttributeDecoration(source.m_attribute, source.m_target);
        }
    }
}
