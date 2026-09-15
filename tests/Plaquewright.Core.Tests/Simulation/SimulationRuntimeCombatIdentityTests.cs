using Plaquewright.Core.Combat;
using Plaquewright.Core.Resources;
using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class SimulationRuntimeCombatIdentityTests
{
    [Fact]
    public void HitExecutionIds_AreAllocatedMonotonically()
    {
        var runtime =
            CreateRuntime();

        var first =
            runtime.AllocateHitExecutionId();

        var second =
            runtime.AllocateHitExecutionId();

        var third =
            runtime.AllocateHitExecutionId();

        Assert.Equal(
            new HitExecutionId(1UL),
            first);

        Assert.Equal(
            new HitExecutionId(2UL),
            second);

        Assert.Equal(
            new HitExecutionId(3UL),
            third);
    }

    [Fact]
    public void DamageExecutionIds_AreAllocatedMonotonically()
    {
        var runtime =
            CreateRuntime();

        var first =
            runtime.AllocateDamageExecutionId();

        var second =
            runtime.AllocateDamageExecutionId();

        var third =
            runtime.AllocateDamageExecutionId();

        Assert.Equal(
            new DamageExecutionId(1UL),
            first);

        Assert.Equal(
            new DamageExecutionId(2UL),
            second);

        Assert.Equal(
            new DamageExecutionId(3UL),
            third);
    }

    [Fact]
    public void HitAndDamageExecutionIds_HaveIndependentSpaces()
    {
        var runtime =
            CreateRuntime();

        var firstHit =
            runtime.AllocateHitExecutionId();

        var firstDamage =
            runtime.AllocateDamageExecutionId();

        var secondDamage =
            runtime.AllocateDamageExecutionId();

        var secondHit =
            runtime.AllocateHitExecutionId();

        Assert.Equal(
            new HitExecutionId(1UL),
            firstHit);

        Assert.Equal(
            new HitExecutionId(2UL),
            secondHit);

        Assert.Equal(
            new DamageExecutionId(1UL),
            firstDamage);

        Assert.Equal(
            new DamageExecutionId(2UL),
            secondDamage);
    }

    [Fact]
    public void AllocationInOneCombatIdentitySpace_DoesNotAdvanceTheOther()
    {
        var runtime =
            CreateRuntime();

        runtime.AllocateHitExecutionId();
        runtime.AllocateHitExecutionId();
        runtime.AllocateHitExecutionId();

        var damage =
            runtime.AllocateDamageExecutionId();

        Assert.Equal(
            new DamageExecutionId(1UL),
            damage);
    }

    [Fact]
    public void SeparateRuntimeInstances_StartWithFreshCombatIdentitySpaces()
    {
        var firstRuntime =
            CreateRuntime();

        var secondRuntime =
            CreateRuntime();

        Assert.Equal(
            new HitExecutionId(1UL),
            firstRuntime.AllocateHitExecutionId());

        Assert.Equal(
            new HitExecutionId(1UL),
            secondRuntime.AllocateHitExecutionId());

        Assert.Equal(
            new DamageExecutionId(1UL),
            firstRuntime.AllocateDamageExecutionId());

        Assert.Equal(
            new DamageExecutionId(1UL),
            secondRuntime.AllocateDamageExecutionId());
    }

    private static SimulationRuntimeState CreateRuntime()
    {
        var registry =
            ResourceRegistryCompiler.Compile(
                Array.Empty<ResourceDefinition>());

        return new SimulationRuntimeState(
            new SimulationSeed(123UL),
            registry);
    }
}