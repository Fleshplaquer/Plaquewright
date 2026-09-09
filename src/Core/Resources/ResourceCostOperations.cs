namespace Idler.Core.Resources;

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
            state.Current >= request.Amount;

        var shortfall =
            isAffordable
                ? 0d
                : request.Amount - state.Current;

        var currentAfter =
            isAffordable
                ? state.Current - request.Amount
                : state.Current;

        return new ResourceCostPreview(
            targetState: state,
            expectedRevision: state.Revision,
            request: request,
            isAffordable: isAffordable,
            availableAmount: state.Current,
            shortfall: shortfall,
            currentBefore: state.Current,
            currentAfter: currentAfter,
            maximum: state.Maximum);
    }

    public static ResourceCostLedgerEntry Commit(
    ResourceState state,
    ResourceCostPreview preview)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(preview);

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

        if (!preview.IsAffordable)
        {
            throw new InvalidOperationException(
                "An unaffordable resource cost cannot be committed.");
        }

        state.SetValues(
            preview.CurrentAfter,
            preview.Maximum);

        var result =
            new ResourceCostResult(
                preview.Request.ResourceId,
                preview.Request.Amount);

        return new ResourceCostLedgerEntry(
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