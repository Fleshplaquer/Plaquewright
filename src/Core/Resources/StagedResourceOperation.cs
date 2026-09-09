namespace Idler.Core.Resources;

internal abstract class StagedResourceOperation
{
    public ResourceState OriginalState { get; }

    public ResourceId ResourceId =>
        OriginalState.Id;

    public abstract ResourceOperationProvenance Provenance { get; }

    protected StagedResourceOperation(
        ResourceState originalState)
    {
        ArgumentNullException.ThrowIfNull(
            originalState);

        OriginalState =
            originalState;
    }
}