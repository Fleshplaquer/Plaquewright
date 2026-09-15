using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Combat;

public sealed class ProtectionResourceRouteBatchExecutorTests
{
    [Fact]
    public void SharedResourceClaims_AreAllocatedProportionallyBeforeStaging()
    {
        var setup =
            CreateSetup(
                currentMana: 35d);

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

        var firstBinding =
            new ProtectionResourceRouteBinding(
                resolution.Assignments[0],
                setup.ManaTarget,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var secondBinding =
            new ProtectionResourceRouteBinding(
                resolution.Assignments[1],
                setup.ManaTarget,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var execution =
            ProtectionResourceRouteBatchExecutor.Stage(
                draft,
                state,
                [
                    firstBinding,
                    secondBinding
                ]);

        Assert.Same(
            state,
            execution.PreviousState);

        Assert.Equal(
            2,
            execution.Items.Count);

        var first =
            execution.Items[0];

        var second =
            execution.Items[1];

        Assert.Equal(
            30d,
            first.RequestedResourceUnits);

        Assert.Equal(
            21d,
            first.AllocatedResourceUnits);

        Assert.Equal(
            21d,
            first.ActualResourceUnitsSpent);

        Assert.Equal(
            9d,
            first.ResourceUnitShortfall);

        Assert.Equal(
            21d,
            first.FinancedDamage);

        Assert.Equal(
            9d,
            first.SpillBackDamage);

        Assert.Equal(
            20d,
            second.RequestedResourceUnits);

        Assert.Equal(
            14d,
            second.AllocatedResourceUnits);

        Assert.Equal(
            14d,
            second.ActualResourceUnitsSpent);

        Assert.Equal(
            6d,
            second.ResourceUnitShortfall);

        Assert.Equal(
            14d,
            second.FinancedDamage);

        Assert.Equal(
            6d,
            second.SpillBackDamage);

        var updated =
            execution.UpdatedState;

        Assert.Equal(
            65d,
            updated.PrimaryPathDamage);

        Assert.Equal(
            35d,
            updated.FinancedDamage);

        Assert.Equal(
            0d,
            updated.ContinueRoutingDamage);

        Assert.True(
            updated.IsComplete);

        Assert.Equal(
            100d,
            updated.AccountedDamage);

        // One shared concrete resource projection,
        // but one causal gross resource operation per claim.
        Assert.Equal(
            1,
            draft.ProjectedResourceCount);

        Assert.Equal(
            2,
            draft.OperationCount);

        Assert.Equal(
            0d,
            draft.Projections[0].Current);

        var firstOperation =
            Assert.IsType<StagedResourceLossOperation>(
                draft.Operations[0]);

        var secondOperation =
            Assert.IsType<StagedResourceLossOperation>(
                draft.Operations[1]);

        // Resource ledger truth is the allocated resource loss,
        // not the original financing claim.
        Assert.Equal(
            21d,
            firstOperation.Preview.Request.Amount);

        Assert.Equal(
            21d,
            firstOperation.Preview.Result.ActualLoss);

        Assert.Equal(
            14d,
            secondOperation.Preview.Request.Amount);

        Assert.Equal(
            14d,
            secondOperation.Preview.Result.ActualLoss);

        // Still projected only.
        Assert.Equal(
            35d,
            setup.ManaState.Current);

        Assert.Equal(
            0UL,
            setup.ManaState.Revision);
    }

    [Fact]
    public void BindingOrder_DoesNotCreateSharedResourcePriority()
    {
        var setup =
            CreateSetup(
                currentMana: 35d);

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

        var thirtyClaim =
            new ProtectionResourceRouteBinding(
                resolution.Assignments[0],
                setup.ManaTarget,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var twentyClaim =
            new ProtectionResourceRouteBinding(
                resolution.Assignments[1],
                setup.ManaTarget,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var execution =
            ProtectionResourceRouteBatchExecutor.Stage(
                draft,
                state,
                [
                    twentyClaim,
                    thirtyClaim
                ]);

        Assert.Same(
            twentyClaim,
            execution.Items[0].Binding);

        Assert.Equal(
            14d,
            execution.Items[0].AllocatedResourceUnits);

        Assert.Same(
            thirtyClaim,
            execution.Items[1].Binding);

        Assert.Equal(
            21d,
            execution.Items[1].AllocatedResourceUnits);

        Assert.Equal(
            35d,
            execution.UpdatedState.FinancedDamage);

        Assert.Equal(
            65d,
            execution.UpdatedState.PrimaryPathDamage);
    }

    [Fact]
    public void DifferentResourceSources_AreRejectedBeforeAnyStaging()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.mana"),
                    ResourceRole.ProtectionSource),

                new ResourceDefinition(
                    ResourceKey.Parse(
                        "resource.barrier"),
                    ResourceRole.ProtectionSource)
            ]);

        var manaId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var barrierId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.barrier"));

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                        manaId,
                        current: 35d,
                        maximum: 100d),

