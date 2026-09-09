using Idler.Core.Combat;
using Idler.Core.Entities;
using Idler.Core.Resources;
using Idler.Core.Simulation;

namespace Idler.Core.Tests.Combat;

public sealed class DamageResourceRoutingInvariantTests
{
    [Fact]
    public void StagedOperation_ProducesTypedProjectedRoutingResult()
    {
        var setup =
            CreateSetup(
                current: 100d,
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

        var operation =
            Assert.IsType<StagedResourceLossOperation>(
                draft.Operations[0]);

        var result =
            new DamageResourceLossResult(
                plan,
                operation);

        Assert.Equal(
            preview.Result,
            result.ResourceResult);

        Assert.Equal(
            70d,
            result.DamageTaken.Amount);

        Assert.Equal(
            40d,
            result.RequestedResourceLoss);

        Assert.Equal(
            10d,
            result.PreventedResourceLoss);

        Assert.Equal(
            30d,
            result.ActualResourceLoss.Amount);

        Assert.Equal(
            0d,
            result.Shortfall);

        Assert.Equal(
            setup.Entity.Id,
            result.TargetEntityId);

        Assert.Equal(
            setup.LifeId,
            result.ResourceId);

        // Still projected only.
        Assert.Equal(
            100d,
            setup.State.Current);

        Assert.Equal(
            0UL,
            setup.State.Revision);
    }

    [Fact]
    public void TypedRoutingResult_AndCommittedLedgerRemainQuantitativelyConsistent()
    {
        var setup =
            CreateSetup(
                current: 100d,
                damageTakenAmount: 80d);

        var plan =
            new DamageResourceLossPlan(
                setup.Context,
                requestedResourceLoss: 50d);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        DamageResourceTransactionStager.Stage(
            draft,
            plan,
            preventedResourceLoss: 15d);

        var operation =
            Assert.IsType<StagedResourceLossOperation>(
                draft.Operations[0]);

        var routingResult =
            new DamageResourceLossResult(
                plan,
                operation);

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        Assert.Equal(
            65d,
            setup.State.Current);

        Assert.Equal(
            1UL,
            setup.State.Revision);

        var ledgerEntry =
            Assert.IsType<ResourceLossLedgerEntry>(
                ledger.Entries[0]);

        Assert.Equal(
            routingResult.TargetEntityId,
            ledgerEntry.TargetEntityId);

        Assert.Equal(
            routingResult.ResourceId,
            ledgerEntry.ResourceId);

        Assert.Equal(
            routingResult.RequestedResourceLoss,
            ledgerEntry.Result.RequestedLoss);

        Assert.Equal(
            routingResult.PreventedResourceLoss,
            ledgerEntry.Result.PreventedLoss);

        Assert.Equal(
            routingResult.ActualResourceLoss.Amount,
            ledgerEntry.Result.ActualLoss);

        Assert.Equal(
            routingResult.Shortfall,
            ledgerEntry.Result.Shortfall);

        Assert.Equal(
            ResourceOperationCause.DamageDerived,
            ledgerEntry.Provenance.Cause);
    }

    [Fact]
    public void TypedProjectedResult_DoesNotMeanTransactionWasCommitted()
    {
        var setup =
            CreateSetup(
                current: 100d,
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
            plan);

        var operation =
            Assert.IsType<StagedResourceLossOperation>(
                draft.Operations[0]);

        var result =
            new DamageResourceLossResult(
                plan,
                operation);

        Assert.Equal(
            40d,
            result.ActualResourceLoss.Amount);

        Assert.Equal(
            100d,
            setup.State.Current);

        Assert.Equal(
            0UL,
            setup.State.Revision);
    }

    [Fact]
    public void StaleTransaction_CanAbortAfterProjectedRoutingResultExists()
    {
        var setup =
            CreateSetup(
                current: 100d,
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
            plan);

        var operation =
            Assert.IsType<StagedResourceLossOperation>(
                draft.Operations[0]);

        var result =
            new DamageResourceLossResult(
                plan,
                operation);

        Assert.Equal(
            40d,
            result.ActualResourceLoss.Amount);

        // External mutation makes the transaction stale.
        setup.State.SetValues(
            current: 90d,
            maximum: 100d);

        var ledger =
            new ResourceOperationLedger();

        Assert.Throws<InvalidOperationException>(
            () =>
                ResourceTransactionCommitter.Commit(
                    draft,
                    ledger));

        // Only the external mutation is visible.
        Assert.Equal(
            90d,
            setup.State.Current);

        Assert.Equal(
            1UL,
            setup.State.Revision);

        Assert.Equal(
            0,
            ledger.Count);
    }

    [Fact]
    public void StagedOperationForDifferentTarget_IsRejectedByRoutingResult()
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var firstEntity =
            CreateEntity(
                registry,
                entityId: 2UL,
                current: 100d);

        var secondEntity =
            CreateEntity(
                registry,
                entityId: 3UL,
                current: 100d);

        var firstTarget =
            new ResourceStateTarget(
                firstEntity,
                lifeId);

        var secondTarget =
            new ResourceStateTarget(
                secondEntity,
                lifeId);

        var firstContext =
            new DamageResourceTargetContext(
                CreateResolution(
                    firstEntity.Id,
                    damageTakenAmount: 70d),
                firstTarget);

        var secondContext =
            new DamageResourceTargetContext(
                CreateResolution(
                    secondEntity.Id,
                    damageTakenAmount: 70d),
                secondTarget);

        var firstPlan =
            new DamageResourceLossPlan(
                firstContext,
                requestedResourceLoss: 40d);

        var secondPlan =
            new DamageResourceLossPlan(
                secondContext,
                requestedResourceLoss: 40d);

        var draft =
            new ResourceTransactionDraft(
                registry);

        DamageResourceTransactionStager.Stage(
            draft,
            secondPlan);

        var secondOperation =
            Assert.IsType<StagedResourceLossOperation>(
                draft.Operations[0]);

        Assert.Throws<InvalidOperationException>(
            () =>
                new DamageResourceLossResult(
                    firstPlan,
                    secondOperation));

        Assert.Equal(
            100d,
            firstTarget.State.Current);

        Assert.Equal(
            100d,
            secondTarget.State.Current);
    }

    private static TestSetup CreateSetup(
        double current,
        double damageTakenAmount)
    {
        var registry =
            CreateRegistry();

        var lifeId =
            GetLifeId(
                registry);

        var entity =
            CreateEntity(
                registry,
                entityId: 2UL,
                current: current);

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

        return new TestSetup(
            registry,
            lifeId,
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
            DamageResolutionQuantities.Create(
                new IncomingDamage(100d),
                postMitigationAmount: 90d,
                postTakenScalingAmount: 80d,
                damageTakenAmount: damageTakenAmount);

        return new DamageResolutionContext(
            damageExecution,
            damageTarget,
            quantities);
    }

    private static EntityRuntimeState CreateEntity(
        CompiledResourceRegistry registry,
        ulong entityId,
        double current)
    {
        return new EntityRuntimeState(
            new EntityId(entityId),
            registry,
            [
                new ResourceState(
                    GetLifeId(registry),
                    current,
                    maximum: 100d)
            ]);
    }

    private static ResourceId GetLifeId(
        CompiledResourceRegistry registry)
    {
        return registry.GetId(
            ResourceKey.Parse(
                "resource.life"));
    }

    private static CompiledResourceRegistry CreateRegistry()
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

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        ResourceId LifeId,
        EntityRuntimeState Entity,
        DamageResourceTargetContext Context,
        ResourceState State);
}