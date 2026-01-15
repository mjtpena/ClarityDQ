# ClarityDQ Infrastructure

Azure infrastructure as code using Bicep templates.

## Prerequisites

- Azure CLI installed and logged in
- Azure subscription with appropriate permissions
- PowerShell or Bash

## Quick Start

### 1. Login to Azure

```bash
az login
az account set --subscription <subscription-id>
```

### 2. Deploy Infrastructure

```bash
# Create a parameters file (copy from main.parameters.example.json)
cp main.parameters.example.json main.parameters.json

# Edit parameters (add SQL password, etc.)
nano main.parameters.json

# Deploy to dev environment
az deployment sub create \
  --location eastus \
  --template-file main.bicep \
  --parameters main.parameters.json

# Deploy to production
az deployment sub create \
  --location eastus \
  --template-file main.bicep \
  --parameters environment=prod
```

## Architecture

The infrastructure includes:

- **Resource Group**: Container for all resources
- **Azure Storage**: OneLake integration, profiles, and results storage
- **Azure SQL Database**: Metadata and configuration storage
- **CosmosDB with Gremlin API**: Lineage graph database
- **Container Apps Environment**: Backend API hosting
- **Static Web Apps**: Frontend hosting
- **Azure ML Workspace**: Anomaly detection and ML models

## Modules

- `storage.bicep` - Storage account with blob containers
- `sql.bicep` - SQL Server and database
- `cosmosdb.bicep` - CosmosDB account with Gremlin API
- `container-apps.bicep` - Container Apps environment
- `static-web-app.bicep` - Static Web App for frontend
- `ml-workspace.bicep` - Azure ML workspace with dependencies

## Environments

- **dev**: Development environment with minimal SKUs
- **staging**: Staging environment for pre-production testing
- **prod**: Production environment with high-availability SKUs

## Cost Estimation

**Development Environment (~$50-100/month):**
- Storage: ~$5/month
- SQL Basic: ~$5/month
- CosmosDB (400 RU/s): ~$25/month
- Container Apps: Pay-per-use
- Static Web App: Free tier
- ML Workspace: ~$10/month

**Production Environment (~$500-1000/month):**
- Storage with redundancy: ~$20/month
- SQL Standard: ~$150/month
- CosmosDB (autoscale): ~$200/month
- Container Apps: ~$100/month
- Static Web App: Standard tier ~$10/month
- ML Workspace: ~$50/month

## Security

All resources are configured with:
- TLS 1.2 minimum
- Managed identities where possible
- Private endpoints (production)
- Soft delete enabled
- Azure RBAC

## Cleanup

```bash
# Delete resource group
az group delete --name claritydq-dev-rg --yes --no-wait
```

## Troubleshooting

### Deployment Fails

Check validation errors:
```bash
az deployment sub validate \
  --location eastus \
  --template-file main.bicep \
  --parameters main.parameters.json
```

### Name Conflicts

Some resource names must be globally unique (Storage, CosmosDB, Static Web App). Adjust the `namePrefix` parameter if you encounter conflicts.
