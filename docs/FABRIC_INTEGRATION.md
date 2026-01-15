# Fabric Custom Workload Integration

ClarityDQ is designed as a **Microsoft Fabric Custom Workload** that embeds natively into the Fabric portal.

## Architecture

### Deployment Model
- **Frontend**: Embedded in Fabric portal (iframe/Web Component)
- **Backend**: Containerized service (Azure Container Apps)
- **Authentication**: Fabric-provided identity context
- **Data Access**: Fabric REST APIs + OneLake

### Integration Points

#### 1. Frontend Embedding
The React frontend is designed to run both:
- **Embedded Mode**: Inside Fabric portal (production)
- **Standalone Mode**: For local development

```typescript
// Detects Fabric context automatically
const fabricContext = getFabricContext();
if (fabricContext) {
  // Use Fabric-provided token
  token = fabricContext.accessToken;
} else {
  // Fall back to MSAL for dev
  token = await msalInstance.acquireToken();
}
```

#### 2. Workload Manifest
See `WorkloadManifest.xml` for:
- Capabilities (Profiling, Rules, Lineage)
- Required permissions
- Navigation structure
- Settings configuration

#### 3. Fabric APIs
The backend integrates with Fabric through:
- **Workspace API**: Access workspace metadata
- **Lakehouse API**: Query tables and schemas
- **Warehouse API**: Execute SQL queries
- **OneLake API**: Read/write data profiles
- **Admin API**: Tenant-level governance

#### 4. Backend Service
Containerized .NET service running on Azure Container Apps:
- Validates Fabric-issued JWT tokens
- Uses Fabric APIs for all data access
- Stores metadata in Azure SQL
- Stores lineage in CosmosDB (Gremlin)

## Development Modes

### Local Development
```bash
# Frontend runs standalone with MSAL
npm run frontend:dev

# Backend runs locally
dotnet run --project src/backend/ClarityDQ.Api
```

### Fabric Integration Testing
1. Deploy to Azure Container Apps
2. Register workload in Fabric Admin Portal
3. Test embedded experience in Fabric

## Registration Process

1. **Deploy Infrastructure**
   ```bash
   az deployment sub create \
     --location eastus \
     --template-file infrastructure/main.bicep
   ```

2. **Upload Workload Package**
   - Package frontend (Static Web App)
   - Package backend (Container image)
   - Include WorkloadManifest.xml

3. **Register in Fabric**
   - Navigate to Fabric Admin Portal
   - Upload workload package
   - Configure permissions
   - Enable for tenant

4. **Verify Installation**
   - Open Fabric workspace
   - Check for ClarityDQ in workspace menu
   - Verify embedded UI loads
   - Test API connectivity

## Security Model

### Authentication Flow
```
User → Fabric Portal → Fabric Identity
                    ↓
              JWT Token (Fabric-issued)
                    ↓
            ClarityDQ Frontend (embedded)
                    ↓
        Token passed to Backend API
                    ↓
         Backend validates with Fabric
```

### Permissions
Workload requests minimum permissions:
- Read workspace metadata
- Read lakehouse/warehouse schemas
- Execute data profiling queries
- Write results to OneLake

Users can only access data they have Fabric permissions for.

## Fabric APIs Used

### Data Access
```csharp
// Lakehouse table profiling
GET /v1/workspaces/{workspaceId}/lakehouses/{lakehouseId}/tables

// Warehouse query execution
POST /v1/workspaces/{workspaceId}/warehouses/{warehouseId}/query

// OneLake file operations
GET /v1/onelake/{workspaceId}/files/{path}
```

### Metadata
```csharp
// Workspace info
GET /v1/workspaces/{workspaceId}

// Item lineage
GET /v1/workspaces/{workspaceId}/items/{itemId}/lineage
```

## Lifecycle Hooks

Backend implements workload lifecycle:
- `OnInstall`: Initialize tenant-specific resources
- `OnUninstall`: Clean up resources
- `OnUpdate`: Handle version upgrades
- `OnWorkspaceAdd`: Setup workspace-specific config
- `OnWorkspaceRemove`: Clean workspace data

## Current Status

✅ Backend architecture (Fabric-compatible)
✅ Container Apps deployment (correct hosting model)
✅ Workload manifest created
✅ Frontend supports dual-mode (embedded + standalone)
⏳ Fabric Extension SDK integration (when available)
⏳ Fabric Admin Portal registration
⏳ Production Fabric API integration

## Next Steps for Full Fabric Integration

1. Replace mock profiling with Fabric Lakehouse APIs
2. Implement OneLake storage for profile results
3. Add Fabric lineage API calls
4. Test embedded experience in Fabric preview
5. Submit to Fabric Marketplace (when available)
