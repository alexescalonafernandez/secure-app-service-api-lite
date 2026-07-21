# Poison message response runbook

## Purpose

Use this runbook when the B3.E6 queue backlog / poison-suspected alert fires. The goal is to confirm whether the alert represents normal queue backlog, failed messages in the poison queue, validation artifacts, or an operational issue that requires investigation.

## Alert

- Alert name: `ma-b3-secure-api-dev-we-01-queue-backlog`
- Severity: `3 - Informational`
- Metric: `QueueMessageCount`
- Threshold: `> 0`
- Affected resource pattern: `stb3sapidevwe01/default`
- Scope: Storage Queue service level
- Important limitation: `QueueMessageCount` is evaluated at the Storage Queue service level and is not guaranteed to identify only `incoming-messages-poison`.

## First interpretation

- `incoming-messages` should normally drain quickly after API messages are accepted.
- `incoming-messages-poison` should normally be empty.
- A fired alert means there are queue messages somewhere in the Queue service.
- In this project, that condition is treated as queue backlog / poison suspected until the queues and telemetry prove otherwise.

## Response checklist

1. [ ] Confirm the alert in Azure Monitor.
2. [ ] Check `incoming-messages` for normal backlog.
3. [ ] Check `incoming-messages-poison` for failed messages.
4. [ ] Review Application Insights requests for `IncomingMessagesQueueTrigger`.
5. [ ] Review Application Insights traces for rejected messages.
6. [ ] Review exceptions for Storage/RBAC/runtime failures.
7. [ ] Run the payload/body leak detection query.
8. [ ] Decide whether the messages are expected validation artifacts or require investigation.
9. [ ] Do not reprocess or delete poison messages blindly.
10. [ ] Document the incident before cleanup.

## Useful Azure CLI checks

Check that the normal queue exists:

```powershell
az storage queue exists `
  --account-name stb3sapidevwe01 `
  --name incoming-messages `
  --auth-mode login `
  --output table
```

Check that the poison queue exists:

```powershell
az storage queue exists `
  --account-name stb3sapidevwe01 `
  --name incoming-messages-poison `
  --auth-mode login `
  --output table
```

Check the queue backlog / poison-suspected alert configuration:

```powershell
az monitor metrics alert show `
  --resource-group rg-b3-secure-api-dev-we-01 `
  --name ma-b3-secure-api-dev-we-01-queue-backlog `
  --query "{name:name, enabled:enabled, severity:severity, evaluationFrequency:evaluationFrequency, windowSize:windowSize, targetResourceType:targetResourceType, description:description}" `
  --output table
```

`infra/scripts/05-inspect.azcli` already includes these checks for the deployed B3 environment.

## Application Insights investigation

Use the [Application Insights KQL troubleshooting guide](application-insights-kql-cheatsheet.md) for the detailed queries. The most relevant query categories for this runbook are:

- Function trigger requests.
- Rejected queue message traces.
- Retry attempt summaries.
- Exceptions for poison queue / RBAC troubleshooting.
- Payload leak detection.

## Decision guide

### Case 1: Only `incoming-messages` has messages

Interpret this as backlog or the Function not processing quickly enough.

Recommended checks:

- Function App state.
- Function trigger requests.
- Recent deployment status.
- App Insights exceptions.

### Case 2: `incoming-messages-poison` has messages

Interpret this as failed processing after retries.

Recommended checks:

- Rejected trace reason.
- Exception details.
- Whether this was a controlled validation message.
- Whether payload must be preserved as evidence.

### Case 3: Alert fired but queues are empty

Interpret this as metric delay or alert auto-mitigation timing.

Recommended checks:

- Alert fired time.
- Metric window.
- Alert history.
- Queue state after the hourly window.

## Cleanup guidance

- Do not delete poison messages until evidence is captured.
- For controlled validation messages, cleanup is acceptable after documenting the alert and telemetry.
- Production-style reprocessing is out of scope for B3.
- Automated reprocessing is explicitly deferred.

## Validation evidence from B3.E6.3

```text
Metric Alert:
ma-b3-secure-api-dev-we-01-queue-backlog

Severity:
3 - Informational

Fired time:
2026-07-21 13:35 Europe/Brussels

Affected resource:
stb3sapidevwe01/default

Monitor service:
Platform

Alert condition:
Fired

Value when alert fired:
2

Threshold:
0

Interpretation:
The alert fired because QueueMessageCount was 2. At validation time, incoming-messages-poison already contained 2 messages, so the fired alert was consistent with the queue backlog / poison-suspected design.
```

## Current limitations

- Alert is queue-service-level.
- Evaluation/window is hourly because `QueueMessageCount` is an hourly capacity metric.
- No email receiver is committed by default.
- No automated poison-message reprocessing.
- No dashboard/workbook yet beyond documented operational view.
