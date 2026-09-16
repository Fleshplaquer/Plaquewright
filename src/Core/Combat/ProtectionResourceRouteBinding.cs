using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Combat;

public sealed class ProtectionResourceRouteBinding
{
    public ProtectionAssignmentResult Assignment { get; }

    public ResourceStateTarget ResourceTarget { get; }

    public EntityId ResourceOwnerEntityId =>
        ResourceTarget.EntityId;

    public ResourceId ResourceId =>
        ResourceTarget.ResourceId;

    public double ResourceUnitsPerDamage { get; }

    public ProtectionFinancingShortfallPolicy ShortfallPolicy { get; }

    public ProtectionResourceRouteBinding(
        ProtectionAssignmentResult assignment,
        ResourceStateTarget resourceTarget,
        double resourceUnitsPerDamage,
        ProtectionFinancingShortfallPolicy shortfallPolicy)
    {
        ArgumentNullException.ThrowIfNull(
            assignment);

        ArgumentNullException.ThrowIfNull(
            resourceTarget);

        ValidateResourceUnitsPerDamage(
            resourceUnitsPerDamage);

        ValidateShortfallPolicy(
            shortfallPolicy);

        var definition =
            resourceTarget.ResourceRegistry.GetDefinition(
                resourceTarget.ResourceId);

        if (!definition.HasRole(
                ResourceRole.ProtectionSource))
        {
            throw new InvalidOperationException(
                $"Resource '{definition.Key}' cannot finance protection.");
        }

        Assignment =
            assignment;

        ResourceTarget =
            resourceTarget;

        ResourceUnitsPerDamage =
            resourceUnitsPerDamage;

        ShortfallPolicy =
            shortfallPolicy;
    }

    internal ResourceBackedProtectionFinancingPlan CreateFinancingPlan(
    ProtectionAssignmentLaneState lane,
    ExecutionId? gameplayExecutionId = null)
    {
        ArgumentNullException.ThrowIfNull(
            lane);

        if (!ReferenceEquals(
                lane.Assignment,
                Assignment))
        {
            throw new InvalidOperationException(
                "Protection resource route binding belongs to a different assignment lane.");
        }

        if (lane.IsComplete)
        {
            throw new InvalidOperationException(
                "Cannot create protection financing for a completed assignment lane.");
        }

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage:
                    lane.ContinueRoutingDamage,
                resourceUnitsPerDamage:
                    ResourceUnitsPerDamage,
                ShortfallPolicy);

        return new ResourceBackedProtectionFinancingPlan(
    financing,
    ResourceTarget,
    gameplayExecutionId);
    }

    private static void ValidateResourceUnitsPerDamage(
        double resourceUnitsPerDamage)
    {
        if (!double.IsFinite(
                resourceUnitsPerDamage))
        {
            throw new ArgumentOutOfRangeException(
                nameof(resourceUnitsPerDamage),
                resourceUnitsPerDamage,
                "Resource units per damage must be finite.");
        }

        if (resourceUnitsPerDamage <= 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(resourceUnitsPerDamage),
                resourceUnitsPerDamage,
                "Resource units per damage must be greater than zero.");
        }
    }

    private static void ValidateShortfallPolicy(
        ProtectionFinancingShortfallPolicy shortfallPolicy)
    {
        if (shortfallPolicy !=
                ProtectionFinancingShortfallPolicy.SpillBack &&
            shortfallPolicy !=
                ProtectionFinancingShortfallPolicy.ContinueRouting)
        {
            throw new ArgumentOutOfRangeException(
                nameof(shortfallPolicy),
                shortfallPolicy,
                "Unknown protection financing shortfall policy.");
        }
    }
}