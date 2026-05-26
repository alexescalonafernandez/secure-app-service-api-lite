@description('Azure region where App Service resources will be deployed.')
param location string

@description('Linux App Service Plan name.')
param appServicePlanName string

@description('Linux Web App name.')
param appServiceName string

@description('Storage Account name used by the API queue integration.')
param storageAccountName string

@description('Storage Queue name used by the API queue integration.')
param queueName string

@description('Application Insights connection string.')
param applicationInsightsConnectionString string

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: 'B1'
    tier: 'Basic'
    size: 'B1'
    capacity: 1
  }
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: appServiceName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: false
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      appSettings: [
        {
          name: 'QueueOptions__StorageAccountName'
          value: storageAccountName
        }
        {
          name: 'QueueOptions__QueueName'
          value: queueName
        }
        {
          name: 'QueueOptions__Provider'
          value: 'AzureStorage'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsightsConnectionString
        }
      ]
    }
  }
}

output appServicePlanName string = appServicePlan.name
output appServiceName string = webApp.name
output appServiceDefaultHostName string = webApp.properties.defaultHostName
output appServicePrincipalId string = webApp.identity.principalId
