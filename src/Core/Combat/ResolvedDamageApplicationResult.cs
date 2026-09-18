using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Combat;

internal sealed class ResolvedDamageApplicationResult
{
    public DamageResolutionContext Resolution { get; }

    public IReadOnlyList<ResourceLossPreview>
        ResourceLossPreviews
    { get; }

    public IReadOnlyList<
        DefeatAwareResourceTransactionCommitResult>
        OwnerCommitResults
    { get; }

    public DefeatAwareResourceTransactionCommitResult
        TargetCommitResult
    { get; }

    public ResolvedDamageApplicationResult(
        DamageResolutionContext resolution,
        IReadOnlyList<ResourceLossPreview>
            resourceLossPreviews,
        IReadOnlyList<
            DefeatAwareResourceTransactionCommitResult>
            ownerCommitResults,
        DefeatAwareResourceTransactionCommitResult
            targetCommitResult)
    {
        ArgumentNullException.ThrowIfNull(
            resolution);

        ArgumentNullException.ThrowIfNull(
            resourceLossPreviews);

        ArgumentNullException.ThrowIfNull(
            ownerCommitResults);

        ArgumentNullException.ThrowIfNull(
            targetCommitResult);

        Resolution =
            resolution;

        ResourceLossPreviews =
            resourceLossPreviews;

        OwnerCommitResults =
            ownerCommitResults;

        TargetCommitResult =
            targetCommitResult;
    }
}