                    new ResourceState(
                        barrierId,
                        current: 35d,
                        maximum: 100d)
                ]);

        var manaTarget =
            new ResourceStateTarget(
                entity,
                manaId);

        var barrierTarget =
            new ResourceStateTarget(
                entity,
                barrierId);

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

        var draft =
            new ResourceTransactionDraft(
                registry);

        var manaBinding =
            new ProtectionResourceRouteBinding(
                resolution.Assignments[0],
                manaTarget,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var barrierBinding =
            new ProtectionResourceRouteBinding(
                resolution.Assignments[1],
                barrierTarget,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        Assert.Throws<InvalidOperationException>(
            () =>
                ProtectionResourceRouteBatchExecutor.Stage(
                    draft,
                    state,
                    [
                        manaBinding,
                        barrierBinding
                    ]));

        Assert.Equal(
            0,
            draft.ProjectedResourceCount);

        Assert.Equal(
            0,
            draft.OperationCount);

        Assert.Equal(
            35d,
            manaTarget.State.Current);

        Assert.Equal(
            35d,
            barrierTarget.State.Current);
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
                        currentMana,
                        maximum: 100d)
                ]);

        var manaTarget =
            new ResourceStateTarget(
                entity,
                manaId);

        return new TestSetup(
            registry,
            manaTarget,
            manaTarget.State);
    }

    [Fact]
    public void ContinueRoutingShortfalls_CanEnterNormalSequentialFallbackRoutes()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
            [
                new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.mana"),
                ResourceRole.ProtectionSource),

            new ResourceDefinition(
                ResourceKey.Parse(
                    "resource.barrier"),
                ResourceRole.ProtectionSource)
            ]);

        var manaId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var barrierId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.barrier"));

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                    manaId,
                    current: 35d,
                    maximum: 100d),

                new ResourceState(
                    barrierId,
                    current: 15d,
                    maximum: 100d)
                ]);

        var manaTarget =
            new ResourceStateTarget(
                entity,
                manaId);

        var barrierTarget =
            new ResourceStateTarget(
                entity,
                barrierId);

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

        var draft =
            new ResourceTransactionDraft(
                registry);

        var batch =
            ProtectionResourceRouteBatchExecutor.Stage(
                draft,
                state,
                [
                    new ProtectionResourceRouteBinding(
                    resolution.Assignments[0],
                    manaTarget,
                    resourceUnitsPerDamage: 1d,
                    ProtectionFinancingShortfallPolicy.ContinueRouting),

                new ProtectionResourceRouteBinding(
                    resolution.Assignments[1],
                    manaTarget,
                    resourceUnitsPerDamage: 1d,
                    ProtectionFinancingShortfallPolicy.ContinueRouting)
                ]);

        state =
            batch.UpdatedState;

        // Shared 35 Mana:
        // 30 / 20 claims become 21 / 14 financed.
        // The remaining 9 / 6 stay on their own lanes.
        Assert.Equal(
            9d,
            state.Lanes[0].ContinueRoutingDamage);

        Assert.Equal(
            6d,
            state.Lanes[1].ContinueRoutingDamage);

        Assert.Equal(
            15d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            35d,
            state.FinancedDamage);

        Assert.Equal(
            50d,
            state.PrimaryPathDamage);

        Assert.False(
            state.IsComplete);

        var firstFallback =
            ProtectionResourceRouteExecutor.Stage(
                draft,
                state,
                new ProtectionResourceRouteBinding(
                    resolution.Assignments[0],
                    barrierTarget,
                    resourceUnitsPerDamage: 1d,
                    ProtectionFinancingShortfallPolicy.SpillBack));

        state =
            firstFallback.UpdatedState;

        // Critical boundary:
        // sequential fallback sees only lane A's 9,
        // never the original 30.
        Assert.Equal(
            9d,
            firstFallback.FinancingResult.AssignedDamage);

        Assert.Equal(
            9d,
            firstFallback.FinancedDamage);

        Assert.Equal(
            6d,
            state.ContinueRoutingDamage);

        var secondFallback =
            ProtectionResourceRouteExecutor.Stage(
                draft,
                state,
                new ProtectionResourceRouteBinding(
                    resolution.Assignments[1],
                    barrierTarget,
                    resourceUnitsPerDamage: 1d,
                    ProtectionFinancingShortfallPolicy.SpillBack));

        state =
            secondFallback.UpdatedState;

        Assert.Equal(
            6d,
            secondFallback.FinancingResult.AssignedDamage);

        Assert.Equal(
            6d,
            secondFallback.FinancedDamage);

        Assert.Equal(
            0d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            50d,
            state.FinancedDamage);

        Assert.Equal(
            50d,
            state.PrimaryPathDamage);

        Assert.True(
            state.IsComplete);

        Assert.Equal(
            100d,
            state.AccountedDamage);

        // 21 + 14 Mana,
        // then sequentially 9 + 6 Barrier.
        Assert.Equal(
            2,
            draft.ProjectedResourceCount);

        Assert.Equal(
            4,
            draft.OperationCount);

        Assert.Equal(
            0d,
            draft.GetProjectedValues(
                manaTarget)
            .Current);

        Assert.Equal(
            0d,
            draft.GetProjectedValues(
                barrierTarget)
            .Current);

        // Still projected only.
        Assert.Equal(
            35d,
            manaTarget.State.Current);

        Assert.Equal(
            15d,
            barrierTarget.State.Current);
    }

    [Fact]
    public void ProportionalBatch_CommitsOneSharedStateAndSeparateAllocatedGrossOperations()
    {
        var setup =
            CreateSetup(
                currentMana: 35d);

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

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var execution =
            ProtectionResourceRouteBatchExecutor.Stage(
                draft,
                state,
                [
                    new ProtectionResourceRouteBinding(
                    resolution.Assignments[0],
                    setup.ManaTarget,
                    resourceUnitsPerDamage: 1d,
                    ProtectionFinancingShortfallPolicy.SpillBack),

                new ProtectionResourceRouteBinding(
                    resolution.Assignments[1],
                    setup.ManaTarget,
                    resourceUnitsPerDamage: 1d,
                    ProtectionFinancingShortfallPolicy.SpillBack)
                ]);

        Assert.True(
            execution.UpdatedState.IsComplete);

        Assert.Equal(
            35d,
            execution.UpdatedState.FinancedDamage);

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        // One concrete shared ResourceState is published once.
        Assert.Equal(
            0d,
            setup.ManaState.Current);

        Assert.Equal(
            1UL,
            setup.ManaState.Revision);

        // But both allocated financing losses remain
        // separately observable causal operations.
        Assert.Equal(
            2,
            ledger.Count);

        var firstEntry =
            Assert.IsType<ResourceLossLedgerEntry>(
                ledger.Entries[0]);

        var secondEntry =
            Assert.IsType<ResourceLossLedgerEntry>(
                ledger.Entries[1]);

        Assert.Equal(
            ResourceOperationCause.ProtectionFinancing,
            firstEntry.Provenance.Cause);

        Assert.Equal(
            ResourceOperationCause.ProtectionFinancing,
            secondEntry.Provenance.Cause);

        Assert.Equal(
            21d,
            firstEntry.Result.RequestedLoss);

        Assert.Equal(
            21d,
            firstEntry.Result.ActualLoss);

        Assert.Equal(
            0d,
            firstEntry.Result.Shortfall);

        Assert.Equal(
            14d,
            secondEntry.Result.RequestedLoss);

        Assert.Equal(
            14d,
            secondEntry.Result.ActualLoss);

        Assert.Equal(
            0d,
            secondEntry.Result.Shortfall);

        // The protection-financing shortfall still belongs
        // to the original 30 / 20 claims.
        Assert.Equal(
            9d,
            execution.Items[0].ResourceUnitShortfall);

        Assert.Equal(
            6d,
            execution.Items[1].ResourceUnitShortfall);
    }

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        ResourceStateTarget ManaTarget,
        ResourceState ManaState);
}