@description('Azure region for resources')
param location string

@description('Name prefix for resources')
param namePrefix string

@description('Environment name')
param environment string

@description('Resource tags')
param tags object

var accountName = '${namePrefix}-${environment}-cosmos'

resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-11-15' = {
  name: accountName
  location: location
  tags: tags
  kind: 'GlobalDocumentDB'
  properties: {
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
    databaseAccountOfferType: 'Standard'
    enableAutomaticFailover: false
    enableMultipleWriteLocations: false
    capabilities: [
      {
        name: 'EnableGremlin'
      }
    ]
    publicNetworkAccess: 'Enabled'
  }
}

resource gremlinDatabase 'Microsoft.DocumentDB/databaseAccounts/gremlinDatabases@2024-11-15' = {
  parent: cosmosAccount
  name: 'lineage'
  properties: {
    resource: {
      id: 'lineage'
    }
    options: {
      throughput: 400
    }
  }
}

resource gremlinGraph 'Microsoft.DocumentDB/databaseAccounts/gremlinDatabases/graphs@2024-11-15' = {
  parent: gremlinDatabase
  name: 'lineagegraph'
  properties: {
    resource: {
      id: 'lineagegraph'
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
      }
      partitionKey: {
        paths: [
          '/workspaceId'
        ]
        kind: 'Hash'
      }
    }
  }
}

output accountName string = cosmosAccount.name
output endpoint string = cosmosAccount.properties.documentEndpoint
output gremlinEndpoint string = cosmosAccount.properties.gremlinEndpoint
