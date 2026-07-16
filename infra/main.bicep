targetScope = 'resourceGroup'

@description('Azure region where resources will be deployed.')
param location string = resourceGroup().location

@description('Deployment environment name.')
@allowed([
  'dev'
])
param environment string = 'dev'

@description('Short project code used for Azure resource naming.')
param projectCode string = 'b3sapi'

@description('Short Azure region code used for Azure resource naming.')
param locationCode string = 'we'

@description('Instance number used for Azure resource naming.')
param instance string = '01'

var resourcePrefix = 'b3-secure-api-${environment}-${locationCode}-${instance}'
var storageAccountName = 'st${projectCode}${environment}${locationCode}${instance}'
var queueName = 'incoming-messages'
var appServicePlanName = 'asp-${resourcePrefix}'
var appServiceName = 'app-${resourcePrefix}'
var logAnalyticsWorkspaceName = 'log-${resourcePrefix}'
var applicationInsightsName = 'appi-${resourcePrefix}'
var functionAppName = 'func-${resourcePrefix}'
var functionPlanName = 'fcp-${resourcePrefix}'
var functionHostStorageAccountName = 'stfunc${projectCode}${environment}${locationCode}${instance}'
var functionDeploymentContainerName = 'app-package-${functionAppName}'

module storage 'modules/storage.bicep' = {
  name: 'storage'
  params: {
    location: location
    storageAccountName: storageAccountName
    queueName: queueName
  }
}

module observability 'modules/observability.bicep' = {
  name: 'observability'
  params: {
    location: location
    logAnalyticsWorkspaceName: logAnalyticsWorkspaceName
    applicationInsightsName: applicationInsightsName
  }
}

module appService 'modules/app-service.bicep' = {
  name: 'app-service'
  params: {
    location: location
    appServicePlanName: appServicePlanName
    appServiceName: appServiceName
    storageAccountName: storage.outputs.storageAccountName
    queueName: storage.outputs.queueName
    applicationInsightsConnectionString: observability.outputs.applicationInsightsConnectionString
  }
}

module functionHostStorage 'modules/function-host-storage.bicep' = {
  name: 'function-host-storage'
  params: {
    location: location
    storageAccountName: functionHostStorageAccountName
    deploymentContainerName: functionDeploymentContainerName
  }
}

module functionApp 'modules/function-app.bicep' = {
  name: 'function-app'
  params: {
    location: location
    functionPlanName: functionPlanName
    functionAppName: functionAppName
    hostStorageAccountName: functionHostStorage.outputs.storageAccountName
    hostStorageBlobServiceUri: functionHostStorage.outputs.blobServiceUri
    hostStorageQueueServiceUri: functionHostStorage.outputs.queueServiceUri
    hostStorageTableServiceUri: functionHostStorage.outputs.tableServiceUri
    deploymentContainerUri: functionHostStorage.outputs.deploymentContainerUri
    sourceQueueServiceUri: 'https://${storage.outputs.storageAccountName}.queue.${environment().suffixes.storage}'
    applicationInsightsConnectionString: observability.outputs.applicationInsightsConnectionString
  }
}

module functionRbac 'modules/function-rbac.bicep' = {
  name: 'function-rbac'
  params: {
    functionPrincipalId: functionApp.outputs.functionAppPrincipalId
    hostStorageAccountName: functionHostStorage.outputs.storageAccountName
    sourceStorageAccountName: storage.outputs.storageAccountName
  }
}

module rbac 'modules/rbac.bicep' = {
  name: 'rbac'
  params: {
    principalId: appService.outputs.appServicePrincipalId
    storageAccountName: storage.outputs.storageAccountName
  }
}

output storageAccountName string = storage.outputs.storageAccountName
output queueName string = storage.outputs.queueName
output appServicePlanName string = appService.outputs.appServicePlanName
output appServiceName string = appService.outputs.appServiceName
output appServiceDefaultHostName string = appService.outputs.appServiceDefaultHostName
output appServicePrincipalId string = appService.outputs.appServicePrincipalId
output logAnalyticsWorkspaceName string = observability.outputs.logAnalyticsWorkspaceName
output applicationInsightsName string = observability.outputs.applicationInsightsName
output applicationInsightsConnectionString string = observability.outputs.applicationInsightsConnectionString
output storageQueueDataContributorRoleAssignmentName string = rbac.outputs.storageQueueDataContributorRoleAssignmentName
output functionHostStorageAccountName string = functionHostStorage.outputs.storageAccountName
output functionDeploymentContainerName string = functionHostStorage.outputs.deploymentContainerName
output functionPlanName string = functionApp.outputs.functionPlanName
output functionAppName string = functionApp.outputs.functionAppName
output functionAppDefaultHostName string = functionApp.outputs.functionAppDefaultHostName
output functionAppPrincipalId string = functionApp.outputs.functionAppPrincipalId
output functionHostStorageBlobDataOwnerRoleAssignmentName string = functionRbac.outputs.functionHostStorageBlobDataOwnerRoleAssignmentName
output functionSourceQueueDataReaderRoleAssignmentName string = functionRbac.outputs.functionSourceQueueDataReaderRoleAssignmentName
output functionSourceQueueDataMessageProcessorRoleAssignmentName string = functionRbac.outputs.functionSourceQueueDataMessageProcessorRoleAssignmentName
