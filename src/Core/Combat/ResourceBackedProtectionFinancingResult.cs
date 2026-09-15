using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

public sealed class ResourceBackedProtectionFinancingResult
{
    public ResourceBackedProtectionFinancingPlan Plan { get; }

    public ResourceLossResult ResourceResult { get; }

    public ProtectionFinancingResult FinancingResult { get; }

    public EntityId ResourceOwnerEntityId =>
        Plan.ResourceOwnerEntityId;

    public ResourceId ResourceId =>
        Plan.ResourceId;

    public double RequestedResourceUnits =>
        ResourceResult.RequestedLoss;

    public double ActualResourceUnitsSpent =>
        ResourceResult.ActualLoss;

    public double ResourceUnitShortfall =>
        ResourceResult.Shortfall;

    public double AssignedDamage =>
        FinancingResult.AssignedDamage;

    public double FinancedDamage =>
        FinancingResult.FinancedDamage;

    public double UnfinancedDamage =>
        FinancingResult.UnfinancedDamage;

    public ProtectionFinancingShortfallPolicy ShortfallPolicy =>
        FinancingResult.ShortfallPolicy;

    internal ResourceBackedProtectionFinancingResult(
        ResourceBackedProtectionFinancingPlan plan,
        ResourceLossPreview preview)
    {
        ArgumentNullException.ThrowIfNull(
            plan);

        ArgumentNullException.ThrowIfNull(
            preview);

        if (preview.Request !=
            plan.ResourceRequest)
        {
            throw new InvalidOperationException(
                "Resource loss preview was created from a different protection financing request.");
        }

        if (preview.Result.ResourceId !=
            plan.ResourceId)
        {
            throw new InvalidOperationException(
                "Resource loss result targets a different protection financing resource.");
        }

        Plan =
            plan;

        ResourceResult =
            preview.Result;

        FinancingResult =
            plan.Financing.Resolve(
                preview.Result.ActualLoss);
    }
}