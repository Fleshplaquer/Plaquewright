using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class ProtectionRouteChainStateTests
{
    [Fact]
    public void Start_PreservesPrimaryAndProtectionRoutingDamage()
    {
        var state =
            ProtectionRouteChainState.Start(
                primaryPathDamage: 60d,
                protectionRoutingDamage: 60d);

        Assert.Equal(
            120d,
            state.InitialDamage);

        Assert.Equal(
            60d,
            state.PrimaryPathDamage);

        Assert.Equal(
            60d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            0d,
            state.FinancedDamage);

        Assert.Equal(
            0,
            state.AppliedRouteCount);

        Assert.False(
            state.IsComplete);

        Assert.Equal(
            120d,
            state.AccountedDamage);
    }

    [Fact]
    public void SpillBack_AddsUnfinancedDamageToPrimaryPath()
    {
        var initial =
            ProtectionRouteChainState.Start(
                primaryPathDamage: 60d,
                protectionRoutingDamage: 60d);

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 60d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var financingResult =
            financing.Resolve(
                actualResourceUnitsSpent: 50d);

        var routing =
            ProtectionShortfallRouter.Route(
                financingResult);

        var result =
            initial.Apply(
                routing);

        Assert.Equal(
            120d,
            result.InitialDamage);

        Assert.Equal(
            70d,
            result.PrimaryPathDamage);

        Assert.Equal(
            0d,
            result.ContinueRoutingDamage);

        Assert.Equal(
            50d,
            result.FinancedDamage);

        Assert.Equal(
            1,
            result.AppliedRouteCount);

        Assert.True(
            result.IsComplete);

        Assert.Equal(
            120d,
            result.AccountedDamage);

        // Previous state remains unchanged.
        Assert.Equal(
            60d,
            initial.PrimaryPathDamage);

        Assert.Equal(
            60d,
            initial.ContinueRoutingDamage);

        Assert.Equal(
            0d,
            initial.FinancedDamage);
    }

    [Fact]
    public void ContinueRouting_ForwardsOnlyUnfinancedDamageToNextRoute()
    {
        var initial =
            ProtectionRouteChainState.Start(
                primaryPathDamage: 60d,
                protectionRoutingDamage: 60d);

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 60d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.ContinueRouting);

        var financingResult =
            financing.Resolve(
                actualResourceUnitsSpent: 50d);

        var routing =
            ProtectionShortfallRouter.Route(
                financingResult);

        var result =
            initial.Apply(
                routing);

        Assert.Equal(
            60d,
            result.PrimaryPathDamage);

        Assert.Equal(
            10d,
            result.ContinueRoutingDamage);

        Assert.Equal(
            50d,
            result.FinancedDamage);

        Assert.Equal(
            120d,
            result.AccountedDamage);

        Assert.False(
            result.IsComplete);
    }

    [Fact]
    public void MultipleRoutes_AreAppliedSequentially()
    {
        var initial =
            ProtectionRouteChainState.Start(
                primaryPathDamage: 60d,
                protectionRoutingDamage: 60d);

        var firstFinancing =
            new ProtectionFinancingPlan(
                assignedDamage: 60d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.ContinueRouting);

        var firstRouting =
            ProtectionShortfallRouter.Route(
                firstFinancing.Resolve(
                    actualResourceUnitsSpent: 50d));

        var afterFirst =
            initial.Apply(
                firstRouting);

        Assert.Equal(
            10d,
            afterFirst.ContinueRoutingDamage);

        var secondFinancing =
            new ProtectionFinancingPlan(
                assignedDamage:
                    afterFirst.ContinueRoutingDamage,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var secondRouting =
            ProtectionShortfallRouter.Route(
                secondFinancing.Resolve(
                    actualResourceUnitsSpent: 4d));

        var afterSecond =
            afterFirst.Apply(
                secondRouting);

        Assert.Equal(
            66d,
            afterSecond.PrimaryPathDamage);

        Assert.Equal(
            0d,
            afterSecond.ContinueRoutingDamage);

        Assert.Equal(
            54d,
            afterSecond.FinancedDamage);

        Assert.Equal(
            2,
            afterSecond.AppliedRouteCount);

        Assert.True(
            afterSecond.IsComplete);

        Assert.Equal(
            120d,
            afterSecond.AccountedDamage);
    }

    [Fact]
    public void FullyFinancedRoute_ConsumesAllProtectionRoutingDamage()
    {
        var initial =
            ProtectionRouteChainState.Start(
                primaryPathDamage: 60d,
                protectionRoutingDamage: 60d);

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 60d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.ContinueRouting);

        var routing =
            ProtectionShortfallRouter.Route(
                financing.Resolve(
                    actualResourceUnitsSpent: 60d));

        var result =
            initial.Apply(
                routing);

        Assert.Equal(
            60d,
            result.PrimaryPathDamage);

        Assert.Equal(
            0d,
            result.ContinueRoutingDamage);

        Assert.Equal(
            60d,
            result.FinancedDamage);

        Assert.True(
            result.IsComplete);

        Assert.Equal(
            120d,
            result.AccountedDamage);
    }

    [Fact]
    public void RouteAssignedDifferentDamageThanCurrentRoutingAmount_IsRejected()
    {
        var state =
            ProtectionRouteChainState.Start(
                primaryPathDamage: 60d,
                protectionRoutingDamage: 60d);

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 50d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var routing =
            ProtectionShortfallRouter.Route(
                financing.Resolve(
                    actualResourceUnitsSpent: 40d));

        Assert.Throws<InvalidOperationException>(
            () =>
                state.Apply(
                    routing));

        Assert.Equal(
            60d,
            state.PrimaryPathDamage);

        Assert.Equal(
            60d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            0d,
            state.FinancedDamage);

        Assert.Equal(
            0,
            state.AppliedRouteCount);
    }

    [Fact]
    public void ZeroProtectionRoutingDamage_StartsComplete()
    {
        var state =
            ProtectionRouteChainState.Start(
                primaryPathDamage: 100d,
                protectionRoutingDamage: 0d);

        Assert.Equal(
            100d,
            state.InitialDamage);

        Assert.Equal(
            100d,
            state.PrimaryPathDamage);

        Assert.Equal(
            0d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            0d,
            state.FinancedDamage);

        Assert.True(
            state.IsComplete);
    }

    [Fact]
    public void EveryAppliedRoute_PreservesDamageAccounting()
    {
        var state =
            ProtectionRouteChainState.Start(
                primaryPathDamage: 30d,
                protectionRoutingDamage: 70d);

        Assert.Equal(
            state.InitialDamage,
            state.AccountedDamage);

        var first =
            new ProtectionFinancingPlan(
                assignedDamage: 70d,
                resourceUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.ContinueRouting);

        state =
            state.Apply(
                ProtectionShortfallRouter.Route(
                    first.Resolve(
                        actualResourceUnitsSpent: 100d)));

        Assert.Equal(
            50d,
            state.FinancedDamage);

        Assert.Equal(
            20d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            state.InitialDamage,
            state.AccountedDamage);

        var second =
            new ProtectionFinancingPlan(
                assignedDamage:
                    state.ContinueRoutingDamage,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        state =
            state.Apply(
                ProtectionShortfallRouter.Route(
                    second.Resolve(
                        actualResourceUnitsSpent: 5d)));

        Assert.Equal(
            45d,
            state.PrimaryPathDamage);

        Assert.Equal(
            55d,
            state.FinancedDamage);

        Assert.Equal(
            0d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            state.InitialDamage,
            state.AccountedDamage);
    }
}