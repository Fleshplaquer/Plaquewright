using Plaquewright.Core.Conditions;
using Plaquewright.Core.Tags;

namespace Plaquewright.Core.Tests.Conditions;

public sealed class CompiledTagRequirementTests
{
    [Fact]
    public void AllRequirement_MatchesDirectAndImpliedTags()
    {
        var registry =
            CreateRegistry();

        var requirement =
            registry.CompileRequirement(
                new TagRequirementDefinition(
                    TagRequirementMode.All,
                    [
                        TagKey.Parse("damage.elemental"),
                        TagKey.Parse("skill.projectile")
                    ]));

        var tags =
            registry.CompileTagSet(
                [
                    TagKey.Parse("damage.fire"),
                    TagKey.Parse("skill.projectile")
                ]);

        Assert.True(
            requirement.Matches(tags));
    }
    [Fact]
    public void Requirement_FromDifferentRegistry_IsRejectedEvenWhenNumericTagIdsCollide()
    {
        var fire =
            TagKey.Parse(
                "damage.fire");

        var cold =
            TagKey.Parse(
                "damage.cold");

        var fireRegistry =
            TagRegistryCompiler.Compile(
            [
                new TagDefinition(
                fire)
            ]);

        var coldRegistry =
            TagRegistryCompiler.Compile(
            [
                new TagDefinition(
                cold)
            ]);

        // Both independent registries assign their only tag ID 1.
        Assert.Equal(
            fireRegistry.GetId(
                fire),
            coldRegistry.GetId(
                cold));

        var tagSet =
            fireRegistry.CompileTagSet(
            [
                fire
            ]);

        var requirement =
            coldRegistry.CompileRequirement(
                new TagRequirementDefinition(
                    TagRequirementMode.All,
                    [
                        cold
                    ]));

        Assert.Throws<InvalidOperationException>(
            () =>
                requirement.Matches(
                    tagSet));
    }
    [Fact]
    public void Requirement_FromSeparatelyCompiledEquivalentRegistry_IsRejected()
    {
        var fire =
            TagKey.Parse(
                "damage.fire");

        var firstRegistry =
            TagRegistryCompiler.Compile(
            [
                new TagDefinition(
                fire)
            ]);

        var secondRegistry =
            TagRegistryCompiler.Compile(
            [
                new TagDefinition(
                fire)
            ]);

        // Deterministic IDs remain equal.
        Assert.Equal(
            firstRegistry.GetId(
                fire),
            secondRegistry.GetId(
                fire));

        var tagSet =
            firstRegistry.CompileTagSet(
            [
                fire
            ]);

        var requirement =
            secondRegistry.CompileRequirement(
                new TagRequirementDefinition(
                    TagRequirementMode.All,
                    [
                        fire
                    ]));

        Assert.Throws<InvalidOperationException>(
            () =>
                requirement.Matches(
                    tagSet));
    }

    [Fact]
    public void AllRequirement_FailsWhenOneTagIsMissing()
    {
        var registry =
            CreateRegistry();

        var requirement =
            registry.CompileRequirement(
                new TagRequirementDefinition(
                    TagRequirementMode.All,
                    [
                        TagKey.Parse("damage.fire"),
                        TagKey.Parse("skill.projectile")
                    ]));

        var tags =
            registry.CompileTagSet(
                [
                    TagKey.Parse("damage.fire")
                ]);

        Assert.False(
            requirement.Matches(tags));
    }

    [Fact]
    public void AnyRequirement_MatchesWhenOneTagMatches()
    {
        var registry =
            CreateRegistry();

        var requirement =
            registry.CompileRequirement(
                new TagRequirementDefinition(
                    TagRequirementMode.Any,
                    [
                        TagKey.Parse("damage.cold"),
                        TagKey.Parse("damage.elemental")
                    ]));

        var tags =
            registry.CompileTagSet(
                [
                    TagKey.Parse("damage.fire")
                ]);

        Assert.True(
            requirement.Matches(tags));
    }

    [Fact]
    public void AnyRequirement_FailsWhenNothingMatches()
    {
        var registry =
            CreateRegistry();

        var requirement =
            registry.CompileRequirement(
                new TagRequirementDefinition(
                    TagRequirementMode.Any,
                    [
                        TagKey.Parse("damage.cold"),
                        TagKey.Parse("skill.projectile")
                    ]));

        var tags =
            registry.CompileTagSet(
                [
                    TagKey.Parse("damage.fire")
                ]);

        Assert.False(
            requirement.Matches(tags));
    }

    [Fact]
    public void Requirement_MatchingMultipleImpliedTags_StillReturnsOneMatch()
    {
        var registry =
            CreateRegistry();

        var requirement =
            registry.CompileRequirement(
                new TagRequirementDefinition(
                    TagRequirementMode.Any,
                    [
                        TagKey.Parse("damage"),
                        TagKey.Parse("damage.elemental")
                    ]));

        var tags =
            registry.CompileTagSet(
                [
                    TagKey.Parse("damage.fire")
                ]);

        Assert.True(
            requirement.Matches(tags));
    }

    [Fact]
    public void CompileRequirement_DeduplicatesTags()
    {
        var registry =
            CreateRegistry();

        var requirement =
            registry.CompileRequirement(
                new TagRequirementDefinition(
                    TagRequirementMode.All,
                    [
                        TagKey.Parse("damage.fire"),
                        TagKey.Parse("damage.fire")
                    ]));

        Assert.Single(
            requirement.Tags);
    }

    [Fact]
    public void CompileRequirement_WithUnknownTag_Throws()
    {
        var registry =
            CreateRegistry();

        var definition =
            new TagRequirementDefinition(
                TagRequirementMode.All,
                [
                    TagKey.Parse("damage.chaos")
                ]);

        Assert.Throws<KeyNotFoundException>(
            () =>
                registry.CompileRequirement(
                    definition));
    }

    private static CompiledTagRegistry CreateRegistry()
    {
        var damage =
            TagKey.Parse("damage");

        var elemental =
            TagKey.Parse("damage.elemental");

        var fire =
            TagKey.Parse("damage.fire");

        var cold =
            TagKey.Parse("damage.cold");

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

            new TagDefinition(
                cold,
                [elemental]),

            new TagDefinition(
                projectile)
        ]);
    }
}