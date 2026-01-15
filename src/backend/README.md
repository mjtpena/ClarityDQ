# ClarityDQ Backend

.NET 8 backend services for ClarityDQ (Fabric Quality Guard).

## Technology Stack

- .NET 8 (C#)
- ASP.NET Core Web API
- Entity Framework Core
- Quartz.NET for job scheduling
- Azure SDK for Azure integrations

## Architecture

The backend follows Clean Architecture principles with clear separation of concerns:

- **API Layer** - Controllers, middleware, authentication
- **Core Layer** - Domain models, business logic, interfaces
- **Infrastructure Layer** - Data access, external services, implementations
- **Job Scheduler** - Quartz.NET scheduled jobs
- **Rule Engine** - Quality rule execution logic

## Projects

### ClarityDQ.Api
ASP.NET Core Web API project containing:
- REST API controllers
- Authentication/authorization middleware
- API documentation (Swagger/OpenAPI)
- Application configuration

### ClarityDQ.Core
Domain layer containing:
- Domain entities and value objects
- Business logic services
- Repository interfaces
- Domain events

### ClarityDQ.Infrastructure
Infrastructure layer containing:
- EF Core DbContext and migrations
- Repository implementations
- External API clients (Fabric, Purview, Azure ML)
- Background services

### ClarityDQ.JobScheduler
Quartz.NET scheduler containing:
- Scheduled job definitions
- Rule execution jobs
- Lineage scanning jobs
- Profile generation jobs

### ClarityDQ.RuleEngine
Quality rule execution engine:
- Rule parsers and validators
- SQL/Spark query generators
- Rule executors
- Result processors

## Getting Started

### Prerequisites
- .NET 8 SDK
- Azure SQL Database (or SQL Server)
- CosmosDB account
- Azure Storage account

### Build

```bash
cd src/backend
dotnet restore
dotnet build
```

### Run

```bash
dotnet run --project ClarityDQ.Api
```

API will be available at `http://localhost:5000`

### Test

```bash
dotnet test
```

## Configuration

Configuration is managed through `appsettings.json` and environment variables:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=ClarityDQ;...",
    "CosmosDb": "AccountEndpoint=https://...;AccountKey=..."
  },
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "contoso.onmicrosoft.com",
    "TenantId": "...",
    "ClientId": "..."
  },
  "FabricApi": {
    "BaseUrl": "https://api.fabric.microsoft.com/v1",
    "ClientId": "...",
    "ClientSecret": "..."
  }
}
```

## API Endpoints

### Quality Management
- `POST /api/v1/quality/profiles` - Create profiling job
- `GET /api/v1/quality/profiles/{id}` - Get profile results
- `POST /api/v1/quality/rules` - Create quality rule
- `PUT /api/v1/quality/rules/{id}` - Update rule
- `DELETE /api/v1/quality/rules/{id}` - Delete rule
- `POST /api/v1/quality/rules/{id}/execute` - Execute rule

### Lineage
- `POST /api/v1/lineage/scan` - Scan workspace for lineage
- `GET /api/v1/lineage/graph` - Get lineage graph
- `GET /api/v1/lineage/impact/{itemId}` - Get impact analysis

### Catalog
- `GET /api/v1/catalog/workspaces` - List workspaces
- `GET /api/v1/catalog/items` - List items
- `GET /api/v1/catalog/items/{id}` - Get item details
- `POST /api/v1/catalog/search` - Search catalog

## Database Migrations

Using Entity Framework Core migrations:

```bash
# Add new migration
dotnet ef migrations add MigrationName --project ClarityDQ.Infrastructure

# Update database
dotnet ef database update --project ClarityDQ.Api
```

## Scheduled Jobs

Jobs are configured in `ClarityDQ.JobScheduler`:

```csharp
// Example: Rule execution job
public class RuleExecutionJob : IJob
{
    public async Task Execute(IJobExecutionContext context)
    {
        // Job implementation
    }
}
```

## Testing

- **Unit Tests** - Test business logic in Core layer
- **Integration Tests** - Test API endpoints and database
- **Component Tests** - Test infrastructure components

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageDirectory=./coverage
```

## Logging

Structured logging with Serilog:

```csharp
_logger.LogInformation(
    "Rule {RuleId} executed with result {Result}",
    ruleId,
    result
);
```

## Error Handling

Global exception handler middleware provides consistent error responses:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Invalid rule configuration",
  "traceId": "00-abc123..."
}
```

## Authentication

Microsoft Entra ID (Azure AD) authentication with JWT tokens:

```csharp
[Authorize]
[ApiController]
[Route("api/v1/quality")]
public class QualityController : ControllerBase
{
    // Protected endpoints
}
```

## Contributing

See [CONTRIBUTING.md](../../CONTRIBUTING.md) for guidelines.
