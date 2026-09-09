using Idler.Core.Tags;

namespace Idler.Core.Tests.Tags;

public sealed class TagSystemInvariantTests
{
    [Fact]
    public void Compile_AllPermutations_ProduceSameIds()
    {
        var damage =
            new TagDefinition(
                TagKey.Parse("damage"));

        var elemental =
            new TagDefinition(
                TagKey.Parse("damage.elemental"),
                [TagKey.Parse("damage")]);

        var fire =
            new TagDefinition(
                TagKey.Parse("damage.fire"),
                [TagKey.Parse("damage.elemental")]);

        var permutations = new[]
        {
            new[] { damage, elemental, fire },
            new[] { damage, fire, elemental },
            new[] { elemental, damage, fire },
            new[] { elemental, fire, damage },
            new[] { fire, damage, elemental },
            new[] { fire, elemental, damage }
        };

        var registries = permutations
            .Select(TagRegistryCompiler.Compile)
            .ToArray();

        var keys = new[]
        {
            TagKey.Parse("damage"),
            TagKey.Parse("damage.elemental"),
            TagKey.Parse("damage.fire")
        };

        foreach (var key in keys)
        {
            var expected =
                registries[0].GetId(key);

            Assert.All(
                registries,
                registry =>
                    Assert.Equal(
                        expected,
                        registry.GetId(key)));
        }
    }

    [Fact]
    public void EffectiveImplications_AreTransitivelyClosed()
    {
        var registry = CreateRegistry();

        for (var value = 1;
             value <= registry.Count;
             value++)
        {
            var source = new TagId(value);

            foreach (var implied
                     in registry.GetEffectiveImplications(source))
            {
                foreach (var transitivelyImplied
                         in registry.GetEffectiveImplications(implied))
                {
                    Assert.Contains(
                        transitivelyImplied,
                        registry.GetEffectiveImplications(source));
                }
            }
        }
    }

    [Fact]
    public void EffectiveTagSets_ContainNoDuplicates()
    {
        var registry = CreateRegistry();

        var set = registry.CompileTagSet(
        [
            TagKey.Parse("damage.fire"),
            TagKey.Parse("damage.elemental")
        ]);

        Assert.Equal(
            set.EffectiveTags.Count,
            set.EffectiveTags
                .Distinct()
                .Count());
    }

    [Fact]
    public void EveryDirectTag_IsAlsoEffective()
    {
        var registry = CreateRegistry();

        var set = registry.CompileTagSet(
        [
            TagKey.Parse("damage.fire"),
            TagKey.Parse("skill.projectile")
        ]);

        foreach (var directTag in set.DirectTags)
        {
            Assert.True(
                set.Has(directTag));
        }
    }

    [Fact]
    public void MatchesAny_WithEmptyRequirements_IsFalse()
    {
        var registry = CreateRegistry();

        var set = registry.CompileTagSet(
        [
            TagKey.Parse("damage.fire")
        ]);

        Assert.False(
            set.MatchesAny([]));
    }

    [Fact]
    public void MatchesAll_WithEmptyRequirements_IsTrue()
    {
        var registry = CreateRegistry();

        var set = registry.CompileTagSet(
        [
            TagKey.Parse("damage.fire")
        ]);

        Assert.True(
            set.MatchesAll([]));
    }

    [Fact]
    public void DefaultTagId_IsRejectedByRegistry()
    {
        var registry = CreateRegistry();

        TagId invalid = default;

        Assert.Throws<ArgumentOutOfRangeException>(
            () => registry.GetKey(invalid));
    }

    private static CompiledTagRegistry CreateRegistry()
    {
        var damage =
            TagKey.Parse("damage");

        var elemental =
            TagKey.Parse("damage.elemental");

        var fire =
            TagKey.Parse("damage.fire");

        var projectile =
            TagKey.Parse("skill.projectile");

        return TagRegistryCompiler.Compile(
        [
            new TagDefinition(damage),

            new TagDefinition(
                elemental,
                [damage]),

            new TagDefinition(
                fire,
                [elemental]),

            new TagDefinition(projectile)
        ]);
    }
}