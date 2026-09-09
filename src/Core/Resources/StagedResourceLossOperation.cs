namespace Idler.Core.Resources;

internal sealed class StagedResourceLossOperation
    : StagedResourceOperation
{
    public ResourceLossPreview Preview { get; }

    public override ResourceOperationProvenance Provenance =>
        Preview.Request.Provenance;

    public StagedResourceLossOperation(
        ResourceState originalState,
        ResourceLossPreview preview)
        : base(originalState)
    {
        ArgumentNullException.ThrowIfNull(
            preview);

        Preview =
            preview;
    }
}