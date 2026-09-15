using Plaquewright.Core.Numerics;

namespace Plaquewright.Core.Combat;

internal static class ProtectionSharedCapacityAllocator
{
    public static ProtectionSharedCapacityAllocation AllocateProportionally(
        double availableCapacity,
        IReadOnlyList<double> requestedCapacities)
    {
        ValidateCapacity(
            availableCapacity,
            nameof(availableCapacity));

        ArgumentNullException.ThrowIfNull(
            requestedCapacities);

        var requests =
            new double[
                requestedCapacities.Count];

        var totalRequested =
            0d;

        var lastPositiveIndex =
            -1;

        for (var index = 0;
             index < requestedCapacities.Count;
             index++)
        {
            var requested =
                requestedCapacities[index];

            ValidateCapacity(
                requested,
                nameof(requestedCapacities));

            requests[index] =
                requested;

            totalRequested +=
                requested;

            if (!double.IsFinite(
                    totalRequested))
            {
                throw new OverflowException(
                    "Total requested shared protection capacity exceeds the finite numeric range.");
            }

            if (requested > 0d)
            {
                lastPositiveIndex =
                    index;
            }
        }

        var allocations =
            new double[
                requests.Length];

        var targetAllocation =
            Math.Min(
                availableCapacity,
                totalRequested);

        if (targetAllocation == 0d)
        {
            return new ProtectionSharedCapacityAllocation(
                availableCapacity,
                totalRequested,
                allocations);
        }

        // Capacity is sufficient:
        // every claim receives exactly what it requested.
        if (availableCapacity >=
            totalRequested)
        {
            Array.Copy(
                requests,
                allocations,
                requests.Length);

            return new ProtectionSharedCapacityAllocation(
                availableCapacity,
                totalRequested,
                allocations);
        }

        var scale =
            availableCapacity /
            totalRequested;

        var allocatedSoFar =
            0d;

        for (var index = 0;
             index < requests.Length;
             index++)
        {
            var requested =
                requests[index];

            if (requested == 0d)
            {
                continue;
            }

            double allocated;

            if (index ==
                lastPositiveIndex)
            {
                // Absorb only the representational remainder.
                // This is not gameplay priority.
                allocated =
                    targetAllocation -
                    allocatedSoFar;
            }
            else
            {
                allocated =
                    requested *
                    scale;
            }

            if (allocated < 0d)
            {
                if (NumericComparison.AreEquivalent(
                        allocated,
                        0d,
                        scale: targetAllocation))
                {
                    allocated =
                        0d;
                }
                else
                {
                    throw new InvalidOperationException(
                        "Shared protection capacity allocation became negative.");
                }
            }

            if (allocated >
                requested)
            {
                if (NumericComparison.AreEquivalent(
                        allocated,
                        requested,
                        scale: requested))
                {
                    allocated =
                        requested;
                }
                else
                {
                    throw new InvalidOperationException(
                        "Shared protection capacity allocation exceeded its claim.");
                }
            }

            allocations[index] =
                allocated == 0d
                    ? 0d
                    : allocated;

            allocatedSoFar +=
                allocations[index];
        }

        return new ProtectionSharedCapacityAllocation(
            availableCapacity,
            totalRequested,
            allocations);
    }

    private static void ValidateCapacity(
        double capacity,
        string parameterName)
    {
        if (!double.IsFinite(
                capacity))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                capacity,
                "Protection capacity must be finite.");
        }

        if (capacity < 0d)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                capacity,
                "Protection capacity cannot be negative.");
        }
    }
}