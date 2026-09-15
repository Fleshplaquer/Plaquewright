using Plaquewright.Core.Tags;

namespace Plaquewright.Core.Tests.Tags;

public sealed class CompiledTagSetTests
{
    private static CompiledTagRegistry CreateRegistry()
    {
        var damage = TagKey.Parse("damage");
        var elemental = TagKey.Parse("damage.elemental");
        var fire = TagKey.Parse("damage.fire");
        var projectile = TagKey.Parse("skill.projectile");

        return TagRegistryCompiler.Compile(
        [
            new TagDefinition(damage),
            new TagDefinition(elemental, [damage]),
            new TagDefinition(fire, [elemental]),
            new TagDefinition(projectile)
        ]);
    }

    [Fact]
    public void CompileTagSet_PreservesDirectTags()
    {
        var registry = CreateRegistry();

        var fire = registry.GetId(
            TagKey.Parse("damage.fire"));

        var projectile = registry.GetId(
            TagKey.Parse("skill.projectile"));

        var set = registry.CompileTagSet(
        [
            TagKey.Parse("damage.fire"),
            TagKey.Parse("skill.projectile")
        ]);

        Assert.True(set.HasDirect(fire));
        Assert.True(set.HasDirect(projectile));
    }

    [Fact]
    public void CompileTagSet_AddsTransitiveEffectiveTags()
    {
        var registry = CreateRegistry();

        var fire = registry.GetId(
            TagKey.Parse("damage.fire"));

        var elemental = registry.GetId(
            TagKey.Parse("damage.elemental"));

        var damage = registry.GetId(
            TagKey.Parse("damage"));

        var set = registry.CompileTagSet(
        [
            TagKey.Parse("damage.fire")
        ]);

        Assert.True(set.Has(fire));
        Assert.True(set.Has(elemental));
        Assert.True(set.Has(damage));
    }

    [Fact]
    public void ImpliedTags_AreNotDirectTags()
    {
        var registry = CreateRegistry();

        var elemental = registry.GetId(
            TagKey.Parse("damage.elemental"));

        var set = registry.CompileTagSet(
        [
            TagKey.Parse("damage.fire")
        ]);

        Assert.False(set.HasDirect(elemental));
        Assert.True(set.Has(elemental));
    }

    [Fact]
    public void CompileTagSet_DeduplicatesDirectTags()
    {
        var registry = CreateRegistry();

        var set = registry.CompileTagSet(
        [
            TagKey.Parse("damage.fire"),
            TagKey.Parse("damage.fire")
        ]);

        Assert.Single(set.DirectTags);
    }

    [Fact]
    public void CompileTagSet_DeduplicatesEffectiveTags()
    {
        var registry = CreateRegistry();

        var set = registry.CompileTagSet(
        [
            TagKey.Parse("damage.fire"),
            TagKey.Parse("damage.elemental")
        ]);

        Assert.Equal(3, set.EffectiveTags.Count);
    }

    [Fact]
    public void MatchesAny_ReturnsSingleBooleanMatch()
    {
        var registry = CreateRegistry();

        var set = registry.CompileTagSet(
        [
            TagKey.Parse("damage.fire")
        ]);

        var elemental = registry.GetId(
            TagKey.Parse("damage.elemental"));

        var damage = registry.GetId(
            TagKey.Parse("damage"));

        Assert.True(
            set.MatchesAny([elemental, damage]));
    }

    [Fact]
    public void MatchesAll_RequiresEveryTag()
    {
        var registry = CreateRegistry();

        var set = registry.CompileTagSet(
        [
            TagKey.Parse("damage.fire")
        ]);

        var fire = registry.GetId(
            TagKey.Parse("damage.fire"));

        var elemental = registry.GetId(
            TagKey.Parse("damage.elemental"));

        var projectile = registry.GetId(
            TagKey.Parse("skill.projectile"));

        Assert.True(
            set.MatchesAll([fire, elemental]));

        Assert.False(
            set.MatchesAll([fire, projectile]));
    }

    [Fact]
    public void CompileTagSet_WithUnknownTag_Throws()
    {
        var registry = CreateRegistry();

        Assert.Throws<KeyNotFoundException>(() =>
            registry.CompileTagSet(
            [
                TagKey.Parse("damage.cold")
            ]));
    }
}