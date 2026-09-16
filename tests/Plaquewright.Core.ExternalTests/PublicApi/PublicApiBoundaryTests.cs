using System.Reflection;
using System.Runtime.CompilerServices;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.ExternalTests.PublicApi;

public sealed class PublicApiBoundaryTests
{
    [Fact]
    public void ExternalTestAssembly_IsNotCoreFriendAssembly()
    {
        var coreAssembly =
            typeof(ResourceState).Assembly;

        var friendAssemblies =
            coreAssembly
                .GetCustomAttributes<InternalsVisibleToAttribute>()
                .Select(
                    attribute =>
                        attribute.AssemblyName)
                .ToArray();

        Assert.DoesNotContain(
            friendAssemblies,
            assemblyName =>
                assemblyName.StartsWith(
                    "Plaquewright.Core.ExternalTests",
                    StringComparison.Ordinal));
    }

    [Fact]
    public void ResourcePreparedPublicationInfrastructure_IsNotPublic()
    {
        var coreAssembly =
            typeof(ResourceState).Assembly;

        var committerType =
            coreAssembly.GetType(
                "Plaquewright.Core.Resources.ResourceTransactionCommitter",
                throwOnError: true)!;

        var preparedCommitType =
            coreAssembly.GetType(
                "Plaquewright.Core.Resources.PreparedResourceTransactionCommit",
                throwOnError: true)!;

        Assert.False(
            committerType.IsPublic);

        Assert.False(
            preparedCommitType.IsPublic);

        var publicApplyPrepared =
            committerType.GetMethod(
                "ApplyPrepared",
                BindingFlags.Static |
                BindingFlags.Public);

        Assert.Null(
            publicApplyPrepared);

        var publicPreparedLedgerAppend =
            typeof(ResourceOperationLedger)
                .GetMethod(
                    "ApplyPreparedAppend",
                    BindingFlags.Instance |
                    BindingFlags.Public);

        Assert.Null(
            publicPreparedLedgerAppend);
    }

    [Fact]
    public void ExternalTestAssembly_IsDifferentFromRegularCoreTests()
    {
        var currentAssemblyName =
            typeof(PublicApiBoundaryTests)
                .Assembly
                .GetName()
                .Name;

        Assert.Equal(
            "Plaquewright.Core.ExternalTests",
            currentAssemblyName);
    }
}