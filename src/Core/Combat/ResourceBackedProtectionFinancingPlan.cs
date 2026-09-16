using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

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
    : this(
        financing,
        resourceTarget,
        gameplayExecutionId: null)
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

    internal ResourceBackedProtectionFinancingPlan(
    ProtectionFinancingPlan financing,
    ResourceStateTarget resourceTarget,
    ExecutionId? gameplayExecutionId)
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

        var provenance =
            gameplayExecutionId.HasValue
                ? new ResourceOperationProvenance(
                    ResourceOperationCause.ProtectionFinancing,
                    gameplayExecutionId.Value)
                : new ResourceOperationProvenance(
                    ResourceOperationCause.ProtectionFinancing);

        ResourceRequest =
            new ResourceLossRequest(
                resourceTarget.ResourceId,
                financing.RequestedResourceUnits,
                provenance);
    }
}