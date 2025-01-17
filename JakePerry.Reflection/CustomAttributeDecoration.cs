﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace JakePerry.Reflection
{
    /// <summary>
    /// A data structure containing an attribute and it's target.
    /// </summary>
    public readonly struct CustomAttributeDecoration : IEquatable<CustomAttributeDecoration>
    {
        private readonly Attribute m_attribute;
        private readonly ICustomAttributeProvider m_target;

        public Attribute CustomAttribute => m_attribute;

        public ICustomAttributeProvider Target => m_target;

        public CustomAttributeDecoration(Attribute attribute, ICustomAttributeProvider target)
        {
            this.m_attribute = attribute ?? throw new ArgumentNullException(nameof(attribute));
            this.m_target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public bool Equals(CustomAttributeDecoration other)
        {
            return EqualityComparer<Attribute>.Default.Equals(m_attribute, other.m_attribute)
                && EqualityComparer<ICustomAttributeProvider>.Default.Equals(m_target, other.m_target);
        }

        public override bool Equals(object obj)
        {
            return (obj is CustomAttributeDecoration other) && this.Equals(other);
        }

        public override int GetHashCode()
        {
            int hashCode = -1047610033;
            hashCode = hashCode * -1521134295 + EqualityComparer<Attribute>.Default.GetHashCode(m_attribute);
            hashCode = hashCode * -1521134295 + EqualityComparer<ICustomAttributeProvider>.Default.GetHashCode(m_target);
            return hashCode;
        }

        public static bool operator ==(CustomAttributeDecoration left, CustomAttributeDecoration right) => left.Equals(right);

        public static bool operator !=(CustomAttributeDecoration left, CustomAttributeDecoration right) => !(left == right);
    }
}
