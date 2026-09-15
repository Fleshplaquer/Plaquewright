namespace Plaquewright.Core.Combat;

internal sealed class ProtectionSharedCapacityAllocation
{
    private readonly double[] _allocatedCapacities;

    public double AvailableCapacity { get; }

    public double TotalRequestedCapacity { get; }

    public double TotalAllocatedCapacity { get; }

    public double UnusedCapacity =>
        NormalizeZero(
            AvailableCapacity -
            TotalAllocatedCapacity);

    public IReadOnlyList<double> AllocatedCapacities { get; }

    internal ProtectionSharedCapacityAllocation(
        double availableCapacity,
        double totalRequestedCapacity,
        double[] allocatedCapacities)
    {
        ArgumentNullException.ThrowIfNull(
            allocatedCapacities);

        AvailableCapacity =
            NormalizeZero(
                availableCapacity);

        TotalRequestedCapacity =
            NormalizeZero(
                totalRequestedCapacity);

        _allocatedCapacities =
            allocatedCapacities;

        AllocatedCapacities =
            Array.AsReadOnly(
                _allocatedCapacities);

        var totalAllocated =
            0d;

        foreach (var allocation in
                 _allocatedCapacities)
        {
            totalAllocated +=
                allocation;

            if (!double.IsFinite(
                    totalAllocated))
            {
                throw new OverflowException(
                    "Total shared protection capacity allocation exceeds the finite numeric range.");
            }
        }

        TotalAllocatedCapacity =
            NormalizeZero(
                totalAllocated);
    }

    private static double NormalizeZero(
        double value)
    {
        return value == 0d
            ? 0d
            : value;
    }
}