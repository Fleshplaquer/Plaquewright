namespace Idler.Core.Resources;

public static class ResourceStateOperations
{
    public static ResourceLossPreview PreviewLoss(
        ResourceState state,
        double requestedLoss,
        double preventedLoss = 0d)
    {
        ArgumentNullException.ThrowIfNull(
            state);

        ValidateNonNegativeFinite(
            requestedLoss,
            nameof(requestedLoss));

        ValidateNonNegativeFinite(
            preventedLoss,
            nameof(preventedLoss));

        if (preventedLoss > requestedLoss)
        {
            throw new ArgumentOutOfRangeException(
                nameof(preventedLoss),
                preventedLoss,
                "Prevented loss cannot exceed requested loss.");
        }

        var remainingLoss =
            requestedLoss - preventedLoss;

        var actualLoss =
            Math.Min(
                state.Current,
                remainingLoss);

        var currentAfter =
            state.Current - actualLoss;

        var result =
            new ResourceLossResult(
                state.Id,
                requestedLoss,
                preventedLoss,
                actualLoss);

        return new ResourceLossPreview(
            targetState: state,
            expectedRevision: state.Revision,
            result: result,
            currentBefore: state.Current,
            currentAfter: currentAfter,
            maximum: state.Maximum);
    }

    public static ResourceRecoveryPreview PreviewRecovery(
        ResourceState state,
        double requestedRecovery)
    {
        ArgumentNullException.ThrowIfNull(
            state);

        ValidateNonNegativeFinite(
            requestedRecovery,
            nameof(requestedRecovery));

        var availableCapacity =
            state.Maximum - state.Current;

        var actualRecovery =
            Math.Min(
                requestedRecovery,
                availableCapacity);

        var currentAfter =
            state.Current + actualRecovery;

        var result =
            new ResourceRecoveryResult(
                state.Id,
                requestedRecovery,
                actualRecovery);

        return new ResourceRecoveryPreview(
            targetState: state,
            expectedRevision: state.Revision,
            result: result,
            currentBefore: state.Current,
            currentAfter: currentAfter,
            maximum: state.Maximum);
    }

    public static void Commit(
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

        state.SetValues(
            preview.CurrentAfter,
            preview.Maximum);
    }

    public static void Commit(
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

        state.SetValues(
            preview.CurrentAfter,
            preview.Maximum);
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
                "Resource preview belongs to a different resource state.");
        }

        if (providedState.Revision !=
            expectedRevision)
        {
            throw new InvalidOperationException(
                "Resource preview is stale because the resource state changed after preview creation.");
        }
    }

    private static void ValidateNonNegativeFinite(
        double value,
        string parameterName)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Resource quantity must be finite.");
        }

        if (value < 0d)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Resource quantity cannot be negative.");
        }
    }
}