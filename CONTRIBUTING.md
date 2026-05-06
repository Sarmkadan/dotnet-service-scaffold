# Contributing to dotnet-service-scaffold

Thank you for your interest in contributing! We welcome contributions from everyone. This document provides guidelines and instructions for contributing.

## Code of Conduct

Please be respectful and inclusive in all interactions with other contributors and maintainers.

## Getting Started

### Prerequisites

- .NET 10.0 SDK or later
- Git
- Text editor or IDE (Visual Studio, VS Code, Rider)
- Basic understanding of ASP.NET Core and C#

### Setup Development Environment

1. **Fork the repository**
   ```bash
   # Visit https://github.com/sarmkadan/dotnet-service-scaffold
   # Click "Fork" button
   ```

2. **Clone your fork**
   ```bash
   git clone https://github.com/YOUR_USERNAME/dotnet-service-scaffold.git
   cd dotnet-service-scaffold
   ```

3. **Add upstream remote**
   ```bash
   git remote add upstream https://github.com/sarmkadan/dotnet-service-scaffold.git
   ```

4. **Create feature branch**
   ```bash
   git checkout -b feature/my-feature
   ```

5. **Install dependencies**
   ```bash
   dotnet restore
   ```

6. **Build project**
   ```bash
   dotnet build
   ```

7. **Run tests**
   ```bash
   dotnet test
   ```

## Development Guidelines

### Code Style

Follow C# naming and formatting conventions as defined in `.editorconfig`:

- **PascalCase** for public types and members
- **camelCase** for local variables and parameters
- **_camelCase** for private fields
- **ALL_UPPER** for constants

Example:
```csharp
public class UserService
{
    private readonly IUserRepository _userRepository;
    
    public async Task<User> GetUserAsync(string userId)
    {
        const string cacheKey = "users";
        var user = await _userRepository.GetByIdAsync(userId);
        return user;
    }
}
```

### Code Quality

1. **Keep methods small** (< 20 lines preferred)
2. **Single responsibility principle** - one job per class/method
3. **DRY** - Don't Repeat Yourself
4. **SOLID principles** - Apply design principles
5. **Comments** - Only explain WHY, not WHAT
6. **No magic numbers** - Use named constants
7. **Preserve author headers** - Keep copyright and author information in code files

Example of good comments:
```csharp
// Retry 3 times due to transient network failures in health checks
for (int i = 0; i < 3; i++)
{
    try
    {
        return await _httpClient.GetAsync(url);
    }
    catch (HttpRequestException) when (i < 2)
    {
        await Task.Delay(1000 * (i + 1));
    }
}
```

### Architecture

- Use **dependency injection** - no `new` operator for services
- Follow **repository pattern** - abstractions for data access
- Apply **clean architecture** - layered design
- Use **async/await** - all I/O operations should be async
- Implement **proper error handling** - use custom exceptions

### Adding New Features

1. **Define domain model** in `src/Domain/Models/`
2. **Create repository interface** in `src/Infrastructure/Data/Repository/`
3. **Implement repository** with DbContext integration
4. **Create service interface** in `src/Application/Services/`
5. **Implement service** with business logic
6. **Create controller** in `src/Presentation/Controllers/`
7. **Register in Program.cs** dependency injection
8. **Write tests** for new functionality
9. **Update documentation** in `docs/`

Example:
```csharp
// 1. Domain Model
public class Notification
{
    public string Id { get; set; }
    public string Message { get; set; }
}

// 2. Repository Interface
public interface INotificationRepository : IRepository<Notification>
{
    Task<List<Notification>> GetByUserAsync(string userId);
}

// 3. Service Interface
public interface INotificationService
{
    Task SendAsync(string userId, string message);
}

// 4. Controller
[ApiController]
[Route("api/notifications")]
public class NotificationController : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> Send(SendNotificationRequest request)
    {
        await _notificationService.SendAsync(request.UserId, request.Message);
        return Ok();
    }
}

// 5. Register in Program.cs
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService>();
```

## Testing

### Unit Tests

Write unit tests for business logic:

```csharp
[Fact]
public async Task CheckService_WithValidUrl_ReturnsHealthy()
{
    // Arrange
    var mockRepository = new Mock<IServiceRepository>();
    var mockHttpClient = new Mock<HttpClient>();
    var service = new HealthCheckService(mockRepository.Object, mockHttpClient.Object);

    // Act
    var result = await service.CheckServiceHealthAsync("service-123");

    // Assert
    Assert.NotNull(result);
    Assert.Equal("Healthy", result.Status);
}
```

### Integration Tests

Write integration tests for full workflows:

```csharp
public class HealthCheckIntegrationTests : IAsyncLifetime
{
    private readonly TestFixture _fixture = new();

    public async Task InitializeAsync()
    {
        await _fixture.InitializeDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await _fixture.CleanupAsync();
    }

    [Fact]
    public async Task RegisterAndCheck_FullWorkflow()
    {
        // Register service
        var serviceId = await _fixture.RegisterServiceAsync("TestService", "http://localhost:9999/health");

        // Run health check
        var result = await _fixture.HealthCheckAsync(serviceId);

        // Verify
        Assert.NotNull(result);
    }
}
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test file
dotnet test tests/ServiceTests.cs

# Run with verbosity
dotnet test --verbosity detailed

# Run with coverage
dotnet test /p:CollectCoverage=true
```

