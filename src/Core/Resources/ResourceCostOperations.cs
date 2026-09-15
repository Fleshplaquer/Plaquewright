using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Resources;

public static class ResourceCostOperations
{
    public static ResourceCostPreview Preview(
        CompiledResourceRegistry registry,
        ResourceState state,
        ResourceCostRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            registry);

        ArgumentNullException.ThrowIfNull(
            state);

        ValidateRequestTargetsState(
            state,
            request);

        var definition =
            registry.GetDefinition(
                request.ResourceId);

        if (!definition.HasRole(
                ResourceRole.CostSource))
        {
            throw new InvalidOperationException(
                $"Resource '{definition.Key}' cannot finance costs.");
        }

        var isAffordable =
    state.Current >=
    request.Amount;

        var shortfall =
            isAffordable
                ? 0d
                : request.Amount -
                  state.Current;

        var isRepresentable =
            false;

        var currentAfter =
            state.Current;

        var projectedActualCost =
            0d;

        if (isAffordable)
        {
            isRepresentable =
                ResourceQuantityMath.TryCalculateCurrentAfterCost(
                    state.Current,
                    request.Amount,
                    out currentAfter,
                    out projectedActualCost);
        }

        return new ResourceCostPreview(
            targetState: state,
            expectedRevision: state.Revision,
            request: request,
            isAffordable: isAffordable,
            availableAmount: state.Current,
            isRepresentable: isRepresentable,
projectedActualCost: projectedActualCost,
            shortfall: shortfall,
            currentBefore: state.Current,
            currentAfter: currentAfter,
            maximum: state.Maximum);
    }

    public static ResourceCostLedgerEntry Commit(
    ResourceStateTarget target,
    ResourceCostPreview preview)
    {
        ArgumentNullException.ThrowIfNull(
            target);

        ValidateCommit(
            target.State,
            preview);

        return ApplyValidatedCommit(
            target,
            preview);
    }

    internal static void ValidateCommit(
        ResourceState state,
        ResourceCostPreview preview)
    {
        ArgumentNullException.ThrowIfNull(
            state);

        ArgumentNullException.ThrowIfNull(
            preview);

        if (!ReferenceEquals(
                state,
                preview.TargetState))
        {
            throw new InvalidOperationException(
                "Resource cost preview belongs to a different resource state.");
        }

        if (state.Revision !=
            preview.ExpectedRevision)
        {
            throw new InvalidOperationException(
                "Resource cost preview is stale because the resource state changed after preview creation.");
        }

        if (!preview.IsPayable)
        {
            throw new InvalidOperationException(
                "An unpayable resource cost cannot be committed.");
        }

        state.ValidateCanSetValues(
            preview.CurrentAfter,
            preview.Maximum);
    }

    internal static ResourceCostLedgerEntry ApplyValidatedCommit(
    ResourceStateTarget target,
    ResourceCostPreview preview)
    {
        ArgumentNullException.ThrowIfNull(
            target);

        target.State.SetValues(
            preview.CurrentAfter,
            preview.Maximum);

        var result =
            new ResourceCostResult(
                preview.Request.ResourceId,
                preview.Request.Amount,
                preview.ProjectedActualCost);

        return new ResourceCostLedgerEntry(
            target.EntityId,
            preview.Request,
            result);
    }

    private static void ValidateRequestTargetsState(
        ResourceState state,
        ResourceCostRequest request)
    {
        if (state.Id !=
            request.ResourceId)
        {
            throw new InvalidOperationException(
                "Resource cost request targets a different resource state.");
        }
    }
}