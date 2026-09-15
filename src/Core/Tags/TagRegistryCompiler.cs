namespace Plaquewright.Core.Tags;

public static class TagRegistryCompiler
{
    public static CompiledTagRegistry Compile(
        IEnumerable<TagDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        var definitionArray = definitions.ToArray();

        if (definitionArray.Any(
        definition =>
            definition is null))
        {
            throw new ArgumentException(
                "Tag definitions must not contain null entries.",
                nameof(definitions));
        }

        ValidateDuplicateKeys(definitionArray);

        var orderedDefinitions = definitionArray
            .OrderBy(
                definition => definition.Key.Value,
                StringComparer.Ordinal)
            .ToArray();

        var idsByKey = new Dictionary<TagKey, TagId>();
        var keysById = new TagKey[orderedDefinitions.Length];

        for (var index = 0; index < orderedDefinitions.Length; index++)
        {
            var key = orderedDefinitions[index].Key;

            // TagId 0 is reserved for invalid/default.
            var id = new TagId(index + 1);

            idsByKey.Add(key, id);
            keysById[index] = key;
        }

        var directImplications =
            BuildDirectImplications(
                orderedDefinitions,
                idsByKey);

        ValidateAcyclic(
            directImplications,
            keysById);

        var effectiveImplications =
            BuildEffectiveImplications(
                directImplications);

        return new CompiledTagRegistry(
            idsByKey,
            keysById,
            directImplications,
            effectiveImplications);
    }

    private static void ValidateDuplicateKeys(
        IReadOnlyList<TagDefinition> definitions)
    {
        var seen = new HashSet<TagKey>();

        foreach (var definition in definitions)
        {
            if (!seen.Add(definition.Key))
            {
                throw new InvalidOperationException(
                    $"Duplicate tag definition '{definition.Key}'.");
            }
        }
    }

    private static TagId[][] BuildDirectImplications(
        IReadOnlyList<TagDefinition> definitions,
        IReadOnlyDictionary<TagKey, TagId> idsByKey)
    {
        var result = new TagId[definitions.Count][];

        foreach (var definition in definitions)
        {
            var sourceId = idsByKey[definition.Key];

            var implications = new HashSet<TagId>();

            foreach (var impliedKey in definition.ImpliedTags)
            {
                if (!idsByKey.TryGetValue(
                        impliedKey,
                        out var impliedId))
                {
                    throw new InvalidOperationException(
                        $"Tag '{definition.Key}' implies unknown tag '{impliedKey}'.");
                }

                implications.Add(impliedId);
            }

            result[ToIndex(sourceId)] = implications
                .OrderBy(id => id.Value)
                .ToArray();
        }

        return result;
    }

    private static void ValidateAcyclic(
        IReadOnlyList<TagId[]> directImplications,
        IReadOnlyList<TagKey> keysById)
    {
        var states =
            new VisitState[directImplications.Count];

        for (var index = 0;
             index < directImplications.Count;
             index++)
        {
            if (states[index] != VisitState.Unvisited)
            {
                continue;
            }

            Visit(
                new TagId(index + 1),
                directImplications,
                keysById,
                states);
        }
    }

    private static void Visit(
        TagId current,
        IReadOnlyList<TagId[]> directImplications,
        IReadOnlyList<TagKey> keysById,
        VisitState[] states)
    {
        var currentIndex = ToIndex(current);

        states[currentIndex] = VisitState.Visiting;

        foreach (var next
                 in directImplications[currentIndex])
        {
            var nextIndex = ToIndex(next);

            if (states[nextIndex] == VisitState.Visiting)
            {
                throw new InvalidOperationException(
                    $"Tag implication cycle detected involving " +
                    $"'{keysById[currentIndex]}' and " +
                    $"'{keysById[nextIndex]}'.");
            }

            if (states[nextIndex] == VisitState.Unvisited)
            {
                Visit(
                    next,
                    directImplications,
                    keysById,
                    states);
            }
        }

        states[currentIndex] = VisitState.Visited;
    }

    private static TagId[][] BuildEffectiveImplications(
        IReadOnlyList<TagId[]> directImplications)
    {
        var result =
            new TagId[directImplications.Count][];

        for (var index = 0;
             index < directImplications.Count;
             index++)
        {
            var visited = new HashSet<TagId>();

            CollectImplications(
                new TagId(index + 1),
                directImplications,
                visited);

            result[index] = visited
                .OrderBy(id => id.Value)
                .ToArray();
        }

        return result;
    }

    private static void CollectImplications(
        TagId current,
        IReadOnlyList<TagId[]> directImplications,
        ISet<TagId> visited)
    {
        foreach (var next
                 in directImplications[ToIndex(current)])
        {
            if (!visited.Add(next))
            {
                continue;
            }

            CollectImplications(
                next,
                directImplications,
                visited);
        }
    }

    private static int ToIndex(TagId id)
    {
        return id.Value - 1;
    }

    private enum VisitState
    {
        Unvisited,
        Visiting,
        Visited
    }
}