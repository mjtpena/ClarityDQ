# Deployment Guide

## Prerequisites

- Azure subscription
- Azure CLI installed
- GitHub account
- Docker installed (for local testing)
- .NET 10 SDK
- Node.js 22+

## Environment Setup

### 1. Azure Resources

Create required Azure resources:

```bash
# Set variables
RESOURCE_GROUP="claritydq-prod"
LOCATION="eastus"
SQL_SERVER="claritydq-sql"
SQL_DB="claritydq"
CONTAINER_APP="claritydq-api"
STATIC_WEB_APP="claritydq-web"
APP_INSIGHTS="claritydq-insights"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create SQL Server and Database
az sql server create \
  --name $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user sqladmin \
  --admin-password '<strong-password>'

az sql db create \
  --name $SQL_DB \
  --server $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --service-objective S1

# Get SQL connection string
az sql db show-connection-string \
  --client ado.net \
  --name $SQL_DB \
  --server $SQL_SERVER

# Create Application Insights
az monitor app-insights component create \
  --app $APP_INSIGHTS \
  --location $LOCATION \
  --resource-group $RESOURCE_GROUP

# Create Container App Environment
az containerapp env create \
  --name claritydq-env \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION

# Create Static Web App
az staticwebapp create \
  --name $STATIC_WEB_APP \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION
```

### 2. Azure AD App Registration

Register application in Azure AD:

```bash
# Create app registration
az ad app create \
  --display-name "ClarityDQ" \
  --web-redirect-uris "http://localhost:5173" "https://$STATIC_WEB_APP.azurestaticapps.net"

# Note the Application (client) ID and Tenant ID
# Create a client secret
az ad app credential reset --id <app-id>
```

Configure API permissions:
1. Go to Azure Portal → Azure AD → App Registrations
2. Select ClarityDQ app
3. Add API permissions:
   - Microsoft Fabric: Workspace.Read.All, Lakehouse.Read.All
   - Microsoft Graph: User.Read

### 3. Configure Fabric Integration

```bash
# Set Fabric service principal
az ad sp create --id <app-id>

# Assign permissions in Fabric workspace
# (Done via Fabric portal - add service principal to workspace)
```

## Database Migration

### Apply Migrations

```bash
# From project root
cd src/backend/ClarityDQ.Infrastructure

# Update connection string in appsettings.json or use environment variable
export ConnectionStrings__DefaultConnection="Server=tcp:$SQL_SERVER.database.windows.net,1433;Database=$SQL_DB;User ID=sqladmin;Password=<password>;Encrypt=True;"

# Run migrations
dotnet ef database update --project ../ClarityDQ.Api
```

### Seed Initial Data (Optional)

```bash
# Run seed script
dotnet run --project ../ClarityDQ.Api -- seed
```

## Backend Deployment

### Option 1: Container Apps (Recommended)

```bash
# Build and push Docker image
cd src/backend
az acr build \
  --registry claritydqregistry \
  --image claritydq-api:latest \
  --file Dockerfile .

# Deploy to Container Apps
az containerapp create \
  --name $CONTAINER_APP \
  --resource-group $RESOURCE_GROUP \
  --environment claritydq-env \
  --image claritydqregistry.azurecr.io/claritydq-api:latest \
  --target-port 8080 \
  --ingress external \
  --min-replicas 1 \
  --max-replicas 5 \
  --env-vars \
    "ConnectionStrings__DefaultConnection=secretref:sql-connection" \
    "AzureAd__TenantId=$TENANT_ID" \
    "AzureAd__ClientId=$CLIENT_ID" \
    "Fabric__TenantId=$TENANT_ID" \
    "Fabric__ClientId=$CLIENT_ID" \
    "ApplicationInsights__ConnectionString=$APP_INSIGHTS_CONN"

# Set secrets
az containerapp secret set \
  --name $CONTAINER_APP \
  --resource-group $RESOURCE_GROUP \
  --secrets \
    sql-connection="<connection-string>" \
    azure-client-secret="<client-secret>" \
    fabric-client-secret="<client-secret>"
```

