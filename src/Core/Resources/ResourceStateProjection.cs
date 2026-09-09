namespace Idler.Core.Resources;

internal sealed class ResourceStateProjection
{
    private readonly CompiledResourceRegistry _registry;
    private readonly ResourceState _projectedState;

    public ResourceState OriginalState { get; }

    public ResourceId Id =>
        OriginalState.Id;

    public ulong ExpectedOriginalRevision { get; }

    public double Current =>
        _projectedState.Current;

    public double Maximum =>
        _projectedState.Maximum;

    public ResourceStateProjection(
        CompiledResourceRegistry registry,
        ResourceState originalState)
    {
        ArgumentNullException.ThrowIfNull(
            registry);

        ArgumentNullException.ThrowIfNull(
            originalState);

        // Ensure that the resource belongs to this registry.
        _ = registry.GetDefinition(
            originalState.Id);

        _registry = registry;

        OriginalState = originalState;

        ExpectedOriginalRevision =
            originalState.Revision;

        _projectedState =
            new ResourceState(
                originalState.Id,
                originalState.Current,
                originalState.Maximum);
    }

    public ResourceLossPreview PreviewLoss(
        ResourceLossRequest request,
        double preventedLoss = 0d)
    {
        return ResourceLossOperations.Preview(
            _projectedState,
            request,
            preventedLoss);
    }

    public ResourceCostPreview PreviewCost(
        ResourceCostRequest request)
    {
        return ResourceCostOperations.Preview(
            _registry,
            _projectedState,
            request);
    }

    public ResourceRecoveryPreview PreviewRecovery(
        ResourceRecoveryRequest request)
    {
        return ResourceRecoveryOperations.Preview(
            _projectedState,
            request);
    }

    public void Apply(
        ResourceLossPreview preview)
    {
        ArgumentNullException.ThrowIfNull(
            preview);

        ResourceLossOperations.ValidateCommit(
            _projectedState,
            preview);

        _projectedState.SetValues(
            preview.CurrentAfter,
            preview.Maximum);
    }

    public void Apply(
        ResourceCostPreview preview)
    {
        ArgumentNullException.ThrowIfNull(
            preview);

        ResourceCostOperations.ValidateCommit(
            _projectedState,
            preview);

        _projectedState.SetValues(
            preview.CurrentAfter,
            preview.Maximum);
    }

    public void Apply(
        ResourceRecoveryPreview preview)
    {
        ArgumentNullException.ThrowIfNull(
            preview);

        ResourceRecoveryOperations.ValidateCommit(
            _projectedState,
            preview);

        _projectedState.SetValues(
            preview.CurrentAfter,
            preview.Maximum);
    }

    public void ValidateCanCommitToOriginal()
    {
        if (OriginalState.Revision !=
            ExpectedOriginalRevision)
        {
            throw new InvalidOperationException(
                "Projected resource state is stale because the original resource state changed after projection creation.");
        }

        OriginalState.ValidateCanSetValues(
            Current,
            Maximum);
    }
    internal void ApplyValidatedToOriginal()
    {
        OriginalState.SetValues(
            Current,
            Maximum);
    }
}