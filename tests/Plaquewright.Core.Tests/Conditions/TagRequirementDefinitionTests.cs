using Plaquewright.Core.Conditions;
using Plaquewright.Core.Tags;

namespace Plaquewright.Core.Tests.Conditions;

public sealed class TagRequirementDefinitionTests
{
    [Fact]
    public void Constructor_PreservesModeAndTags()
    {
        var fire =
            TagKey.Parse("damage.fire");

        var projectile =
            TagKey.Parse("skill.projectile");

        var definition =
            new TagRequirementDefinition(
                TagRequirementMode.All,
                [fire, projectile]);

        Assert.Equal(
            TagRequirementMode.All,
            definition.Mode);

        Assert.Equal(
            [fire, projectile],
            definition.Tags);
    }

    [Fact]
    public void Constructor_WithUnknownMode_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                new TagRequirementDefinition(
                    (TagRequirementMode)999,
                    [
                        TagKey.Parse("damage.fire")
                    ]));
    }

    [Fact]
    public void Constructor_CopiesProvidedCollection()
    {
        var tags = new[]
        {
            TagKey.Parse("damage.fire")
        };

        var definition =
            new TagRequirementDefinition(
                TagRequirementMode.All,
                tags);

        tags[0] =
            TagKey.Parse("damage.cold");

        Assert.Equal(
            TagKey.Parse("damage.fire"),
            definition.Tags[0]);
    }

    [Fact]
    public void Constructor_WithEmptyTags_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new TagRequirementDefinition(
                TagRequirementMode.All,
                []));
    }

    [Fact]
    public void Constructor_WithNullTags_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new TagRequirementDefinition(
                TagRequirementMode.All,
                null!));
    }
}