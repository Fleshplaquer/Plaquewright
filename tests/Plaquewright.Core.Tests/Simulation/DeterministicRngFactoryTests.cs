using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class DeterministicRngFactoryTests
{
    [Fact]
    public void SameSeedAndDomain_ProduceSameSequence()
    {
        var seed =
            new SimulationSeed(42UL);

        var domain =
            RngDomainKey.Parse("roll.crit");

        var first =
            DeterministicRngFactory.CreateStream(
                seed,
                domain);

        var second =
            DeterministicRngFactory.CreateStream(
                seed,
                domain);

        for (var index = 0;
             index < 100;
             index++)
        {
            Assert.Equal(
                first.NextUInt64(),
                second.NextUInt64());
        }
    }
    [Fact]
    public void CreateStream_WithUnknownAlgorithmVersion_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                DeterministicRngFactory.CreateStream(
                    new SimulationSeed(123UL),
                    RngDomainKey.Parse(
                        "test.rng"),
                    (RngAlgorithmVersion)999));
    }

    [Fact]
    public void DifferentDomains_ProduceDifferentSequences()
    {
        var seed =
            new SimulationSeed(42UL);

        var crit =
            DeterministicRngFactory.CreateStream(
                seed,
                RngDomainKey.Parse("roll.crit"));

        var damage =
            DeterministicRngFactory.CreateStream(
                seed,
                RngDomainKey.Parse("roll.damage"));

        Assert.NotEqual(
            crit.NextUInt64(),
            damage.NextUInt64());
    }

    [Fact]
    public void CreateExecutionStream_WithSameInputs_ProducesSameSequence()
    {
        var seed =
            new SimulationSeed(
                123456789UL);

        var domain =
            RngDomainKey.Parse(
                "combat.crit");

        var executionId =
            new ExecutionId(
                42UL);

        var first =
            DeterministicRngFactory.CreateExecutionStream(
                seed,
                domain,
                executionId);

        var second =
            DeterministicRngFactory.CreateExecutionStream(
                seed,
                domain,
                executionId);

        for (var index = 0;
             index < 100;
             index++)
        {
            Assert.Equal(
                first.NextUInt64(),
                second.NextUInt64());
        }
    }

    [Fact]
    public void CreateExecutionStream_WithDifferentExecutionIds_ProducesDifferentStream()
    {
        var seed =
            new SimulationSeed(
                123456789UL);

        var domain =
            RngDomainKey.Parse(
                "combat.crit");

        var first =
            DeterministicRngFactory.CreateExecutionStream(
                seed,
                domain,
                new ExecutionId(1UL));

        var second =
            DeterministicRngFactory.CreateExecutionStream(
                seed,
                domain,
                new ExecutionId(2UL));

        Assert.NotEqual(
            first.NextUInt64(),
            second.NextUInt64());
    }

    [Fact]
    public void CreateExecutionStream_WithDifferentDomains_ProducesDifferentStream()
    {
        var seed =
            new SimulationSeed(
                123456789UL);

        var executionId =
            new ExecutionId(
                42UL);

        var crit =
            DeterministicRngFactory.CreateExecutionStream(
                seed,
                RngDomainKey.Parse(
                    "combat.crit"),
                executionId);

        var damage =
            DeterministicRngFactory.CreateExecutionStream(
                seed,
                RngDomainKey.Parse(
                    "combat.damage"),
                executionId);

        Assert.NotEqual(
            crit.NextUInt64(),
            damage.NextUInt64());
    }

    [Fact]
    public void CreateExecutionStream_CreationOrderDoesNotAffectSequence()
    {
        var seed =
            new SimulationSeed(
                123456789UL);

        var domain =
            RngDomainKey.Parse(
                "combat.crit");

        var firstExecutionOne =
            DeterministicRngFactory.CreateExecutionStream(
                seed,
                domain,
                new ExecutionId(1UL));

        var firstExecutionTwo =
            DeterministicRngFactory.CreateExecutionStream(
                seed,
                domain,
                new ExecutionId(2UL));

        var secondExecutionTwo =
            DeterministicRngFactory.CreateExecutionStream(
                seed,
                domain,
                new ExecutionId(2UL));

        var secondExecutionOne =
            DeterministicRngFactory.CreateExecutionStream(
                seed,
                domain,
                new ExecutionId(1UL));

        for (var index = 0;
             index < 50;
             index++)
        {
            Assert.Equal(
                firstExecutionOne.NextUInt64(),
                secondExecutionOne.NextUInt64());

            Assert.Equal(
                firstExecutionTwo.NextUInt64(),
                secondExecutionTwo.NextUInt64());
        }
    }

    [Fact]
    public void CreateExecutionStream_WithInvalidExecutionId_Throws()
    {
        ExecutionId executionId =
            default;

        Assert.Throws<ArgumentException>(
            () =>
                DeterministicRngFactory.CreateExecutionStream(
                    new SimulationSeed(1UL),
                    RngDomainKey.Parse(
                        "combat.crit"),
                    executionId));
    }

    [Fact]
    public void CreateExecutionStream_KnownReplayVector_IsStable()
    {
        var rng =
            DeterministicRngFactory.CreateExecutionStream(
                new SimulationSeed(
                    123456789UL),
                RngDomainKey.Parse(
                    "combat.crit"),
                new ExecutionId(
                    42UL));

        Assert.Equal(
            14991322147324762200UL,
            rng.NextUInt64());

        Assert.Equal(
            1060276390660293113UL,
            rng.NextUInt64());

        Assert.Equal(
            9576494504821972008UL,
            rng.NextUInt64());
    }

    [Fact]
    public void DrawingFromOneDomain_DoesNotAdvanceAnotherDomain()
    {
        var seed =
            new SimulationSeed(42UL);

        var critA =
            DeterministicRngFactory.CreateStream(
                seed,
                RngDomainKey.Parse("roll.crit"));

        var critB =
            DeterministicRngFactory.CreateStream(
                seed,
                RngDomainKey.Parse("roll.crit"));

        var damage =
            DeterministicRngFactory.CreateStream(
                seed,
                RngDomainKey.Parse("roll.damage"));

        for (var index = 0;
             index < 1000;
             index++)
        {
            damage.NextUInt64();
        }

        Assert.Equal(
            critA.NextUInt64(),
            critB.NextUInt64());
    }

    [Fact]
    public void KnownSeedAndDomain_ProduceStableReplayVector()
    {
        var rng =
            DeterministicRngFactory.CreateStream(
                new SimulationSeed(42UL),
                RngDomainKey.Parse("roll.crit"));

        var actual = new[]
        {
            rng.NextUInt64(),
            rng.NextUInt64(),
            rng.NextUInt64()
        };

        var expected = new ulong[]
        {
            3383696975979578596UL,
            16642844257182129811UL,
            14380237515708524945UL
        };

        Assert.Equal(
            expected,
            actual);
    }

    [Fact]
    public void DefaultDomain_IsRejected()
    {
        RngDomainKey domain = default;

        Assert.Throws<ArgumentException>(
            () =>
                DeterministicRngFactory.CreateStream(
                    new SimulationSeed(42UL),
                    domain));
    }
}