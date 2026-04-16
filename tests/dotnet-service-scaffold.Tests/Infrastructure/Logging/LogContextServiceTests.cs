#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using DotnetServiceScaffold.Infrastructure.Logging;
using FluentAssertions;
using Xunit;

namespace DotnetServiceScaffold.Tests.Infrastructure.Logging;

public class LogContextServiceTests
{
    private readonly LogContextService _service = new();

    [Fact]
    public void CorrelationId_ShouldReturnSetValue()
    {
        _service.CorrelationId = "req-123";

        _service.CorrelationId.Should().Be("req-123");
    }

    [Fact]
    public void UserId_ShouldReturnSetValue()
    {
        _service.UserId = "user-42";

        _service.UserId.Should().Be("user-42");
    }

    [Fact]
    public void AddProperty_ShouldStoreCustomProperty()
    {
        _service.AddProperty("TenantId", "tenant-A");

        _service.GetProperties()["TenantId"].Should().Be("tenant-A");
    }

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

    [Fact]
    public void PushProperties_ShouldReturnDisposable()
    {
        _service.CorrelationId = "test-id";

        using var context = _service.PushProperties();

        context.Should().NotBeNull();
    }

    [Fact]
    public void AddProperty_ShouldThrow_WhenKeyIsNull()
    {
        var act = () => _service.AddProperty(null!, "value");

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void AddProperty_ShouldOverwrite_WhenKeyExists()
    {
        _service.AddProperty("Key", "first");
        _service.AddProperty("Key", "second");

        _service.GetProperties()["Key"].Should().Be("second");
    }
}
