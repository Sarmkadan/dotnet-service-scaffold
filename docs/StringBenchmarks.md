# StringBenchmarks

Provides a set of pure string transformation utilities aimed at common formatting, masking, and generation tasks. The methods are designed to be stateless and safe for concurrent use.

## API

### ToSnakeCase
**Purpose:** Converts a string to snake_case format.  
**Parameters:** `input` (string) – The text to convert.  
**Return value:** A new string where words are separated by underscores and all characters are lowercased.  
**Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### ToSnakeCasePascal
**Purpose:** Converts a string to snake_case while preserving the original Pascal casing of each word.  
**Parameters:** `input` (string) – The text to convert.  
**Return value:** A new string where words are separated by underscores; each word retains its original capitalization (e.g., `HelloWorld` → `Hello_World`).  
**Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### ToCamelCase
**Purpose:** Converts a string to camelCase format.  
**Parameters:** `input` (string) – The text to convert.  
**Return value:** A new string where the first word is lowercased and subsequent words are capitalized without separators (e.g., `hello world` → `helloWorld`).  
**Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### MaskSensitive
**Purpose:** Masks a string to hide sensitive information, showing only the first and last two characters.  
**Parameters:** `input` (string) – The text to mask.  
**Return value:** A new string of the same length where all characters except the first two and last two are replaced with `*`. If the string length is less than or equal to 4, the original string is returned unchanged.  
**Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### GenerateRandomString32
**Purpose:** Generates a cryptographically strong random string of length 32.  
**Parameters:** None.  
**Return value:** A new string containing 32 random alphanumeric characters.  
**Exceptions:** None.

### GenerateRandomString64
**Purpose:** Generates a cryptographically strong random string of length 64.  
**Parameters:** None.  
**Return value:** A new string containing 64 random alphanumeric characters.  
**Exceptions:** None.

### ToSlug
**Purpose:** Converts a string to a URL‑friendly slug.  
**Parameters:** `input` (string) – The text to slugify.  
**Return value:** A new string that is lowercased, with spaces and underscores replaced by hyphens, and all non‑alphanumeric characters removed. Consecutive hyphens are collapsed into a single hyphen, and leading/trailing hyphens are trimmed.  
**Exceptions:** Throws `ArgumentNullException` if `input` is `null`.

### Truncate
**Purpose:** Truncates a string to a configurable maximum length, appending an ellipsis when needed.  
**Parameters:** `input` (string) – The text to truncate; `maxLength` (int, optional, default = 100) – The maximum allowed length of the returned string, not counting the ellipsis.  
**Return value:** If `input` length ≤ `maxLength`, returns `input` unchanged; otherwise returns the first `maxLength` characters followed by `…`.  
**Exceptions:** Throws `ArgumentNullException` if `input` is `null`; throws `ArgumentOutOfRangeException` if `maxLength` is less than 0.

## Usage

```csharp
using DotnetServiceScaffold.Benchmarks; // namespace containing StringBenchmarks

var original = "  Hello   World!  ";
var snake    = original.ToSnakeCase();          // "hello   world!"
var camel    = original.ToCamelCase();          // "hello   World!"
var slug     = original.ToSlug();               // "hello-world"
var masked   = original.MaskSensitive();        // "*****orld!  "
var truncated = original.Truncate(8);           // "  Hello…"
```

```csharp
var rand32 = StringBenchmarks.GenerateRandomString32(); // e.g., "G7fT2qZ9…"
var rand64 = StringBenchmarks.GenerateRandomString64(); // e.g., "aB3dEfGhIjKlMnOpQrStUvWxYz12…"
var password = "SuperSecretPassword123";
var maskedPwd = password.MaskSensitive();               // "Su………………23"
```

## Notes

- All methods are pure; they do not modify the input string and have no side effects, making them thread‑safe for concurrent invocation.
- Null inputs consistently result in an `ArgumentNullException` to fail fast.
- Empty strings are handled gracefully: transformation methods return an empty string, while `MaskSensitive` returns an empty string, and `GenerateRandomString*` methods are unaffected.
- `ToSnakeCasePascal` assumes that word boundaries are indicated by changes in casing; acronyms such as `XML` are treated as a single word (`XML` → `XML`).
- The `Truncate` method’s default length of 100 characters can be overridden via the optional `maxLength` parameter; specifying a value greater than the actual string length returns the original string unchanged.
- The random string generators use a cryptographically secure random number generator; they are suitable for generating tokens, nonces, or other security‑sensitive values.
