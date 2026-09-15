using Plaquewright.Core.Combat;

namespace Plaquewright.Core.Tests.Combat;

public sealed class ProtectionBackedFinancingSummaryNumericTests
{
    [Fact]
    public void Constructor_AcceptsOrdinaryFloatingPointAccountingNoise()
    {
        var summary =
            new ProtectionBackedFinancingSummary(
                assignedDamage: 0.9d,
                financedDamage: 0.2d,
                unfinancedDamage: 0.7d,
                ProtectionFinancingShortfallPolicy.SpillBack);

        Assert.Equal(
            0.9d,
            summary.AssignedDamage);

        Assert.Equal(
            0.2d,
            summary.FinancedDamage);

        Assert.Equal(
            0.7d,
            summary.UnfinancedDamage);
    }

    [Fact]
    public void Constructor_RejectsMeaningfulAccountingMismatch()
    {
        Assert.Throws<InvalidOperationException>(
            () =>
                new ProtectionBackedFinancingSummary(
                    assignedDamage: 0.9d,
                    financedDamage: 0.2d,
                    unfinancedDamage: 0.69d,
                    ProtectionFinancingShortfallPolicy.SpillBack));
    }

    [Fact]
    public void Constructor_StillAcceptsExactAccounting()
    {
        var summary =
            new ProtectionBackedFinancingSummary(
                assignedDamage: 100d,
                financedDamage: 25d,
                unfinancedDamage: 75d,
                ProtectionFinancingShortfallPolicy.ContinueRouting);

        Assert.Equal(
            100d,
            summary.AssignedDamage);
    }
}