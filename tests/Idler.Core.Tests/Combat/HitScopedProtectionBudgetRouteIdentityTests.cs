using Idler.Core.Combat;
using Idler.Core.Entities;

namespace Idler.Core.Tests.Combat;

public sealed class HitScopedProtectionBudgetRouteIdentityTests
{
    [Fact]
    public void HitBasedStarter_PreservesHitExecutionIdentity()
    {
        var resolution =
            CreateAssignmentResolution();

        var hitExecutionId =
            new HitExecutionId(7UL);

        var damageTarget =
            CreateDamageTarget(
                hitExecutionId);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution,
                damageTarget);

        Assert.True(
            state.IsHitBased);

        Assert.Equal(
            hitExecutionId,
            state.RelatedHitExecutionId);

        Assert.Equal(
            100d,
            state.AccountedDamage);
    }

    [Fact]
    public void NonHitDamageTarget_CannotStartHitBasedRoutingState()
    {
        var resolution =
            CreateAssignmentResolution();

        var damageTarget =
            new DamageTargetContext(
                new DamageExecutionId(1UL),
                new EntityId(2UL),
                relatedHitExecutionId: null);

        Assert.False(
            damageTarget.IsHitBased);

        Assert.Throws<InvalidOperationException>(
            () =>
                ProtectionAssignmentRoutingStarter.Start(
                    resolution,
                    damageTarget));
    }

    [Fact]
    public void UnscopedRoutingState_CannotConsumeHitScopedBudget()
    {
        var resolution =
            CreateAssignmentResolution();

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        Assert.False(
            state.IsHitBased);

        var budget =
            new HitScopedProtectionBudget(
                new HitExecutionId(7UL),
                initialCapacity: 100d);

        var binding =
            new HitScopedProtectionBudgetRouteBinding(
                resolution.Assignments[0],
                budget,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        Assert.Throws<InvalidOperationException>(
            () =>
                HitScopedProtectionBudgetRouteExecutor.Execute(
                    state,
                    binding));

        Assert.Equal(
            0d,
            budget.ReservedCapacity);

        Assert.Equal(
            100d,
            budget.RemainingCapacity);

        Assert.Equal(
            100d,
            state.AccountedDamage);
    }

    [Fact]
    public void BudgetForDifferentHit_IsRejectedBeforeReservation()
    {
        var resolution =
            CreateAssignmentResolution();

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution,
                CreateDamageTarget(
                    new HitExecutionId(7UL)));

        var foreignBudget =
            new HitScopedProtectionBudget(
                new HitExecutionId(8UL),
                initialCapacity: 100d);

        var binding =
            new HitScopedProtectionBudgetRouteBinding(
                resolution.Assignments[0],
                foreignBudget,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        Assert.Throws<InvalidOperationException>(
            () =>
                HitScopedProtectionBudgetRouteExecutor.Execute(
                    state,
                    binding));

        Assert.Equal(
            0d,
            foreignBudget.ReservedCapacity);

        Assert.Equal(
            100d,
            foreignBudget.RemainingCapacity);

        Assert.Equal(
            30d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            0d,
            state.FinancedDamage);
    }

    [Fact]
    public void BudgetForSameHit_CanExecute()
    {
        var resolution =
            CreateAssignmentResolution();

        var hitExecutionId =
            new HitExecutionId(7UL);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution,
                CreateDamageTarget(
                    hitExecutionId));

        var budget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 20d);

        var binding =
            new HitScopedProtectionBudgetRouteBinding(
                resolution.Assignments[0],
                budget,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var execution =
            HitScopedProtectionBudgetRouteExecutor.Execute(
                state,
                binding);

        Assert.Equal(
            20d,
            execution.FinancedDamage);

        Assert.Equal(
            10d,
            execution.SpillBackDamage);

        Assert.Equal(
            0d,
            budget.RemainingCapacity);

        Assert.Equal(
            80d,
            execution.UpdatedState.PrimaryPathDamage);

        Assert.Equal(
            20d,
            execution.UpdatedState.FinancedDamage);

        Assert.True(
            execution.UpdatedState.IsComplete);

        Assert.Equal(
            100d,
            execution.UpdatedState.AccountedDamage);
    }

    [Fact]
    public void ApplyingRoute_PreservesRoutingHitIdentity()
    {
        var resolution =
            CreateAssignmentResolution();

        var hitExecutionId =
            new HitExecutionId(7UL);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution,
                CreateDamageTarget(
                    hitExecutionId));

        var budget =
            new HitScopedProtectionBudget(
                hitExecutionId,
                initialCapacity: 10d);

        var binding =
            new HitScopedProtectionBudgetRouteBinding(
                resolution.Assignments[0],
                budget,
                budgetUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.ContinueRouting);

        var execution =
            HitScopedProtectionBudgetRouteExecutor.Execute(
                state,
                binding);

        Assert.Equal(
            hitExecutionId,
            state.RelatedHitExecutionId);

        Assert.Equal(
            hitExecutionId,
            execution.UpdatedState.RelatedHitExecutionId);

        Assert.True(
            execution.UpdatedState.IsHitBased);

        Assert.Equal(
            20d,
            execution.UpdatedState.ContinueRoutingDamage);
    }

    private static ProtectionAssignmentResolution
        CreateAssignmentResolution()
    {
        return ProtectionAssignmentResolver.Resolve(
            damage: 100d,
            [
                new ProtectionAssignmentRequest(
                    requestedFraction: 0.3d)
            ]);
    }

    private static DamageTargetContext CreateDamageTarget(
        HitExecutionId hitExecutionId)
    {
        return new DamageTargetContext(
            new DamageExecutionId(1UL),
            new EntityId(2UL),
            hitExecutionId);
    }
}