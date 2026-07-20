# ADR-0004 - Operational alerting baseline

## Status

Accepted.

## Context

B3.E6 is the final operational layer for Project B3 before project closure. The system already supports the validated asynchronous path from API producer to Azure Storage Queue, Azure Function consumer, and Application Insights telemetry. B3.E5 also validated retry behavior and poison queue handling for messages that fail processing after retries.

The goal for B3.E6 is to add proactive detection when the queue processing path enters a problematic state. The initial operational concern is that messages reaching `incoming-messages-poison` indicate failed processing that should be noticed without relying on manual portal inspection.

Azure Monitor exposes the `QueueMessageCount` metric for Azure Queue Storage at the queue service level. The supported metrics reference does not show a queue-name dimension for that metric, so this design does not assume Azure Monitor can filter the metric alert specifically to `incoming-messages-poison`.

## Decision

Adopt a minimal Azure Monitor metric alert based on `QueueMessageCount > 0` for the Queue service of `stb3sapidevwe01`.

Name the alert conceptually as a queue backlog / poison-suspected alert rather than a perfect queue-specific poison alert. The alert is intended to indicate that queue messages are present when the validated processing path is expected to drain them quickly.

The first implementation should use:

- Azure Monitor Metric Alert
- Azure Monitor Action Group
- Bicep-managed infrastructure
- Low-frequency evaluation suitable for a lab/portfolio project

## Rationale

`incoming-messages` should normally drain quickly after valid messages are submitted. `incoming-messages-poison` should normally remain empty because messages should only reach it after repeated processing failures.

Any persistent queue message count after processing validation is operationally relevant for this project. It may indicate normal backlog, poison messages, or another processing issue, but each case is worth investigating before closing Project B3.

A simple metric alert keeps the implementation scope small and aligned with B3.E6. It provides a portfolio-grade operational baseline without adding custom monitoring components before the core alerting infrastructure exists.

## Tradeoff

`QueueMessageCount` is not treated as a perfect poison-queue-specific signal in this design. Because the metric alert may not be able to filter by queue name, it may alert on normal backlog in `incoming-messages` as well as messages in `incoming-messages-poison`.

This tradeoff is acceptable for B3.E6 because the goal is minimal proactive detection, not production-grade alert precision. The alert should be interpreted as queue backlog / poison suspected and followed by a runbook check of the Storage Queue state and Application Insights telemetry.

## Rejected alternatives

### Exact poison queue polling

Rejected for now because it would require custom polling logic, an additional scheduled Function or script, or another operational component. That would increase the scope beyond the minimal alerting baseline needed for B3.E6.

### Automated poison-message reprocessing

Rejected because B3.E6 is about detection and response, not automated remediation. Reprocessing requires separate safety decisions about validation, idempotency, and operator controls.

### Complex dashboard/workbook first

Rejected because alerting and runbook response are higher priority for project closure. Dashboard and workbook views can improve operations later, but they should not block the first proactive alert.

### Service Bus dead-letter queue

Rejected because migration from Azure Storage Queue to Service Bus is out of scope for B3. The current milestone should improve operations for the existing Storage Queue architecture.

## Validation approach

The alert should be validated by:

1. Deploying the alert rule and action group.
2. Inserting a controlled invalid message into `incoming-messages`.
3. Allowing the Function to reject and retry the message.
4. Confirming the message eventually appears in `incoming-messages-poison`.
5. Confirming the alert fires or appears in Azure Monitor alert history.
6. Confirming Application Insights telemetry remains sanitized.

## Future improvements

- Use a more precise signal if Azure Monitor exposes queue-name-level metrics in the future.
- Add a custom poison queue count probe if exact queue-level alerting becomes necessary.
- Add alert rules for Function failures.
- Add workbook/dashboard panels for API requests, Function failures, queue backlog, and poison handling.
- Add a poison-message reprocessing runbook or tool.
