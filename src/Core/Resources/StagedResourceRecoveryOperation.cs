namespace Idler.Core.Resources;

internal sealed class StagedResourceRecoveryOperation
    : StagedResourceOperation
{
    public ResourceRecoveryPreview Preview { get; }

    public override ResourceOperationProvenance Provenance =>
        Preview.Request.Provenance;

    public StagedResourceRecoveryOperation(
        ResourceStateTarget target,
        ResourceRecoveryPreview preview)
        : base(target)
    {
        ArgumentNullException.ThrowIfNull(
            preview);

        Preview =
            preview;
    }
}