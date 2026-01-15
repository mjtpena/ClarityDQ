# ClarityDQ Technical Specification

## Product Overview

**Product Name:** Fabric Quality Guard  
**Category:** Microsoft Fabric Custom Workload - Data Quality & Governance Automation  
**Version:** 1.0.0

For complete technical specification, see individual documents:

- [Architecture Overview](./architecture/overview.md)
- [API Specification](./api/README.md)
- [Data Models](./data-models/README.md)
- [Component Specifications](./components/README.md)
- [Development Roadmap](./roadmap.md)

## Quick Links

- [Market Opportunity](./market-analysis.md)
- [Feature Requirements](./features/README.md)
- [Integration Points](./integrations/README.md)
- [Security & Compliance](./security.md)
- [Deployment Guide](./deployment/README.md)

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                    Fabric Portal (UI)                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │   Quality    │  │   Lineage    │  │  Governance  │      │
│  │  Dashboard   │  │    Viewer    │  │   Console    │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                            ↕ Extension Client SDK
┌─────────────────────────────────────────────────────────────┐
│              Frontend Service (React/TypeScript)             │
│                  (Azure Static Web Apps)                     │
└─────────────────────────────────────────────────────────────┘
                            ↕ REST APIs
┌─────────────────────────────────────────────────────────────┐
│           Backend Workload Service (.NET 8)                  │
│               (Azure Container Apps)                         │
└─────────────────────────────────────────────────────────────┘
            ↕                   ↕                    ↕
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│  Job Scheduler  │  │  Rule Execution │  │   ML Service    │
│  (Quartz.NET)   │  │     Engine      │  │  (Anomaly Det.) │
└─────────────────┘  └─────────────────┘  └─────────────────┘
```

## Core Features

### Phase 1 - MVP (Weeks 1-16)
1. **Data Profiling Engine** - Statistical profiling of Lakehouse/Warehouse tables
2. **Rule Builder** - Visual interface for quality rules
3. **Quality Monitoring** - Scheduled checks with alerting
4. **Basic Lineage** - Within-workspace lineage tracking
5. **Quality Dashboard** - Visualization of quality metrics

### Phase 2 - Enterprise (Weeks 17-28)
1. **Cross-Workspace Lineage** - Complete end-to-end lineage
2. **AI Anomaly Detection** - ML-based quality issue detection
3. **Auto-Remediation** - Automated responses to failures
4. **Business Glossary** - Metadata management
5. **Compliance Templates** - GDPR, HIPAA, SOX rule sets

### Phase 3 - Advanced (Weeks 29-40)
1. **Column-Level Lineage** - Fine-grained transformation tracking
2. **Quality SLAs** - Service level agreements
3. **Impact Analysis** - Downstream impact assessment
4. **Public API & SDK** - Programmatic access
5. **Multi-Tenant Management** - Cross-tenant governance

## Technology Stack

- **Frontend**: React 18+, TypeScript, Fluent UI v9, Redux Toolkit
- **Backend**: .NET 8, ASP.NET Core, Entity Framework Core
- **Scheduling**: Quartz.NET
- **Data Processing**: Apache Spark, SQL
- **AI/ML**: Azure Machine Learning
- **Storage**: Azure SQL, CosmosDB (Gremlin), OneLake
- **Hosting**: Azure Static Web Apps, Azure Container Apps
- **Authentication**: Microsoft Entra ID (Azure AD)

## Development Workflow

See [CONTRIBUTING.md](../CONTRIBUTING.md) for:
- Development environment setup
- Coding standards
- Testing requirements
- PR process
- Release procedures
