namespace Plaquewright.Core.Resources;

internal sealed class StagedResourceCostOperation
    : StagedResourceOperation
{
    public ResourceCostPreview Preview { get; }

    public override ResourceOperationProvenance Provenance =>
        Preview.Request.Provenance;

    public StagedResourceCostOperation(
        ResourceStateTarget target,
        ResourceCostPreview preview)
        : base(target)
    {
        ArgumentNullException.ThrowIfNull(
            preview);

        Preview =
            preview;
    }
}