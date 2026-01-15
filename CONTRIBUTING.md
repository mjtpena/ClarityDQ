# Contributing to ClarityDQ

Thank you for your interest in contributing to ClarityDQ (Fabric Quality Guard)!

## Development Environment Setup

### Prerequisites
- Node.js 18+ and npm
- .NET 8 SDK
- Git
- Azure CLI
- Docker Desktop (for local development)
- Visual Studio Code or Visual Studio 2022

### Local Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/[org]/ClarityDQ.git
   cd ClarityDQ
   ```

2. **Frontend Setup**
   ```bash
   cd src/frontend
   npm install
   npm run dev
   ```

3. **Backend Setup**
   ```bash
   cd src/backend
   dotnet restore
   dotnet build
   dotnet run --project ClarityDQ.Api
   ```

4. **Configure Local Settings**
   - Copy `.env.example` to `.env` and configure
   - Set up local Azure resources or use dev environment

## Project Structure

```
ClarityDQ/
├── src/
│   ├── frontend/              # React application
│   │   ├── components/        # Reusable UI components
│   │   ├── services/          # API clients
│   │   ├── hooks/             # Custom React hooks
│   │   └── utils/             # Helper functions
│   ├── backend/               # .NET services
│   │   ├── ClarityDQ.Api/            # Web API project
│   │   ├── ClarityDQ.Core/           # Domain models & interfaces
│   │   ├── ClarityDQ.Infrastructure/ # Data access & external services
│   │   ├── ClarityDQ.JobScheduler/   # Quartz.NET scheduling
│   │   └── ClarityDQ.RuleEngine/     # Quality rule execution
│   ├── ml-service/            # Python ML/AI services
│   └── shared/                # Shared contracts
├── tests/
│   ├── unit/
│   ├── integration/
│   └── e2e/
└── infrastructure/            # IaC templates
```

## Coding Standards

### TypeScript/React
- Use TypeScript strict mode
- Follow React hooks best practices
- Use Fluent UI components
- Write unit tests with Jest & React Testing Library
- ESLint configuration must pass

### C#/.NET
- Follow Microsoft C# coding conventions
- Use async/await for I/O operations
- Write unit tests with xUnit
- Maintain >80% code coverage
- XML documentation for public APIs

### Python
- Follow PEP 8 style guide
- Type hints for function signatures
- Docstrings for all functions/classes
- Unit tests with pytest

## Git Workflow

### Branch Naming
- `feature/[issue-number]-short-description` - New features
- `bugfix/[issue-number]-short-description` - Bug fixes
- `hotfix/[issue-number]-short-description` - Production hotfixes
- `docs/[description]` - Documentation updates

### Commit Messages
Follow conventional commits:
```
type(scope): subject

body (optional)

footer (optional)
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

Example:
```
feat(rule-engine): add regex pattern validation

Implement regex pattern matching for accuracy rules
with configurable pattern libraries

Closes #123
```

### Pull Request Process

1. **Create Feature Branch**
   ```bash
   git checkout -b feature/123-add-anomaly-detection
   ```

2. **Make Changes & Test**
   - Write code
   - Add/update tests
   - Ensure all tests pass
   - Update documentation

3. **Commit Changes**
   ```bash
   git add .
   git commit -m "feat(ml): add anomaly detection service"
   ```

4. **Push & Create PR**
   ```bash
   git push origin feature/123-add-anomaly-detection
   ```
   - Open PR on GitHub
   - Fill out PR template
   - Link related issues
   - Request reviewers

5. **Code Review**
   - Address reviewer feedback
   - Ensure CI/CD passes
   - Obtain approval

6. **Merge**
   - Squash and merge to main
   - Delete feature branch

## Testing Requirements

### Unit Tests
- Required for all business logic
- Minimum 80% code coverage
- Run before committing: `npm test` or `dotnet test`

### Integration Tests
- Required for API endpoints
- Test database interactions
- Test external service integrations

### E2E Tests
- Required for critical user flows
- Use Playwright for UI tests
- Run in CI/CD pipeline

## Code Review Guidelines

### As a Reviewer
- Be respectful and constructive
- Focus on code quality, not style (linters handle that)
- Check for: logic errors, security issues, performance concerns
- Approve when satisfied, request changes if needed

### As an Author
- Keep PRs focused and reasonably sized
- Respond to all comments
- Don't take feedback personally
- Ask questions if unclear

## Release Process

1. **Version Bump** - Update version in package.json and .csproj
2. **Changelog** - Update CHANGELOG.md
3. **Tag Release** - Create git tag `v1.0.0`
4. **Deploy** - CI/CD automatically deploys to staging
5. **Test** - QA validates staging environment
6. **Promote** - Manual promotion to production

## Questions?

- Open a discussion in GitHub Discussions
- Tag maintainers in issues
- Check documentation in `/docs`

## License

By contributing, you agree that your contributions will be licensed under the project's license.
