using Idler.Core.Combat;
using Idler.Core.Entities;
using Idler.Core.Resources;

namespace Idler.Core.Tests.Combat;

public sealed class DefeatAwareResourceTransactionSingleOwnerGuardTests
{
    [Fact]
    public void MultiOwnerDraft_IsRejectedBeforeAnyStateBecomesVisible()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.life"),
                    ResourceRole.DamageTarget |
                    ResourceRole.DefeatRelevant)
            ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var firstEntity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        var secondEntity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                        lifeId,
                        current: 100d,
                        maximum: 100d)
                ]);

        var firstLife =
            new ResourceStateTarget(
                firstEntity,
                lifeId);

        var secondLife =
            new ResourceStateTarget(
                secondEntity,
                lifeId);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            firstLife,
            new ResourceLossRequest(
                lifeId,
                10d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        draft.StageLoss(
            secondLife,
            new ResourceLossRequest(
                lifeId,
                100d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        Assert.Equal(
            90d,
            draft.GetProjectedValues(
                firstLife)
            .Current);

        Assert.Equal(
            0d,
            draft.GetProjectedValues(
                secondLife)
            .Current);

        var ledger =
            new ResourceOperationLedger();

        Assert.Throws<InvalidOperationException>(
            () =>
                DefeatAwareResourceTransactionCommitter.Commit(
                    draft,
                    ledger,
                    firstEntity,
                    DefeatRelevantResourcePolicy.AnyDepleted));

        Assert.Equal(
            100d,
            firstLife.State.Current);

        Assert.Equal(
            100d,
            secondLife.State.Current);

        Assert.Equal(
            0UL,
            firstLife.State.Revision);

        Assert.Equal(
            0UL,
            secondLife.State.Revision);

        Assert.Equal(
            0,
            ledger.Count);

        Assert.Equal(
            90d,
            draft.GetProjectedValues(
                firstLife)
            .Current);

        Assert.Equal(
            0d,
            draft.GetProjectedValues(
                secondLife)
            .Current);
    }

    [Fact]
    public void ForeignOnlyDraft_IsRejectedBeforeAnyStateBecomesVisible()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.life"),
                    ResourceRole.DamageTarget |
                    ResourceRole.DefeatRelevant)
            ]);

        var evaluatedEntity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                        registry.GetId(
                            ResourceKey.Parse(
                                "resource.life")),
                        current: 100d,
                        maximum: 100d)
                ]);

        var foreignEntity =
            new EntityRuntimeState(
                new EntityId(2UL),
                registry,
                [
                    new ResourceState(
                        registry.GetId(
                            ResourceKey.Parse(
                                "resource.life")),
                        current: 100d,
                        maximum: 100d)
                ]);

        var lifeId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.life"));

        var evaluatedLife =
            new ResourceStateTarget(
                evaluatedEntity,
                lifeId);

        var foreignLife =
            new ResourceStateTarget(
                foreignEntity,
                lifeId);

        var draft =
            new ResourceTransactionDraft(
                registry);

        draft.StageLoss(
            foreignLife,
            new ResourceLossRequest(
                lifeId,
                100d,
                new ResourceOperationProvenance(
                    ResourceOperationCause.DamageDerived)));

        Assert.Equal(
            100d,
            evaluatedLife.State.Current);

        Assert.Equal(
            0d,
            draft.GetProjectedValues(
                foreignLife)
            .Current);

        var ledger =
            new ResourceOperationLedger();

        Assert.Throws<InvalidOperationException>(
            () =>
                DefeatAwareResourceTransactionCommitter.Commit(
                    draft,
                    ledger,
                    evaluatedEntity,
                    DefeatRelevantResourcePolicy.AnyDepleted));

        // The entity passed to the gate was never part
        // of the transaction.
        Assert.Equal(
            100d,
            evaluatedLife.State.Current);

        Assert.Equal(
            0UL,
            evaluatedLife.State.Revision);

        // The actual transaction owner must also remain
        // untouched.
        Assert.Equal(
            100d,
            foreignLife.State.Current);

        Assert.Equal(
            0UL,
            foreignLife.State.Revision);

        Assert.Equal(
            0,
            ledger.Count);

        // The rejected draft remains intact.
        Assert.Equal(
            0d,
            draft.GetProjectedValues(
                foreignLife)
            .Current);
    }
}