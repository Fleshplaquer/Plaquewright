namespace Idler.Core.Combat;

public static class ProtectionAssignmentResolver
{
    public static ProtectionAssignmentResolution Resolve(
        double damage,
        IReadOnlyList<ProtectionAssignmentRequest> requests)
    {
        ValidateDamage(
            damage);

        ArgumentNullException.ThrowIfNull(
            requests);

        var copiedRequests =
            new ProtectionAssignmentRequest[
                requests.Count];

        var requestedFractionTotal =
            0d;

        var lastPositiveIndex =
            -1;

        for (var index = 0;
             index < requests.Count;
             index++)
        {
            var request =
                requests[index];

            ArgumentNullException.ThrowIfNull(
                request);

            copiedRequests[index] =
                request;

            requestedFractionTotal +=
                request.RequestedFraction;

            if (!double.IsFinite(
                    requestedFractionTotal))
            {
                throw new OverflowException(
                    "Total requested protection assignment fraction exceeds the finite numeric range.");
            }

            if (request.RequestedFraction > 0d)
            {
                lastPositiveIndex =
                    index;
            }
        }

        var appliedFractionTotal =
            Math.Min(
                1d,
                requestedFractionTotal);

        var assignmentScale =
            requestedFractionTotal > 1d
                ? 1d / requestedFractionTotal
                : 1d;

        var totalAssignedDamage =
            damage *
            appliedFractionTotal;

        var primaryPathDamage =
            damage -
            totalAssignedDamage;

        var results =
            new ProtectionAssignmentResult[
                copiedRequests.Length];

        var assignedSoFar =
            0d;

        for (var index = 0;
             index < copiedRequests.Length;
             index++)
        {
            var request =
                copiedRequests[index];

            var appliedFraction =
                request.RequestedFraction *
                assignmentScale;

            double assignedDamage;

            if (index ==
                lastPositiveIndex)
            {
                assignedDamage =
                    totalAssignedDamage -
                    assignedSoFar;
            }
            else
            {
                assignedDamage =
                    damage *
                    appliedFraction;

                assignedSoFar +=
                    assignedDamage;
            }

            results[index] =
                new ProtectionAssignmentResult(
                    request,
                    appliedFraction,
                    assignedDamage);
        }

        return new ProtectionAssignmentResolution(
            damage,
            requestedFractionTotal,
            appliedFractionTotal,
            assignmentScale,
            primaryPathDamage,
            results);
    }

    private static void ValidateDamage(
        double damage)
    {
        if (!double.IsFinite(
                damage))
        {
            throw new ArgumentOutOfRangeException(
                nameof(damage),
                damage,
                "Damage must be finite.");
        }

        if (damage < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(damage),
                damage,
                "Damage cannot be negative.");
        }
    }
}