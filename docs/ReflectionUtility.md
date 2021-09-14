# ReflectionUtility
Utility class providing static helper methods for common reflection tasks such as inspecting types, reading and writing property values, invoking methods, creating instances, and working with attributes and collections.

## API
### GetPublicProperties
```csharp
public static PropertyInfo[] GetPublicProperties(Type type)
```
**Purpose:** Returns an array of all public instance properties declared on the specified type.  
**Parameters:**  
- `type`: The type to inspect. Must not be `null`.  
**Return Value:** An array of `PropertyInfo` objects; empty array if the type has no public properties.  
**Exceptions:**  
- `ArgumentNullException` if `type` is `null`.

### GetPropertyValue
```csharp
public static object? GetPropertyValue(object obj, string propertyName)
```
**Purpose:** Retrieves the value of a public instance property on an object.  
**Parameters:**  
- `obj`: The object whose property is read. Must not be `null`.  
- `propertyName`: The name of the property. Must not be `null` or empty.  
**Return Value:** The property value, or `null` if the property does not exist, is not public, or the getter is inaccessible.  
**Exceptions:**  
- `ArgumentNullException` if `obj` or `propertyName` is `null`.  
- `ArgumentException` if `propertyName` is empty.  
- `TargetException` if `obj` does not declare the property (unlikely given the implementation).

### SetPropertyValue
```csharp
public static bool SetPropertyValue(object obj, string propertyName, object? value)
```
**Purpose:** Attempts to set the value of a public instance property on an object.  
**Parameters:**  
- `obj`: The target object. Must not be `null`.  
- `propertyName`: The name of the property. Must not be `null` or empty.  
- `value`: The value to assign; may be `null` if the property type permits it.  
**Return Value:** `true` if the property was found and set successfully; otherwise `false`.  
**Exceptions:**  
- `ArgumentNullException` if `obj` or `propertyName` is `null`.  
- `ArgumentException` if `propertyName` is empty.  

### GetAttribute<T>
```csharp
public static T? GetAttribute<T>(MemberInfo member) where T : Attribute
```
**Purpose:** Returns the first attribute of type `T` applied to the supplied member, or `null` if none is present.  
**Parameters:**  
- `member`: The member to inspect. Must not be `null`.  
**Return Value:** An instance of `T` if found; otherwise `null`.  
**Exceptions:**  
- `ArgumentNullException` if `member` is `null`.  

### GetAttributes<T>
```csharp
public static IEnumerable<T> GetAttributes<T>(MemberInfo member) where T : Attribute
```
**Purpose:** Returns all attributes of type `T` applied to the supplied member.  
**Parameters:**  
- `member`: The member to inspect. Must not be `null`.  
**Return Value:** An enumerable of `T` instances; empty if none are present.  
**Exceptions:**  
- `ArgumentNullException` if `member` is `null`.  

### HasAttribute<T>
```csharp
public static bool HasAttribute<T>(MemberInfo member) where T : Attribute
```
**Purpose:** Determines whether the supplied member has at least one attribute of type `T`.  
**Parameters:**  
- `member`: The member to inspect. Must not be `null`.  
**Return Value:** `true` if an attribute of type `T` is present; otherwise `false`.  
**Exceptions:**  
- `ArgumentNullException` if `member` is `null`.  

### GetTypesByBaseClass
```csharp
public static IEnumerable<Type> GetTypesByBaseClass(Type baseClass)
```
**Purpose:** Returns all loaded types that derive from the specified base class (excluding the base class itself).  
**Parameters:**  
- `baseClass`: The base type to match. Must not be `null`.  
**Return Value:** An enumerable of matching types; empty if none are found.  
**Exceptions:**  
- `ArgumentNullException` if `baseClass` is `null`.  

### GetTypesByInterface
```csharp
public static IEnumerable<Type> GetTypesByInterface(Type interfaceType)
```
**Purpose:** Returns all loaded types that implement the specified interface.  
**Parameters:**  
- `interfaceType`: The interface type to match. Must not be `null` and must be an interface.  
**Return Value:** An enumerable of matching types; empty if none are found.  
**Exceptions:**  
- `ArgumentNullException` if `interfaceType` is `null`.  
- `ArgumentException` if `interfaceType` is not an interface type.  