### Option 2: App Service

```bash
# Create App Service Plan
az appservice plan create \
  --name claritydq-plan \
  --resource-group $RESOURCE_GROUP \
  --sku P1V3 \
  --is-linux

# Create Web App
az webapp create \
  --name claritydq-api \
  --resource-group $RESOURCE_GROUP \
  --plan claritydq-plan \
  --runtime "DOTNET:10"

# Configure settings
az webapp config appsettings set \
  --name claritydq-api \
  --resource-group $RESOURCE_GROUP \
  --settings \
    ConnectionStrings__DefaultConnection="<connection-string>" \
    AzureAd__TenantId="$TENANT_ID" \
    AzureAd__ClientId="$CLIENT_ID"

# Deploy code
az webapp deployment source config-zip \
  --name claritydq-api \
  --resource-group $RESOURCE_GROUP \
  --src ./publish.zip
```

## Frontend Deployment

### Deploy to Azure Static Web Apps

```bash
cd src/frontend

# Install dependencies
npm install

# Build production bundle
npm run build

# Deploy using Static Web Apps CLI
npx @azure/static-web-apps-cli deploy \
  --app-location dist \
  --api-location ../backend/ClarityDQ.Api \
  --deployment-token $DEPLOYMENT_TOKEN
```

### Configure Environment Variables

Create `.env.production`:

```bash
VITE_API_BASE_URL=https://claritydq-api.azurecontainerapps.io
VITE_AZURE_AD_CLIENT_ID=<client-id>
VITE_AZURE_AD_TENANT_ID=<tenant-id>
VITE_AZURE_AD_REDIRECT_URI=https://claritydq-web.azurestaticapps.net
```

## CI/CD Setup

### GitHub Actions

1. Create repository secrets:
   - `AZURE_CREDENTIALS` - Service principal JSON
   - `SQL_CONNECTION_STRING`
   - `AZURE_CLIENT_SECRET`
   - `STATIC_WEB_APP_TOKEN`

2. Workflows are in `.github/workflows/`:
   - `backend-ci.yml` - Backend build and test
   - `frontend-ci.yml` - Frontend build and test
   - `deploy-backend.yml` - Deploy API to Azure
   - `deploy-frontend.yml` - Deploy frontend to Static Web Apps

### Manual Deployment

```bash
# Trigger backend deployment
gh workflow run deploy-backend.yml

# Trigger frontend deployment
gh workflow run deploy-frontend.yml
```

## Configuration

### Environment Variables

#### Backend (API)
```bash
# Database
ConnectionStrings__DefaultConnection=<sql-connection-string>

# Azure AD
AzureAd__Instance=https://login.microsoftonline.com/
AzureAd__TenantId=<tenant-id>
AzureAd__ClientId=<client-id>
AzureAd__Audience=api://claritydq

# Fabric
Fabric__TenantId=<tenant-id>
Fabric__ClientId=<client-id>
Fabric__ClientSecret=<client-secret>
Fabric__ApiBaseUrl=https://api.fabric.microsoft.com/v1

# OneLake
OneLake__ConnectionString=<storage-connection-string>
OneLake__FileSystemName=claritydq-results
OneLake__Enabled=true

# Application Insights
ApplicationInsights__ConnectionString=<app-insights-connection>

# Hangfire
Hangfire__DashboardEnabled=true
```

#### Frontend (React)
```bash
VITE_API_BASE_URL=<api-url>
VITE_AZURE_AD_CLIENT_ID=<client-id>
VITE_AZURE_AD_TENANT_ID=<tenant-id>
VITE_AZURE_AD_REDIRECT_URI=<redirect-uri>
VITE_AZURE_AD_AUTHORITY=https://login.microsoftonline.com/<tenant-id>
```

## Verification

### Health Checks

```bash
# API health
curl https://claritydq-api.azurecontainerapps.io/health

# Frontend
curl https://claritydq-web.azurestaticapps.net
```

### Smoke Tests

