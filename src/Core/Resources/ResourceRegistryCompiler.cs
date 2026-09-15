namespace Plaquewright.Core.Resources;

public static class ResourceRegistryCompiler
{
    public static CompiledResourceRegistry Compile(
        IEnumerable<ResourceDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        var ordered =
            definitions
                .Select(definition =>
                    definition
                    ?? throw new ArgumentException(
                        "Resource definitions cannot contain null entries.",
                        nameof(definitions)))
                .OrderBy(
                    definition => definition.Key.Value,
                    StringComparer.Ordinal)
                .ToArray();

        ValidateNoDuplicateKeys(
            ordered);

        var idsByKey =
            new Dictionary<ResourceKey, ResourceId>();

        var compiledDefinitions =
            new CompiledResourceDefinition[
                ordered.Length];

        for (var index = 0;
             index < ordered.Length;
             index++)
        {
            var source =
                ordered[index];

            var id =
                new ResourceId(
                    index + 1);

            idsByKey.Add(
                source.Key,
                id);

            compiledDefinitions[index] =
                new CompiledResourceDefinition(
                    id,
                    source.Key,
                    source.Roles);
        }

        return new CompiledResourceRegistry(
            idsByKey,
            compiledDefinitions);
    }

    private static void ValidateNoDuplicateKeys(
        IReadOnlyList<ResourceDefinition> definitions)
    {
        for (var index = 1;
             index < definitions.Count;
             index++)
        {
            if (definitions[index - 1].Key ==
                definitions[index].Key)
            {
                throw new InvalidOperationException(
                    $"Duplicate resource key " +
                    $"'{definitions[index].Key}'.");
            }
        }
    }
}