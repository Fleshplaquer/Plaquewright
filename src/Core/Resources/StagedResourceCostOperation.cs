namespace Idler.Core.Resources;

internal sealed class StagedResourceCostOperation
    : StagedResourceOperation
{
    public ResourceCostPreview Preview { get; }

    public override ResourceOperationProvenance Provenance =>
        Preview.Request.Provenance;

    public StagedResourceCostOperation(
        ResourceState originalState,
        ResourceCostPreview preview)
        : base(originalState)
    {
        ArgumentNullException.ThrowIfNull(
            preview);

        Preview =
            preview;
    }
}