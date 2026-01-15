targetScope = 'subscription'

@description('Environment name (dev, staging, prod)')
param environment string = 'dev'

@description('Azure region for resources')
param location string = 'eastus'

@description('Name prefix for all resources')
param namePrefix string = 'claritydq'

@description('Tags to apply to all resources')
param tags object = {
  Application: 'ClarityDQ'
  Environment: environment
  ManagedBy: 'Bicep'
}

var resourceGroupName = '${namePrefix}-${environment}-rg'

// Resource Group
resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: tags
}

// Core Infrastructure
module storage 'modules/storage.bicep' = {
  scope: rg
  name: 'storage-deployment'
  params: {
    location: location
    namePrefix: namePrefix
    environment: environment
    tags: tags
  }
}

module sql 'modules/sql.bicep' = {
  scope: rg
  name: 'sql-deployment'
  params: {
    location: location
    namePrefix: namePrefix
    environment: environment
    tags: tags
  }
}

module cosmosdb 'modules/cosmosdb.bicep' = {
  scope: rg
  name: 'cosmosdb-deployment'
  params: {
    location: location
    namePrefix: namePrefix
    environment: environment
    tags: tags
  }
}

module containerApps 'modules/container-apps.bicep' = {
  scope: rg
  name: 'container-apps-deployment'
  params: {
    location: location
    namePrefix: namePrefix
    environment: environment
    tags: tags
  }
}

module staticWebApp 'modules/static-web-app.bicep' = {
  scope: rg
  name: 'static-web-app-deployment'
  params: {
    location: location
    namePrefix: namePrefix
    environment: environment
    tags: tags
  }
}

module mlWorkspace 'modules/ml-workspace.bicep' = {
  scope: rg
  name: 'ml-workspace-deployment'
  params: {
    location: location
    namePrefix: namePrefix
    environment: environment
    tags: tags
    storageAccountId: storage.outputs.storageAccountId
  }
}

// Outputs
output resourceGroupName string = rg.name
output storageAccountName string = storage.outputs.storageAccountName
output sqlServerName string = sql.outputs.sqlServerName
output cosmosDbAccountName string = cosmosdb.outputs.accountName
output containerAppsEnvironmentId string = containerApps.outputs.environmentId
output staticWebAppUrl string = staticWebApp.outputs.defaultHostname
output mlWorkspaceName string = mlWorkspace.outputs.workspaceName
