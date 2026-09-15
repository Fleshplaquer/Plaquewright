using Plaquewright.Core.Tags;

namespace Plaquewright.Core.Tests.Tags;

public sealed class TagDefinitionTests
{
    [Fact]
    public void Constructor_WithOnlyKey_HasNoImplications()
    {
        var definition =
            new TagDefinition(
                TagKey.Parse(
                    "damage.fire"));

        Assert.Equal(
            TagKey.Parse(
                "damage.fire"),
            definition.Key);

        Assert.Empty(
            definition.ImpliedTags);
    }

    [Fact]
    public void Constructor_WithImplications_PreservesThem()
    {
        var elemental =
            TagKey.Parse(
                "damage.elemental");

        var damage =
            TagKey.Parse(
                "damage");

        var definition =
            new TagDefinition(
                TagKey.Parse(
                    "damage.fire"),
                [
                    elemental,
                    damage
                ]);

        Assert.Equal(
            [
                elemental,
                damage
            ],
            definition.ImpliedTags);
    }

    [Fact]
    public void Constructor_CopiesProvidedCollection()
    {
        var implications =
            new[]
            {
                TagKey.Parse(
                    "damage.elemental")
            };

        var definition =
            new TagDefinition(
                TagKey.Parse(
                    "damage.fire"),
                implications);

        implications[0] =
            TagKey.Parse(
                "damage.cold");

        Assert.Equal(
            TagKey.Parse(
                "damage.elemental"),
            definition.ImpliedTags[0]);
    }

    [Fact]
    public void Constructor_WithDefaultKey_Throws()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new TagDefinition(
                    default));
    }
    [Fact]
    public void Constructor_WithDefaultImpliedTag_Throws()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new TagDefinition(
                    TagKey.Parse(
                        "damage.fire"),
                    [
                        default
                    ]));
    }

    [Fact]
    public void Constructor_WithDefaultAmongValidImpliedTags_Throws()
    {
        Assert.Throws<ArgumentException>(
            () =>
                new TagDefinition(
                    TagKey.Parse(
                        "damage.fire"),
                    [
                        TagKey.Parse(
                        "damage.elemental"),

                    default,

                    TagKey.Parse(
                        "damage")
                    ]));
    }
}