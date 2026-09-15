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