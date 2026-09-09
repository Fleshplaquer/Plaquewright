using Idler.Core.Simulation;

namespace Idler.Core.Tests.Simulation;

public sealed class DeterministicRngTests
{
    [Fact]
    public void SplitMix64V1_WithKnownInitialState_ProducesExpectedSequence()
    {
        var rng =
            new DeterministicRng(
                initialState: 0UL,
                RngAlgorithmVersion.SplitMix64V1);

        var actual = new[]
        {
            rng.NextUInt64(),
            rng.NextUInt64(),
            rng.NextUInt64(),
            rng.NextUInt64(),
            rng.NextUInt64()
        };

        var expected = new ulong[]
        {
            16294208416658607535UL,
            7960286522194355700UL,
            487617019471545679UL,
            17909611376780542444UL,
            1961750202426094747UL
        };

        Assert.Equal(
            expected,
            actual);
    }

    [Fact]
    public void SameInitialState_ProducesSameSequence()
    {
        var first =
            new DeterministicRng(
                123UL,
                RngAlgorithmVersion.SplitMix64V1);

        var second =
            new DeterministicRng(
                123UL,
                RngAlgorithmVersion.SplitMix64V1);

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
    public void DifferentInitialStates_ProduceDifferentSequences()
    {
        var first =
            new DeterministicRng(
                123UL,
                RngAlgorithmVersion.SplitMix64V1);

        var second =
            new DeterministicRng(
                456UL,
                RngAlgorithmVersion.SplitMix64V1);

        Assert.NotEqual(
            first.NextUInt64(),
            second.NextUInt64());
    }

    [Fact]
    public void BoundedRoll_AlwaysStaysWithinRange()
    {
        var rng =
            new DeterministicRng(
                123UL,
                RngAlgorithmVersion.SplitMix64V1);

        for (var index = 0;
             index < 10_000;
             index++)
        {
            var value =
                rng.NextUInt64(17UL);

            Assert.True(
                value < 17UL);
        }
    }

    [Fact]
    public void BoundedRoll_WithZeroUpperBound_Throws()
    {
        var rng =
            new DeterministicRng(
                123UL,
                RngAlgorithmVersion.SplitMix64V1);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => rng.NextUInt64(0UL));
    }
}