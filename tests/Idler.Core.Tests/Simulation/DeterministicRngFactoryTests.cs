using Idler.Core.Simulation;

namespace Idler.Core.Tests.Simulation;

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