namespace Plaquewright.Core.Resources;

public readonly record struct ResourceId
    : IComparable<ResourceId>
{
    public int Value { get; }

    public bool IsValid =>
        Value > 0;

    public ResourceId(int value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "ResourceId must be greater than zero.");
        }

        Value = value;
    }

    public int CompareTo(ResourceId other)
    {
        return Value.CompareTo(
            other.Value);
    }

    public override string ToString()
    {
        return IsValid
            ? Value.ToString()
            : "<invalid>";
    }
}