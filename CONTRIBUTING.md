# Contributing to KafkaGenericProcessor.Core

Thank you for your interest in contributing to KafkaGenericProcessor.Core! This document provides guidelines for contributing to the project and explains the development workflow.

## Code of Conduct

This project adheres to a Code of Conduct that all participants are expected to follow. Please read and respect our commitment to making this community a welcoming environment.

## Development Environment Setup

### Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version 8.0 or later)
- [Docker](https://www.docker.com/) (for running Kafka locally during development)
- [Git](https://git-scm.com/)
- IDE of your choice (we recommend [Visual Studio](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with C# extensions)

### Setting up the Development Environment

1. **Fork the repository**

   Start by forking the repository to your GitHub account.

2. **Clone your fork**

   ```bash
   git clone https://github.com/YOUR-USERNAME/KafkaGenericProcessor.Core.git
   cd KafkaGenericProcessor.Core
   ```

3. **Add upstream remote**

   ```bash
   git remote add upstream https://github.com/ORIGINAL-OWNER/KafkaGenericProcessor.Core.git
   ```

4. **Install dependencies**

   ```bash
   dotnet restore
   ```

5. **Set up local Kafka**

   We provide a Docker Compose file to run Kafka locally:

   ```bash
   docker-compose up -d
   ```

### Building and Testing

- **Building the project**

  ```bash
   dotnet build
   ```

- **Running tests**

  ```bash
  dotnet test
  ```

## Development Workflow

1. **Create a new branch**

   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**

   Write your code, following the coding standards described below.

3. **Write or update tests**

   All new features should include appropriate tests.

4. **Run code quality checks**

   ```bash
   dotnet format
   ```

5. **Commit your changes**

   Use clear, descriptive commit messages following the [Conventional Commits](https://www.conventionalcommits.org/) standard:

   ```
   feat: add new retry policy implementation
   ```

6. **Push to your fork**

   ```bash
   git push origin feature/your-feature-name
   ```

7. **Create a pull request**

   Go to your fork on GitHub and create a pull request to the main repository's `main` branch.

## Pull Request Process

1. **PR Title and Description**

   Use a clear, descriptive title and provide adequate details about your changes in the description.

2. **PR Checklist**

   Ensure your PR satisfies the following requirements:
   - [ ] Code builds successfully
   - [ ] All tests pass
   - [ ] New code has adequate test coverage (aim for >80%)
   - [ ] Documentation is updated
   - [ ] Code follows project style guidelines

3. **Code Review**

   Maintainers will review your PR and may request changes. Be responsive to feedback and make necessary adjustments.

4. **Merge**

   Once approved, a maintainer will merge your PR.

## Coding Standards

### General Guidelines

- Follow [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Keep methods short and focused on a single responsibility
- Document public APIs with XML documentation comments
- Use async/await for asynchronous operations

### Project-Specific Guidelines

- Always include correlation IDs in logs and exceptions
- Add appropriate unit tests for all functionality
- Use dependency injection where appropriate
- Follow existing architectural patterns in the codebase

## Documentation Guidelines

- Update README.md and other docs when adding features
- Use clear, concise language
- Include code examples where applicable
- Document public APIs thoroughly with XML comments

## Testing Requirements

### Unit Tests

- Each new feature or bug fix should be covered by unit tests
- Test edge cases and error conditions
- Use descriptive test method names that indicate what's being tested
- Organize tests in a structure mirroring the main code

### Integration Tests

- Include integration tests for components that interact with external systems
- Use mocks/stubs when appropriate

## Versioning and Release Process

We follow [Semantic Versioning](https://semver.org/):

- MAJOR version for incompatible API changes
- MINOR version for new functionality in a backward-compatible manner
- PATCH version for backward-compatible bug fixes

## Questions and Support

If you have any questions or need help with the contribution process, please:

- Open an issue for project-related questions
- Contact the maintainers for more complex inquiries

Thank you for contributing to KafkaGenericProcessor.Core!