# ClarityDQ (Fabric Quality Guard)

**The first native Microsoft Fabric workload for automated data quality management, comprehensive lineage tracking, and enterprise governance.**

## Overview

ClarityDQ transforms your Fabric environment into a self-healing data platform with:
- ✅ Automated data quality checks and profiling
- 🔍 Cross-workspace lineage visualization
- 🤖 AI-powered anomaly detection
- 🛡️ Enterprise governance controls
- 🔄 Automated remediation workflows

## Product Vision

"Turn your Fabric environment into a self-healing data platform with automated quality checks, intelligent anomaly detection, and complete lineage visibility - all without leaving the Fabric portal."

## Target Market

Enterprise organizations using Microsoft Fabric who need:
- Unified data quality interface
- Cross-workspace lineage visibility
- Automated quality validation
- Proactive problem detection
- Governance and compliance controls

## Architecture

### High-Level Components

- **Frontend Service**: React + TypeScript (Azure Static Web Apps)
- **Backend Workload**: .NET 8 (Azure Container Apps)
- **Job Scheduler**: Quartz.NET
- **Rule Execution Engine**: Spark/SQL
- **ML Service**: Azure ML (Anomaly Detection)
- **Data Layer**: Azure SQL + CosmosDB + OneLake

### Technology Stack

**Frontend:**
- React 18+ with TypeScript
- Fluent UI React v9
- Redux Toolkit
- D3.js/Cytoscape.js for lineage visualization

**Backend:**
- .NET 8 (C#)
- ASP.NET Core Web API
- Entity Framework Core
- Quartz.NET for scheduling

**Data:**
- Azure SQL (Metadata)
- CosmosDB with Gremlin (Lineage graph)
- OneLake (Results storage)

**ML/AI:**
- Azure Machine Learning
- Prophet/ARIMA for time-series
- Isolation Forest for anomaly detection

## Project Structure

```
ClarityDQ/
├── src/
│   ├── frontend/           # React frontend application
│   ├── backend/            # .NET backend services
│   ├── ml-service/         # ML/AI anomaly detection
│   └── shared/             # Shared contracts and utilities
├── infrastructure/         # IaC templates (Bicep/Terraform)
├── tests/                  # Integration and E2E tests
├── docs/                   # Documentation
├── scripts/                # Build and deployment scripts
└── .github/                # GitHub Actions workflows
```

## Development Roadmap

### Phase 1 - MVP (Weeks 1-16)
- Data Profiling Engine
- Rule Builder (Visual interface)
- Quality Monitoring & Alerting
- Basic Within-Workspace Lineage
- Quality Dashboard

### Phase 2 - Enterprise Features (Weeks 17-28)
- Cross-Workspace Lineage
- AI Anomaly Detection
- Auto-Remediation Workflows
- Business Glossary
- Compliance Templates (GDPR, HIPAA, SOX)

### Phase 3 - Advanced Capabilities (Weeks 29-40)
- Column-Level Lineage
- Quality SLAs
- Impact Analysis
- Public API & SDK
- Multi-Tenant Management

## Getting Started

### Prerequisites
- Node.js 18+
- .NET 8 SDK
- Azure subscription
- Microsoft Fabric workspace

### Quick Start

Coming soon...

## Contributing

See [CONTRIBUTING.md](./CONTRIBUTING.md) for development guidelines.

## License

TBD

## Contact

For questions and support, please open an issue in this repository.
