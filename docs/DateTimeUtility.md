# DateTimeUtility
Utility class providing common date and time calculations and formatting helpers.

## API
### CalculateAge
- **Purpose**: Returns the number of full years elapsed between a birth date and a reference date.
- **Parameters**: 
  - `birthDate` (`DateTime`) – The date of birth.
  - `referenceDate` (`DateTime?`, optional) – The date to calculate the age against; if omitted, `DateTime.Today` is used.
- **Return**: `int` – The age in years.
- **Throws**: 
  - `ArgumentOutOfRangeException` if `birthDate` is later than `referenceDate`.

### GetRelativeTime
- **Purpose**: Produces a human‑readable string describing how far a date‑time is from the current moment (e.g., “2 hours ago”, “in 5 days”).
- **Parameters**: 
  - `dateTime` (`DateTime`) – The timestamp to format.
- **Return**: `string?` – A relative time description, or `null` if the input is `DateTime.MinValue`.
- **Throws**: None.

### IsBusinessHours
- **Purpose**: Determines whether a given moment falls within typical business hours (Monday‑Friday, 09:00‑17:00) in the local time zone.
- **Parameters**: 
  - `dateTime` (`DateTime`) – The moment to evaluate.
- **Return**: `bool` – `true` if the moment is within business hours; otherwise `false`.
- **Throws**: 
  - `ArgumentException` if `dateTime.Kind` is `DateTimeKind.Unspecified`.

### GetStartOfDay
- **Purpose**: Returns a `DateTime` representing the start of the day (00:00:00) for the supplied date.
- **Parameters**: 
  - `dateTime` (`DateTime`) – The input date.
- **Return**: `DateTime` – Same date with the time component set to midnight.
- **Throws**: 
  - `ArgumentException` if `dateTime.Kind` is `DateTimeKind.Unspecified`.

### GetEndOfDay
- **Purpose**: Returns a `DateTime` representing the end of the day (23:59:59.999) for the supplied date.
- **Parameters**: 
  - `dateTime` (`DateTime`) – The input date.
- **Return**: `DateTime` – Same date with the time component set to the last tick of the day.
- **Throws**: 
  - `ArgumentException` if `dateTime.Kind` is `DateTimeKind.Unspecified`.

### GetStartOfWeek
- **Purpose**: Returns a `DateTime` representing the start of the week (Monday) for the supplied date.
- **Parameters**: 
  - `dateTime` (`DateTime`) – The input date.
- **Return**: `DateTime` – The Monday of the week containing `dateTime`, with time set to 00:00:00.
- **Throws**: 
  - `ArgumentException` if `dateTime.Kind` is `DateTimeKind.Unspecified`.

### GetStartOfMonth
- **Purpose**: Returns a `DateTime` representing the first day of the month (00:00:00) for the supplied date.
- **Parameters**: 
  - `dateTime` (`DateTime`) – The input date.
- **Return**: `DateTime` – The first day of the month of `dateTime`, with time set to 00:00:00.
- **Throws**: 
  - `ArgumentException` if `dateTime.Kind` is `DateTimeKind.Unspecified`.

### IsPast
- **Purpose**: Checks whether a date‑time is earlier than the current moment.
- **Parameters**: 
  - `dateTime` (`DateTime`) – The moment to test.
- **Return**: `bool` – `true` if `dateTime` is in the past; otherwise `false`.
- **Throws**: 
  - `ArgumentException` if `dateTime.Kind` is `DateTimeKind.Unspecified`.

### IsFuture
- **Purpose**: Checks whether a date‑time is later than the current moment.
- **Parameters**: 
  - `dateTime` (`DateTime`) – The moment to test.
- **Return**: `bool` – `true` if `dateTime` is in the future; otherwise `false`.
- **Throws**: 
  - `ArgumentException` if `dateTime.Kind` is `DateTimeKind.Unspecified`.

### IsToday
- **Purpose**: Determines whether a date‑time falls on the current day.
- **Parameters**: 
  - `dateTime` (`DateTime`) – The moment to test.
- **Return**: `bool` – `true` if the date component of `dateTime` equals `DateTime.Today`; otherwise `false`.
- **Throws**: 
  - `ArgumentException` if `dateTime.Kind` is `DateTimeKind.Unspecified`.

### ParseIsoDuration
- **Purpose**: Parses an ISO 8601 duration string (e.g., “P1Y2M3DT4H5M6S”) into a `TimeSpan`.
- **Parameters**: 
  - `isoDuration` (`string`) – The duration string to parse.
- **Return**: `TimeSpan` – The parsed duration.
- **Throws**: 
  - `ArgumentNullException` if `isoDuration` is `null`.
  - `FormatException` if the string does not conform to the ISO 8601 duration format.
  - `OverflowException` if the resulting duration exceeds the range of `TimeSpan`.

## Usage
```csharp
// Example 1: Determine if a user is at least 18 years old.
DateTime birth = new DateTime(2000, 4, 15);
int age = DateTimeUtility.CalculateAge(birth);
bool isAdult = age >= 18; // true

// Example 2: Show a friendly timestamp and check if a meeting is during business hours.
DateTime meetingTime = DateTime.UtcNow.AddHours(2);
string friendly = DateTimeUtility.GetRelativeTime(meetingTime);
// friendly might be "in 2 hours"
bool duringWork = DateTimeUtility.IsBusinessHours(meetingTime);
```
```csharp
// Example 3: Get the start of the current week for reporting.
DateTime today = DateTime.Today;
DateTime weekStart = DateTimeUtility.GetStartOfWeek(today);
// weekStart is the Monday of the current week at 00:00:00

// Example 4: Convert an ISO duration to a TimeSpan for scheduling.
string iso = "P0Y0M0DT2H30M"; // 2 hours 30 minutes
TimeSpan span = DateTimeUtility.ParseIsoDuration(iso);
// span equals 02:30:00
```

## Notes
- All methods that accept a `DateTime` parameter validate that the `Kind` property is not `Unspecified`; passing an Unspecified value will throw an `ArgumentException`.
- The week‑start calculation assumes Monday as the first day of the week, independent of the current culture.
- `GetRelativeTime` returns `null` only for the sentinel value `DateTime.MinValue`; for all other inputs a non‑null string is produced.
- The static methods contain no mutable state, making them thread‑safe for concurrent calls.
- `ParseIsoDuration` follows the ISO 8601 duration grammar; fractional seconds are not supported and will cause a `FormatException`.
- When calculating age, leap years are handled correctly by counting full years only; the result does not increment until the anniversary of the birth date has passed in the reference year.