### GetPublicMethods
```csharp
public static MethodInfo[] GetPublicMethods(Type type)
```
**Purpose:** Returns an array of all public instance methods declared on the specified type.  
**Parameters:**  
- `type`: The type to inspect. Must not be `null`.  
**Return Value:** An array of `MethodInfo` objects; empty array if the type has no public methods.  
**Exceptions:**  
- `ArgumentNullException` if `type` is `null`.  

### GetMethod
```csharp
public static MethodInfo? GetMethod(Type type, string name, Type[]? parameterTypes = null)
```
**Purpose:** Retrieves a public instance method matching the given name and optional parameter types.  
**Parameters:**  
- `type`: The type containing the method. Must not be `null`.  
- `name`: The method name. Must not be `null` or empty.  
- `parameterTypes`: Optional array of parameter types to match; if `null`, any matching name is returned (the first overload found).  
**Return Value:** A `MethodInfo` if a match is found; otherwise `null`.  
**Exceptions:**  
- `ArgumentNullException` if `type` or `name` is `null`.  
- `ArgumentException` if `name` is empty.  

### InvokeMethod
```csharp
public static object? InvokeMethod(object obj, string methodName, params object[] args)
```
**Purpose:** Invokes a public instance method on an object with the supplied arguments.  
**Parameters:**  
- `obj`: The target object. Must not be `null`.  
- `methodName`: The name of the method to invoke. Must not be `null` or empty.  
- `args`: Arguments to pass to the method; may be empty.  
**Return Value:** The method's return value, or `null` if the method returns `void` or is not found.  
**Exceptions:**  
- `ArgumentNullException` if `obj` or `methodName` is `null`.  
- `ArgumentException` if `methodName` is empty.  
- `TargetException` if the method cannot be found or is not accessible.  
- `TargetInvocationException` if the invoked method throws; the original exception is accessible via `InnerException`.  

### CreateInstance (parameterless overload)
```csharp
public static object? CreateInstance(Type type)
```
**Purpose:** Creates an instance of the specified type using its parameterless constructor.  
**Parameters:**  
- `type`: The type to instantiate. Must not be `null` and must have a public parameterless constructor.  
**Return Value:** A new instance of `type`, or `null` if instantiation fails.  
**Exceptions:**  
- `ArgumentNullException` if `type` is `null`.  
- `MissingMethodException` if no suitable constructor exists.  

### CreateInstance (overload with arguments)
```csharp
public static object? CreateInstance(Type type, params object[] args)
```
**Purpose:** Creates an instance of the specified type using a constructor that matches the supplied argument types.  
**Parameters:**  
- `type`: The type to instantiate. Must not be `null`.  
- `args`: Constructor arguments; must match a public constructor signature.  
**Return Value:** A new instance of `type`, or `null` if instantiation fails.  
**Exceptions:**  
- `ArgumentNullException` if `type` is `null`.  
- `TargetInvocationException` if the constructor throws; the original exception is accessible via `InnerException`.  
- `MissingMethodException` if no matching constructor exists.  

### IsNullableType
```csharp
public static bool IsNullableType(Type type)
```
**Purpose:** Determines whether the supplied type is a nullable value type (e.g., `int?`).  
**Parameters:**  
- `type`: The type to examine. Must not be `null`.  
**Return Value:** `true` if `type` is a nullable struct; otherwise `false`.  
**Exceptions:**  
- `ArgumentNullException` if `type` is `null`.  

### GetUnderlyingType
```csharp
public static Type? GetUnderlyingType(Type type)
```
**Purpose:** Returns the underlying type of a nullable type, or `null` if the type is not nullable.  
**Parameters:**  
- `type`: The type to examine. Must not be `null`.  
**Return Value:** The underlying non‑nullable type if `type` is nullable; otherwise `null`.  
**Exceptions:**  
- `ArgumentNullException` if `type` is `null`.  

### IsCollectionType
```csharp
public static bool IsCollectionType(Type type)
```
**Purpose:** Determines whether the supplied type represents a collection (implements `IEnumerable` and is not a string or array).  
**Parameters:**  
- `type`: The type to examine. Must not be `null`.  
**Return Value:** `true` if the type is a collection; otherwise `false`.  
**Exceptions:**  
- `ArgumentNullException` if `type` is `null`.  

### GetCollectionElementType
```csharp
public static Type? GetCollectionElementType(Type collectionType)
```
**Purpose:** Returns the element type of a collection type (e.g., returns `int` for `List<int>`).  
**Parameters:**  
- `collectionType`: The collection type to examine. Must not be `null` and must implement `IEnumerable`.  
**Return Value:** The element type, or `null` if the type is not a recognized collection.  
**Exceptions:**  
- `ArgumentNullException` if `collectionType` is `null`.  
- `ArgumentException` if `collectionType` does not implement `IEnumerable`.  

