@description('Azure region for monitoring resources.')
param location string

@description('Action Group name.')
param actionGroupName string

@description('Metric Alert name.')
param metricAlertName string

@description('Queue service resource ID to monitor.')
param queueServiceResourceId string

@description('Optional email receiver for the Action Group. Leave empty to create the Action Group without email receivers.')
param actionGroupEmail string = ''

@description('Whether the Action Group is enabled.')
param actionGroupEnabled bool = true

@description('Whether the metric alert is enabled.')
param metricAlertEnabled bool = true

@description('How often the metric alert is evaluated. QueueMessageCount is an hourly capacity metric.')
param evaluationFrequency string = 'PT1H'

@description('Time window over which the metric alert is evaluated. QueueMessageCount is an hourly capacity metric.')
param windowSize string = 'PT1H'

@description('Queue message count threshold for the backlog alert.')
param threshold int = 0

@description('Metric alert severity.')
param severity int = 3

resource actionGroup 'Microsoft.Insights/actionGroups@2022-06-01' = {
  name: actionGroupName
  location: 'global'
  properties: {
    enabled: actionGroupEnabled
    groupShortName: 'b3qalert'
    emailReceivers: empty(actionGroupEmail) ? [] : [
      {
        name: 'PrimaryEmailReceiver'
        emailAddress: actionGroupEmail
        useCommonAlertSchema: true
      }
    ]
  }
}

resource queueBacklogAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: metricAlertName
  location: 'global'
  properties: {
    description: 'Queue backlog / poison suspected. QueueMessageCount is evaluated at queue service level and is not treated as a perfect poison-queue-specific signal.'
    severity: severity
    enabled: metricAlertEnabled
    scopes: [
      queueServiceResourceId
    ]
    evaluationFrequency: evaluationFrequency
    windowSize: windowSize
    autoMitigate: true
    targetResourceRegion: location
    targetResourceType: 'Microsoft.Storage/storageAccounts/queueServices'
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          name: 'QueueMessageCountGreaterThanThreshold'
          metricNamespace: 'Microsoft.Storage/storageAccounts/queueServices'
          metricName: 'QueueMessageCount'
          timeAggregation: 'Average'
          operator: 'GreaterThan'
          threshold: threshold
          criterionType: 'StaticThresholdCriterion'
        }
      ]
    }
    actions: [
      {
        actionGroupId: actionGroup.id
      }
    ]
  }
}

output actionGroupName string = actionGroup.name
output actionGroupId string = actionGroup.id
output queueBacklogMetricAlertName string = queueBacklogAlert.name
output queueBacklogMetricAlertId string = queueBacklogAlert.id
