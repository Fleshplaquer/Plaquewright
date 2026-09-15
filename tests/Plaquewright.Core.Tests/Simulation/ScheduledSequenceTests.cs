using Plaquewright.Core.Simulation;

namespace Plaquewright.Core.Tests.Simulation;

public sealed class ScheduledSequenceTests
{
    [Fact]
    public void PositiveValue_IsValid()
    {
        var sequence =
            new ScheduledSequence(42UL);

        Assert.True(sequence.IsValid);
        Assert.Equal(42UL, sequence.Value);
    }

    [Fact]
    public void Zero_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ScheduledSequence(0UL));
    }

    [Fact]
    public void DefaultValue_IsInvalid()
    {
        ScheduledSequence sequence = default;

        Assert.False(sequence.IsValid);
    }
}