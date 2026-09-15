namespace Plaquewright.Core.Tags;

public readonly record struct TagId
{
    public int Value { get; }

    public bool IsValid => Value > 0;

    public TagId(int value)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "TagId must be greater than zero.");
        }

        Value = value;
    }

    public override string ToString()
    {
        return IsValid
            ? Value.ToString()
            : "<invalid>";
    }
}