@description('Azure region where the Storage Account will be deployed.')
param location string

@description('Storage Account name.')
param storageAccountName string

@description('Storage Queue name.')
param queueName string

var poisonQueueName = '${queueName}-poison'

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
    allowSharedKeyAccess: true
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource queueService 'Microsoft.Storage/storageAccounts/queueServices@2023-05-01' = {
  name: 'default'
  parent: storageAccount
}

resource queue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-05-01' = {
  name: queueName
  parent: queueService
}

resource poisonQueue 'Microsoft.Storage/storageAccounts/queueServices/queues@2023-05-01' = {
  name: poisonQueueName
  parent: queueService
}

output storageAccountName string = storageAccount.name
output queueName string = queue.name
output poisonQueueName string = poisonQueue.name
