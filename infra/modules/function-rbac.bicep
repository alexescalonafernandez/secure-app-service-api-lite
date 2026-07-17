@description('Function App system-assigned managed identity principal ID.')
param functionPrincipalId string

@description('Function host Storage Account name.')
param hostStorageAccountName string

@description('Producer source Storage Account name.')
param sourceStorageAccountName string

@description('Built-in role definition ID for Storage Blob Data Owner.')
param storageBlobDataOwnerRoleDefinitionId string = 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'

@description('Built-in role definition ID for Storage Queue Data Reader.')
param storageQueueDataReaderRoleDefinitionId string = '19e7f393-937e-4f77-808e-94535e297925'

@description('Built-in role definition ID for Storage Queue Data Message Processor.')
param storageQueueDataMessageProcessorRoleDefinitionId string = '8a0f0c08-91a1-4084-bc3d-661d67233fed'

@description('Built-in role definition ID for Storage Queue Data Message Sender.')
param storageQueueDataMessageSenderRoleDefinitionId string = 'c6a89b2d-59bc-44d0-9896-0f6e12d7b80a'

resource hostStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: hostStorageAccountName
}

resource sourceStorageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: sourceStorageAccountName
}

resource functionHostStorageBlobDataOwnerRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(hostStorageAccount.id, functionPrincipalId, storageBlobDataOwnerRoleDefinitionId)
  scope: hostStorageAccount
  properties: {
    principalId: functionPrincipalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataOwnerRoleDefinitionId)
    principalType: 'ServicePrincipal'
  }
}

resource functionSourceQueueDataReaderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(sourceStorageAccount.id, functionPrincipalId, storageQueueDataReaderRoleDefinitionId)
  scope: sourceStorageAccount
  properties: {
    principalId: functionPrincipalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageQueueDataReaderRoleDefinitionId)
    principalType: 'ServicePrincipal'
  }
}

resource functionSourceQueueDataMessageProcessorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(sourceStorageAccount.id, functionPrincipalId, storageQueueDataMessageProcessorRoleDefinitionId)
  scope: sourceStorageAccount
  properties: {
    principalId: functionPrincipalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageQueueDataMessageProcessorRoleDefinitionId)
    principalType: 'ServicePrincipal'
  }
}

resource functionSourceQueueDataMessageSenderRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(sourceStorageAccount.id, functionPrincipalId, storageQueueDataMessageSenderRoleDefinitionId)
  scope: sourceStorageAccount
  properties: {
    principalId: functionPrincipalId
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageQueueDataMessageSenderRoleDefinitionId)
    principalType: 'ServicePrincipal'
  }
}

output functionHostStorageBlobDataOwnerRoleAssignmentName string = functionHostStorageBlobDataOwnerRoleAssignment.name
output functionSourceQueueDataReaderRoleAssignmentName string = functionSourceQueueDataReaderRoleAssignment.name
output functionSourceQueueDataMessageProcessorRoleAssignmentName string = functionSourceQueueDataMessageProcessorRoleAssignment.name
output functionSourceQueueDataMessageSenderRoleAssignmentName string = functionSourceQueueDataMessageSenderRoleAssignment.name
