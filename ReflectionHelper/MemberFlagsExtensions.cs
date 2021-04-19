namespace JakePerry
{
    internal static class MemberFlagsExtensions
    {
        internal static bool HasNoFlags(this MemberFlags flags)
        {
            return flags == MemberFlags.None || (int)flags > (int)MemberFlags.Method;
        }

        internal static bool HasFieldFlag(this MemberFlags flags)
        {
            return (flags & MemberFlags.Field) == MemberFlags.Field;
        }

        internal static bool HasPropertyFlag(this MemberFlags flags)
        {
            return (flags & MemberFlags.Property) == MemberFlags.Property;
        }

        internal static bool HasMethodFlag(this MemberFlags flags)
        {
            return (flags & MemberFlags.Method) == MemberFlags.Method;
        }

        internal static bool HasAllMemberFlags(this MemberFlags flags)
        {
            MemberFlags allMembers = MemberFlags.Field | MemberFlags.Property | MemberFlags.Method;
            return (flags & allMembers) == allMembers;
        }
    }
}
