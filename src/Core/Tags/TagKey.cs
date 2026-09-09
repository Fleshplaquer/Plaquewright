namespace Idler.Core.Tags;

public readonly record struct TagKey
{
    public string Value { get; }

    private TagKey(string value)
    {
        Value = value;
    }

    public static TagKey Parse(string value)
    {
        if (!TryParse(value, out var tagKey))
        {
            throw new FormatException($"'{value}' is not a valid tag key.");
        }

        return tagKey;
    }

    public static bool TryParse(string? value, out TagKey tagKey)
    {
        tagKey = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (!IsValid(value))
        {
            return false;
        }

        tagKey = new TagKey(value);
        return true;
    }

    public override string ToString()
    {
        return Value ?? string.Empty;
    }

    private static bool IsValid(string value)
    {
        if (value[0] == '.' || value[^1] == '.')
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

            if (!IsAllowedCharacter(character))
            {
                return false;
            }

            segmentHasCharacter = true;
        }

        return segmentHasCharacter;
    }

    private static bool IsAllowedCharacter(char character)
    {
        return character is >= 'a' and <= 'z'
            or >= '0' and <= '9'
            or '_';
    }
}