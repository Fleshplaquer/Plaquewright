namespace Plaquewright.Core.Resources;

public sealed class ResourceState
{
    public ResourceId Id { get; }

    public double Current { get; private set; }

    public double Maximum { get; private set; }

    public ulong Revision { get; private set; }

    public ResourceState(
        ResourceId id,
        double current,
        double maximum)
    {
        if (!id.IsValid)
        {
            throw new ArgumentException(
                "Resource ID must be valid.",
                nameof(id));
        }

        ValidateValues(
            current,
            maximum);

        Id = id;
        Current = NormalizeZero(current);
        Maximum = NormalizeZero(maximum);
        Revision = 0UL;
    }

    internal void ValidateCanSetValues(
    double current,
    double maximum)
    {
        ValidateValues(
            current,
            maximum);

        if (Revision ==
            ulong.MaxValue)
        {
            throw new OverflowException(
                "Resource state revision space has been exhausted.");
        }
    }

    internal void SetValues(
        double current,
        double maximum)
    {
        ValidateCanSetValues(
            current,
            maximum);

        Current =
            NormalizeZero(current);

        Maximum =
            NormalizeZero(maximum);

        Revision++;
    }

    private static void ValidateValues(
        double current,
        double maximum)
    {
        if (!double.IsFinite(current))
        {
            throw new ArgumentOutOfRangeException(
                nameof(current),
                current,
                "Current resource value must be finite.");
        }

        if (!double.IsFinite(maximum))
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximum),
                maximum,
                "Maximum resource value must be finite.");
        }

        if (maximum < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximum),
                maximum,
                "Maximum resource value cannot be negative.");
        }

        if (current < 0d)
        {
            throw new ArgumentOutOfRangeException(
                nameof(current),
                current,
                "Current resource value cannot be negative.");
        }

        if (current > maximum)
        {
            throw new ArgumentOutOfRangeException(
                nameof(current),
                current,
                "Current resource value cannot exceed maximum.");
        }
    }

    private static double NormalizeZero(
        double value)
    {
        return value == 0d
            ? 0d
            : value;
    }
}