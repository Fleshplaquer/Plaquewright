using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;

namespace Plaquewright.Core.Tests.Combat;

public sealed class ProtectionAssignmentRoutingFinalizerTests
{
    [Fact]
    public void Finalize_MovesUnresolvedContinueRoutingDamageToPrimaryPath()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.4d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        var assignment =
            resolution.Assignments[0];

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 40d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.ContinueRouting);

        state =
            state.Apply(
                assignment,
                ProtectionShortfallRouter.Route(
                    financing.Resolve(
                        actualResourceUnitsSpent: 25d)));

        Assert.Equal(
            60d,
            state.PrimaryPathDamage);

        Assert.Equal(
            15d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            25d,
            state.FinancedDamage);

        Assert.False(
            state.IsComplete);

        var finalized =
            ProtectionAssignmentRoutingFinalizer.Finalize(
                state);

        Assert.Equal(
            75d,
            finalized.PrimaryPathDamage);

        Assert.Equal(
            0d,
            finalized.ContinueRoutingDamage);

        Assert.Equal(
            25d,
            finalized.FinancedDamage);

        Assert.True(
            finalized.IsComplete);

        Assert.Equal(
            100d,
            finalized.AccountedDamage);

        // Original immutable state remains unresolved.
        Assert.Equal(
            15d,
            state.ContinueRoutingDamage);
    }

    [Fact]
    public void Finalize_CollectsUnresolvedDamageFromMultipleLanes()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d),

                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.2d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        var firstAssignment =
            resolution.Assignments[0];

        state =
            state.Apply(
                firstAssignment,
                ProtectionShortfallRouter.Route(
                    new ProtectionFinancingPlan(
                        assignedDamage: 30d,
                        resourceUnitsPerDamage: 1d,
                        ProtectionFinancingShortfallPolicy.ContinueRouting)
                    .Resolve(
                        actualResourceUnitsSpent: 20d)));

        var secondAssignment =
            resolution.Assignments[1];

        state =
            state.Apply(
                secondAssignment,
                ProtectionShortfallRouter.Route(
                    new ProtectionFinancingPlan(
                        assignedDamage: 20d,
                        resourceUnitsPerDamage: 1d,
                        ProtectionFinancingShortfallPolicy.ContinueRouting)
                    .Resolve(
                        actualResourceUnitsSpent: 5d)));

        // Base primary = 50
        // Lane A: 20 financed, 10 unresolved
        // Lane B: 5 financed, 15 unresolved
        Assert.Equal(
            50d,
            state.PrimaryPathDamage);

        Assert.Equal(
            25d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            25d,
            state.FinancedDamage);

        var finalized =
            ProtectionAssignmentRoutingFinalizer.Finalize(
                state);

        Assert.Equal(
            75d,
            finalized.PrimaryPathDamage);

        Assert.Equal(
            25d,
            finalized.FinancedDamage);

        Assert.Equal(
            0d,
            finalized.ContinueRoutingDamage);

        Assert.True(
            finalized.IsComplete);

        Assert.Equal(
            100d,
            finalized.AccountedDamage);

        Assert.True(
            finalized.Lanes[0].IsComplete);

        Assert.True(
            finalized.Lanes[1].IsComplete);

        Assert.Equal(
            10d,
            finalized.Lanes[0].SpillBackDamage);

        Assert.Equal(
            15d,
            finalized.Lanes[1].SpillBackDamage);
    }

    [Fact]
    public void Finalize_CompleteStateReturnsSameInstance()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        var assignment =
            resolution.Assignments[0];

        state =
            state.Apply(
                assignment,
                ProtectionShortfallRouter.Route(
                    new ProtectionFinancingPlan(
                        assignedDamage: 30d,
                        resourceUnitsPerDamage: 1d,
                        ProtectionFinancingShortfallPolicy.SpillBack)
                    .Resolve(
                        actualResourceUnitsSpent: 20d)));

        Assert.True(
            state.IsComplete);

        var finalized =
            ProtectionAssignmentRoutingFinalizer.Finalize(
                state);

        Assert.Same(
            state,
            finalized);

        Assert.Equal(
            100d,
            finalized.AccountedDamage);
    }

    [Fact]
    public void Finalize_PreservesHitExecutionIdentity()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.4d)
                ]);

        var hitExecutionId =
            new HitExecutionId(7UL);

        var damageTarget =
            new DamageTargetContext(
                new DamageExecutionId(1UL),
                new EntityId(2UL),
                hitExecutionId);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution,
                damageTarget);

        var assignment =
            resolution.Assignments[0];

        state =
            state.Apply(
                assignment,
                ProtectionShortfallRouter.Route(
                    new ProtectionFinancingPlan(
                        assignedDamage: 40d,
                        resourceUnitsPerDamage: 1d,
                        ProtectionFinancingShortfallPolicy.ContinueRouting)
                    .Resolve(
                        actualResourceUnitsSpent: 10d)));

        Assert.Equal(
            30d,
            state.ContinueRoutingDamage);

        var finalized =
            ProtectionAssignmentRoutingFinalizer.Finalize(
                state);

        Assert.True(
            finalized.IsHitBased);

        Assert.Equal(
            hitExecutionId,
            finalized.RelatedHitExecutionId);

        Assert.Equal(
            0d,
            finalized.ContinueRoutingDamage);

        Assert.True(
            finalized.IsComplete);
    }

    [Fact]
    public void Finalization_DoesNotCountAsAdditionalProtectionRoute()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.4d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        var assignment =
            resolution.Assignments[0];

        state =
            state.Apply(
                assignment,
                ProtectionShortfallRouter.Route(
                    new ProtectionFinancingPlan(
                        assignedDamage: 40d,
                        resourceUnitsPerDamage: 1d,
                        ProtectionFinancingShortfallPolicy.ContinueRouting)
                    .Resolve(
                        actualResourceUnitsSpent: 10d)));

        Assert.Equal(
            1,
            state.Lanes[0].Chain.AppliedRouteCount);

        var finalized =
            ProtectionAssignmentRoutingFinalizer.Finalize(
                state);

        Assert.Equal(
            1,
            finalized.Lanes[0].Chain.AppliedRouteCount);

        Assert.True(
            finalized.Lanes[0].IsComplete);
    }

    [Fact]
    public void Finalize_WithNoAssignments_IsNoOp()
    {
        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                Array.Empty<ProtectionAssignmentRequest>());

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        Assert.True(
            state.IsComplete);

        var finalized =
            ProtectionAssignmentRoutingFinalizer.Finalize(
                state);

        Assert.Same(
            state,
            finalized);

        Assert.Equal(
            100d,
            finalized.PrimaryPathDamage);

        Assert.Equal(
            0d,
            finalized.ContinueRoutingDamage);

        Assert.Equal(
            100d,
            finalized.AccountedDamage);
    }
}