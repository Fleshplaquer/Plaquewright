using Idler.Core;

namespace Idler.Core.Tests;

public sealed class CoreBoundaryTests
{
    [Fact]
    public void CoreAssembly_IsAccessible()
    {
        Assert.Equal("Idler.Core", CoreInfo.Name);
    }
}