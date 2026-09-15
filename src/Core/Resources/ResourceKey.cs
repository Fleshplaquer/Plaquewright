namespace Plaquewright.Core.Resources;

public readonly record struct ResourceKey
{
    public string Value { get; }

    public bool IsValid =>
    !string.IsNullOrEmpty(Value);

    private ResourceKey(string value)
    {
        Value = value;
    }

    public static ResourceKey Parse(string value)
    {
        if (!TryParse(value, out var key))
        {
            throw new FormatException(
                $"'{value}' is not a valid resource key.");
        }

        return key;
    }

    public static bool TryParse(
        string? value,
        out ResourceKey key)
    {
        key = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!IsValidValue(value))
        {
            return false;
        }

        key = new ResourceKey(value);

        return true;
    }

    public override string ToString()
    {
        return Value ?? string.Empty;
    }

    private static bool IsValidValue(string value)
    {
        if (value[0] == '.' ||
            value[^1] == '.')
        {
            return false;
        }

        var segmentHasCharacter = false;

        foreach (var character in value)
        {
            if (character == '.')
            {
                if (!segmentHasCharacter)
                {
                    return false;
                }

                segmentHasCharacter = false;
                continue;
            }

            if (character is not (
                >= 'a' and <= 'z'
                or >= '0' and <= '9'
                or '_'))
            {
                return false;
            }

            segmentHasCharacter = true;
        }

        return segmentHasCharacter;
    }
}