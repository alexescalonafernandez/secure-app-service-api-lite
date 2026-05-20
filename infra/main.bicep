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


module storage 'modules/storage.bicep' = {
  name: 'storage'
  params: {
    location: location
    storageAccountName: storageAccountName
    queueName: queueName
  }
}

output storageAccountName string = storage.outputs.storageAccountName
output queueName string = storage.outputs.queueName
output plannedAppServicePlanName string = appServicePlanName
output plannedAppServiceName string = appServiceName
output plannedLogAnalyticsWorkspaceName string = logAnalyticsWorkspaceName
output plannedApplicationInsightsName string = applicationInsightsName
