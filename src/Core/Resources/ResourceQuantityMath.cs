namespace Plaquewright.Core.Resources;

internal static class ResourceQuantityMath
{
    public static double CalculateCurrentAfterLoss(
        double current,
        double requestedLoss,
        out double actualLoss)
    {
        ValidateNonNegativeFinite(
            current,
            nameof(current));

        ValidateNonNegativeFinite(
            requestedLoss,
            nameof(requestedLoss));

        var boundedLoss =
            Math.Min(
                current,
                requestedLoss);

        if (boundedLoss == 0d)
        {
            actualLoss =
                0d;

            return current;
        }

        var currentAfter =
            current -
            boundedLoss;

        var realizedLoss =
            current -
            currentAfter;

        //
        // IEEE-754 rounding can move the result to the
        // lower neighbouring value and thereby realize
        // MORE loss than was requested.
        //
        // Move one representable value back toward
        // 'current' in that case.
        //
        if (realizedLoss >
            boundedLoss)
        {
            currentAfter =
                Math.BitIncrement(
                    currentAfter);

            realizedLoss =
                current -
                currentAfter;
        }

        if (realizedLoss < 0d ||
            realizedLoss > boundedLoss)
        {
            throw new InvalidOperationException(
                "Representable resource loss exceeded its valid bounds.");
        }

        actualLoss =
            realizedLoss == 0d
                ? 0d
                : realizedLoss;

        return currentAfter == 0d
            ? 0d
            : currentAfter;
    }

    private static void ValidateNonNegativeFinite(
        double value,
        string parameterName)
    {
        if (!double.IsFinite(
                value))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Resource quantity must be finite.");
        }

        if (value < 0d)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Resource quantity cannot be negative.");
        }
    }

    private const double CostRelativeRepresentationTolerance =
    1e-9d;

    public static bool TryCalculateCurrentAfterCost(
        double current,
        double requestedCost,
        out double currentAfter,
        out double actualCost)
    {
        ValidateNonNegativeFinite(
            current,
            nameof(current));

        ValidateNonNegativeFinite(
            requestedCost,
            nameof(requestedCost));

        if (requestedCost >
            current)
        {
            currentAfter =
                current;

            actualCost =
                0d;

            return false;
        }

        if (requestedCost == 0d)
        {
            currentAfter =
                current;

            actualCost =
                0d;

            return true;
        }

        var candidateAfter =
            CalculateCurrentAfterLoss(
                current,
                requestedCost,
                out var realizedCost);

        var relativeError =
            Math.Abs(
                requestedCost -
                realizedCost) /
            requestedCost;

        if (relativeError >
            CostRelativeRepresentationTolerance)
        {
            currentAfter =
                current;

            actualCost =
                0d;

            return false;
        }

        currentAfter =
            candidateAfter;

        actualCost =
            realizedCost;

        return true;
    }
    public static double CalculateCurrentAfterRecovery(
    double current,
    double maximum,
    double requestedRecovery,
    out double actualRecovery)
    {
        ValidateNonNegativeFinite(
            current,
            nameof(current));

        ValidateNonNegativeFinite(
            maximum,
            nameof(maximum));

        ValidateNonNegativeFinite(
            requestedRecovery,
            nameof(requestedRecovery));

        if (current >
            maximum)
        {
            throw new ArgumentOutOfRangeException(
                nameof(current),
                current,
                "Current resource value cannot exceed maximum.");
        }

        var availableCapacity =
            maximum -
            current;

        var boundedRecovery =
            Math.Min(
                requestedRecovery,
                availableCapacity);

        if (boundedRecovery == 0d)
        {
            actualRecovery =
                0d;

            return current;
        }

        var currentAfter =
            current +
            boundedRecovery;

        if (currentAfter >
            maximum)
        {
            currentAfter =
                maximum;
        }

        var realizedRecovery =
            currentAfter -
            current;

        //
        // IEEE-754 rounding can move an addition to the
        // higher neighbouring value and thereby realize
        // MORE recovery than was requested.
        //
        // Move one representable value back toward
        // 'current' in that case.
        //
        if (realizedRecovery >
            boundedRecovery)
        {
            currentAfter =
                Math.BitDecrement(
                    currentAfter);

            realizedRecovery =
                currentAfter -
                current;
        }

        if (realizedRecovery < 0d ||
            realizedRecovery >
            boundedRecovery ||
            currentAfter >
            maximum)
        {
            throw new InvalidOperationException(
                "Representable resource recovery exceeded its valid bounds.");
        }

        actualRecovery =
            realizedRecovery == 0d
                ? 0d
                : realizedRecovery;

        return currentAfter == 0d
            ? 0d
            : currentAfter;
    }
}