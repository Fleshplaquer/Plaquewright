using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

public sealed class ResourceBackedProtectionFinancingPlan
{
    public ProtectionFinancingPlan Financing { get; }

    public ResourceStateTarget ResourceTarget { get; }

    public ResourceLossRequest ResourceRequest { get; }

    public EntityId ResourceOwnerEntityId =>
        ResourceTarget.EntityId;

    public ResourceId ResourceId =>
        ResourceTarget.ResourceId;

    public ResourceBackedProtectionFinancingPlan(
        ProtectionFinancingPlan financing,
        ResourceStateTarget resourceTarget)
    {
        ArgumentNullException.ThrowIfNull(
            financing);

        ArgumentNullException.ThrowIfNull(
            resourceTarget);

        var definition =
            resourceTarget.ResourceRegistry.GetDefinition(
                resourceTarget.ResourceId);

        if (!definition.HasRole(
                ResourceRole.ProtectionSource))
        {
            throw new InvalidOperationException(
                $"Resource '{definition.Key}' cannot finance protection.");
        }

        Financing =
            financing;

        ResourceTarget =
            resourceTarget;

        ResourceRequest =
            new ResourceLossRequest(
                resourceTarget.ResourceId,
                financing.RequestedResourceUnits,
                new ResourceOperationProvenance(
                    ResourceOperationCause.ProtectionFinancing));
    }
}