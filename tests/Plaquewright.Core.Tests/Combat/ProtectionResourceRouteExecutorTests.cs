using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Combat;

public sealed class ProtectionResourceRouteExecutorTests
{
    [Fact]
    public void Stage_WithSpillBack_StagesResourceAndUpdatesRoutingState()
    {
        var setup =
            CreateSingleResourceSetup(
                currentMana: 20d);

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

        var binding =
            new ProtectionResourceRouteBinding(
                resolution.Assignments[0],
                setup.ManaTarget,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var execution =
            ProtectionResourceRouteExecutor.Stage(
                draft,
                state,
                binding);

        Assert.Same(
            state,
            execution.PreviousState);

        Assert.Same(
            binding,
            execution.Binding);

        Assert.Equal(
            30d,
            execution.FinancingResult.AssignedDamage);

        Assert.Equal(
            30d,
            execution.FinancingResult.RequestedResourceUnits);

        Assert.Equal(
            20d,
            execution.FinancingResult.ActualResourceUnitsSpent);

        Assert.Equal(
            20d,
            execution.FinancedDamage);

        Assert.Equal(
            10d,
            execution.UnfinancedDamage);

        Assert.Equal(
            10d,
            execution.SpillBackDamage);

        Assert.Equal(
            0d,
            execution.ContinueRoutingDamage);

        var updated =
            execution.UpdatedState;

        // Base primary = 70
        // + 10 spillback
        Assert.Equal(
            80d,
            updated.PrimaryPathDamage);

        Assert.Equal(
            20d,
            updated.FinancedDamage);

        Assert.Equal(
            0d,
            updated.ContinueRoutingDamage);

        Assert.True(
            updated.IsComplete);

        Assert.Equal(
            100d,
            updated.AccountedDamage);

        // Previous immutable state is untouched.
        Assert.Equal(
            70d,
            state.PrimaryPathDamage);

        Assert.Equal(
            30d,
            state.ContinueRoutingDamage);

        Assert.Equal(
            0d,
            state.FinancedDamage);

        // Resource is projected only.
        Assert.Equal(
            20d,
            setup.ManaState.Current);

        Assert.Equal(
            0UL,
            setup.ManaState.Revision);

        Assert.Equal(
            1,
            draft.ProjectedResourceCount);

        Assert.Equal(
            1,
            draft.OperationCount);

        Assert.Equal(
            0d,
            draft.Projections[0].Current);
    }

    [Fact]
    public void ContinueRouting_NextRouteReceivesOnlyRemainingLaneDamage()
    {
        var setup =
            CreateTwoResourceSetup(
                currentMana: 20d,
                currentBarrier: 4d);

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

        var manaBinding =
            new ProtectionResourceRouteBinding(
                assignment,
                setup.ManaTarget,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.ContinueRouting);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        var firstExecution =
            ProtectionResourceRouteExecutor.Stage(
                draft,
                state,
                manaBinding);

        state =
            firstExecution.UpdatedState;

        Assert.Equal(
            20d,
            firstExecution.FinancedDamage);

        Assert.Equal(
            10d,
            firstExecution.ContinueRoutingDamage);

        Assert.Equal(
            10d,
            state.Lanes[0].ContinueRoutingDamage);

        var barrierBinding =
            new ProtectionResourceRouteBinding(
                assignment,
                setup.BarrierTarget,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var secondExecution =
            ProtectionResourceRouteExecutor.Stage(
                draft,
                state,
                barrierBinding);

        state =
            secondExecution.UpdatedState;

        // Critical invariant:
        // second route sees the 10 remaining damage,
        // not the original 30 assignment damage.
        Assert.Equal(
            10d,
            secondExecution.FinancingResult.AssignedDamage);

        Assert.Equal(
            10d,
            secondExecution.FinancingResult.RequestedResourceUnits);

        Assert.Equal(
            4d,
            secondExecution.FinancedDamage);

        Assert.Equal(
            6d,
            secondExecution.SpillBackDamage);

        Assert.Equal(
            0d,
            secondExecution.ContinueRoutingDamage);

        Assert.Equal(
            76d,
            state.PrimaryPathDamage);

        Assert.Equal(
            24d,
            state.FinancedDamage);

        Assert.Equal(
            0d,
            state.ContinueRoutingDamage);

        Assert.True(
            state.IsComplete);

        Assert.Equal(
            100d,
            state.AccountedDamage);

        // Still projected.
        Assert.Equal(
            20d,
            setup.ManaState.Current);

        Assert.Equal(
            4d,
            setup.BarrierState.Current);

        Assert.Equal(
            0UL,
            setup.ManaState.Revision);

        Assert.Equal(
            0UL,
            setup.BarrierState.Revision);

        Assert.Equal(
            2,
            draft.ProjectedResourceCount);

        Assert.Equal(
            2,
            draft.OperationCount);
    }

    [Fact]
    public void ParallelLanes_SharingResource_UseSameProjectedResourceBudget()
    {
        var setup =
            CreateSingleResourceSetup(
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

        var first =
            ProtectionResourceRouteExecutor.Stage(
                draft,
                state,
                firstBinding);

        state =
            first.UpdatedState;

        Assert.Equal(
            30d,
            first.FinancedDamage);

        Assert.Equal(
            0d,
            first.SpillBackDamage);

        // Only 5 projected Mana remains.
        Assert.Equal(
            5d,
            draft.Projections[0].Current);

        var second =
            ProtectionResourceRouteExecutor.Stage(
                draft,
                state,
                secondBinding);

        state =
            second.UpdatedState;

        // Lane B asks for 20,
        // but only the projected remaining 5 Mana exists.
        Assert.Equal(
            20d,
            second.FinancingResult.RequestedResourceUnits);

        Assert.Equal(
            5d,
            second.FinancingResult.ActualResourceUnitsSpent);

        Assert.Equal(
            5d,
            second.FinancedDamage);

        Assert.Equal(
            15d,
            second.SpillBackDamage);

        Assert.Equal(
            0d,
            draft.Projections[0].Current);

        Assert.Equal(
            65d,
            state.PrimaryPathDamage);

        Assert.Equal(
            35d,
            state.FinancedDamage);

        Assert.Equal(
            0d,
            state.ContinueRoutingDamage);

        Assert.True(
            state.IsComplete);

        Assert.Equal(
            100d,
            state.AccountedDamage);

        // Both operations share one concrete projection.
        Assert.Equal(
            1,
            draft.ProjectedResourceCount);

        Assert.Equal(
            2,
            draft.OperationCount);

        // Original Mana is still untouched.
        Assert.Equal(
            35d,
            setup.ManaState.Current);

        Assert.Equal(
            0UL,
            setup.ManaState.Revision);

        var firstOperation =
            Assert.IsType<StagedResourceLossOperation>(
                draft.Operations[0]);

        var secondOperation =
            Assert.IsType<StagedResourceLossOperation>(
                draft.Operations[1]);

        Assert.Equal(
            30d,
            firstOperation.Preview.Result.ActualLoss);

        Assert.Equal(
            5d,
            secondOperation.Preview.Result.ActualLoss);
    }

    [Fact]
    public void ParallelLanes_SharingResource_CommitOneFinalStateMutationAndTwoGrossOperations()
    {
        var setup =
            CreateSingleResourceSetup(
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

        foreach (var assignment in
                 resolution.Assignments)
        {
            var binding =
                new ProtectionResourceRouteBinding(
                    assignment,
                    setup.ManaTarget,
                    resourceUnitsPerDamage: 1d,
                    ProtectionFinancingShortfallPolicy.SpillBack);

            state =
                ProtectionResourceRouteExecutor.Stage(
                    draft,
                    state,
                    binding)
                .UpdatedState;
        }

        Assert.True(
            state.IsComplete);

        Assert.Equal(
            65d,
            state.PrimaryPathDamage);

        Assert.Equal(
            35d,
            state.FinancedDamage);

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        // Same concrete resource was touched twice,
        // but final state is committed once.
        Assert.Equal(
            0d,
            setup.ManaState.Current);

        Assert.Equal(
            1UL,
            setup.ManaState.Revision);

        // Gross operations remain separately observable.
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
            30d,
            firstEntry.Result.RequestedLoss);

        Assert.Equal(
            30d,
            firstEntry.Result.ActualLoss);

        Assert.Equal(
            20d,
            secondEntry.Result.RequestedLoss);

        Assert.Equal(
            5d,
            secondEntry.Result.ActualLoss);

        Assert.Equal(
            15d,
            secondEntry.Result.Shortfall);
    }

    [Fact]
    public void ForeignAssignmentBinding_IsRejectedBeforeResourceStaging()
    {
        var setup =
            CreateSingleResourceSetup(
                currentMana: 50d);

        var firstResolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d)
                ]);

        var secondResolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                firstResolution);

