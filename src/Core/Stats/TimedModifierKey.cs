namespace Plaquewright.Core.Stats;

public readonly record struct TimedModifierKey
{
    public string Value { get; }

    public bool IsValid =>
        !string.IsNullOrEmpty(
            Value);

    private TimedModifierKey(
        string value)
    {
        Value =
            value;
    }

    public static TimedModifierKey Parse(
        string value)
    {
        if (!TryParse(
                value,
                out var key))
        {
            throw new FormatException(
                $"'{value}' is not a valid timed modifier key.");
        }

        return key;
    }

    public static bool TryParse(
        string? value,
        out TimedModifierKey key)
    {
        key =
            default;

        if (string.IsNullOrWhiteSpace(
                value))
        {
            return false;
        }

        if (value[0] == '.' ||
            value[^1] == '.')
        {
            return false;
        }

        var segmentHasCharacter =
            false;

        foreach (var character in value)
        {
            if (character == '.')
            {
                if (!segmentHasCharacter)
                {
                    return false;
                }

                segmentHasCharacter =
                    false;

                continue;
            }

            if (character is not
                (>= 'a' and <= 'z'
                 or >= '0' and <= '9'
                 or '_'))
            {
                return false;
            }

            segmentHasCharacter =
                true;
        }

        if (!segmentHasCharacter)
        {
            return false;
        }

        key =
            new TimedModifierKey(
                value);

        return true;
    }

    public override string ToString()
    {
        return Value ??
               string.Empty;
    }
}