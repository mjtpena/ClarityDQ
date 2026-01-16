# ClarityDQ Architecture

## System Overview

ClarityDQ is a cloud-native data quality management platform built for Microsoft Fabric. It provides automated profiling, quality rules, lineage tracking, and governance capabilities.

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Microsoft Fabric                          │
│  ┌──────────┐  ┌───────────┐  ┌──────────┐  ┌──────────┐  │
│  │Lakehouse │  │ Warehouse │  │ Dataset  │  │ Pipeline │  │
│  └────┬─────┘  └─────┬─────┘  └────┬─────┘  └────┬─────┘  │
└───────┼──────────────┼─────────────┼─────────────┼─────────┘
        │              │             │             │
        └──────────────┴─────────────┴─────────────┘
                       │
                       ▼
┌─────────────────────────────────────────────────────────────┐
│                    ClarityDQ Platform                        │
│                                                              │
│  ┌───────────────────────────────────────────────────────┐ │
│  │              Frontend (React + Fluent UI)              │ │
│  └───────────────────────┬───────────────────────────────┘ │
│                          │                                  │
│  ┌───────────────────────▼───────────────────────────────┐ │
│  │           Backend API (.NET 10 + ASP.NET)             │ │
│  │  ┌─────────┐  ┌────────┐  ┌────────┐  ┌──────────┐  │ │
│  │  │Profiling│  │  Rules │  │Lineage │  │Scheduling│  │ │
│  │  └─────────┘  └────────┘  └────────┘  └──────────┘  │ │
│  └───────────────────────┬───────────────────────────────┘ │
│                          │                                  │
│  ┌───────────────────────▼───────────────────────────────┐ │
│  │              Core Domain Layer                         │ │
│  │  • Entities  • Interfaces  • Domain Logic              │ │
│  └───────────────────────┬───────────────────────────────┘ │
│                          │                                  │
│  ┌───────────────────────▼───────────────────────────────┐ │
│  │            Infrastructure Layer                        │ │
│  │  ┌──────────┐  ┌───────────┐  ┌──────────────────┐   │ │
│  │  │ SQL DB   │  │  Fabric   │  │  OneLake Storage │   │ │
│  │  │ (EF Core)│  │  Client   │  │   (Results)      │   │ │
│  │  └──────────┘  └───────────┘  └──────────────────┘   │ │
│  └───────────────────────────────────────────────────────┘ │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

## Component Architecture

### Frontend Layer
- **Technology**: React 18.2 + TypeScript
- **UI Framework**: Fluent UI 9
- **State Management**: Redux Toolkit
- **Routing**: React Router v7
- **Authentication**: MSAL for Azure AD

**Key Components**:
- Dashboard (metrics, alerts, trends)
- Data Profiling UI (profile results, column statistics)
- Rule Builder (visual rule designer)
- Lineage Viewer (interactive graph visualization)
- Schedule Manager (CRON expression editor)

### Backend API Layer
- **Technology**: .NET 10 + ASP.NET Core
- **API Style**: RESTful
- **Documentation**: OpenAPI 3.0 (Swagger)
- **Authentication**: JWT Bearer (Azure AD)
- **Background Jobs**: Hangfire

**Controllers**:
- `ProfilingController` - Data profiling operations
- `RulesController` - Quality rule CRUD
- `LineageController` - Lineage graph management
- `SchedulesController` - Schedule management
- `FabricController` - Fabric integration endpoints

### Core Domain Layer

**ClarityDQ.Core**
- Domain entities (Rule, DataProfile, LineageNode, Schedule)
- Service interfaces
- Business logic
- Domain events

**Key Entities**:
```csharp
Rule              // Quality rules definition
RuleExecution     // Execution history
DataProfile       // Profiling results
LineageNode       // Lineage graph nodes
LineageEdge       // Lineage relationships
Schedule          // Recurring job schedules
ScheduleExecution // Schedule run history
```

### Service Layer

**ClarityDQ.Profiling**
- `ProfilingService` - Profile Fabric tables
- Calculates: row count, null %, distinct values, min/max, data types
- Stores results in OneLake

**ClarityDQ.RuleEngine**
- `RuleExecutor` - Execute quality rules
- Supports: Completeness, Uniqueness, Validity, Accuracy, Consistency, Custom
- Returns violations and metrics

**ClarityDQ.Lineage**
- `LineageService` - Build and query lineage graphs
- Tracks: table-to-table, column-level relationships
- Supports: impact analysis, upstream/downstream queries

