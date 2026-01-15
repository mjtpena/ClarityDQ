@description('Azure region for resources')
param location string

@description('Name prefix for resources')
param namePrefix string

@description('Environment name')
param environment string

@description('Resource tags')
param tags object

@description('Storage account resource ID')
param storageAccountId string

var workspaceName = '${namePrefix}-${environment}-ml'
var keyVaultName = '${namePrefix}-${environment}-kv'
var appInsightsName = '${namePrefix}-${environment}-ai'

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  tags: tags
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    accessPolicies: []
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
  }
}

resource mlWorkspace 'Microsoft.MachineLearningServices/workspaces@2024-10-01' = {
  name: workspaceName
  location: location
  tags: tags
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    friendlyName: 'ClarityDQ ML Workspace'
    storageAccount: storageAccountId
    keyVault: keyVault.id
    applicationInsights: appInsights.id
    publicNetworkAccess: 'Enabled'
  }
}

output workspaceName string = mlWorkspace.name
output workspaceId string = mlWorkspace.id
