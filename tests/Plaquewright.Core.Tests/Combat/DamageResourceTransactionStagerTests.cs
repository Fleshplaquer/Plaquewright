using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Combat;

public sealed class DamageResourceTransactionStagerTests
{
    [Fact]
    public void Stage_AddsDamageDerivedLossToTransaction()
    {
        var setup =
            CreateSingleResourceSetup(
                current: 100d,
                maximum: 100d,
                damageTakenAmount: 70d);

        var plan =
            new DamageResourceLossPlan(
                setup.Context,
                requestedResourceLoss: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var preview =
            DamageResourceTransactionStager.Stage(
                draft,
                plan);

        Assert.Equal(
            1,
            draft.ProjectedResourceCount);

        Assert.Equal(
            1,
            draft.OperationCount);

        Assert.Equal(
            plan.Request,
            preview.Request);

        Assert.Equal(
            40d,
            preview.Result.RequestedLoss);

        Assert.Equal(
            40d,
            preview.Result.ActualLoss);

        Assert.Equal(
            60d,
            draft.Projections[0].Current);

        var operation =
            Assert.IsType<StagedResourceLossOperation>(
                draft.Operations[0]);

        Assert.Equal(
            setup.Entity.Id,
            operation.EntityId);

        Assert.Equal(
            setup.Context.ResourceTarget.ResourceId,
            operation.ResourceId);

        Assert.Equal(
            ResourceOperationCause.DamageDerived,
            operation.Provenance.Cause);

        // Staging remains projected only.
        Assert.Equal(
            100d,
            setup.State.Current);

        Assert.Equal(
            0UL,
            setup.State.Revision);
    }

    [Fact]
    public void Stage_AppliesExplicitResourcePreventionToProjection()
    {
        var setup =
            CreateSingleResourceSetup(
                current: 100d,
                maximum: 100d,
                damageTakenAmount: 70d);

        var plan =
            new DamageResourceLossPlan(
                setup.Context,
                requestedResourceLoss: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var preview =
            DamageResourceTransactionStager.Stage(
                draft,
                plan,
                preventedResourceLoss: 10d);

        Assert.Equal(
            40d,
            preview.Result.RequestedLoss);

        Assert.Equal(
            10d,
            preview.Result.PreventedLoss);

        Assert.Equal(
            30d,
            preview.Result.ActualLoss);

        Assert.Equal(
            0d,
            preview.Result.Shortfall);

        Assert.Equal(
            70d,
            draft.Projections[0].Current);

        Assert.Equal(
            100d,
            setup.State.Current);

        Assert.Equal(
            0UL,
            setup.State.Revision);
    }

    [Fact]
    public void SameDamageResolution_CanStageLossAgainstMultipleResources()
    {
        var registry =
            CreateMultiResourceRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var wardId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.ward"));

        var entity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d),

                    new ResourceState(
                        wardId,
                        current: 50d,
                        maximum: 50d)
                ]);

        var lifeTarget =
            new ResourceStateTarget(
                entity,
                lifeId);

        var wardTarget =
            new ResourceStateTarget(
                entity,
                wardId);

        var resolution =
            CreateResolution(
                targetEntityId: entity.Id,
                damageTakenAmount: 70d);

        var lifeContext =
            new DamageResourceTargetContext(
                resolution,
                lifeTarget);

        var wardContext =
            new DamageResourceTargetContext(
                resolution,
                wardTarget);

        var lifePlan =
            new DamageResourceLossPlan(
                lifeContext,
                requestedResourceLoss: 40d);

        var wardPlan =
            new DamageResourceLossPlan(
                wardContext,
                requestedResourceLoss: 30d);

        var draft =
            new ResourceTransactionDraft(
                registry);

        DamageResourceTransactionStager.Stage(
            draft,
            lifePlan);

        DamageResourceTransactionStager.Stage(
            draft,
            wardPlan);

        Assert.Equal(
            2,
            draft.ProjectedResourceCount);

        Assert.Equal(
            2,
            draft.OperationCount);

        Assert.Equal(
            lifeId,
            draft.Operations[0].ResourceId);

        Assert.Equal(
            wardId,
            draft.Operations[1].ResourceId);

        Assert.Equal(
            60d,
            draft.Projections[0].Current);

        Assert.Equal(
            20d,
            draft.Projections[1].Current);

        Assert.Equal(
            100d,
            lifeTarget.State.Current);

