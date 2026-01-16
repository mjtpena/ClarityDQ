# API Reference

## Base URL
```
Development: http://localhost:5000
Production: https://claritydq-prod-api.azurecontainerapps.io
```

## Authentication

All API requests require authentication via Azure AD JWT Bearer tokens.

```http
Authorization: Bearer <token>
```

### Obtaining a Token
Use MSAL.js for frontend or Azure CLI for testing:
```bash
az login
az account get-access-token --resource https://claritydq-prod-api.azurecontainerapps.io
```

## API Endpoints

### Health Check

#### GET /health
Check API health status.

**Response 200**:
```json
{
  "status": "Healthy",
  "timestamp": "2026-01-16T04:00:00Z",
  "checks": [
    {
      "name": "api",
      "status": "Healthy",
      "description": "API is running",
      "duration": 0.5
    },
    {
      "name": "sqlserver",
      "status": "Healthy",
      "description": null,
      "duration": 12.3
    }
  ]
}
```

---

## Profiling API

### POST /api/profiling/profile
Profile a Fabric table.

**Request Body**:
```json
{
  "workspaceId": "abc123",
  "datasetName": "SalesLakehouse",
  "tableName": "Customers"
}
```

**Response 200**:
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "workspaceId": "abc123",
  "datasetName": "SalesLakehouse",
  "tableName": "Customers",
  "rowCount": 150000,
  "columnCount": 12,
  "profiledAt": "2026-01-16T04:00:00Z",
  "columnProfiles": [
    {
      "columnName": "CustomerId",
      "dataType": "int",
      "nullCount": 0,
      "nullPercentage": 0.0,
      "distinctCount": 150000,
      "minValue": "1",
      "maxValue": "150000"
    },
    {
      "columnName": "Email",
      "dataType": "string",
      "nullCount": 150,
      "nullPercentage": 0.1,
      "distinctCount": 149850,
      "minValue": null,
      "maxValue": null
    }
  ]
}
```

**Error Responses**:
- 400 Bad Request - Invalid request parameters
- 401 Unauthorized - Missing or invalid token
- 404 Not Found - Table not found
- 500 Internal Server Error

### GET /api/profiling/{workspaceId}/history
Get profiling history for a workspace.

**Query Parameters**:
- `datasetName` (optional): Filter by dataset
- `tableName` (optional): Filter by table
- `since` (optional): ISO 8601 timestamp

**Response 200**:
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "workspaceId": "abc123",
    "datasetName": "SalesLakehouse",
    "tableName": "Customers",
    "rowCount": 150000,
    "columnCount": 12,
    "profiledAt": "2026-01-16T04:00:00Z"
  }
]
```

---

## Rules API

### GET /api/rules
List all quality rules.

**Query Parameters**:
- `workspaceId` (optional): Filter by workspace
- `type` (optional): Filter by rule type (Completeness, Uniqueness, Validity, Accuracy, Consistency, Custom)
- `isEnabled` (optional): Filter by enabled status

**Response 200**:
```json
[
  {
    "id": "660e8400-e29b-41d4-a716-446655440000",
    "name": "Customer Email Required",
    "description": "All customers must have an email",
    "type": "Completeness",
    "workspaceId": "abc123",
    "datasetName": "SalesLakehouse",
    "tableName": "Customers",
    "columnName": "Email",
    "expression": "",
    "threshold": 99.0,
    "severity": "High",
    "isEnabled": true,
    "createdAt": "2026-01-15T10:00:00Z",
    "updatedAt": "2026-01-15T10:00:00Z"
  }
]
```

### POST /api/rules
Create a new quality rule.

**Request Body**:
```json
{
  "name": "Customer Email Required",
  "description": "All customers must have an email",
  "type": "Completeness",
  "workspaceId": "abc123",
  "datasetName": "SalesLakehouse",
  "tableName": "Customers",
  "columnName": "Email",
  "expression": "",
  "threshold": 99.0,
  "severity": "High",
  "isEnabled": true
}
```

**Response 201**:
```json
{
  "id": "660e8400-e29b-41d4-a716-446655440000",
  "name": "Customer Email Required",
  ...
}
```

### GET /api/rules/{id}
Get a specific rule.

**Response 200**: Same as rule object above.

### PUT /api/rules/{id}
Update a rule.

**Request Body**: Same as POST /api/rules

**Response 200**: Updated rule object.

### DELETE /api/rules/{id}
Delete a rule.

**Response 204**: No content.

### POST /api/rules/{id}/execute
Execute a rule immediately.

**Response 200**:
```json
{
  "id": "770e8400-e29b-41d4-a716-446655440000",
  "ruleId": "660e8400-e29b-41d4-a716-446655440000",
  "executedAt": "2026-01-16T04:05:00Z",
  "status": "Completed",
  "recordsChecked": 150000,
  "recordsPassed": 149850,
  "recordsFailed": 150,
  "successRate": 99.9,
  "executionTimeMs": 5432,
  "violations": [
    {
      "rowIndex": 42,
      "rowData": {
        "CustomerId": "43",
        "Email": null
      },
      "violationMessage": "Column 'Email' is null or empty"
    }
  ]
}
```

### GET /api/rules/{id}/executions
Get execution history for a rule.

**Query Parameters**:
- `limit` (optional, default 50): Number of executions to return
- `since` (optional): ISO 8601 timestamp

**Response 200**: Array of rule execution objects.

---

## Lineage API

### GET /api/lineage/graph
Get lineage graph for a workspace.

**Query Parameters**:
- `workspaceId` (required): Workspace ID
- `depth` (optional, default 2): Graph traversal depth

