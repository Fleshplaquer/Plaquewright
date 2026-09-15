using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.ExternalTests.PublicApi;

public sealed class ResourceOperationOwnershipApiTests
{
    [Fact]
    public void ResourceLossCommit_DoesNotExposeFreeEntityIdAndStatePair()
    {
        var unsafeCommit =
            typeof(ResourceLossOperations)
                .GetMethod(
                    nameof(ResourceLossOperations.Commit),
                    [
                        typeof(EntityId),
                        typeof(ResourceState),
                        typeof(ResourceLossPreview)
                    ]);

        Assert.Null(
            unsafeCommit);
    }

    [Fact]
    public void ResourceRecoveryCommit_DoesNotExposeFreeEntityIdAndStatePair()
    {
        var unsafeCommit =
            typeof(ResourceRecoveryOperations)
                .GetMethod(
                    nameof(ResourceRecoveryOperations.Commit),
                    [
                        typeof(EntityId),
                    typeof(ResourceState),
                    typeof(ResourceRecoveryPreview)
                    ]);

        Assert.Null(
            unsafeCommit);
    }

    [Fact]
    public void ResourceRecoveryCommit_UsesOwnerBoundTarget()
    {
        var safeCommit =
            typeof(ResourceRecoveryOperations)
                .GetMethod(
                    nameof(ResourceRecoveryOperations.Commit),
                    [
                        typeof(ResourceStateTarget),
                    typeof(ResourceRecoveryPreview)
                    ]);

        Assert.NotNull(
            safeCommit);

        Assert.True(
            safeCommit.IsPublic);
    }

    [Fact]
    public void ResourceCostCommit_DoesNotExposeFreeEntityIdAndStatePair()
    {
        var unsafeCommit =
            typeof(ResourceCostOperations)
                .GetMethod(
                    nameof(ResourceCostOperations.Commit),
                    [
                        typeof(EntityId),
                    typeof(ResourceState),
                    typeof(ResourceCostPreview)
                    ]);

        Assert.Null(
            unsafeCommit);
    }

    [Fact]
    public void ResourceCostCommit_UsesOwnerBoundTarget()
    {
        var safeCommit =
            typeof(ResourceCostOperations)
                .GetMethod(
                    nameof(ResourceCostOperations.Commit),
                    [
                        typeof(ResourceStateTarget),
                    typeof(ResourceCostPreview)
                    ]);

        Assert.NotNull(
            safeCommit);

        Assert.True(
            safeCommit.IsPublic);
    }

    [Fact]
    public void ResourceLossCommit_UsesOwnerBoundTarget()
    {
        var safeCommit =
            typeof(ResourceLossOperations)
                .GetMethod(
                    nameof(ResourceLossOperations.Commit),
                    [
                        typeof(ResourceStateTarget),
                        typeof(ResourceLossPreview)
                    ]);

        Assert.NotNull(
            safeCommit);

        Assert.True(
            safeCommit.IsPublic);
    }
}