using System;
using System.Collections;
using System.Collections.Generic;

namespace JakePerry.Reflection
{
    /// <summary>
    /// Represents the inheritance hierarchy of a <see cref="System.Type"/> object.
    /// </summary>
    public readonly struct TypeHierarchy : IEnumerable<Type>, IEquatable<TypeHierarchy>
    {
        private readonly Type m_type;

        public Type Type => m_type;

        public TypeHierarchy(Type type)
        {
            m_type = type;
        }

        /// <summary>
        /// An object that enumerates all types in the source type's inheritance hierarchy.
        /// </summary>
        public struct Enumerator : IEnumerator<Type>
        {
            private readonly Type m_type;
            private Type m_currentValue;
            private Type m_nextValue;

            public Type Current => m_currentValue;

            object IEnumerator.Current => Current;

            public Enumerator(Type type)
            {
                m_type = m_nextValue = type;
                m_currentValue = null;
            }

            public bool MoveNext()
            {
                if (m_nextValue != null)
                {
                    m_currentValue = m_nextValue;
                    m_nextValue = m_nextValue.BaseType;
                    return true;
                }

                m_currentValue = null;
                return false;
            }

            public void Reset()
            {
                m_nextValue = m_type;
                m_currentValue = null;
            }

            public void Dispose() { /* Not implemented */ }
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(m_type);
        }

#pragma warning disable HAA0601 // Value type to reference type conversion causing boxing allocation
        // Justification: Boxing is unavoidable when this object is cast to an IEnumerable.
        // In cases where the source enumerable of a foreach loop is guaranteed to be of this type
        // at compile time, boxing will not occur; instead, the compiler will directly invoke
        // the TypeHierarchy.GetEnumerator() method as an optimization.

        IEnumerator<Type> IEnumerable<Type>.GetEnumerator()
        {
            return new Enumerator(m_type);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new Enumerator(m_type);
        }

#pragma warning restore HAA0601

        public bool Equals(TypeHierarchy other)
        {
            return EqualityComparer<Type>.Default.Equals(m_type, other.m_type);
        }

        public override bool Equals(object obj)
        {
            return obj is TypeHierarchy other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<Type>.Default.GetHashCode(m_type);
        }

        public static bool operator ==(TypeHierarchy left, TypeHierarchy right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TypeHierarchy left, TypeHierarchy right)
        {
            return !(left == right);
        }
    }
}
