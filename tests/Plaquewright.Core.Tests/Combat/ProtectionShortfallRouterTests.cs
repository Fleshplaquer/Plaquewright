using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Combat;

public sealed class ProtectionShortfallRouterTests
{
    [Fact]
    public void SpillBack_RoutesAllUnfinancedDamageBackToOriginalPath()
    {
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

        Assert.Equal(
            10d,
            routing.UnfinancedDamage);

        Assert.Equal(
            10d,
            routing.SpillBackDamage);

        Assert.Equal(
            0d,
            routing.ContinueRoutingDamage);

        Assert.Equal(
            ProtectionFinancingShortfallPolicy.SpillBack,
            routing.Policy);
    }

    [Fact]
    public void ContinueRouting_RoutesAllUnfinancedDamageToNextProtectionRoute()
    {
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

        Assert.Equal(
            10d,
            routing.UnfinancedDamage);

        Assert.Equal(
            0d,
            routing.SpillBackDamage);

        Assert.Equal(
            10d,
            routing.ContinueRoutingDamage);

        Assert.Equal(
            ProtectionFinancingShortfallPolicy.ContinueRouting,
            routing.Policy);
    }

    [Fact]
    public void FullyFinancedDamage_ProducesNoRoutedShortfall()
    {
        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 60d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var financingResult =
            financing.Resolve(
                actualResourceUnitsSpent: 60d);

        var routing =
            ProtectionShortfallRouter.Route(
                financingResult);

        Assert.Equal(
            0d,
            routing.UnfinancedDamage);

        Assert.Equal(
            0d,
            routing.SpillBackDamage);

        Assert.Equal(
            0d,
            routing.ContinueRoutingDamage);
    }

    [Fact]
    public void TwoResourceUnitsPerDamage_RoutesDamageUnitsNotResourceUnits()
    {
        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 60d,
                resourceUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var financingResult =
            financing.Resolve(
                actualResourceUnitsSpent: 50d);

        var routing =
            ProtectionShortfallRouter.Route(
                financingResult);

        // 50 resource units at 2 resource / damage
        // finance 25 damage.
        Assert.Equal(
            25d,
            financingResult.FinancedDamage);

        // Therefore 35 DAMAGE remains.
        Assert.Equal(
            35d,
            routing.UnfinancedDamage);

        Assert.Equal(
            35d,
            routing.SpillBackDamage);

        // Resource shortfall is a different unit.
        Assert.Equal(
            70d,
            financingResult.ResourceUnitShortfall);

        Assert.NotEqual(
            financingResult.ResourceUnitShortfall,
            routing.SpillBackDamage);
    }

    [Fact]
    public void ResourceBackedResult_CanBeRoutedDirectly()
    {
        var setup =
            CreateResourceSetup(
                currentMana: 50d);

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 60d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var plan =
            new ResourceBackedProtectionFinancingPlan(
                financing,
                setup.Target);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var financingResult =
            ProtectionResourceTransactionStager.Stage(
                draft,
                plan);

        var routing =
            ProtectionShortfallRouter.Route(
                financingResult);

        Assert.Equal(
            50d,
            financingResult.FinancedDamage);

        Assert.Equal(
            10d,
            routing.SpillBackDamage);

        Assert.Equal(
            0d,
            routing.ContinueRoutingDamage);

        // Still projected only.
        Assert.Equal(
            50d,
            setup.State.Current);

        Assert.Equal(
            0UL,
            setup.State.Revision);
    }

    [Fact]
    public void RoutingAlwaysAccountsForAllUnfinancedDamage()
    {
        foreach (var policy in new[]
        {
            ProtectionFinancingShortfallPolicy.SpillBack,
            ProtectionFinancingShortfallPolicy.ContinueRouting
        })
        {
            var financing =
                new ProtectionFinancingPlan(
                    assignedDamage: 100d,
                    resourceUnitsPerDamage: 1d,
                    policy);

            var financingResult =
                financing.Resolve(
                    actualResourceUnitsSpent: 30d);

            var routing =
                ProtectionShortfallRouter.Route(
                    financingResult);

            Assert.Equal(
                routing.UnfinancedDamage,
                routing.SpillBackDamage +
                routing.ContinueRoutingDamage);
        }
    }

    private static ResourceSetup CreateResourceSetup(
        double currentMana)
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.mana"),
                    ResourceRole.ProtectionSource)
            ]);

        var manaId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                        manaId,
                        currentMana,
                        maximum: 100d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                manaId);

        return new ResourceSetup(
            registry,
            target,
            target.State);
    }

    private sealed record ResourceSetup(
        CompiledResourceRegistry Registry,
        ResourceStateTarget Target,
        ResourceState State);
}