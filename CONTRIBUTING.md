# Contributing to dotnet-service-scaffold

We welcome contributions! Please follow these guidelines:

1. **Fork & Branch**: Fork the repo and create a feature branch (`git checkout -b feature/name`).
2. **Coding Standards**:
   - Follow `.editorconfig` settings.
   - Use PascalCase for public members, camelCase for local vars, _camelCase for private fields.
   - Add XML documentation to all public APIs.
   - Keep methods small (< 20 lines) and focused.
3. **Architecture**: Use Clean Architecture, dependency injection, and the repository pattern.
4. **Testing**: Add unit/integration tests for all changes. Run `dotnet test` before submitting.
5. **Commits**: Use descriptive messages (`type(scope): subject`).
6. **Pull Requests**:
   - Update `README.md` and documentation if needed.
   - Submit a PR to `main`.
   - Ensure CI/CD passes.

By contributing, you agree that your work is licensed under the MIT License.

Questions? Contact: rutova2@gmail.com
