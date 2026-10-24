# Contributing to dotnet-service-scaffold

Thanks for helping out. This guide covers the basics; see `CLAUDE.md` for architecture details.

## Prerequisites

- **.NET SDK 10.0.x.** `global.json` pins `10.0.100` with `rollForward: latestMinor`, so any 10.0 patch SDK works. Target framework is `net10.0`.
- `git`, and optionally `make` (see `make help`).

## Build

```bash
dotnet build                                  # main project, Debug
dotnet build -c Release dotnet-service-scaffold.sln   # main + tests + benchmarks
```

## Run tests

```bash
dotnet test                                                       # whole solution
dotnet test tests/dotnet-service-scaffold.Tests                   # test project only
dotnet test --filter "FullyQualifiedName~HealthCheckServiceTests" # single class
```

Some test folders are excluded in the test `.csproj` (`<Compile Remove>`), so check it before assuming a test runs. Tests use xUnit, FluentAssertions, and NSubstitute.

## Coding conventions

- Formatting and style come from `.editorconfig` (4 spaces, LF, Allman braces). Run `dotnet format` before committing.
- `Nullable` is enabled. Keep the build free of new nullable warnings.
- Warnings are not errors yet (`TreatWarningsAsErrors` is `false`), so read the build output and fix any warnings your change introduces. `make analyze` runs code-style analysis.
- Public members are PascalCase. Private fields are `_camelCase`. Locals and parameters are camelCase.
- Use file-scoped namespaces (`namespace DotnetServiceScaffold.<Layer>.<Folder>;`).
- Async methods end with the `Async` suffix.
- Public APIs carry XML documentation (`GenerateDocumentationFile` is on).
- Put magic strings and numbers in the matching `*Constants` class, and keep one type per file.

## Branching and PR process

1. Branch from `main`: `git checkout -b feature/<short-name>` (or `fix/`, `docs/`).
2. Keep PRs small and focused on one change. Split unrelated work into separate PRs.
3. Run `dotnet build` and `dotnet test` locally before pushing.
4. Open a PR against `main`. The PR description should say **what** changed and **why**. Link the issue it addresses (for example, `Closes #123`).
5. CI (`.github/workflows/ci.yml`: restore, build, test in Release) must be green before review. Fix failures rather than merging around them.
6. Use descriptive commit messages, for example `fix(health): handle timeout in DNS probe`.