### ConvertValue
```csharp
public static object? ConvertValue(object value, Type targetType)
```
**Purpose:** Attempts to convert `value` to `targetType` using `Convert.ChangeType` and nullable‑aware logic.  
**Parameters:**  
- `value`: The value to convert; may be `null`.  
- `targetType`: The desired type. Must not be `null`.  
**Return Value:** The converted value, or `null` if conversion is not possible or `value` is `null` and `targetType` is non‑nullable.  
**Exceptions:**  
- `ArgumentNullException` if `targetType` is `null`.  
- `InvalidCastException` if the conversion is not supported.  
- `FormatException` or `OverflowException` as thrown by `Convert.ChangeType`.  

### GetPropertiesWithAttribute<T>
```csharp
public static IEnumerable<PropertyInfo> GetPropertiesWithAttribute<T>(Type type) where T : Attribute
```
**Purpose:** Returns all public instance properties on `type` that are decorated with attribute `T`.  
**Parameters:**  
- `type`: The type to inspect. Must not be `null`.  
**Return Value:** An enumerable of `PropertyInfo` objects; empty if none match.  
**Exceptions:**  
- `ArgumentNullException` if `type` is `null`.  

## Usage
### Example 1: Reading and updating a property via reflection
```csharp
public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
}

var person = new Person { Name = "Ada", Age = 30 };

// Get the value of the Age property
object? ageValue = ReflectionUtility.GetPropertyValue(person, nameof(Person.Age));
Console.WriteLine(ageValue); // 30

// Increment the age using SetPropertyValue
if (ageValue is int currentAge)
{
    bool success = ReflectionUtility.SetPropertyValue(person, nameof(Person.Age), currentAge + 1);
    Console.WriteLine(success); // True
}

// Verify the update
Console.WriteLine(ReflectionUtility.GetPropertyValue(person, nameof(Person.Age))); // 31
```

### Example 2: Invoking a method and creating an instance with attributes
```csharp
[AttributeUsage(AttributeTargets.Class)]
public class ServiceAttribute : Attribute { }

[Service]
public class Calculator
{
    public int Add(int a, int b) => a + b;
}

// Retrieve the Service attribute
var attr = ReflectionUtility.GetAttribute<ServiceAttribute>(typeof(Calculator));
Console.WriteLine(attr is not null); // True

// Create an instance of Calculator
object? calcInstance = ReflectionUtility.CreateInstance(typeof(Calculator));
if (calcInstance is Calculator calc)
{
    // Invoke the Add method
    int result = (int)ReflectionUtility.InvokeMethod(calc, nameof(Calculator.Add), 5, 7)!;
    Console.WriteLine(result); // 12
}
```

## Notes
- All methods are static and stateless; they are safe to call concurrently from multiple threads as long as the caller does not pass mutable objects that are modified elsewhere during the call.  
- Passing `null` for any required reference parameter will result in an `ArgumentNullException`.  
- Methods that search for members (`GetMethod`, `GetPropertiesWithAttribute<T>`, etc.) return the first match or an empty enumeration when no match is found; they do not throw for missing members.  
- `CreateInstance` overloads rely on public constructors; types with only internal or private constructors will return `null`.  
- `IsCollectionType` excludes `string` and array types intentionally; arrays are considered collections by the underlying `IEnumerable` check, but the implementation treats them as non‑collection for this utility. Adjust the check if array handling is required.  
- `ConvertValue` uses `Convert.ChangeType` under the hood; therefore, it supports the same set of built‑in conversions and throws the same exceptions (`InvalidCastException`, `FormatException`, `OverflowException`). For custom conversion logic, callers should handle those exceptions or pre‑check compatibility.  
- Reflection operations are relatively expensive compared to direct code; consider caching results (e.g., `PropertyInfo` or `MethodInfo`) if the same metadata is needed repeatedly.  
- The utility does not perform security elevation; callers must have the necessary reflection permissions for the target types (e.g., in partially trusted environments).  
- Generic type constraints (`where T : Attribute`) are enforced at compile time; supplying a non‑attribute type will result in a compile‑time error.  
- When using `GetTypesByBaseClass` or `GetTypesByInterface`, the returned set reflects the types currently loaded in the application domain; types loaded after the call will not be included unless the method is invoked again.
