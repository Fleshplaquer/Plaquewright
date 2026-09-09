using Idler.Core.Tags;

namespace Idler.Core.Tests.Tags;

public sealed class TagDefinitionTests
{
    [Fact]
    public void Constructor_WithOnlyKey_HasNoImplications()
    {
        var definition = new TagDefinition(
            TagKey.Parse("damage.fire"));

        Assert.Equal(
            TagKey.Parse("damage.fire"),
            definition.Key);

        Assert.Empty(definition.ImpliedTags);
    }

    [Fact]
    public void Constructor_WithImplications_PreservesThem()
    {
        var elemental = TagKey.Parse("damage.elemental");
        var damage = TagKey.Parse("damage");

        var definition = new TagDefinition(
            TagKey.Parse("damage.fire"),
            [elemental, damage]);

        Assert.Equal(
            [elemental, damage],
            definition.ImpliedTags);
    }

    [Fact]
    public void Constructor_CopiesProvidedCollection()
    {
        var implications = new[]
        {
            TagKey.Parse("damage.elemental")
        };

        var definition = new TagDefinition(
            TagKey.Parse("damage.fire"),
            implications);

        implications[0] = TagKey.Parse("damage.cold");

        Assert.Equal(
            TagKey.Parse("damage.elemental"),
            definition.ImpliedTags[0]);
    }
}