        Assert.Equal(
            50d,
            wardTarget.State.Current);
    }

    [Fact]
    public void StagedDamageLoss_CanCommitAtomicallyToStateAndLedger()
    {
        var setup =
            CreateSingleResourceSetup(
                current: 100d,
                maximum: 100d,
                damageTakenAmount: 70d);

        var plan =
            new DamageResourceLossPlan(
                setup.Context,
                requestedResourceLoss: 40d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        DamageResourceTransactionStager.Stage(
            draft,
            plan,
            preventedResourceLoss: 10d);

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        Assert.Equal(
            70d,
            setup.State.Current);

        Assert.Equal(
            1UL,
            setup.State.Revision);

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
            setup.Context.ResourceTarget.ResourceId,
            entry.ResourceId);

        Assert.Equal(
            40d,
            entry.Result.RequestedLoss);

        Assert.Equal(
            10d,
            entry.Result.PreventedLoss);

        Assert.Equal(
            30d,
            entry.Result.ActualLoss);

        Assert.Equal(
            ResourceOperationCause.DamageDerived,
            entry.Provenance.Cause);
    }

    [Fact]
    public void TargetFromDifferentRegistry_IsRejectedWithoutStaging()
    {
        var firstRegistry =
            CreateSingleResourceRegistry();

        var secondRegistry =
            CreateSingleResourceRegistry();

        var lifeId =
            secondRegistry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var entity =
            new EntityRuntimeState(
                new EntityId(2UL),
                secondRegistry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        var resourceTarget =
            new ResourceStateTarget(
                entity,
                lifeId);

        var resolution =
            CreateResolution(
                targetEntityId: entity.Id,
                damageTakenAmount: 50d);

        var targetContext =
            new DamageResourceTargetContext(
                resolution,
                resourceTarget);

        var plan =
            new DamageResourceLossPlan(
                targetContext,
                requestedResourceLoss: 25d);

        var draft =
            new ResourceTransactionDraft(
                firstRegistry);

        Assert.Throws<ArgumentException>(
            () =>
                DamageResourceTransactionStager.Stage(
                    draft,
                    plan));

        Assert.Equal(
            0,
            draft.ProjectedResourceCount);

        Assert.Equal(
            0,
            draft.OperationCount);

        Assert.Equal(
            100d,
            resourceTarget.State.Current);

        Assert.Equal(
            0UL,
            resourceTarget.State.Revision);
    }

    private static SingleResourceSetup CreateSingleResourceSetup(
        double current,
        double maximum,
        double damageTakenAmount)
    {
        var registry =
            CreateSingleResourceRegistry();

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var entity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current,
                        maximum)
                ]);

        var resourceTarget =
            new ResourceStateTarget(
                entity,
                lifeId);

        var resolution =
            CreateResolution(
                entity.Id,
                damageTakenAmount);

        var context =
            new DamageResourceTargetContext(
                resolution,
                resourceTarget);

        return new SingleResourceSetup(
            registry,
            entity,
            context,
            resourceTarget.State);
    }

    private static DamageResolutionContext CreateResolution(
        EntityId targetEntityId,
        double damageTakenAmount)
    {
        var damageExecution =
            new DamageExecutionContext(
                new DamageExecutionId(1UL),
                new ExecutionId(1UL),
                new EntityId(1UL),
                new SimulationTime(100L));

        var damageTarget =
            new DamageTargetContext(
                damageExecution.Id,
                targetEntityId,
                relatedHitExecutionId: null);

        var quantities =
            DamageResolutionTestFactory.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 90d,
                postTakenScalingAmount: 80d,
                damageTakenAmount: damageTakenAmount);

        return new DamageResolutionContext(
            damageExecution,
            damageTarget,
            quantities);
    }

    private static CompiledResourceRegistry CreateSingleResourceRegistry()
    {
        return ResourceRegistryCompiler.Compile(
        [
            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget |
                ResourceRole.DefeatRelevant)
        ]);
    }

    private static CompiledResourceRegistry CreateMultiResourceRegistry()
    {
        return ResourceRegistryCompiler.Compile(
        [
            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.life"),
                ResourceRole.DamageTarget |
                ResourceRole.DefeatRelevant),

            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.ward"),
                ResourceRole.DamageTarget)
        ]);
    }

    private sealed record SingleResourceSetup(
        CompiledResourceRegistry Registry,
        EntityRuntimeState Entity,
        DamageResourceTargetContext Context,
        ResourceState State);
}