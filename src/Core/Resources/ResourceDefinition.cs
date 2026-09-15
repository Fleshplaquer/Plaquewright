namespace Plaquewright.Core.Resources;

public sealed class ResourceDefinition
{
    private const ResourceRole KnownRoles =
        ResourceRole.DamageTarget |
        ResourceRole.CostSource |
        ResourceRole.ProtectionSource |
        ResourceRole.DefeatRelevant;

    public ResourceKey Key { get; }

    public ResourceRole Roles { get; }

    public ResourceDefinition(
        ResourceKey key,
        ResourceRole roles = ResourceRole.None)
    {
        if (!key.IsValid)
        {
            throw new ArgumentException(
                "Resource key must be valid.",
                nameof(key));
        }

        if ((roles & ~KnownRoles) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(roles),
                roles,
                "Resource roles contain unknown values.");
        }

        Key = key;
        Roles = roles;
    }

    public bool HasRole(
        ResourceRole role)
    {
        ValidateSingleRole(role);

        return (Roles & role) == role;
    }

    private static void ValidateSingleRole(
        ResourceRole role)
    {
        if (role == ResourceRole.None ||
            !IsPowerOfTwo((int)role) ||
            (role & ~KnownRoles) != 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(role),
                role,
                "A single known resource role is required.");
        }
    }

    private static bool IsPowerOfTwo(
        int value)
    {
        return value > 0 &&
               (value & (value - 1)) == 0;
    }
}