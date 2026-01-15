# ClarityDQ Project Structure

This document describes the organization of the ClarityDQ repository.

## Root Structure

```
ClarityDQ/
├── .github/                    # GitHub configuration
│   ├── workflows/              # GitHub Actions CI/CD
│   └── ISSUE_TEMPLATE/         # Issue templates
├── src/                        # Source code
│   ├── frontend/               # React frontend application
│   ├── backend/                # .NET backend services
│   ├── ml-service/             # Python ML/AI services
│   └── shared/                 # Shared contracts and utilities
├── tests/                      # Test suites
│   ├── unit/                   # Unit tests
│   ├── integration/            # Integration tests
│   └── e2e/                    # End-to-end tests
├── infrastructure/             # Infrastructure as Code
│   ├── bicep/                  # Azure Bicep templates
│   ├── terraform/              # Terraform configurations (alternative)
│   └── scripts/                # Deployment scripts
├── docs/                       # Documentation
│   ├── architecture/           # Architecture diagrams and docs
│   ├── api/                    # API documentation
│   ├── features/               # Feature specifications
│   └── deployment/             # Deployment guides
├── scripts/                    # Build and utility scripts
├── .gitignore                  # Git ignore rules
├── .env.example                # Environment variables template
├── package.json                # Root package.json for npm workspaces
├── README.md                   # Project overview
├── CONTRIBUTING.md             # Contribution guidelines
└── CHANGELOG.md                # Version history
```

## Source Code Structure

### Frontend (`src/frontend/`)
```
frontend/
├── public/                     # Static assets
├── src/
│   ├── components/             # React components
│   │   ├── quality/            # Quality dashboard components
│   │   ├── lineage/            # Lineage viewer components
│   │   ├── catalog/            # Data catalog components
│   │   ├── governance/         # Governance console components
│   │   └── common/             # Shared UI components
│   ├── services/               # API client services
│   ├── hooks/                  # Custom React hooks
│   ├── utils/                  # Helper functions
│   ├── styles/                 # Global styles
│   ├── types/                  # TypeScript type definitions
│   ├── store/                  # Redux store configuration
│   ├── App.tsx                 # Root component
│   └── main.tsx                # Application entry point
├── package.json
├── tsconfig.json
├── vite.config.ts
└── README.md
```

### Backend (`src/backend/`)
```
backend/
├── ClarityDQ.Api/              # ASP.NET Core Web API
│   ├── Controllers/            # API controllers
│   ├── Middleware/             # Custom middleware
│   ├── Program.cs              # Application entry point
│   └── appsettings.json        # Configuration
├── ClarityDQ.Core/             # Domain layer
│   ├── Entities/               # Domain models
│   ├── Interfaces/             # Repository & service interfaces
│   ├── Services/               # Business logic services
│   └── Specifications/         # Query specifications
├── ClarityDQ.Infrastructure/   # Infrastructure layer
│   ├── Data/                   # EF Core DbContext
│   ├── Repositories/           # Data repositories
│   ├── ExternalServices/       # External API clients
│   └── Configuration/          # DI configuration
├── ClarityDQ.JobScheduler/     # Quartz.NET scheduling
│   ├── Jobs/                   # Scheduled job implementations
│   └── Configuration/          # Scheduler configuration
├── ClarityDQ.RuleEngine/       # Quality rule execution
│   ├── Executors/              # Rule executors
│   ├── Parsers/                # Rule definition parsers
│   └── Generators/             # Query generators
└── ClarityDQ.sln               # Solution file
```

### ML Service (`src/ml-service/`)
```
ml-service/
├── models/                     # ML model definitions
├── training/                   # Training scripts
├── inference/                  # Inference/scoring services
├── api/                        # FastAPI endpoints
├── utils/                      # Helper functions
├── requirements.txt            # Python dependencies
└── README.md
```

## Documentation Structure

### Architecture Documentation
- `architecture/overview.md` - High-level architecture
- `architecture/components.md` - Component specifications
- `architecture/data-flow.md` - Data flow diagrams
- `architecture/security.md` - Security architecture

### API Documentation
- `api/README.md` - API overview
- `api/quality-management.md` - Quality APIs
- `api/lineage.md` - Lineage APIs
- `api/catalog.md` - Catalog APIs
- `api/governance.md` - Governance APIs

### Feature Documentation
- `features/data-profiling.md` - Profiling engine
- `features/rule-engine.md` - Rule execution
- `features/lineage-tracking.md` - Lineage system
- `features/anomaly-detection.md` - ML anomaly detection
- `features/auto-remediation.md` - Remediation workflows

## Infrastructure Structure

### Bicep Templates
```
infrastructure/bicep/
├── main.bicep                  # Main orchestration
├── modules/
│   ├── storage.bicep           # Storage accounts
│   ├── sql.bicep               # Azure SQL
│   ├── cosmosdb.bicep          # CosmosDB
│   ├── container-apps.bicep    # Container Apps
│   ├── static-web-app.bicep    # Static Web Apps
│   ├── app-insights.bicep      # Application Insights
│   └── key-vault.bicep         # Key Vault
└── parameters/
    ├── dev.parameters.json     # Dev environment
    ├── staging.parameters.json # Staging environment
    └── prod.parameters.json    # Production environment
```

## Testing Structure

### Unit Tests
- `tests/unit/frontend/` - Frontend component tests
- `tests/unit/backend/` - Backend service tests
- `tests/unit/ml-service/` - ML model tests

### Integration Tests
- `tests/integration/api/` - API integration tests
- `tests/integration/database/` - Database tests
- `tests/integration/external/` - External service tests

### E2E Tests
- `tests/e2e/quality-workflow.spec.ts` - Quality check workflows
- `tests/e2e/lineage-visualization.spec.ts` - Lineage viewer
- `tests/e2e/rule-creation.spec.ts` - Rule builder

## Scripts

- `scripts/setup-dev.sh` - Development environment setup
- `scripts/build-all.sh` - Build all components
- `scripts/deploy-dev.sh` - Deploy to dev environment
- `scripts/run-tests.sh` - Run all tests
- `scripts/generate-docs.sh` - Generate API documentation

## Naming Conventions

### Files
- React components: PascalCase (e.g., `QualityDashboard.tsx`)
- TypeScript utilities: camelCase (e.g., `formatDate.ts`)
- C# classes: PascalCase (e.g., `QualityRule.cs`)
- Python modules: snake_case (e.g., `anomaly_detector.py`)

### Directories
- kebab-case for all directory names (e.g., `rule-engine/`, `data-profiling/`)

### Branches
- `feature/[issue-number]-description`
- `bugfix/[issue-number]-description`
- `hotfix/[issue-number]-description`
- `docs/description`

## Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/[org]/ClarityDQ.git
   cd ClarityDQ
   ```

2. **Install dependencies**
   ```bash
   npm install
   cd src/backend && dotnet restore
   ```

3. **Configure environment**
   ```bash
   cp .env.example .env
   # Edit .env with your configuration
   ```

4. **Run development servers**
   ```bash
   npm run frontend:dev  # Terminal 1
   npm run backend:run   # Terminal 2
   ```

See [CONTRIBUTING.md](../CONTRIBUTING.md) for detailed development guidelines.
