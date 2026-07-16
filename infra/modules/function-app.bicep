@description('Azure region where Function App resources will be deployed.')
param location string

@description('Flex Consumption plan name.')
param functionPlanName string

@description('Function App name.')
param functionAppName string

@description('Function host Storage Account name.')
param hostStorageAccountName string

@description('Function host Storage Account blob service URI.')
param hostStorageBlobServiceUri string

@description('Function host Storage Account queue service URI.')
param hostStorageQueueServiceUri string

@description('Function host Storage Account table service URI.')
param hostStorageTableServiceUri string

@description('Deployment package blob container URI.')
param deploymentContainerUri string

@description('Producer source Storage Account queue service URI.')
param sourceQueueServiceUri string

@description('Application Insights connection string.')
param applicationInsightsConnectionString string

@description('Maximum scale-out instance count for the Function App.')
param maximumInstanceCount int = 40

@description('Instance memory size in MB for the Function App.')
param instanceMemoryMB int = 512

resource functionPlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: functionPlanName
  location: location
  kind: 'functionapp'
  sku: {
    name: 'FC1'
    tier: 'FlexConsumption'
  }
  properties: {
    reserved: true
  }
}

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: functionPlan.id
    httpsOnly: true
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: deploymentContainerUri
          authentication: {
            type: 'SystemAssignedIdentity'
          }
        }
      }
      runtime: {
        name: 'dotnet-isolated'
        version: '8.0'
      }
      scaleAndConcurrency: {
        maximumInstanceCount: maximumInstanceCount
        instanceMemoryMB: instanceMemoryMB
      }
    }
    siteConfig: {
      appSettings: [
        {
          name: 'FUNCTIONS_EXTENSION_VERSION'
          value: '~4'
        }
        {
          name: 'FUNCTIONS_WORKER_RUNTIME'
          value: 'dotnet-isolated'
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: applicationInsightsConnectionString
        }
        {
          name: 'AzureWebJobsStorage__credential'
          value: 'managedidentity'
        }
        {
          name: 'AzureWebJobsStorage__blobServiceUri'
          value: hostStorageBlobServiceUri
        }
        {
          name: 'AzureWebJobsStorage__queueServiceUri'
          value: hostStorageQueueServiceUri
        }
        {
          name: 'AzureWebJobsStorage__tableServiceUri'
          value: hostStorageTableServiceUri
        }
        {
          name: 'IncomingMessagesStorage__credential'
          value: 'managedidentity'
        }
        {
          name: 'IncomingMessagesStorage__queueServiceUri'
          value: sourceQueueServiceUri
        }
      ]
    }
  }
}

output functionPlanName string = functionPlan.name
output functionAppName string = functionApp.name
output functionAppDefaultHostName string = functionApp.properties.defaultHostName
output functionAppPrincipalId string = functionApp.identity.principalId