**ClarityDQ.Scheduling**
- `SchedulingService` - Manage recurring jobs
- CRON expression parsing
- Next run calculation
- Execution tracking via Hangfire

### Infrastructure Layer

**ClarityDQ.Infrastructure**
- Entity Framework Core DbContext
- SQL Server persistence
- Migrations

**ClarityDQ.FabricClient**
- Microsoft Fabric API client
- Workspace and item enumeration
- Table schema retrieval
- Azure AD authentication

**ClarityDQ.OneLake**
- OneLake storage integration
- Write profiling results
- Write rule execution history
- Query historical data

## Data Flow

### Profiling Flow
```
User → API → ProfilingService → FabricClient → Fabric Tables
                     ↓
                OneLakeService → OneLake Storage
                     ↓
                 SQL Database
```

### Rule Execution Flow
```
Schedule/Manual → API → RuleService → RuleExecutor → FabricClient
                                          ↓
                                    Validation Logic
                                          ↓
                                    RuleExecution Record
                                          ↓
                                   OneLake + SQL DB
```

### Lineage Tracking Flow
```
User/Pipeline → API → LineageService → SQL Graph Storage
                                           ↓
                                    LineageNode + Edge
                                           ↓
                                      Query API
```

## Deployment Architecture

### Azure Services
- **Frontend**: Azure Static Web Apps
- **Backend API**: Azure Container Apps
- **Database**: Azure SQL Database
- **Storage**: OneLake (Fabric native)
- **Auth**: Azure AD
- **Monitoring**: Application Insights
- **CI/CD**: GitHub Actions

### Fabric Integration
- Deployed as Custom Workload
- Embedded in Fabric workspace UI
- Native authentication via Fabric tokens
- Direct access to Lakehouse/Warehouse data

## Security Architecture

### Authentication & Authorization
- Azure AD integration
- Service Principal for backend-to-Fabric
- User tokens for frontend
- RBAC via Azure AD groups

### Data Security
- Encrypted at rest (Azure SQL + OneLake)
- Encrypted in transit (TLS 1.3)
- No data extraction - queries executed in Fabric
- Audit logging via Application Insights

## Scalability & Performance

### Backend Scaling
- Horizontal scaling via Container Apps
- Stateless API design
- Connection pooling (SQL + HTTP)
- Hangfire distributed job processing

### Database Optimization
- Indexed on WorkspaceId, RuleId, ExecutedAt
- Query optimization via EF Core
- Read replicas for reporting (future)

### Caching Strategy
- In-memory cache for Fabric metadata
- Redis for distributed cache (future)
- Browser caching for static assets

## Observability

### Logging
- Structured logging via Serilog
- Log levels: Info, Warning, Error
- Correlation IDs across services

### Monitoring
- Application Insights integration
- Custom metrics: rule execution time, profiling duration
- Health check endpoints

### Alerting
- Failed rule executions
- API error rate threshold
- Database connection issues

## Technology Stack

| Layer | Technology | Version |
|-------|-----------|---------|
| Frontend | React | 18.2 |
| UI Framework | Fluent UI | 9.72+ |
| State Management | Redux Toolkit | 2.11+ |
| Backend | .NET | 10.0 |
| Database | SQL Server | 2022 |
| ORM | Entity Framework Core | 10.0 |
| Background Jobs | Hangfire | 1.8+ |
| API Docs | OpenAPI/Swagger | 3.0 |
| Testing | xUnit + Vitest | Latest |
| E2E Testing | Playwright | Latest |

## Design Patterns

### Backend
- **Clean Architecture**: Separation of concerns
- **Repository Pattern**: Data access abstraction
- **Dependency Injection**: Built-in ASP.NET Core DI
- **CQRS Lite**: Read/Write separation where needed
- **Strategy Pattern**: Rule execution strategies

### Frontend
- **Component-Based**: Reusable React components
- **Container/Presenter**: Smart vs Dumb components
- **Hooks**: State and side-effect management
- **Redux Slices**: Feature-based state management

## Future Architecture Enhancements

### Phase 2
- ML Service for anomaly detection
- Cross-workspace lineage aggregation
- Auto-remediation engine

### Phase 3
- Column-level lineage
- Quality SLAs with alerting
- Public SDK and webhooks
- Multi-tenant support

## References

- [Fabric Integration Guide](./FABRIC_INTEGRATION.md)
- [Project Structure](./PROJECT_STRUCTURE.md)
- [Getting Started](../GETTING_STARTED.md)
- [API Documentation](http://localhost:5000/openapi)