## Commit Guidelines

### Commit Messages

Use clear, descriptive commit messages:

```
type(scope): subject

body

footer
```

**Type** can be:
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation
- `style:` Code style changes (formatting, missing semicolons, etc.)
- `refactor:` Code refactoring
- `perf:` Performance improvements
- `test:` Adding or updating tests
- `chore:` Maintenance tasks

**Examples:**
```
feat(healthcheck): add response time trending

Add capability to track health check response times over time
and identify performance degradation patterns.

Closes #123

fix(auth): prevent timing attacks in password comparison

Use secure comparison method to prevent timing-based attacks
on password validation.

docs(api): update health check endpoint documentation

Add examples and clarify response codes.
```

### Commit Size

Keep commits atomic and focused:
- One logical change per commit
- Ideally < 100 lines of code change
- Include all related tests and docs

## Pull Request Process

### Before Submitting

1. **Update your branch**
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Run tests locally**
   ```bash
   dotnet test
   ```

3. **Code formatting**
   ```bash
   dotnet format
   ```

4. **Build in Release mode**
   ```bash
   dotnet build -c Release
   ```

### Submitting PR

1. **Push to your fork**
   ```bash
   git push origin feature/my-feature
   ```

2. **Create Pull Request**
   - Go to GitHub repository
   - Click "New Pull Request"
   - Select your branch
   - Fill in PR template

3. **PR Description should include:**
   - Clear title describing the change
   - Description of what changed and why
   - Link to related issue(s)
   - Testing instructions
   - Screenshots (if UI changes)

4. **Example PR Description:**
   ```markdown
   ## Description
   Adds request tracing with correlation IDs to improve debugging
   in distributed environments.

   ## Type of Change
   - [x] New feature
   - [ ] Bug fix
   - [ ] Breaking change
   - [ ] Documentation update

   ## Testing
   - [x] Unit tests added
   - [x] Integration tests added
   - [x] Manual testing performed

   ## Screenshots
   N/A

   ## Related Issues
   Closes #456
   ```

### Review Process

- At least one review required
- All tests must pass
- No merge conflicts
- CI/CD checks must pass
- Code coverage should not decrease significantly

## Documentation

### Code Documentation

Use XML documentation for public APIs:

```csharp
/// <summary>
/// Performs a health check on the specified service.
/// </summary>
/// <param name="serviceId">The unique identifier of the service</param>
/// <returns>Health check result with status and response time</returns>
/// <exception cref="ServiceNotFoundException">Thrown when service not found</exception>
public async Task<HealthCheckResult> CheckServiceHealthAsync(string serviceId)
{
    // Implementation
}
```

### User Documentation

Update relevant documentation files:
- `README.md` - Overview and quick start
- `docs/getting-started.md` - Installation and setup
- `docs/api-reference.md` - API documentation
- `docs/deployment.md` - Deployment instructions
- `docs/faq.md` - Common questions

## Reporting Issues

### Bug Reports

Include:
- Clear description of the problem
- Steps to reproduce
- Expected behavior
- Actual behavior
- Environment details (.NET version, OS, etc.)
- Error messages or logs

**Template:**
```markdown
### Description
[Clear description of the bug]

### Steps to Reproduce
1. ...
2. ...
3. ...

### Expected Behavior
[What should happen]

### Actual Behavior
[What actually happens]

### Environment
- .NET Version: 10.0.0
- OS: Ubuntu 22.04
- Browser: N/A

### Logs
[Relevant log output]
```

### Feature Requests

Include:
- Clear description of the requested feature
- Use case and motivation
- Proposed implementation (if you have ideas)
- Examples of similar features

## Developer Resources

### Important Files

- `Program.cs` - Application entry point and DI configuration
- `src/Presentation/Controllers/` - API endpoints
- `src/Application/Services/` - Business logic
- `src/Domain/Models/` - Entity definitions
- `src/Infrastructure/Data/` - Data access layer
- `.github/workflows/` - CI/CD configuration

### Useful Commands

```bash
# Build and run
dotnet run

# Build with auto-reload
dotnet watch run

# Run tests
dotnet test

# Run specific test
dotnet test --filter "TestName"

# Code analysis
dotnet build /p:EnforceCodeStyleInBuild=true

# Format code
dotnet format

# Generate NuGet package
dotnet pack -c Release

# Publish for production
dotnet publish -c Release -o ./publish
```

### Database

```bash
# List migrations
dotnet ef migrations list

# Add migration
dotnet ef migrations add MigrationName

# Remove last migration
dotnet ef migrations remove

# Update database
dotnet ef database update

# Drop database
dotnet ef database drop
```

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

## Recognition

Contributors will be recognized in:
- `CHANGELOG.md`
- GitHub contributors page
- Project documentation

## Questions?

- **Documentation**: See `/docs` directory
- **Issues**: Open a GitHub issue
- **Email**: rutova2@gmail.com

---

Thank you for contributing to dotnet-service-scaffold!

**Happy coding!** 🚀
