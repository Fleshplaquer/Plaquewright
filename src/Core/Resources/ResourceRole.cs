namespace Plaquewright.Core.Resources;

[Flags]
public enum ResourceRole
{
    None = 0,

    DamageTarget = 1 << 0,
    CostSource = 1 << 1,
    ProtectionSource = 1 << 2,
    DefeatRelevant = 1 << 3
}