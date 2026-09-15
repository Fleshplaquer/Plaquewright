using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class ProtectionSharedCapacityAllocatorTests
{
    [Fact]
    public void SufficientCapacity_FulfillsEveryClaimExactly()
    {
        var result =
            ProtectionSharedCapacityAllocator.AllocateProportionally(
                availableCapacity: 100d,
                requestedCapacities:
                [
                    30d,
                    20d
                ]);

        Assert.Equal(
            50d,
            result.TotalRequestedCapacity);

        Assert.Equal(
            50d,
            result.TotalAllocatedCapacity);

        Assert.Equal(
            50d,
            result.UnusedCapacity);

        Assert.Equal(
            30d,
            result.AllocatedCapacities[0]);

        Assert.Equal(
            20d,
            result.AllocatedCapacities[1]);
    }

    [Fact]
    public void InsufficientCapacity_IsAllocatedProportionally()
    {
        var result =
            ProtectionSharedCapacityAllocator.AllocateProportionally(
                availableCapacity: 35d,
                requestedCapacities:
                [
                    30d,
                    20d
                ]);

        Assert.Equal(
            50d,
            result.TotalRequestedCapacity);

        Assert.Equal(
            35d,
            result.TotalAllocatedCapacity);

        Assert.Equal(
            0d,
            result.UnusedCapacity);

        Assert.Equal(
            21d,
            result.AllocatedCapacities[0]);

        Assert.Equal(
            14d,
            result.AllocatedCapacities[1]);
    }

    [Fact]
    public void ReorderingClaims_DoesNotCreateImplicitPriority()
    {
        var first =
            ProtectionSharedCapacityAllocator.AllocateProportionally(
                availableCapacity: 35d,
                requestedCapacities:
                [
                    30d,
                    20d
                ]);

        var second =
            ProtectionSharedCapacityAllocator.AllocateProportionally(
                availableCapacity: 35d,
                requestedCapacities:
                [
                    20d,
                    30d
                ]);

        Assert.Equal(
            21d,
            first.AllocatedCapacities[0]);

        Assert.Equal(
            14d,
            first.AllocatedCapacities[1]);

        Assert.Equal(
            14d,
            second.AllocatedCapacities[0]);

        Assert.Equal(
            21d,
            second.AllocatedCapacities[1]);
    }

    [Fact]
    public void AllocationOccursInCapacityUnits()
    {
        var result =
            ProtectionSharedCapacityAllocator.AllocateProportionally(
                availableCapacity: 35d,
                requestedCapacities:
                [
                    60d,
                    20d
                ]);

        // Claims are source-capacity units, not damage units.
        //
        // 60 / 80 * 35 = 26.25
        // 20 / 80 * 35 = 8.75
        Assert.Equal(
            26.25d,
            result.AllocatedCapacities[0]);

        Assert.Equal(
            8.75d,
            result.AllocatedCapacities[1]);

        Assert.Equal(
            35d,
            result.TotalAllocatedCapacity);
    }

    [Fact]
    public void ZeroCapacity_AllocatesNothing()
    {
        var result =
            ProtectionSharedCapacityAllocator.AllocateProportionally(
                availableCapacity: 0d,
                requestedCapacities:
                [
                    30d,
                    20d
                ]);

        Assert.Equal(
            0d,
            result.AllocatedCapacities[0]);

        Assert.Equal(
            0d,
            result.AllocatedCapacities[1]);

        Assert.Equal(
            0d,
            result.TotalAllocatedCapacity);

        Assert.Equal(
            0d,
            result.UnusedCapacity);
    }

    [Fact]
    public void ZeroClaims_ArePreservedWithoutAffectingOtherClaims()
    {
        var result =
            ProtectionSharedCapacityAllocator.AllocateProportionally(
                availableCapacity: 20d,
                requestedCapacities:
                [
                    0d,
                    30d,
                    0d,
                    10d
                ]);

        Assert.Equal(
            0d,
            result.AllocatedCapacities[0]);

        Assert.Equal(
            15d,
            result.AllocatedCapacities[1]);

        Assert.Equal(
            0d,
            result.AllocatedCapacities[2]);

        Assert.Equal(
            5d,
            result.AllocatedCapacities[3]);

        Assert.Equal(
            20d,
            result.TotalAllocatedCapacity);
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidAvailableCapacity_IsRejected(
        double availableCapacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                ProtectionSharedCapacityAllocator.AllocateProportionally(
                    availableCapacity,
                    [10d]));
    }

    [Fact]
    public void DefaultPolicy_IsProportional()
    {
        var result =
            ProtectionSharedCapacityAllocator.Allocate(
                availableCapacity: 35d,
                requestedCapacities:
                [
                    30d,
                20d
                ]);

        Assert.Equal(
            21d,
            result.AllocatedCapacities[0]);

        Assert.Equal(
            14d,
            result.AllocatedCapacities[1]);
    }

    [Fact]
    public void UnknownPolicy_IsRejected()
    {
        var invalidPolicy =
            (ProtectionSharedCapacityAllocationPolicy)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                ProtectionSharedCapacityAllocator.Allocate(
                    availableCapacity: 35d,
                    requestedCapacities:
                    [
                        30d,
                    20d
                    ],
                    invalidPolicy));
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidClaim_IsRejected(
        double requestedCapacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                ProtectionSharedCapacityAllocator.AllocateProportionally(
                    availableCapacity: 100d,
                    requestedCapacities:
                    [
                        10d,
                        requestedCapacity
                    ]));
    }
}