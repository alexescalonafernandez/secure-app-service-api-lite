@description('Azure region where the Function host Storage Account will be deployed.')
param location string

@description('Function host Storage Account name.')
param storageAccountName string

@description('Blob container name used for Function App deployment packages.')
param deploymentContainerName string

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: false
    allowSharedKeyAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  name: 'default'
  parent: storageAccount
}

resource deploymentContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  name: deploymentContainerName
  parent: blobService
  properties: {
    publicAccess: 'None'
  }
}

resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2023-05-01' = {
  name: 'default'
  parent: storageAccount
}

resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2023-05-01' = {
  name: 'default'
  parent: storageAccount
}

var storageEndpointSuffix = environment().suffixes.storage

output storageAccountName string = storageAccount.name
output storageAccountId string = storageAccount.id
output blobServiceUri string = 'https://${storageAccount.name}.blob.${storageEndpointSuffix}'
output queueServiceUri string = 'https://${storageAccount.name}.queue.${storageEndpointSuffix}'
output tableServiceUri string = 'https://${storageAccount.name}.table.${storageEndpointSuffix}'
output deploymentContainerName string = deploymentContainer.name
output deploymentContainerUri string = 'https://${storageAccount.name}.blob.${storageEndpointSuffix}/${deploymentContainer.name}'
