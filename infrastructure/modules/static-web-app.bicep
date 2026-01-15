@description('Azure region for resources')
param location string

@description('Name prefix for resources')
param namePrefix string

@description('Environment name')
param environment string

@description('Resource tags')
param tags object

var staticWebAppName = '${namePrefix}-${environment}-web'

resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: staticWebAppName
  location: location
  tags: tags
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {
    repositoryUrl: 'https://github.com/mjtpena/ClarityDQ'
    branch: 'main'
    buildProperties: {
      appLocation: '/src/frontend'
      apiLocation: ''
      outputLocation: 'dist'
    }
  }
}

output staticWebAppId string = staticWebApp.id
output defaultHostname string = staticWebApp.properties.defaultHostname
output deploymentToken string = staticWebApp.listSecrets().properties.apiKey
