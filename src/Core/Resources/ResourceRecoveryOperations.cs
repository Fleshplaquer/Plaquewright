using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Resources;

public static class ResourceRecoveryOperations
{
    public static ResourceRecoveryPreview Preview(
        ResourceState state,
        ResourceRecoveryRequest request)
    {
        ArgumentNullException.ThrowIfNull(
            state);

        ValidateRequestTargetsState(
            state,
            request);

        var availableCapacity =
            state.Maximum - state.Current;

        var actualRecovery =
            Math.Min(
                request.Amount,
                availableCapacity);

        var currentAfter =
            state.Current + actualRecovery;

        var result =
            new ResourceRecoveryResult(
                state.Id,
                requestedRecovery: request.Amount,
                actualRecovery: actualRecovery);

        return new ResourceRecoveryPreview(
            targetState: state,
            expectedRevision: state.Revision,
            request: request,
            result: result,
            currentBefore: state.Current,
            currentAfter: currentAfter,
            maximum: state.Maximum);
    }

    public static ResourceRecoveryLedgerEntry Commit(
        EntityId targetEntityId,
        ResourceState state,
        ResourceRecoveryPreview preview)
    {
        ValidateTargetEntityId(
            targetEntityId);

        ValidateCommit(
            state,
            preview);

        return ApplyValidatedCommit(
            targetEntityId,
            state,
            preview);
    }

    internal static void ValidateCommit(
        ResourceState state,
        ResourceRecoveryPreview preview)
    {
        ArgumentNullException.ThrowIfNull(
            state);

        ArgumentNullException.ThrowIfNull(
            preview);

        ValidatePreviewTarget(
            state,
            preview.TargetState,
            preview.ExpectedRevision);

        state.ValidateCanSetValues(
            preview.CurrentAfter,
            preview.Maximum);
    }

    internal static ResourceRecoveryLedgerEntry ApplyValidatedCommit(
        EntityId targetEntityId,
        ResourceState state,
        ResourceRecoveryPreview preview)
    {
        ValidateTargetEntityId(
            targetEntityId);

        state.SetValues(
            preview.CurrentAfter,
            preview.Maximum);

        return new ResourceRecoveryLedgerEntry(
            targetEntityId,
            preview.Request,
            preview.Result);
    }

    private static void ValidateTargetEntityId(
        EntityId targetEntityId)
    {
        if (!targetEntityId.IsValid)
        {
            throw new ArgumentException(
                "Target entity ID must be valid.",
                nameof(targetEntityId));
        }
    }

    private static void ValidateRequestTargetsState(
        ResourceState state,
        ResourceRecoveryRequest request)
    {
        if (state.Id != request.ResourceId)
        {
            throw new InvalidOperationException(
                "Resource recovery request targets a different resource state.");
        }
    }

    private static void ValidatePreviewTarget(
        ResourceState providedState,
        ResourceState previewState,
        ulong expectedRevision)
    {
        if (!ReferenceEquals(
                providedState,
                previewState))
        {
            throw new InvalidOperationException(
                "Resource recovery preview belongs to a different resource state.");
        }

        if (providedState.Revision !=
            expectedRevision)
        {
            throw new InvalidOperationException(
                "Resource recovery preview is stale because the resource state changed after preview creation.");
        }
    }
}