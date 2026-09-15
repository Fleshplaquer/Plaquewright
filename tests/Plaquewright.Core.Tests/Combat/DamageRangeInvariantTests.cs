using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class DamageRangeInvariantTests
{
    [Fact]
    public void Resolve_ForRepresentativeCandidates_AlwaysProducesValidRange()
    {
        var values = new[]
        {
            -1000d,
            -100d,
            -10d,
            0d,
            10d,
            100d,
            1000d
        };

        foreach (var candidateMin in values)
        {
            foreach (var candidateMax in values)
            {
                var range =
                    DamageRangeResolver.Resolve(
                        candidateMin,
                        candidateMax);

                Assert.True(range.Min >= 0d);
                Assert.True(range.Max >= 0d);
                Assert.True(range.Min <= range.Max);

                Assert.True(double.IsFinite(range.Min));
                Assert.True(double.IsFinite(range.Max));
            }
        }
    }
}