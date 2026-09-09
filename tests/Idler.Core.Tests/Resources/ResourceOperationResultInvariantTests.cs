using Idler.Core.Resources;

namespace Idler.Core.Tests.Resources;

public sealed class ResourceOperationResultInvariantTests
{
    [Fact]
    public void RepresentativeLossResults_AlwaysAccountForRequestedLoss()
    {
        var requestedValues = new[]
        {
            0d,
            1d,
            10d,
            100d,
            1_000d
        };

        foreach (var requested in requestedValues)
        {
            var preventedValues = new[]
            {
                0d,
                requested / 2d,
                requested
            };

            foreach (var prevented in preventedValues)
            {
                var remaining =
                    requested - prevented;

                var actualValues = new[]
                {
                    0d,
                    remaining / 2d,
                    remaining
                };

                foreach (var actual in actualValues)
                {
                    var result =
                        new ResourceLossResult(
                            new ResourceId(1),
                            requested,
                            prevented,
                            actual);

                    Assert.Equal(
                        result.RequestedLoss,
                        result.PreventedLoss +
                        result.ActualLoss +
                        result.Shortfall);
                }
            }
        }
    }

    [Fact]
    public void RepresentativeRecoveryResults_AlwaysAccountForRequestedRecovery()
    {
        var requestedValues = new[]
        {
            0d,
            1d,
            10d,
            100d,
            1_000d
        };

        foreach (var requested in requestedValues)
        {
            var actualValues = new[]
            {
                0d,
                requested / 2d,
                requested
            };

            foreach (var actual in actualValues)
            {
                var result =
                    new ResourceRecoveryResult(
                        new ResourceId(1),
                        requested,
                        actual);

                Assert.Equal(
                    result.RequestedRecovery,
                    result.ActualRecovery +
                    result.Overflow);
            }
        }
    }
}