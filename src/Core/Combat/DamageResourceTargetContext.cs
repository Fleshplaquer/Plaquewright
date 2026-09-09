using Idler.Core.Resources;

namespace Idler.Core.Combat;

public sealed class DamageResourceTargetContext
{
    public DamageResolutionContext Resolution { get; }

    public ResourceStateTarget ResourceTarget { get; }

    internal DamageResourceTargetContext(
        DamageResolutionContext resolution,
        ResourceStateTarget resourceTarget)
    {
        ArgumentNullException.ThrowIfNull(
            resolution);

        ArgumentNullException.ThrowIfNull(
            resourceTarget);

        if (resolution.TargetEntityId !=
            resourceTarget.EntityId)
        {
            throw new InvalidOperationException(
                "Damage resource target belongs to a different target entity.");
        }

        var definition =
            resourceTarget.ResourceRegistry.GetDefinition(
                resourceTarget.ResourceId);

        if (!definition.HasRole(
                ResourceRole.DamageTarget))
        {
            throw new InvalidOperationException(
                $"Resource '{definition.Key}' cannot receive damage.");
        }

        Resolution =
            resolution;

        ResourceTarget =
            resourceTarget;
    }
}