        var foreignBinding =
            new ProtectionResourceRouteBinding(
                secondResolution.Assignments[0],
                setup.ManaTarget,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        Assert.Throws<InvalidOperationException>(
            () =>
                ProtectionResourceRouteExecutor.Stage(
                    draft,
                    state,
                    foreignBinding));

        Assert.Equal(
            0,
            draft.ProjectedResourceCount);

        Assert.Equal(
            0,
            draft.OperationCount);

        Assert.Equal(
            50d,
            setup.ManaState.Current);

        Assert.Equal(
            0UL,
            setup.ManaState.Revision);

        Assert.Equal(
            100d,
            state.AccountedDamage);
    }

    [Fact]
    public void CompletedLane_IsRejectedWithoutAdditionalStaging()
    {
        var setup =
            CreateSingleResourceSetup(
                currentMana: 50d);

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

        var binding =
            new ProtectionResourceRouteBinding(
                resolution.Assignments[0],
                setup.ManaTarget,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var draft =
            new ResourceTransactionDraft(
                setup.Registry);

        state =
            ProtectionResourceRouteExecutor.Stage(
                draft,
                state,
                binding)
            .UpdatedState;

        Assert.True(
            state.IsComplete);

        Assert.Equal(
            1,
            draft.OperationCount);

        Assert.Throws<InvalidOperationException>(
            () =>
                ProtectionResourceRouteExecutor.Stage(
                    draft,
                    state,
                    binding));

        // No second operation was staged.
        Assert.Equal(
            1,
            draft.OperationCount);

        Assert.Equal(
            1,
            draft.ProjectedResourceCount);

        // Original state still not committed.
        Assert.Equal(
            50d,
            setup.ManaState.Current);

        Assert.Equal(
            0UL,
            setup.ManaState.Revision);
    }

    private static SingleResourceSetup CreateSingleResourceSetup(
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

        return new SingleResourceSetup(
            registry,
            manaTarget,
            manaTarget.State);
    }

    private static TwoResourceSetup CreateTwoResourceSetup(
        double currentMana,
        double currentBarrier)
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
    "resource.barrier.energy"),
                    ResourceRole.ProtectionSource)
            ]);

        var manaId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.mana"));

        var barrierId =
            registry.GetId(
                ResourceKey.Parse(
                    "resource.barrier.energy"));

        var entity =
            new EntityRuntimeState(
                new EntityId(1UL),
                registry,
                [
                    new ResourceState(
                        manaId,
                        currentMana,
                        maximum: 100d),

                    new ResourceState(
                        barrierId,
                        currentBarrier,
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

        return new TwoResourceSetup(
            registry,
            manaTarget,
            barrierTarget,
            manaTarget.State,
            barrierTarget.State);
    }

    private sealed record SingleResourceSetup(
        CompiledResourceRegistry Registry,
        ResourceStateTarget ManaTarget,
        ResourceState ManaState);

    private sealed record TwoResourceSetup(
        CompiledResourceRegistry Registry,
        ResourceStateTarget ManaTarget,
        ResourceStateTarget BarrierTarget,
        ResourceState ManaState,
        ResourceState BarrierState);
}