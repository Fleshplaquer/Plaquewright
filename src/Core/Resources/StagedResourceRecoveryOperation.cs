namespace Idler.Core.Resources;

internal sealed class StagedResourceRecoveryOperation
    : StagedResourceOperation
{
    public ResourceRecoveryPreview Preview { get; }

    public override ResourceOperationProvenance Provenance =>
        Preview.Request.Provenance;

    public StagedResourceRecoveryOperation(
        ResourceState originalState,
        ResourceRecoveryPreview preview)
        : base(originalState)
    {
        ArgumentNullException.ThrowIfNull(
            preview);

        Preview =
            preview;
    }
}