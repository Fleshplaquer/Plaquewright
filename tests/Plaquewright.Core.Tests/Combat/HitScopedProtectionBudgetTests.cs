using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class HitScopedProtectionBudgetTests
{
    [Fact]
    public void Constructor_BindsBudgetToHitExecution()
    {
        var hitExecutionId =
            new HitExecutionId(7UL);

        var budget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 100d);

        Assert.Equal(
            hitExecutionId,
            budget.HitExecutionId);

        Assert.Equal(
            100d,
            budget.InitialCapacity);

        Assert.Equal(
            0d,
            budget.ReservedCapacity);

        Assert.Equal(
            100d,
            budget.RemainingCapacity);

        Assert.False(
            budget.IsExhausted);
    }

    [Fact]
    public void PendingReservation_AbortRestoresPreviousCapacityExactly()
    {
        var hitExecutionId =
            new HitExecutionId(7UL);

        var budget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 100d);

        budget.Reserve(
            hitExecutionId,
            requestedCapacity: 30d);

        Assert.Equal(
            30d,
            budget.ReservedCapacity);

        var pending =
            budget.ReservePending(
                hitExecutionId,
                requestedCapacity: 50d);

        Assert.Equal(
            80d,
            budget.ReservedCapacity);

        Assert.Equal(
            20d,
            budget.RemainingCapacity);

        pending.Abort();

        Assert.Equal(
            30d,
            budget.ReservedCapacity);

        Assert.Equal(
            70d,
            budget.RemainingCapacity);
    }

    [Fact]
    public void PendingReservation_CommitKeepsReservedCapacity()
    {
        var hitExecutionId =
            new HitExecutionId(7UL);

        var budget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 100d);

        var pending =
            budget.ReservePending(
                hitExecutionId,
                requestedCapacity: 60d);

        Assert.Equal(
            60d,
            budget.ReservedCapacity);

        pending.Commit();

        Assert.Equal(
            60d,
            budget.ReservedCapacity);

        Assert.Equal(
            40d,
            budget.RemainingCapacity);

        // Successful finalization must release the pending
        // lifetime guard for future reservations.
        var next =
            budget.Reserve(
                hitExecutionId,
                requestedCapacity: 10d);

        Assert.Equal(
            10d,
            next.Reserved);

        Assert.Equal(
            70d,
            budget.ReservedCapacity);
    }

    [Fact]
    public void PendingReservation_CannotBeFinalizedTwice()
    {
        var hitExecutionId =
            new HitExecutionId(7UL);

        var budget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 100d);

        var pending =
            budget.ReservePending(
                hitExecutionId,
                requestedCapacity: 40d);

        pending.Commit();

        Assert.Throws<InvalidOperationException>(
            () =>
                pending.Commit());

        Assert.Throws<InvalidOperationException>(
            () =>
                pending.Abort());

        Assert.Equal(
            40d,
            budget.ReservedCapacity);

        Assert.Equal(
            60d,
            budget.RemainingCapacity);
    }

    [Fact]
    public void PendingReservation_BlocksConcurrentReservationWithoutAdditionalConsumption()
    {
        var hitExecutionId =
            new HitExecutionId(7UL);

        var budget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 100d);

        var pending =
            budget.ReservePending(
                hitExecutionId,
                requestedCapacity: 40d);

        Assert.Throws<InvalidOperationException>(
            () =>
                budget.Reserve(
                    hitExecutionId,
                    requestedCapacity: 20d));

        Assert.Equal(
            40d,
            budget.ReservedCapacity);

        Assert.Equal(
            60d,
            budget.RemainingCapacity);

        pending.Abort();

        Assert.Equal(
            0d,
            budget.ReservedCapacity);

        Assert.Equal(
            100d,
            budget.RemainingCapacity);
    }



    [Fact]
    public void Reserve_ConsumesSharedCapacity()
    {
        var hitExecutionId =
            new HitExecutionId(7UL);

        var budget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 100d);

        var reservation =
            budget.Reserve(
                hitExecutionId,
                requestedCapacity: 60d);

        Assert.Equal(
            60d,
            reservation.Requested);

        Assert.Equal(
            60d,
            reservation.Reserved);

        Assert.Equal(
            0d,
            reservation.Shortfall);

        Assert.Equal(
            60d,
            budget.ReservedCapacity);

        Assert.Equal(
            40d,
            budget.RemainingCapacity);
    }

    [Fact]
    public void MultipleReservations_ShareSameHitScopedCapacity()
    {
        var hitExecutionId =
            new HitExecutionId(7UL);

        var budget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 100d);

        var first =
            budget.Reserve(
                hitExecutionId,
                requestedCapacity: 60d);

        var second =
            budget.Reserve(
                hitExecutionId,
                requestedCapacity: 60d);

        Assert.Equal(
            60d,
            first.Reserved);

        Assert.Equal(
            0d,
            first.Shortfall);

        Assert.Equal(
            40d,
            second.Reserved);

        Assert.Equal(
            20d,
            second.Shortfall);

        Assert.Equal(
            100d,
            budget.ReservedCapacity);

        Assert.Equal(
            0d,
            budget.RemainingCapacity);

        Assert.True(
            budget.IsExhausted);
    }

    [Fact]
    public void ReservationAccounting_IsComplete()
    {
        var hitExecutionId =
            new HitExecutionId(7UL);

        var budget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 25d);

        var reservation =
            budget.Reserve(
                hitExecutionId,
                requestedCapacity: 40d);

        Assert.Equal(
            reservation.Requested,
            reservation.Reserved +
            reservation.Shortfall);

        Assert.Equal(
            40d,
            reservation.Requested);

        Assert.Equal(
            25d,
            reservation.Reserved);

        Assert.Equal(
            15d,
            reservation.Shortfall);
    }

    [Fact]
    public void ExhaustedBudget_ReturnsFullShortfall()
    {
        var hitExecutionId =
            new HitExecutionId(7UL);

        var budget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 20d);

        budget.Reserve(
            hitExecutionId,
            requestedCapacity: 20d);

        var second =
            budget.Reserve(
                hitExecutionId,
                requestedCapacity: 10d);

        Assert.Equal(
            10d,
            second.Requested);

        Assert.Equal(
            0d,
            second.Reserved);

        Assert.Equal(
            10d,
            second.Shortfall);

        Assert.Equal(
            20d,
            budget.ReservedCapacity);

        Assert.Equal(
            0d,
            budget.RemainingCapacity);
    }

    [Fact]
    public void DifferentHitExecution_CannotConsumeBudget()
    {
        var owningHit =
            new HitExecutionId(7UL);

        var foreignHit =
            new HitExecutionId(8UL);

        var budget =
            new HitScopedProtectionBudget(
                owningHit,
                initialCapacity: 100d);

        Assert.Throws<InvalidOperationException>(
            () =>
                budget.Reserve(
                    foreignHit,
                    requestedCapacity: 40d));

        Assert.Equal(
            0d,
            budget.ReservedCapacity);

        Assert.Equal(
            100d,
            budget.RemainingCapacity);
    }

    [Fact]
    public void SeparateBudgetsForSameHit_DoNotShareCapacity()
    {
        var hitExecutionId =
            new HitExecutionId(7UL);

        var firstBudget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 100d);

        var secondBudget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 100d);

        firstBudget.Reserve(
            hitExecutionId,
            requestedCapacity: 70d);

        Assert.Equal(
            30d,
            firstBudget.RemainingCapacity);

        Assert.Equal(
            100d,
            secondBudget.RemainingCapacity);
    }

    [Fact]
    public void ZeroCapacityBudget_IsImmediatelyExhausted()
    {
        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 0d);

        Assert.Equal(
            0d,
            budget.RemainingCapacity);

        Assert.True(
            budget.IsExhausted);

        var reservation =
            budget.Reserve(
                budget.HitExecutionId,
                requestedCapacity: 10d);

        Assert.Equal(
            0d,
            reservation.Reserved);

        Assert.Equal(
            10d,
            reservation.Shortfall);
    }

    [Fact]
    public void ZeroReservation_DoesNotConsumeCapacity()
    {
        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 100d);

        var reservation =
            budget.Reserve(
                budget.HitExecutionId,
                requestedCapacity: 0d);

        Assert.Equal(
            0d,
            reservation.Requested);

        Assert.Equal(
            0d,
            reservation.Reserved);

        Assert.Equal(
            0d,
            reservation.Shortfall);

        Assert.Equal(
            0d,
            budget.ReservedCapacity);

        Assert.Equal(
            100d,
            budget.RemainingCapacity);
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidInitialCapacity_Throws(
        double capacity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new HitScopedProtectionBudget(
                    new HitExecutionId(7UL),
                    capacity));
    }

    [Theory]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidReservation_ThrowsWithoutConsumingCapacity(
        double requested)
    {
        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 100d);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                budget.Reserve(
                    budget.HitExecutionId,
                    requested));

        Assert.Equal(
            0d,
            budget.ReservedCapacity);

        Assert.Equal(
            100d,
            budget.RemainingCapacity);
    }

    [Fact]
    public void DefaultHitExecutionId_IsRejected()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new HitScopedProtectionBudget(
                    default,
                    initialCapacity: 100d));
    }
}