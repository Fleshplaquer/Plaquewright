namespace Idler.Core.Simulation;

public readonly record struct RngDomainKey
{
    public string Value { get; }

    public bool IsValid =>
        !string.IsNullOrEmpty(Value);

    private RngDomainKey(string value)
    {
        Value = value;
    }

    public static RngDomainKey Parse(string value)
    {
        if (!TryParse(value, out var key))
        {
            throw new FormatException(
                $"'{value}' is not a valid RNG domain key.");
        }

        return key;
    }

    public static bool TryParse(
        string? value,
        out RngDomainKey key)
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

        key = new RngDomainKey(value);
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