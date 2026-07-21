# Application Insights KQL troubleshooting guide

This guide collects the Application Insights / KQL queries used during B3.E5 Azure end-to-end validation and retry/poison troubleshooting for this project.

The queries are intended for the deployed Azure path:

```text
Client/Postman
  -> API App Service
  -> Azure Storage Queue incoming-messages
  -> Azure Function IncomingMessagesQueueTrigger
  -> Application Insights
```

Use them when validating API-to-queue-to-Function processing, sanitized telemetry, retry behavior, poison-message handling, and storage queue RBAC issues.

## 1. Function trigger requests

Purpose: verify whether the queue-triggered Function is running.

```kusto
requests
| where timestamp > ago(2h)
| where name == "IncomingMessagesQueueTrigger"
| order by timestamp desc
| project timestamp, name, success, duration, operation_Id, itemType
```

Interpretation:

- `success == true` means the Function invocation completed successfully.
- `success == false` means the Function invocation failed and may be retried by the Azure Functions runtime.

## 2. Successful queue message processing traces

Purpose: verify that the Function processed a message successfully.

```kusto
traces
| where timestamp > ago(2h)
| where message has "Queue message processed"
   or tostring(customDimensions["OriginalFormat"]) has "Queue message processed"
| order by timestamp desc
| project timestamp, severityLevel, message, customDimensions
```

A successful trace should look like:

```text
Queue message processed. MessageId: <message-id>; Priority: Normal; Outcome: Succeeded
```

`MessageId` can be correlated with the API response from `POST /api/messages`.

## 3. Sanitized successful processing validation

Purpose: confirm that message body/payload content is not logged.

```kusto
traces
| where timestamp > ago(2h)
| where message has "Queue message processed"
   or tostring(customDimensions["OriginalFormat"]) has "Queue message processed"
| where message has "Body"
   or message has "payload"
   or tostring(customDimensions) has "Body"
   or tostring(customDimensions) has "payload"
| order by timestamp desc
```

Expected result:

```text
0 records
```

Logs should include operational metadata such as `MessageId`, `Priority`, and `Outcome`, but never the message body.

## 4. Rejected queue message traces

Purpose: verify that invalid messages are rejected with sanitized telemetry.

```kusto
traces
| where timestamp > ago(2h)
| where message has "Queue message rejected"
   or tostring(customDimensions["OriginalFormat"]) has "Queue message rejected"
| order by timestamp desc
| project timestamp, severityLevel, message, customDimensions
```

Expected trace example:

```text
Queue message rejected. FailureReason: InvalidEnvelope; Outcome: Rejected
```

This validates the Function processing baseline and keeps the payload out of logs.

## 5. Retry attempt summary

Purpose: summarize retry behavior for the Function trigger.

```kusto
requests
| where timestamp > ago(2h)
| where name == "IncomingMessagesQueueTrigger"
| summarize
    attempts = count(),
    failedAttempts = countif(success == false),
    firstAttempt = min(timestamp),
    lastAttempt = max(timestamp)
  by name
```

Interpretation:

- Azure Functions retries failed queue messages.
- After retry exhaustion, the runtime attempts to move the failed message to `<queue-name>-poison`.
- In this project, the poison queue is `incoming-messages-poison`.

## 6. Retry attempts one by one

Purpose: inspect each Function invocation during retry testing.

```kusto
requests
| where timestamp > ago(2h)
| where name == "IncomingMessagesQueueTrigger"
| order by timestamp asc
| project timestamp, name, success, duration, operation_Id
```

Multiple `success == false` records indicate repeated processing failures before poison handling.

## 7. Poison queue / RBAC troubleshooting

Purpose: detect whether the Functions runtime failed to move a message to the poison queue because of RBAC.

```kusto
traces
| where timestamp > ago(2h)
| where message has_any (
    "poison",
    "incoming-messages-poison",
    "AuthorizationPermissionMismatch",
    "403",
    "Forbidden",
    "Storage"
)
or tostring(customDimensions) has_any (
    "poison",
    "incoming-messages-poison",
    "AuthorizationPermissionMismatch",
    "403",
    "Forbidden",
    "Storage"
)
| order by timestamp desc
| project timestamp, severityLevel, message, customDimensions
```

Also check captured exceptions:

```kusto
exceptions
| where timestamp > ago(2h)
| where outerMessage has_any (
    "poison",
    "incoming-messages-poison",
    "AuthorizationPermissionMismatch",
    "403",
    "Forbidden",
    "Storage"
)
or tostring(details) has_any (
    "poison",
    "incoming-messages-poison",
    "AuthorizationPermissionMismatch",
    "403",
    "Forbidden",
    "Storage"
)
| order by timestamp desc
| project timestamp, type, outerMessage, problemId, details
```

During B3.E5 validation, the project observed:

```text
Azure.RequestFailedException
Status: 403
ErrorCode: AuthorizationPermissionMismatch
QueueProcessor.CopyMessageToPoisonQueueAsync
QueueClient.SendMessageAsync
```

Interpretation:

```text
The Function runtime had enough permission to read/process/delete normal queue messages,
but not enough permission to enqueue failed messages into the poison queue.
```

Fix:

- Explicitly provision `incoming-messages-poison`.
- Grant `Storage Queue Data Message Sender` to the Function managed identity.
- Keep `Storage Queue Data Reader` and `Storage Queue Data Message Processor`.
- Avoid broad `Storage Queue Data Contributor` unless future requirements justify it.

## 8. Payload leak detection during negative tests

Purpose: confirm that invalid message body content is not leaked.

Use this query when testing with a body such as `this-body-must-not-appear-in-logs`.

```kusto
traces
| where timestamp > ago(2h)
| where message has "this-body-must-not-appear-in-logs"
   or tostring(customDimensions) has "this-body-must-not-appear-in-logs"
   or message has "Body"
   or message has "payload"
   or tostring(customDimensions) has "Body"
   or tostring(customDimensions) has "payload"
| order by timestamp desc
```

Expected result:

```text
0 records
```

## 9. Validation checklist

Happy path:

- [ ] `POST /api/messages` returns `Accepted`.
- [ ] `incoming-messages` becomes empty after processing.
- [ ] `requests` contains `IncomingMessagesQueueTrigger` with `success == true`.
- [ ] `traces` contains `Queue message processed`.
- [ ] `MessageId` in trace matches API response.
- [ ] No `Body` or payload appears in traces.

Negative path:

- [ ] Invalid message is inserted directly into `incoming-messages`.
- [ ] `traces` contains `Queue message rejected`.
- [ ] `FailureReason == InvalidEnvelope`.
- [ ] `requests` contains failed Function invocations.
- [ ] Message eventually appears in `incoming-messages-poison`.
- [ ] No body/payload appears in telemetry.

## Related runbook

For operational response when the queue backlog / poison-suspected alert fires, see:

[Poison message response runbook](poison-message-runbook.md)
