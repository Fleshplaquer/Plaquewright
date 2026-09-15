using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

public sealed class DamageResourceLossResult
{
    public DamageResourceLossPlan Plan { get; }

    public ResourceLossResult ResourceResult { get; }

    public DamageTaken DamageTaken =>
        Plan.DamageTaken;

    public EntityId TargetEntityId =>
        Plan.TargetEntityId;

    public ResourceId ResourceId =>
        Plan.ResourceId;

    public double RequestedResourceLoss =>
        ResourceResult.RequestedLoss;

    public double PreventedResourceLoss =>
        ResourceResult.PreventedLoss;

    public ActualResourceLoss ActualResourceLoss { get; }

    public double Shortfall =>
        ResourceResult.Shortfall;

    internal DamageResourceLossResult(
        DamageResourceLossPlan plan,
        ResourceLossPreview preview)
    {
        ArgumentNullException.ThrowIfNull(
            plan);

        ArgumentNullException.ThrowIfNull(
            preview);

        if (!ReferenceEquals(
                plan.TargetContext.ResourceTarget.State,
                preview.TargetState))
        {
            throw new InvalidOperationException(
                "Resource loss preview belongs to a different resource state than the damage resource loss plan.");
        }

        if (preview.Request !=
            plan.Request)
        {
            throw new InvalidOperationException(
                "Resource loss preview was created from a different request than the damage resource loss plan.");
        }

        if (preview.Result.ResourceId !=
            plan.ResourceId)
        {
            throw new InvalidOperationException(
                "Resource loss result targets a different resource than the damage resource loss plan.");
        }

        Plan =
            plan;

        ResourceResult =
            preview.Result;

        ActualResourceLoss =
            plan.DamageTaken.AdvanceToActualResourceLoss(
                preview.Result.ActualLoss);
    }

    internal DamageResourceLossResult(
    DamageResourceLossPlan plan,
    StagedResourceLossOperation stagedOperation)
    {
        ArgumentNullException.ThrowIfNull(
            plan);

        ArgumentNullException.ThrowIfNull(
            stagedOperation);

        if (!ReferenceEquals(
                plan.TargetContext.ResourceTarget,
                stagedOperation.Target))
        {
            throw new InvalidOperationException(
                "Staged resource loss operation belongs to a different resource target than the damage resource loss plan.");
        }

        if (stagedOperation.Preview.Request !=
            plan.Request)
        {
            throw new InvalidOperationException(
                "Staged resource loss operation was created from a different request than the damage resource loss plan.");
        }

        if (stagedOperation.ResourceId !=
            plan.ResourceId)
        {
            throw new InvalidOperationException(
                "Staged resource loss operation targets a different resource than the damage resource loss plan.");
        }

        Plan =
            plan;

        ResourceResult =
            stagedOperation.Preview.Result;

        ActualResourceLoss =
            plan.DamageTaken.AdvanceToActualResourceLoss(
                stagedOperation.Preview.Result.ActualLoss);
    }
}