```bash
# Test authentication
curl -H "Authorization: Bearer $TOKEN" \
  https://claritydq-api.azurecontainerapps.io/api/fabric/workspaces

# Test profiling
curl -X POST \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"workspaceId":"abc","datasetName":"test","tableName":"table1"}' \
  https://claritydq-api.azurecontainerapps.io/api/profiling/profile
```

## Monitoring

### Application Insights

View metrics:
```bash
az monitor app-insights metrics show \
  --app $APP_INSIGHTS \
  --resource-group $RESOURCE_GROUP \
  --metric requests/count
```

### Log Analytics

Query logs:
```kusto
traces
| where timestamp > ago(1h)
| where severityLevel >= 3
| project timestamp, message, severityLevel
| order by timestamp desc
```

### Alerts

Create alert rules:
```bash
# High error rate
az monitor metrics alert create \
  --name high-error-rate \
  --resource-group $RESOURCE_GROUP \
  --scopes /subscriptions/<sub-id>/resourceGroups/$RESOURCE_GROUP \
  --condition "avg requests/failed > 5" \
  --window-size 5m \
  --evaluation-frequency 1m
```

## Scaling

### Auto-scaling Rules

```bash
# Configure Container Apps scaling
az containerapp update \
  --name $CONTAINER_APP \
  --resource-group $RESOURCE_GROUP \
  --min-replicas 2 \
  --max-replicas 10 \
  --scale-rule-name cpu-scale \
  --scale-rule-type cpu \
  --scale-rule-metadata "type=Utilization" "value=70"
```

### Database Scaling

```bash
# Upgrade to higher tier
az sql db update \
  --name $SQL_DB \
  --server $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --service-objective P1
```

## Backup & Recovery

### Database Backup

```bash
# Automated backups are enabled by default
# Manual backup
az sql db export \
  --name $SQL_DB \
  --server $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --storage-key-type StorageAccessKey \
  --storage-key $STORAGE_KEY \
  --storage-uri https://<storage>.blob.core.windows.net/backups/backup.bacpac
```

### Disaster Recovery

1. Enable geo-replication:
```bash
az sql db replica create \
  --name $SQL_DB \
  --server $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --partner-server claritydq-sql-secondary \
  --partner-resource-group $RESOURCE_GROUP
```

2. Deploy to secondary region with load balancer

## Security Checklist

- [ ] Enable Azure AD authentication
- [ ] Configure SSL/TLS certificates
- [ ] Enable firewall rules on SQL Server
- [ ] Set up Key Vault for secrets
- [ ] Enable Application Insights
- [ ] Configure CORS policies
- [ ] Enable managed identities
- [ ] Set up private endpoints (production)
- [ ] Enable audit logging
- [ ] Configure backup retention

## Troubleshooting

### Common Issues

1. **Database connection fails**
   - Check firewall rules
   - Verify connection string
   - Ensure migrations ran

2. **Authentication errors**
   - Verify Azure AD app registration
   - Check token expiration
   - Validate redirect URIs

3. **Fabric API errors**
   - Confirm service principal permissions
   - Check workspace access
   - Verify API scopes

### Logs

```bash
# Container Apps logs
az containerapp logs show \
  --name $CONTAINER_APP \
  --resource-group $RESOURCE_GROUP \
  --follow

# Application Insights query
az monitor app-insights query \
  --app $APP_INSIGHTS \
  --analytics-query "traces | where timestamp > ago(1h)"
```

## Rollback Procedure

```bash
# Rollback to previous container image
az containerapp update \
  --name $CONTAINER_APP \
  --resource-group $RESOURCE_GROUP \
  --image claritydqregistry.azurecr.io/claritydq-api:previous

# Rollback database migration
dotnet ef database update <previous-migration> --project src/backend/ClarityDQ.Api
```

## Post-Deployment

1. Verify all endpoints respond correctly
2. Run smoke tests
3. Check Application Insights for errors
4. Monitor resource utilization
5. Update documentation
6. Notify team of deployment

## Support

- Deployment issues: deployment@claritydq.com
- Infrastructure: infrastructure@claritydq.com
- Documentation: https://github.com/mjtpena/ClarityDQ/tree/main/docs
