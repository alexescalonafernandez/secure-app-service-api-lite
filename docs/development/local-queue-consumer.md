# Local Azure Queue consumer validation

## 1. Purpose and scope

Use this guide to validate the Azure Queue consumer locally with Azurite, Azure Storage Explorer, VS Code, and Azure Functions Core Tools.

This is a consumer-focused local validation flow. The current API producer is not part of this local end-to-end procedure. Instead, add messages directly to the local Azure Storage queue so you can validate how the Function app consumes, accepts, rejects, and retries queue messages.

The Function project is:

```powershell
src/SecureAppServiceApiLite.QueueConsumer
```

The queue trigger listens to `incoming-messages`, uses the trigger connection setting `IncomingMessagesStorage`, and the Functions host also requires `AzureWebJobsStorage`.

## 2. Prerequisites

Install these tools before starting:

- .NET 8 SDK.
- Azure Functions Core Tools v4.
- VS Code with these extensions:
  - Azure Functions.
  - Azurite.
- Azure Storage Explorer.

If the Function host fails during startup with a `System.Memory.Data` assembly error, update Azure Functions Core Tools to the latest v4 version, close all Function host terminals, and restart VS Code before trying again.

## 3. Open the correct workspace

Open VS Code directly in the Function project folder, not at the repository root:

```powershell
cd .\src\SecureAppServiceApiLite.QueueConsumer
code .
```

This matters because the existing VS Code task is named `Run Queue Consumer locally` and uses `${workspaceFolder}` as its working directory. If VS Code is opened at the repository root, the task runs from the wrong folder.

## 4. Local settings

Create `local.settings.json` in the Function project folder only:

```powershell
cd .\src\SecureAppServiceApiLite.QueueConsumer
notepad .\local.settings.json
```

Use this exact local configuration:

```json
{
  "IsEncrypted": false,
  "Values": {
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "IncomingMessagesStorage": "UseDevelopmentStorage=true"
  }
}
```

`local.settings.json` must remain local and ignored by Git. Do not commit it.

## 5. Start Azurite

In VS Code, open the Command Palette and run:

```text
Azurite: Start
```

Expected outcome: Azurite starts the local blob, queue, and table emulator services on the default local endpoints.

Azurite data is currently created inside the Function project and ignored by `.gitignore`. The ignored local artifacts include Azurite database JSON files and these folders:

- `__blobstorage__`
- `__queuestorage__`

Do not commit Azurite database files or storage folders.

## 6. Create the local queue in Azure Storage Explorer

1. Open Azure Storage Explorer.
2. Connect to the local emulator at the default Azurite ports.
3. Expand the local storage account.
4. Open **Queues**.
5. Create a queue named:

```text
incoming-messages
```

Expected outcome: the local `incoming-messages` queue exists and is empty before validation starts.

## 7. Start the Function host

In VS Code, open the Command Palette and run:

```text
Tasks: Run Task
```

Select:

```text
Run Queue Consumer locally
```

Expected outcome: Azure Functions Core Tools starts the Function host from the Function project folder. Confirm the terminal output shows that `IncomingMessagesQueueTrigger` was found and that the host started successfully.

## 8. Valid-message validation

In Azure Storage Explorer, add this raw JSON message to the `incoming-messages` queue:

```json
{
  "Id": "11111111-1111-1111-1111-111111111111",
  "Subject": "Local validation message",
  "Body": "This body is only for local validation and must not appear in logs.",
  "Priority": "normal",
  "CreatedAtUtc": "2026-06-29T12:00:00Z"
}
```

The consumer expects raw JSON because `host.json` configures queue `messageEncoding` as `none`. Do not Base64-encode the message body.

Expected outcome:

- The message is consumed and disappears from the queue.
- The application log includes safe operational metadata only:
  - `MessageId` with the message ID.
  - normalized `Priority`, for example `Normal`.
  - `Outcome=Succeeded`.
- The message `Body` value must not appear in logs.
- The raw queue payload must not appear in logs.

## 9. Invalid-message validation

Use these manual validation cases to confirm both malformed JSON and contract-validation failures are rejected safely. These procedures describe expected reproducible results; they do not assume invalid-message validation has already been completed in your local environment.

### 9.1 Invalid JSON validation

In Azure Storage Explorer, add this intentionally malformed raw JSON payload to the `incoming-messages` queue:

```json
{ "Id": "not-valid-json",
```

Expected outcome:

- The structured application log reports `FailureReason=InvalidJson` and `Outcome=Rejected`.
- The raw payload and body must not appear in the structured application log.
- The Function rethrows the failure, so the Azure Functions runtime retries the message.
- In the current local baseline, `host.json` does not override `maxDequeueCount`; therefore the runtime uses its current default retry behavior.
- After all retry attempts fail, the runtime moves the message from `incoming-messages` to `incoming-messages-poison`.
- In Azure Storage Explorer, confirm that the message is no longer in `incoming-messages` and appears in `incoming-messages-poison`.

The poison queue is Azure Functions runtime behavior. Do not describe it as created or managed by application code. The current local baseline is the default five-attempt behavior followed by poison-queue handling; revisit this section when the project later configures explicit queue resilience settings in `host.json`.

### 9.2 Invalid envelope validation

In Azure Storage Explorer, add this syntactically valid JSON payload to the `incoming-messages` queue:

```json
{
  "Id": "22222222-2222-2222-2222-222222222222",
  "Subject": "Envelope validation",
  "Body": "this-body-must-not-appear-in-the-log",
  "Priority": "Urgent",
  "CreatedAtUtc": "2026-06-29T12:00:00Z"
}
```

This JSON is valid, but `Priority=Urgent` is not accepted by the consumer.

Expected outcome:

- The structured application log reports `FailureReason=InvalidEnvelope`.
- The structured application log reports `Outcome=Rejected`.
- The raw payload and body must not appear in the structured application log.
- The rejection is rethrown, follows the same Azure Functions runtime retry behavior, and can end in `incoming-messages-poison` after all retry attempts fail.

## 10. Stop and reset

Stop the Function host from its VS Code terminal:

```powershell
Ctrl+C
```

Stop Azurite from the VS Code Command Palette:

```text
Azurite: Close
```

To reset queue contents after validation:

1. In Azure Storage Explorer, clear or delete `incoming-messages`.
2. In Azure Storage Explorer, clear or delete `incoming-messages-poison` if it was created by runtime poison-message handling.
3. Treat poison messages as potentially sensitive because they contain the original queue payload. Do not copy poison-message contents into logs, issues, pull requests, or documentation.

To reset all local emulator state:

1. Stop the Function host.
2. Stop Azurite.
3. Delete the ignored Azurite database JSON files from the Function project folder.
4. Delete the ignored `__blobstorage__` and `__queuestorage__` folders from the Function project folder.
5. Start Azurite again with `Azurite: Start`.
6. Recreate the `incoming-messages` queue in Azure Storage Explorer.

Expected outcome: the local emulator starts with clean storage state and a newly created empty queue. The `incoming-messages-poison` queue should be absent until the Azure Functions runtime creates it after failed retry attempts.

## 11. Safety and Git hygiene

- Never commit `local.settings.json`.
- Never commit Azurite local data, including database JSON files, `__blobstorage__`, or `__queuestorage__`.
- Never paste production Storage connection strings into local documentation or source code.
- Do not log message body content.
- Do not log raw queue payload content.
- Successful logs must contain only safe operational metadata: `MessageId`, `Priority`, and `Outcome=Succeeded`.
- Rejected logs must contain `FailureReason` and `Outcome=Rejected`.
- A rejected message is rethrown so the Functions host can retry it according to host behavior.
