using Idler.Core.Combat;
using Idler.Core.Entities;
using Idler.Core.Resources;

namespace Idler.Core.Tests.Combat;

public sealed class ProtectionResourceRouteBindingTests
{
    [Fact]
    public void Constructor_BindsAssignmentToConcreteProtectionResource()
    {
        var setup =
            CreateSetup();

        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.3d)
                ]);

        var assignment =
            resolution.Assignments[0];

        var binding =
            new ProtectionResourceRouteBinding(
                assignment,
                setup.ManaTarget,
                resourceUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.ContinueRouting);

        Assert.Same(
            assignment,
            binding.Assignment);

        Assert.Same(
            setup.ManaTarget,
            binding.ResourceTarget);

        Assert.Equal(
            setup.Entity.Id,
            binding.ResourceOwnerEntityId);

        Assert.Equal(
            setup.ManaId,
            binding.ResourceId);

        Assert.Equal(
            2d,
            binding.ResourceUnitsPerDamage);

        Assert.Equal(
            ProtectionFinancingShortfallPolicy.ContinueRouting,
            binding.ShortfallPolicy);
    }

    [Fact]
    public void CreateFinancingPlan_UsesCurrentLaneRoutingDamage()
    {
        var setup =
            CreateSetup();

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

        var binding =
            new ProtectionResourceRouteBinding(
                assignment,
                setup.ManaTarget,
                resourceUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var plan =
            binding.CreateFinancingPlan(
                state.Lanes[0]);

        Assert.Equal(
            30d,
            plan.Financing.AssignedDamage);

        Assert.Equal(
            2d,
            plan.Financing.ResourceUnitsPerDamage);

        Assert.Equal(
            60d,
            plan.Financing.RequestedResourceUnits);

        Assert.Equal(
            ProtectionFinancingShortfallPolicy.SpillBack,
            plan.Financing.ShortfallPolicy);

        Assert.Same(
            setup.ManaTarget,
            plan.ResourceTarget);

        Assert.Equal(
            setup.ManaId,
            plan.ResourceId);

        Assert.Equal(
            ResourceOperationCause.ProtectionFinancing,
            plan.ResourceRequest.Provenance.Cause);
    }

    [Fact]
    public void NextRoute_UsesOnlyDamageContinuedFromPreviousRoute()
    {
        var setup =
            CreateSetup();

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

        // First route:
        // 30 damage enters.
        // 20 gets financed.
        // 10 continues.
        var firstFinancing =
            new ProtectionFinancingPlan(
                assignedDamage: 30d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.ContinueRouting);

        var firstRouting =
            ProtectionShortfallRouter.Route(
                firstFinancing.Resolve(
                    actualResourceUnitsSpent: 20d));

        state =
            state.Apply(
                assignment,
                firstRouting);

        Assert.Equal(
            10d,
            state.Lanes[0].ContinueRoutingDamage);

        var secondBinding =
            new ProtectionResourceRouteBinding(
                assignment,
                setup.ManaTarget,
                resourceUnitsPerDamage: 2d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        var secondPlan =
            secondBinding.CreateFinancingPlan(
                state.Lanes[0]);

        // Critical invariant:
        // second route receives 10 damage,
        // not the original assignment's 30.
        Assert.Equal(
            10d,
            secondPlan.Financing.AssignedDamage);

        Assert.Equal(
            20d,
            secondPlan.Financing.RequestedResourceUnits);
    }

    [Fact]
    public void BindingCannotBeAppliedToDifferentAssignmentLane()
    {
        var setup =
            CreateSetup();

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

        var binding =
            new ProtectionResourceRouteBinding(
                firstAssignment,
                setup.ManaTarget,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        Assert.Throws<InvalidOperationException>(
            () =>
                binding.CreateFinancingPlan(
                    state.Lanes[1]));

        Assert.Equal(
            30d,
            state.Lanes[0].ContinueRoutingDamage);

        Assert.Equal(
            20d,
            state.Lanes[1].ContinueRoutingDamage);
    }

    [Fact]
    public void BindingUsesAssignmentIdentity_NotEqualDamageAmount()
    {
        var setup =
            CreateSetup();

        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.25d),

                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.25d)
                ]);

        var state =
            ProtectionAssignmentRoutingStarter.Start(
                resolution);

        Assert.Equal(
            25d,
            state.Lanes[0].AssignedDamage);

        Assert.Equal(
            25d,
            state.Lanes[1].AssignedDamage);

        var binding =
            new ProtectionResourceRouteBinding(
                resolution.Assignments[0],
                setup.ManaTarget,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        Assert.Throws<InvalidOperationException>(
            () =>
                binding.CreateFinancingPlan(
                    state.Lanes[1]));
    }

    [Fact]
    public void CompletedLane_RejectsFinancingPlanCreation()
    {
        var setup =
            CreateSetup();

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

        var financing =
            new ProtectionFinancingPlan(
                assignedDamage: 30d,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        state =
            state.Apply(
                assignment,
                ProtectionShortfallRouter.Route(
                    financing.Resolve(
                        actualResourceUnitsSpent: 20d)));

        Assert.True(
            state.Lanes[0].IsComplete);

        var binding =
            new ProtectionResourceRouteBinding(
                assignment,
                setup.ManaTarget,
                resourceUnitsPerDamage: 1d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        Assert.Throws<InvalidOperationException>(
            () =>
                binding.CreateFinancingPlan(
                    state.Lanes[0]));
    }

    [Fact]
    public void NonProtectionSourceResource_IsRejected()
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
                        maximum: 100d)
                ]);

        var target =
            new ResourceStateTarget(
                entity,
                manaId);

        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.5d)
                ]);

        Assert.Throws<InvalidOperationException>(
            () =>
                new ProtectionResourceRouteBinding(
                    resolution.Assignments[0],
                    target,
                    resourceUnitsPerDamage: 1d,
                    ProtectionFinancingShortfallPolicy.SpillBack));
    }

    [Theory]
    [InlineData(0d)]
    [InlineData(-1d)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void InvalidResourceUnitsPerDamage_Throws(
        double rate)
    {
        var setup =
            CreateSetup();

        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.5d)
                ]);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ProtectionResourceRouteBinding(
                    resolution.Assignments[0],
                    setup.ManaTarget,
                    rate,
                    ProtectionFinancingShortfallPolicy.SpillBack));
    }

    [Fact]
    public void UnknownShortfallPolicy_Throws()
    {
        var setup =
            CreateSetup();

        var resolution =
            ProtectionAssignmentResolver.Resolve(
                damage: 100d,
                [
                    new ProtectionAssignmentRequest(
                        requestedFraction: 0.5d)
                ]);

        var invalidPolicy =
            (ProtectionFinancingShortfallPolicy)999;

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ProtectionResourceRouteBinding(
                    resolution.Assignments[0],
                    setup.ManaTarget,
                    resourceUnitsPerDamage: 1d,
                    invalidPolicy));
    }

    private static TestSetup CreateSetup()
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
                        current: 50d,
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
            manaTarget);
    }

    private sealed record TestSetup(
        CompiledResourceRegistry Registry,
        ResourceId ManaId,
        EntityRuntimeState Entity,
        ResourceStateTarget ManaTarget);
}