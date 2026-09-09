using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceStateInvariantTests
{
    [Fact]
    public void RepresentativeValidStates_AlwaysSatisfyResourceInvariant()
    {
        var maximumValues = new[]
        {
            0d,
            1d,
            10d,
            100d,
            1_000_000d
        };

        foreach (var maximum in maximumValues)
        {
            var currentValues = new[]
            {
                0d,
                maximum / 2d,
                maximum
            };

            foreach (var current in currentValues)
            {
                var state =
                    new ResourceState(
                        new ResourceId(1),
                        current,
                        maximum);

                Assert.True(
                    double.IsFinite(
                        state.Current));

                Assert.True(
                    double.IsFinite(
                        state.Maximum));

                Assert.True(
                    state.Current >= 0d);

                Assert.True(
                    state.Maximum >= 0d);

                Assert.True(
                    state.Current <=
                    state.Maximum);
            }
        }
    }
}