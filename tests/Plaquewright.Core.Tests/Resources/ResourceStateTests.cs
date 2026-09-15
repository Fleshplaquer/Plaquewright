using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceStateTests
{
    [Fact]
    public void Constructor_PreservesValidState()
    {
        var id =
            new ResourceId(1);

        var state =
            new ResourceState(
                id,
                current: 75d,
                maximum: 100d);

        Assert.Equal(
            id,
            state.Id);

        Assert.Equal(
            75d,
            state.Current);

        Assert.Equal(
            100d,
            state.Maximum);
    }

    [Fact]
    public void ZeroCurrentAndMaximum_AreValid()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 0d,
                maximum: 0d);

        Assert.Equal(
            0d,
            state.Current);

        Assert.Equal(
            0d,
            state.Maximum);
    }

    [Fact]
    public void FullResource_IsValid()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 100d,
                maximum: 100d);

        Assert.Equal(
            state.Maximum,
            state.Current);
    }

    [Fact]
    public void Constructor_WithInvalidId_Throws()
    {
        ResourceId id = default;

        Assert.Throws<ArgumentException>(
            () =>
                new ResourceState(
                    id,
                    current: 0d,
                    maximum: 100d));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_WithNonFiniteCurrent_Throws(
        double current)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceState(
                    new ResourceId(1),
                    current,
                    maximum: 100d));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void Constructor_WithNonFiniteMaximum_Throws(
        double maximum)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceState(
                    new ResourceId(1),
                    current: 0d,
                    maximum));
    }

    [Fact]
    public void Constructor_WithNegativeCurrent_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceState(
                    new ResourceId(1),
                    current: -1d,
                    maximum: 100d));
    }

    [Fact]
    public void Constructor_WithNegativeMaximum_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceState(
                    new ResourceId(1),
                    current: 0d,
                    maximum: -1d));
    }

    [Fact]
    public void Constructor_WithCurrentAboveMaximum_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new ResourceState(
                    new ResourceId(1),
                    current: 101d,
                    maximum: 100d));
    }

    [Fact]
    public void SetValues_ChangesCurrentAndMaximumTogether()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 100d,
                maximum: 100d);

        state.SetValues(
            current: 50d,
            maximum: 75d);

        Assert.Equal(
            50d,
            state.Current);

        Assert.Equal(
            75d,
            state.Maximum);
    }

    [Fact]
    public void SetValues_WithInvalidValues_DoesNotPartiallyMutateState()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 75d,
                maximum: 100d);

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                state.SetValues(
                    current: 80d,
                    maximum: 50d));

        Assert.Equal(
            75d,
            state.Current);

        Assert.Equal(
            100d,
            state.Maximum);
    }
    [Fact]
    public void NewState_StartsAtRevisionZero()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 50d,
                maximum: 100d);

        Assert.Equal(
            0UL,
            state.Revision);
    }

    [Fact]
    public void SetValues_IncrementsRevision()
    {
        var state =
            new ResourceState(
                new ResourceId(1),
                current: 50d,
                maximum: 100d);

        state.SetValues(
            current: 40d,
            maximum: 100d);

        Assert.Equal(
            1UL,
            state.Revision);
    }
}