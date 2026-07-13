#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;
using System.Reflection;

namespace DotnetServiceScaffold.Shared.Utilities;

/// <summary>
/// Utility class for reflection operations. Provides helpers for type inspection,
/// property access, and attribute discovery.
/// </summary>
public static class ReflectionUtility
{
    /// <summary>
    /// Gets all public properties of a type.
    /// </summary>
    public static PropertyInfo[] GetPublicProperties(Type type)
    {
        return type.GetProperties(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
    }

    /// <summary>
    /// Gets a property value from an object.
    /// Returns null if property not found.
    /// </summary>
    public static object? GetPropertyValue(object obj, string propertyName)
    {
        var property = obj.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        return property?.GetValue(obj);
    }

    /// <summary>
    /// Sets a property value on an object.
    /// Returns false if property not found or cannot be set.
    /// </summary>
    public static bool SetPropertyValue(object obj, string propertyName, object? value)
    {
        var property = obj.GetType().GetProperty(
            propertyName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        if (property is null || !property.CanWrite)
            return false;

        try
        {
            property.SetValue(obj, value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets an attribute from a type or member.
    /// </summary>
    public static T? GetAttribute<T>(MemberInfo member) where T : Attribute
    {
        return member.GetCustomAttribute<T>();
    }

    /// <summary>
    /// Gets all attributes of a specific type from a member.
    /// </summary>
    public static IEnumerable<T> GetAttributes<T>(MemberInfo member) where T : Attribute
    {
        return member.GetCustomAttributes<T>();
    }

    /// <summary>
    /// Checks if a type has a specific attribute.
    /// </summary>
    public static bool HasAttribute<T>(MemberInfo member) where T : Attribute
    {
        return member.GetCustomAttribute<T>() is not null;
    }

    /// <summary>
    /// Gets all types in an assembly that inherit from a base type.
    /// </summary>
    public static IEnumerable<Type> GetTypesByBaseClass(Assembly assembly, Type baseType)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t));
    }

    /// <summary>
    /// Gets all types in an assembly that implement an interface.
    /// </summary>
    public static IEnumerable<Type> GetTypesByInterface(Assembly assembly, Type interfaceType)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && interfaceType.IsAssignableFrom(t));
    }

    /// <summary>
    /// Gets all public methods of a type.
    /// </summary>
    public static MethodInfo[] GetPublicMethods(Type type)
    {
        return type.GetMethods(
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
    }

    /// <summary>
    /// Gets a method by name from a type.
    /// </summary>
    public static MethodInfo? GetMethod(Type type, string methodName)
    {
        return type.GetMethod(
            methodName,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
    }

    /// <summary>
    /// Invokes a method on an object.
    /// </summary>
    public static object? InvokeMethod(object obj, string methodName, params object?[] parameters)
    {
        var method = GetMethod(obj.GetType(), methodName);
        if (method is null)
            return null;

        return method.Invoke(obj, parameters);
    }

    /// <summary>
    /// Creates an instance of a type using its default constructor.
    /// </summary>
    public static object? CreateInstance(Type type)
    {
        try
        {
            return Activator.CreateInstance(type);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Creates an instance of a type with specific constructor parameters.
    /// </summary>
    public static object? CreateInstance(Type type, params object?[] constructorParams)
    {
        try
        {
            return Activator.CreateInstance(type, constructorParams);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks if a type is nullable (e.g., int?, string, etc.).
    /// </summary>
    public static bool IsNullableType(Type type)
    {
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(Nullable<>);
    }

    /// <summary>
    /// Gets the underlying type of a nullable type.
    /// </summary>
    public static Type? GetUnderlyingType(Type type)
    {
        return IsNullableType(type)
            ? Nullable.GetUnderlyingType(type)
            : type;
    }

    /// <summary>
    /// Checks if a type is a collection (IEnumerable but not string).
    /// </summary>
    public static bool IsCollectionType(Type type)
    {
        return type != typeof(string) &&
               typeof(System.Collections.IEnumerable).IsAssignableFrom(type);
    }

    /// <summary>
    /// Gets the element type of a collection.
    /// </summary>
    public static Type? GetCollectionElementType(Type collectionType)
    {
        if (collectionType.IsArray)
            return collectionType.GetElementType();

        var enumerableType = collectionType.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                                  i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

        return enumerableType?.GetGenericArguments().FirstOrDefault();
    }

    /// <summary>
    /// Converts a value to a specific type.
    /// </summary>
    public static object? ConvertValue(object? value, Type targetType)
    {
        if (value is null)
            return null;

        if (targetType.IsAssignableFrom(value.GetType()))
            return value;

        try
        {
            if (targetType == typeof(string))
                return value.ToString();

            if (targetType.IsEnum)
                return Enum.Parse(targetType, value.ToString() ?? string.Empty, ignoreCase: true);

            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets all properties with a specific attribute.
    /// </summary>
    public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<T>(Type type) where T : Attribute
    {
        return GetPublicProperties(type)
            .Where(p => HasAttribute<T>(p));
    }
}
