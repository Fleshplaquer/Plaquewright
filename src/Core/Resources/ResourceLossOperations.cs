using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Resources;

public static class ResourceLossOperations
{
    public static ResourceLossPreview Preview(
        ResourceState state,
        ResourceLossRequest request,
        double preventedLoss = 0d)
    {
        ArgumentNullException.ThrowIfNull(
            state);

        ValidateRequestTargetsState(
            state,
            request);

        ValidatePreventedLoss(
            preventedLoss,
            request.Amount);

        var remainingLoss =
    request.Amount -
    preventedLoss;

        var currentAfter =
            ResourceQuantityMath.CalculateCurrentAfterLoss(
                state.Current,
                remainingLoss,
                out var actualLoss);
        var result =
            new ResourceLossResult(
                state.Id,
                requestedLoss: request.Amount,
                preventedLoss: preventedLoss,
                actualLoss: actualLoss);

        return new ResourceLossPreview(
            targetState: state,
            expectedRevision: state.Revision,
            request: request,
            result: result,
            currentBefore: state.Current,
            currentAfter: currentAfter,
            maximum: state.Maximum);
    }

    public static ResourceLossLedgerEntry Commit(
        EntityId targetEntityId,
        ResourceState state,
        ResourceLossPreview preview)
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
        ResourceLossPreview preview)
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

    internal static ResourceLossLedgerEntry ApplyValidatedCommit(
        EntityId targetEntityId,
        ResourceState state,
        ResourceLossPreview preview)
    {
        ValidateTargetEntityId(
            targetEntityId);

        state.SetValues(
            preview.CurrentAfter,
            preview.Maximum);

        return new ResourceLossLedgerEntry(
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
        ResourceLossRequest request)
    {
        if (state.Id != request.ResourceId)
        {
            throw new InvalidOperationException(
                "Resource loss request targets a different resource state.");
        }
    }

    private static void ValidatePreventedLoss(
        double preventedLoss,
        double requestedLoss)
    {
        if (!double.IsFinite(
                preventedLoss))
        {
            throw new ArgumentOutOfRangeException(
                nameof(preventedLoss),
                preventedLoss,
                "Prevented loss must be finite.");
        }

        if (preventedLoss < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(preventedLoss),
                preventedLoss,
                "Prevented loss cannot be negative.");
        }

        if (preventedLoss > requestedLoss)
        {
            throw new ArgumentOutOfRangeException(
                nameof(preventedLoss),
                preventedLoss,
                "Prevented loss cannot exceed requested loss.");
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
                "Resource loss preview belongs to a different resource state.");
        }

        if (providedState.Revision !=
            expectedRevision)
        {
            throw new InvalidOperationException(
                "Resource loss preview is stale because the resource state changed after preview creation.");
        }
    }
}