**Response 200**:
```json
{
  "nodes": [
    {
      "id": "node-1",
      "workspaceId": "abc123",
      "datasetName": "SalesLakehouse",
      "tableName": "Customers",
      "nodeType": "Table"
    }
  ],
  "edges": [
    {
      "id": "edge-1",
      "sourceNodeId": "node-1",
      "targetNodeId": "node-2",
      "transformationType": "Join",
      "description": "Customer orders join"
    }
  ]
}
```

### POST /api/lineage/nodes
Create a lineage node.

**Request Body**:
```json
{
  "workspaceId": "abc123",
  "datasetName": "SalesLakehouse",
  "tableName": "Customers",
  "nodeType": "Table"
}
```

**Response 201**: Created node object.

### POST /api/lineage/edges
Create a lineage edge (relationship).

**Request Body**:
```json
{
  "sourceNodeId": "node-1",
  "targetNodeId": "node-2",
  "transformationType": "Join",
  "description": "Customer orders join"
}
```

**Response 201**: Created edge object.

### GET /api/lineage/impact/{nodeId}
Get downstream impact analysis.

**Response 200**:
```json
{
  "impactedNodes": [
    {
      "nodeId": "node-3",
      "distance": 1,
      "path": ["node-1", "node-3"]
    }
  ]
}
```

---

## Schedules API

### GET /api/schedules
List all schedules.

**Query Parameters**:
- `enabledOnly` (optional): Filter by enabled status

**Response 200**:
```json
[
  {
    "id": "880e8400-e29b-41d4-a716-446655440000",
    "name": "Daily Customer Profile",
    "description": "Profile customers table daily",
    "scheduleType": "Profiling",
    "targetId": "abc123",
    "cronExpression": "0 2 * * *",
    "isEnabled": true,
    "nextRunAt": "2026-01-17T02:00:00Z",
    "lastRunAt": "2026-01-16T02:00:00Z",
    "createdAt": "2026-01-15T10:00:00Z"
  }
]
```

### POST /api/schedules
Create a new schedule.

**Request Body**:
```json
{
  "name": "Daily Customer Profile",
  "description": "Profile customers table daily",
  "scheduleType": "Profiling",
  "targetId": "abc123",
  "cronExpression": "0 2 * * *",
  "isEnabled": true
}
```

**Response 201**: Created schedule object.

### PUT /api/schedules/{id}
Update a schedule.

**Request Body**: Same as POST.

**Response 200**: Updated schedule object.

### DELETE /api/schedules/{id}
Delete a schedule.

**Response 204**: No content.

### POST /api/schedules/{id}/execute
Trigger a schedule immediately.

**Response 200**:
```json
{
  "id": "990e8400-e29b-41d4-a716-446655440000",
  "scheduleId": "880e8400-e29b-41d4-a716-446655440000",
  "startedAt": "2026-01-16T04:10:00Z",
  "completedAt": "2026-01-16T04:15:00Z",
  "status": "Completed",
  "message": "Successfully profiled Customers table"
}
```

---

## Fabric API

### GET /api/fabric/workspaces
List Fabric workspaces.

**Response 200**:
```json
[
  {
    "id": "abc123",
    "displayName": "Sales Analytics",
    "description": "Sales team workspace",
    "type": "Workspace"
  }
]
```

### GET /api/fabric/workspaces/{workspaceId}/items
List items in a workspace.

**Response 200**:
```json
[
  {
    "id": "lakehouse-1",
    "displayName": "SalesLakehouse",
    "type": "Lakehouse",
    "description": "Sales data lakehouse"
  }
]
```

### GET /api/fabric/workspaces/{workspaceId}/lakehouses/{lakehouseId}/tables/{tableName}/schema
Get table schema.

**Response 200**:
```json
{
  "name": "Customers",
  "columns": [
    {
      "name": "CustomerId",
      "dataType": "int",
      "isNullable": false
    },
    {
      "name": "Email",
      "dataType": "string",
      "isNullable": true
    }
  ]
}
```

---

## Error Responses

All error responses follow this format:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "The workspaceId field is required.",
  "traceId": "00-abc123-def456-00"
}
```

### Common Status Codes
- `200 OK` - Success
- `201 Created` - Resource created
- `204 No Content` - Success with no body
- `400 Bad Request` - Invalid request
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Resource conflict
- `500 Internal Server Error` - Server error

## Rate Limiting

- Default: 100 requests per minute per user
- Burst: 20 requests per second
- Headers returned:
  - `X-RateLimit-Limit`: Request limit
  - `X-RateLimit-Remaining`: Remaining requests
  - `X-RateLimit-Reset`: Reset time (Unix timestamp)

## Pagination

List endpoints support pagination via query parameters:

```
GET /api/rules?page=2&pageSize=50
```

**Response Headers**:
- `X-Total-Count`: Total number of items
- `X-Page`: Current page
- `X-Page-Size`: Items per page

## CORS

Allowed origins:
- `http://localhost:5173` (development)
- `https://claritydq-prod-web.azurestaticapps.net` (production)

## Webhooks (Future)

Subscribe to quality events:
- `rule.execution.completed`
- `rule.execution.failed`
- `profile.completed`
- `schedule.execution.failed`

## SDK Support (Future)

Official SDKs:
- JavaScript/TypeScript
- Python
- .NET

## OpenAPI Specification

Interactive documentation available at:
- Development: http://localhost:5000/openapi
- Production: https://claritydq-prod-api.azurecontainerapps.io/openapi

## Support

- GitHub Issues: https://github.com/mjtpena/ClarityDQ/issues
- Email: support@claritydq.com
- Documentation: https://github.com/mjtpena/ClarityDQ/tree/main/docs
