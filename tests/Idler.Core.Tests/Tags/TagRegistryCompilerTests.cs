using Idler.Core.Tags;

namespace Idler.Core.Tests.Tags;

public sealed class TagRegistryCompilerTests
{
    [Fact]
    public void Compile_AssignsIdsDeterministically()
    {
        var first = TagRegistryCompiler.Compile(
        [
            new(TagKey.Parse("damage.fire")),
            new(TagKey.Parse("damage")),
            new(TagKey.Parse("damage.elemental"))
        ]);

        var second = TagRegistryCompiler.Compile(
        [
            new(TagKey.Parse("damage.elemental")),
            new(TagKey.Parse("damage.fire")),
            new(TagKey.Parse("damage"))
        ]);

        Assert.Equal(
            first.GetId(TagKey.Parse("damage")),
            second.GetId(TagKey.Parse("damage")));

        Assert.Equal(
            first.GetId(TagKey.Parse("damage.elemental")),
            second.GetId(TagKey.Parse("damage.elemental")));

        Assert.Equal(
            first.GetId(TagKey.Parse("damage.fire")),
            second.GetId(TagKey.Parse("damage.fire")));
    }
    [Fact]
    public void Compile_WithNullDefinition_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () =>
                TagRegistryCompiler.Compile(
                [
                    new TagDefinition(
                    TagKey.Parse(
                        "damage")),

                null!
                ]));
    }
    [Fact]
    public void Compile_WithNullDefinitionAmongValidDefinitions_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(
            () =>
                TagRegistryCompiler.Compile(
                [
                    new TagDefinition(
                    TagKey.Parse(
                        "damage")),

                null!,

                new TagDefinition(
                    TagKey.Parse(
                        "damage.fire"))
                ]));
    }

    [Fact]
    public void Compile_BuildsTransitiveImplications()
    {
        var damage = TagKey.Parse("damage");
        var elemental = TagKey.Parse("damage.elemental");
        var fire = TagKey.Parse("damage.fire");

        var registry = TagRegistryCompiler.Compile(
        [
            new TagDefinition(damage),
            new TagDefinition(elemental, [damage]),
            new TagDefinition(fire, [elemental])
        ]);

        var fireId = registry.GetId(fire);

        var effective = registry
            .GetEffectiveImplications(fireId)
            .Select(registry.GetKey)
            .ToArray();

        Assert.Equal(
            [damage, elemental],
            effective.OrderBy(x => x.Value, StringComparer.Ordinal));
    }

    [Fact]
    public void Compile_DeduplicatesDuplicateImplicationPaths()
    {
        var damage = TagKey.Parse("damage");
        var elemental = TagKey.Parse("damage.elemental");
        var spell = TagKey.Parse("skill.spell");
        var fireSpell = TagKey.Parse("skill.fire_spell");

        var registry = TagRegistryCompiler.Compile(
        [
            new TagDefinition(damage),
            new TagDefinition(elemental, [damage]),
            new TagDefinition(spell, [damage]),
            new TagDefinition(fireSpell, [elemental, spell])
        ]);

        var effective = registry
            .GetEffectiveImplications(registry.GetId(fireSpell))
            .Select(registry.GetKey)
            .ToArray();

        Assert.Equal(3, effective.Length);

        Assert.Contains(damage, effective);
        Assert.Contains(elemental, effective);
        Assert.Contains(spell, effective);
    }

    [Fact]
    public void Compile_WithDuplicateDefinition_Throws()
    {
        var fire = TagKey.Parse("damage.fire");

        Assert.Throws<InvalidOperationException>(() =>
            TagRegistryCompiler.Compile(
            [
                new TagDefinition(fire),
                new TagDefinition(fire)
            ]));
    }

    [Fact]
    public void Compile_WithUnknownImplication_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            TagRegistryCompiler.Compile(
            [
                new TagDefinition(
                    TagKey.Parse("damage.fire"),
                    [TagKey.Parse("damage.elemental")])
            ]));
    }

    [Fact]
    public void Compile_WithDirectCycle_Throws()
    {
        var fire = TagKey.Parse("damage.fire");
        var elemental = TagKey.Parse("damage.elemental");

        Assert.Throws<InvalidOperationException>(() =>
            TagRegistryCompiler.Compile(
            [
                new TagDefinition(fire, [elemental]),
                new TagDefinition(elemental, [fire])
            ]));
    }

    [Fact]
    public void Compile_WithIndirectCycle_Throws()
    {
        var fire = TagKey.Parse("damage.fire");
        var elemental = TagKey.Parse("damage.elemental");
        var damage = TagKey.Parse("damage");

        Assert.Throws<InvalidOperationException>(() =>
            TagRegistryCompiler.Compile(
            [
                new TagDefinition(fire, [elemental]),
                new TagDefinition(elemental, [damage]),
                new TagDefinition(damage, [fire])
            ]));
    }

    [Fact]
    public void Compile_WithRepeatedDirectImplication_DeduplicatesIt()
    {
        var damage = TagKey.Parse("damage");
        var fire = TagKey.Parse("damage.fire");

        var registry = TagRegistryCompiler.Compile(
        [
            new TagDefinition(damage),
            new TagDefinition(fire, [damage, damage])
        ]);

        var implications =
            registry.GetDirectImplications(
                registry.GetId(fire));

        Assert.Single(implications);
    }
}