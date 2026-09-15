using System.Reflection;
using Plaquewright.Core.Resources;

namespace Plaquewright.Core.Tests.Resources;

public sealed class ResourceOperationLedgerApiBoundaryTests
{
    [Fact]
    public void Append_IsAssemblyInternal_NotPublic()
    {
        var publicAppend =
            typeof(ResourceOperationLedger)
                .GetMethod(
                    "Append",
                    BindingFlags.Instance |
                    BindingFlags.Public);

        Assert.Null(
            publicAppend);

        var internalAppend =
            typeof(ResourceOperationLedger)
                .GetMethod(
                    "Append",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

        Assert.NotNull(
            internalAppend);

        Assert.True(
            internalAppend.IsAssembly);
    }

    [Fact]
    public void LedgerEntryConstructor_IsAssemblyInternal()
    {
        var constructors =
            typeof(ResourceOperationLedgerEntry)
                .GetConstructors(
                    BindingFlags.Instance |
                    BindingFlags.NonPublic);

        var constructor =
            Assert.Single(
                constructors);

        Assert.True(
            constructor.IsAssembly);

        Assert.False(
            constructor.IsFamily);

        Assert.False(
            constructor.IsFamilyOrAssembly);
    }

    [Fact]
    public void Ledger_RemainsPubliclyReadable()
    {
        var countProperty =
            typeof(ResourceOperationLedger)
                .GetProperty(
                    nameof(
                        ResourceOperationLedger.Count));

        var entriesProperty =
            typeof(ResourceOperationLedger)
                .GetProperty(
                    nameof(
                        ResourceOperationLedger.Entries));

        Assert.NotNull(
            countProperty);

        Assert.NotNull(
            entriesProperty);

        Assert.NotNull(
            countProperty.GetMethod);

        Assert.NotNull(
            entriesProperty.GetMethod);

        Assert.True(
            countProperty.GetMethod.IsPublic);

        Assert.True(
            entriesProperty.GetMethod.IsPublic);
    }
}