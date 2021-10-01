# StringUtilityTests

Unit test class for `StringUtility` helper methods, validating behavior under normal, edge, and invalid inputs. Each test asserts expected outcomes against known-good inputs and outputs.

## API

### `void Truncate_StringLongerThanMaxLength_TruncatesAndAppendsSuffix()`
Ensures that strings exceeding the configured maximum length are truncated and a suffix is appended. The test verifies that the original string is truncated to the specified length minus the suffix length, and that the suffix is appended without exceeding the total length.

### `void Truncate_NullOrEmptyInput_ReturnsEmptyString()`
Validates that null or empty input strings are handled gracefully and return an empty string. No exceptions are expected; the method must return `string.Empty`.

### `void ToSnakeCase_CamelCaseOrPascalInput_InsertsUnderscoresBeforeUpperCaseLetters()`
Checks that camelCase or PascalCase input strings are correctly converted to snake_case by inserting underscores before each uppercase letter and converting the entire string to lowercase. The test confirms that consecutive uppercase letters are treated as a single word boundary.

### `void MaskSensitive_LongApiKey_KeepsEdgeCharactersAndMasksMiddle()`
Confirms that long API keys are masked such that the first and last few characters remain visible while the middle portion is replaced with asterisks. The test ensures that the output length matches the input length and that only the middle portion is obscured.

### `void IsValidEmail_VariousInputs_ReturnsExpectedValidationOutcome()`
Validates email address validation logic by testing a range of inputs, including valid, invalid, and edge-case email formats. The test asserts that the method returns the expected boolean outcome for each input scenario.

## Usage
