using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Combat;

public sealed class ResourceBackedProtectionFinancingTests
{
    [Fact]
    public void Stage_WithPartialOneToOneFinancing_ProducesDamageShortfall()
    {
        var setup =
            CreateSetup(
                currentMana: 50d);

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 60d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var plan =
            new ResourceBackedProtectionFinancingPlan(
                financing,
                setup.ManaTarget);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var result =
            ProtectionResourceTransactionStager.Stage(
                draft,
                plan);

        Assert.Equal(
            60d,
            result.AssignedDamage);

        Assert.Equal(
            60d,
            result.RequestedResourceUnits);

        Assert.Equal(
            50d,
            result.ActualResourceUnitsSpent);

        Assert.Equal(
            10d,
            result.ResourceUnitShortfall);

        Assert.Equal(
            50d,
            result.FinancedDamage);

        Assert.Equal(
            10d,
            result.UnfinancedDamage);

        Assert.Equal(
            ProtectionFinancingShortfallPolicy.SpillBack,
            result.ShortfallPolicy);

        Assert.Equal(
            1,
            draft.ProjectedResourceCount);

        Assert.Equal(
            1,
            draft.OperationCount);

        // Projected only.
        Assert.Equal(
            50d,
            setup.ManaState.Current);

        Assert.Equal(
            0UL,
            setup.ManaState.Revision);
    }

    [Fact]
    public void Stage_WithTwoResourceUnitsPerDamage_PreservesUnitSeparation()
    {
        var setup =
            CreateSetup(
                currentMana: 50d);

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 60d,
                resourceUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var plan =
            new ResourceBackedProtectionFinancingPlan(
                financing,
                setup.ManaTarget);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var result =
            ProtectionResourceTransactionStager.Stage(
                draft,
                plan);

        Assert.Equal(
            120d,
            result.RequestedResourceUnits);

        Assert.Equal(
            50d,
            result.ActualResourceUnitsSpent);

        Assert.Equal(
            70d,
            result.ResourceUnitShortfall);

        Assert.Equal(
            25d,
            result.FinancedDamage);

        Assert.Equal(
            35d,
            result.UnfinancedDamage);

        Assert.Equal(
            50d,
            setup.ManaState.Current);

        Assert.Equal(
            0UL,
            setup.ManaState.Revision);
    }

    [Fact]
    public void Stage_WithSufficientResource_FinancesAllAssignedDamage()
    {
        var setup =
            CreateSetup(
                currentMana: 100d);

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 60d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var plan =
            new ResourceBackedProtectionFinancingPlan(
                financing,
                setup.ManaTarget);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var result =
            ProtectionResourceTransactionStager.Stage(
                draft,
                plan);

        Assert.Equal(
            60d,
            result.ActualResourceUnitsSpent);

        Assert.Equal(
            60d,
            result.FinancedDamage);

        Assert.Equal(
            0d,
            result.UnfinancedDamage);

        Assert.Equal(
            0d,
            result.ResourceUnitShortfall);

        Assert.Equal(
            40d,
            draft.Projections[0].Current);

        // Original remains untouched before commit.
        Assert.Equal(
            100d,
            setup.ManaState.Current);
    }

    [Fact]
    public void Stage_CreatesProtectionFinancingGrossOperation()
    {
        var setup =
            CreateSetup(
                currentMana: 50d);

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 60d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var plan =
            new ResourceBackedProtectionFinancingPlan(
                financing,
                setup.ManaTarget);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        ProtectionResourceTransactionStager.Stage(
            draft,
            plan);

        var operation =
            Assert.IsType<StagedResourceLossOperation>(
                draft.Operations[0]);

        Assert.Equal(
            setup.Entity.Id,
            operation.EntityId);

        Assert.Equal(
            setup.ManaId,
            operation.ResourceId);

        Assert.Equal(
            ResourceOperationCause.ProtectionFinancing,
            operation.Provenance.Cause);

        Assert.Equal(
            60d,
            operation.Preview.Result.RequestedLoss);

        Assert.Equal(
            50d,
            operation.Preview.Result.ActualLoss);

        Assert.Equal(
            10d,
            operation.Preview.Result.Shortfall);
    }

    [Fact]
    public void Commit_PublishesResourceLossWithProtectionFinancingProvenance()
    {
        var setup =
            CreateSetup(
                currentMana: 50d);

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 60d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var plan =
            new ResourceBackedProtectionFinancingPlan(
                financing,
                setup.ManaTarget);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var result =
            ProtectionResourceTransactionStager.Stage(
                draft,
                plan);

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        Assert.Equal(
            0d,
            setup.ManaState.Current);

        Assert.Equal(
            1UL,
            setup.ManaState.Revision);

        Assert.Equal(
            50d,
            result.ActualResourceUnitsSpent);

        Assert.Equal(
            50d,
            result.FinancedDamage);

        Assert.Equal(
            10d,
            result.UnfinancedDamage);

        Assert.Equal(
            1,
            ledger.Count);

        var entry =
            Assert.IsType<ResourceLossLedgerEntry>(
                ledger.Entries[0]);

        Assert.Equal(
            setup.Entity.Id,
            entry.TargetEntityId);

        Assert.Equal(
            setup.ManaId,
            entry.ResourceId);

        Assert.Equal(
            ResourceOperationCause.ProtectionFinancing,
            entry.Provenance.Cause);

        Assert.Equal(
            60d,
            entry.Result.RequestedLoss);

        Assert.Equal(
            50d,
            entry.Result.ActualLoss);

        Assert.Equal(
            10d,
            entry.Result.Shortfall);
    }

    [Fact]
    public void NonProtectionSourceResource_IsRejectedBeforeStaging()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.mana"),
                    ResourceRole.CostSource)
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
                        current: 50d,
                        maximum: 50d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                manaId);

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 40d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        Assert.Throws<InvalidOperationException>(
            () =>
                new ResourceBackedProtectionFinancingPlan(
                    financing,
                    target));

        Assert.Equal(
            50d,
            target.State.Current);

        Assert.Equal(
            0UL,
            target.State.Revision);
    }

    private static TestSetup CreateSetup(
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
                        current: currentMana,
                        maximum: 100d)
                ]);

        var manaTarget =
            new ResourceStateTarget(
                entity,
                manaId);

        return new TestSetup(
            registry,
            manaId,
            entity,
            manaTarget,
            manaTarget.State);
    }

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        ResourceId ManaId,
        EntityRuntimeState Entity,
        ResourceStateTarget ManaTarget,
        ResourceState ManaState);
}