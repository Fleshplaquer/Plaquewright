namespace Plaquewright.Core.Resources;

public sealed class CompiledResourceDefinition
{
    private const ResourceRole KnownRoles =
        ResourceRole.DamageTarget |
        ResourceRole.CostSource |
        ResourceRole.ProtectionSource |
        ResourceRole.DefeatRelevant;

    public ResourceId Id { get; }

    public ResourceKey Key { get; }

    public ResourceRole Roles { get; }

    internal CompiledResourceDefinition(
        ResourceId id,
        ResourceKey key,
        ResourceRole roles)
    {
        if (!id.IsValid)
        {
            throw new ArgumentException(
                "Resource ID must be valid.",
                nameof(id));
        }

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

        Id = id;
        Key = key;
        Roles = roles;
    }

    public bool HasRole(
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

        return (Roles & role) == role;
    }

    private static bool IsPowerOfTwo(
        int value)
    {
        return value > 0 &&
               (value & (value - 1)) == 0;
    }
}