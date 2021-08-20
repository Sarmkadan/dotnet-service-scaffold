#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Infrastructure.Logging;
using FluentAssertions;
using Xunit;

/// <summary>
/// Tests for the LogContextService class.
/// </summary>
public class LogContextServiceTests
{
    private readonly LogContextService _service = new();

    /// <summary>
    /// Verifies that the CorrelationId property returns the set value.
    /// </summary>
    [Fact]
    public void CorrelationId_ShouldReturnSetValue()
    {
        _service.CorrelationId = "req-123";

        _service.CorrelationId.Should().Be("req-123");
    }

    /// <summary>
    /// Verifies that the UserId property returns the set value.
    /// </summary>
    [Fact]
    public void UserId_ShouldReturnSetValue()
    {
        _service.UserId = "user-42";

        _service.UserId.Should().Be("user-42");
    }

    /// <summary>
    /// Verifies that the AddProperty method stores a custom property.
    /// </summary>
    /// <param name="key">The key of the property to add.</param>
    /// <param name="value">The value of the property to add.</param>
    [Fact]
    public void AddProperty_ShouldStoreCustomProperty()
    {
        _service.AddProperty("TenantId", "tenant-A");

        _service.GetProperties()["TenantId"].Should().Be("tenant-A");
    }

    /// <summary>
    /// Verifies that the GetProperties method reflects all set values.
    /// </summary>
    [Fact]
    public void GetProperties_ShouldReflectAllSetValues()
    {
        _service.CorrelationId = "cid-1";
        _service.UserId = "uid-2";
        _service.AddProperty("Extra", "val");

        var props = _service.GetProperties();

        props.Should().ContainKey("CorrelationId").WhoseValue.Should().Be("cid-1");
        props.Should().ContainKey("UserId").WhoseValue.Should().Be("uid-2");
        props.Should().ContainKey("Extra").WhoseValue.Should().Be("val");
    }

    /// <summary>
    /// Verifies that the PushProperties method returns a disposable context.
    /// </summary>
    [Fact]
    public void PushProperties_ShouldReturnDisposable()
    {
        _service.CorrelationId = "test-id";

        using var context = _service.PushProperties();

        context.Should().NotBeNull();
    }

    /// <summary>
    /// Verifies that the AddProperty method throws an exception when the key is null.
    /// </summary>
    /// <param name="key">The key to add (set to null).</param>
    /// <param name="value">The value to add.</param>
    [Fact]
    public void AddProperty_ShouldThrow_WhenKeyIsNull()
    {
        var act = () => _service.AddProperty(null!, "value");

        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifies that the AddProperty method overwrites a property when the key exists.
    /// </summary>
    [Fact]
    public void AddProperty_ShouldOverwrite_WhenKeyExists()
    {
        _service.AddProperty("Key", "first");
        _service.AddProperty("Key", "second");

        _service.GetProperties()["Key"].Should().Be("second");
    }
}
