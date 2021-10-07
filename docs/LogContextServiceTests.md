# LogContextServiceTests

`LogContextServiceTests` is the unit-test suite for the `LogContextService` class, verifying that the service correctly manages ambient log-context properties such as correlation identifiers, user identifiers, and arbitrary custom key-value pairs. The tests ensure that property storage, retrieval, overwriting, and disposal semantics behave as specified, including validation of null-key rejection.

## API

### `public void CorrelationId_ShouldReturnSetValue`
Asserts that after setting a correlation ID on the service, the same value is returned by the corresponding accessor.  
**Parameters:** None (test method).  
**Returns:** void.  
**Throws:** Test-framework assertion failures if the stored and retrieved values differ.

### `public void UserId_ShouldReturnSetValue`
Asserts that after setting a user ID on the service, the same value is returned by the corresponding accessor.  
**Parameters:** None (test method).  
**Returns:** void.  
**Throws:** Test-framework assertion failures if the stored and retrieved values differ.

### `public void AddProperty_ShouldStoreCustomProperty`
Verifies that a custom key-value pair added via `AddProperty` is subsequently retrievable through the properties collection.  
**Parameters:** None (test method).  
**Returns:** void.  
**Throws:** Test-framework assertion failures if the property is absent or its value does not match.

### `public void GetProperties_ShouldReflectAllSetValues`
Confirms that the aggregate returned by `GetProperties` includes every property previously set — both built-in identifiers and custom entries — with correct values.  
**Parameters:** None (test method).  
**Returns:** void.  
**Throws:** Test-framework assertion failures if the returned dictionary is missing entries or contains incorrect values.

### `public void PushProperties_ShouldReturnDisposable`
Ensures that calling `PushProperties` returns a non-null `IDisposable` that can be used to scope temporary property overrides.  
**Parameters:** None (test method).  
**Returns:** void.  
**Throws:** Test-framework assertion failures if the returned object is null or does not implement `IDisposable`.

### `public void AddProperty_ShouldThrow_WhenKeyIsNull`
Validates that passing a null key to `AddProperty` causes an `ArgumentNullException` (or a derived argument-validation exception) to be thrown.  
**Parameters:** None (test method).  
**Returns:** void.  
**Throws:** The test expects the method under test to throw; the test itself fails if no exception is raised.

### `public void AddProperty_ShouldOverwrite_WhenKeyExists`
Demonstrates that adding a property with a key that already exists overwrites the previous value with the new one, rather than duplicating or ignoring the entry.  
**Parameters:** None (test method).  
**Returns:** void.  
**Throws:** Test-framework assertion failures if the final value does not equal the overwritten value.

## Usage

### Example 1: Verifying correlation-ID round-trip
```csharp
[Test]
public void CorrelationId_ShouldReturnSetValue()
{
    var service = new LogContextService();
    string expected = Guid.NewGuid().ToString("D");

    service.SetCorrelationId(expected);
    string actual = service.CorrelationId;

    Assert.That(actual, Is.EqualTo(expected));
}
```

### Example 2: Ensuring property overwrite behaviour
```csharp
[Test]
public void AddProperty_ShouldOverwrite_WhenKeyExists()
{
    var service = new LogContextService();
    const string key = "session";
    
    service.AddProperty(key, "initial");
    service.AddProperty(key, "overwritten");

    var properties = service.GetProperties();
    Assert.That(properties[key], Is.EqualTo("overwritten"));
    Assert.That(properties.Count, Is.EqualTo(1)); // no duplicate keys
}
```

## Notes

- **Null-key enforcement:** `AddProperty_ShouldThrow_WhenKeyIsNull` confirms that the service rejects null keys immediately. Callers must guard against null or empty keys before invoking `AddProperty` in production code.
- **Overwrite semantics:** The overwrite test ensures the property store behaves as a dictionary with unique keys. No exception is thrown on duplicate keys; the last value written silently replaces the previous one.
- **Disposable scope:** `PushProperties_ShouldReturnDisposable` validates that the returned token implements `IDisposable`. Consumers should wrap it in a `using` block to guarantee properties are popped even if an exception occurs.
- **Thread safety:** The test signatures do not expose any synchronization primitives, and the suite contains no concurrency-focused tests. Unless the underlying `LogContextService` documents thread-safety guarantees (e.g., via `AsyncLocal` or locks), assume that concurrent writes from multiple threads may lead to race conditions or lost updates.
- **Test isolation:** Each test method operates independently and does not persist state to other tests. The suite assumes a fresh service instance per test or proper teardown between executions.
