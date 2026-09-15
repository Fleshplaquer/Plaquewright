using Plaquewright.Core.Combat;
using Plaquewright.Core.Entities;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Combat;

public sealed class ProtectionAssignmentRouteChainStarterTests
{
    [Fact]
    public void SingleAssignment_StartsRouteChainFromResolvedSplit()
    {
        var assignment =
            ProtectionAssignmentResolver.Resolve(
                damage: 120d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.5d)
                ]);

        var chain =
            ProtectionAssignmentRouteChainStarter.Start(
                assignment);

        Assert.Equal(
            120d,
            chain.InitialDamage);

        Assert.Equal(
            60d,
            chain.PrimaryPathDamage);

        Assert.Equal(
            60d,
            chain.ContinueRoutingDamage);

        Assert.Equal(
            0d,
            chain.FinancedDamage);

        Assert.Equal(
            120d,
            chain.AccountedDamage);

        Assert.False(
            chain.IsComplete);
    }

    [Fact]
    public void NoAssignments_StartsCompletedPrimaryOnlyChain()
    {
        var assignment =
            ProtectionAssignmentResolver.Resolve(
                damage: 120d,
                Array.Empty<ProtectionAssignmentRequest>());

        var chain =
            ProtectionAssignmentRouteChainStarter.Start(
                assignment);

        Assert.Equal(
            120d,
            chain.InitialDamage);

        Assert.Equal(
            120d,
            chain.PrimaryPathDamage);

        Assert.Equal(
            0d,
            chain.ContinueRoutingDamage);

        Assert.Equal(
            0d,
            chain.FinancedDamage);

        Assert.True(
            chain.IsComplete);

        Assert.Equal(
            120d,
            chain.AccountedDamage);
    }

    [Fact]
    public void ZeroFractionAssignment_StartsCompletedPrimaryOnlyChain()
    {
        var assignment =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0d)
                ]);

        var chain =
            ProtectionAssignmentRouteChainStarter.Start(
                assignment);

        Assert.Equal(
            100d,
            chain.PrimaryPathDamage);

        Assert.Equal(
            0d,
            chain.ContinueRoutingDamage);

        Assert.True(
            chain.IsComplete);
    }

    [Fact]
    public void MultipleActiveAssignments_AreRejected()
    {
        var assignment =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d),

                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.2d)
                ]);

        Assert.Throws<InvalidOperationException>(
            () =>
                ProtectionAssignmentRouteChainStarter.Start(
                    assignment));
    }

    [Fact]
    public void OversubscribedMultipleAssignments_AreAlsoRejected()
    {
        var assignment =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.8d),

                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.8d)
                ]);

        Assert.Equal(
            50d,
            assignment.Assignments[0].AssignedDamage);

        Assert.Equal(
            50d,
            assignment.Assignments[1].AssignedDamage);

        Assert.Throws<InvalidOperationException>(
            () =>
                ProtectionAssignmentRouteChainStarter.Start(
                    assignment));
    }

    [Fact]
    public void ZeroDamage_WithMultipleRequests_StartsCompletedZeroChain()
    {
        var assignment =
            ProtectionAssignmentResolver.Resolve(
                damage: 0d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.5d),

                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.5d)
                ]);

        var chain =
            ProtectionAssignmentRouteChainStarter.Start(
                assignment);

        Assert.Equal(
            0d,
            chain.InitialDamage);

        Assert.Equal(
            0d,
            chain.PrimaryPathDamage);

        Assert.Equal(
            0d,
            chain.ContinueRoutingDamage);

        Assert.True(
            chain.IsComplete);
    }

    [Fact]
    public void PriorWardThenFiftyPercentManaProtection_WithInsufficientMana_PreservesAllDamage()
    {
        const double originalHitDamage =
            150d;

        const double priorWardAbsorbedDamage =
            30d;

        var lifeBoundDamage =
            originalHitDamage -
            priorWardAbsorbedDamage;

        Assert.Equal(
            120d,
            lifeBoundDamage);

        // 50% of the damage currently bound for Life
        // is assigned to Mana protection.
        var assignment =
            ProtectionAssignmentResolver.Resolve(
                lifeBoundDamage,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.5d)
                ]);

        var chain =
            ProtectionAssignmentRouteChainStarter.Start(
                assignment);

        Assert.Equal(
            60d,
            chain.PrimaryPathDamage);

        Assert.Equal(
            60d,
            chain.ContinueRoutingDamage);

        var resourceSetup =
            CreateManaSetup(
                currentMana: 50d);

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage:
                    chain.ContinueRoutingDamage,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var resourcePlan =
            new ResourceBackedProtectionFinancingPlan(
                financing,
                resourceSetup.ManaTarget);

        var draft =
            new ResourceTransactionDraft(
                resourceSetup.Registry);

        var financingResult =
            ProtectionResourceTransactionStager.Stage(
                draft,
                resourcePlan);

        Assert.Equal(
            60d,
            financingResult.AssignedDamage);

        Assert.Equal(
            50d,
            financingResult.FinancedDamage);

        Assert.Equal(
            10d,
            financingResult.UnfinancedDamage);

        // Mana is still only projected at this point.
        Assert.Equal(
            50d,
            resourceSetup.ManaState.Current);

        Assert.Equal(
            0UL,
            resourceSetup.ManaState.Revision);

        var shortfallRouting =
            ProtectionShortfallRouter.Route(
                financingResult);

        chain =
            chain.Apply(
                shortfallRouting);

        Assert.Equal(
            70d,
            chain.PrimaryPathDamage);

        Assert.Equal(
            0d,
            chain.ContinueRoutingDamage);

        Assert.Equal(
            50d,
            chain.FinancedDamage);

        Assert.True(
            chain.IsComplete);

        Assert.Equal(
            120d,
            chain.AccountedDamage);

        // Prior Ward + everything remaining after Ward
        // still accounts for the original hit.
        Assert.Equal(
            originalHitDamage,
            priorWardAbsorbedDamage +
            chain.AccountedDamage);

        var ledger =
            new ResourceOperationLedger();

        ResourceTransactionCommitter.Commit(
            draft,
            ledger);

        Assert.Equal(
            0d,
            resourceSetup.ManaState.Current);

        Assert.Equal(
            1UL,
            resourceSetup.ManaState.Revision);

        Assert.Equal(
            1,
            ledger.Count);

        var manaEntry =
            Assert.IsType<ResourceLossLedgerEntry>(
                ledger.Entries[0]);

        Assert.Equal(
            ResourceOperationCause.ProtectionFinancing,
            manaEntry.Provenance.Cause);

        Assert.Equal(
            60d,
            manaEntry.Result.RequestedLoss);

        Assert.Equal(
            50d,
            manaEntry.Result.ActualLoss);

        Assert.Equal(
            10d,
            manaEntry.Result.Shortfall);

        // Final semantic distribution:
        //
        // 30 Ward absorbed before this layer
        // 50 Damage financed by Mana
        // 70 Damage remains on the Life path
        Assert.Equal(
            originalHitDamage,
            priorWardAbsorbedDamage +
            chain.FinancedDamage +
            chain.PrimaryPathDamage);
    }

    private static ManaSetup CreateManaSetup(
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

        return new ManaSetup(
            registry,
            manaTarget,
            manaTarget.State);
    }

    private sealed record ManaSetup(
        CompiledResourceRegistry Registry,
        ResourceStateTarget ManaTarget,
        ResourceState ManaState);
}