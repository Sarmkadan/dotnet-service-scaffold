#nullable enable
using System.Reflection;
using DotnetServiceScaffold.Shared.Utilities;
using FluentAssertions;

namespace DotnetServiceScaffold.Tests;

public class ReflectionUtilityTests
{
    private class TestAttribute : Attribute { }

    private class TestModel
    {
        [TestAttribute]
        public string? Property { get; set; }
        public int IntProperty { get; set; }
        public void TestMethod() { }
    }

    [Fact]
    public void GetPublicProperties_ReturnsExpectedProperties()
    {
        var properties = ReflectionUtility.GetPublicProperties(typeof(TestModel));
        properties.Should().HaveCount(2);
        properties.Select(p => p.Name).Should().Contain(["Property", "IntProperty"]);
    }

    [Fact]
    public void GetPropertyValue_ReturnsExpectedValue()
    {
        var model = new TestModel { Property = "hello" };
        var value = ReflectionUtility.GetPropertyValue(model, "Property");
        value.Should().Be("hello");
    }

    [Fact]
    public void SetPropertyValue_UpdatesValue()
    {
        var model = new TestModel();
        var success = ReflectionUtility.SetPropertyValue(model, "Property", "world");
        success.Should().BeTrue();
        model.Property.Should().Be("world");
    }

    [Fact]
    public void GetAttribute_ReturnsExpectedAttribute()
    {
        var property = typeof(TestModel).GetProperty("Property")!;
        var attribute = ReflectionUtility.GetAttribute<TestAttribute>(property);
        attribute.Should().NotBeNull();
    }

    [Fact]
    public void HasAttribute_ReturnsTrueForExistingAttribute()
    {
        var property = typeof(TestModel).GetProperty("Property")!;
        var hasAttribute = ReflectionUtility.HasAttribute<TestAttribute>(property);
        hasAttribute.Should().BeTrue();
    }

    [Fact]
    public void GetPublicMethods_ReturnsExpectedMethods()
    {
        var methods = ReflectionUtility.GetPublicMethods(typeof(TestModel));
        methods.Should().Contain(m => m.Name == "TestMethod");
    }

    [Fact]
    public void GetMethod_ReturnsExpectedMethod()
    {
        var method = ReflectionUtility.GetMethod(typeof(TestModel), "TestMethod");
        method.Should().NotBeNull();
        method!.Name.Should().Be("TestMethod");
    }

    [Fact]
    public void GetMethod_ReturnsNullForNonExistentMethod()
    {
        var method = ReflectionUtility.GetMethod(typeof(TestModel), "NonExistent");
        method.Should().BeNull();
    }
}
