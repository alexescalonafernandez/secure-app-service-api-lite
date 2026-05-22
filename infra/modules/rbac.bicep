@description('Principal ID that will receive the Storage Queue data-plane role assignment.')
param principalId string

@description('Storage Account name used to generate a deterministic role assignment name.')
param storageAccountName string

@description('Built-in role definition ID for Storage Queue Data Contributor.')
param storageQueueDataContributorRoleDefinitionId string = '974c5e8b-45b9-4653-ba55-5f855dd0fb88'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' existing = {
  name: storageAccountName
}

resource storageQueueDataContributorRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, principalId, storageQueueDataContributorRoleDefinitionId)
  scope: storageAccount
  properties: {
    principalId: principalId
    roleDefinitionId: subscriptionResourceId(
      'Microsoft.Authorization/roleDefinitions',
      storageQueueDataContributorRoleDefinitionId
    )
    principalType: 'ServicePrincipal'
  }
}

output storageQueueDataContributorRoleAssignmentName string = storageQueueDataContributorRoleAssignment.name
