namespace Idler.Core.Resources;

internal sealed class StagedResourceLossOperation
    : StagedResourceOperation
{
    public ResourceLossPreview Preview { get; }

    public override ResourceOperationProvenance Provenance =>
        Preview.Request.Provenance;

    public StagedResourceLossOperation(
        ResourceStateTarget target,
        ResourceLossPreview preview)
        : base(target)
    {
        ArgumentNullException.ThrowIfNull(
            preview);

        Preview =
            preview;
    }
}