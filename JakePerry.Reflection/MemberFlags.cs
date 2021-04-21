using System;

namespace JakePerry.Reflection
{
    [Flags]
    public enum MemberFlags
    {
        None = 0,
        Field = 1 << 0,
        Property = 1 << 1,
        Method = 1 << 2
    }
}
