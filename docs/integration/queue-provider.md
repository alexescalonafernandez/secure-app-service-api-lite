# Queue provider integration path

## Purpose

The API uses `IMessageQueue` as its queue abstraction. This allows the application to switch between local in-memory queueing and Azure Storage Queue without changing endpoint behavior.

## Provider model

- `IMessageQueue` is the abstraction used by the API.
- `InMemoryMessageQueue` is the local and default implementation.
- `AzureStorageMessageQueue` is the Azure implementation.
- `MessagingServiceCollectionExtensions.AddMessageQueue(...)` registers the implementation selected by configuration.

## Local/default behavior

`appsettings.json` sets `QueueOptions:Provider` to `InMemory`. Local development therefore does not require Azure credentials, and the existing `/api/messages` behavior remains testable locally.

## Azure App Service behavior

Azure App Service app settings override the default configuration. The Azure environment uses:

```text
QueueOptions__Provider = AzureStorage
QueueOptions__StorageAccountName = stb3sapidevwe01
QueueOptions__QueueName = incoming-messages
```

With this configuration, the application registers `AzureStorageMessageQueue`.

## Configuration keys

The queue provider configuration keys are:

- `QueueOptions:Provider`
- `QueueOptions:StorageAccountName`
- `QueueOptions:QueueName`

Environment variables map nested configuration keys by replacing the colon with a double underscore:

- `QueueOptions__Provider`
- `QueueOptions__StorageAccountName`
- `QueueOptions__QueueName`

## Managed Identity and RBAC

The Linux Web App has a system-assigned Managed Identity. Bicep assigns the `Storage Queue Data Contributor` role to that identity at Storage Account scope. RBAC propagation may take time after deployment, so access might not be available immediately.

## DefaultAzureCredential behavior

In Azure App Service, `DefaultAzureCredential` uses the Managed Identity. Locally, it may use developer credentials if `AzureStorage` is explicitly configured. The local default remains `InMemory` so normal development does not require an Azure login.

## Why no connection strings

The project intentionally avoids Storage connection strings. Identity-based access is preferred because it reduces secret management needs.

## Why no Key Vault yet

The current design does not require a real secret. Key Vault is deferred until there is a concrete secret or configuration management need.

## Testing strategy

Provider registration tests verify `InMemory` versus `AzureStorage` selection. These tests do not call Azure or send messages to Azure Storage Queue. Existing endpoint tests continue to run with the in-memory provider.

## End-to-end validation

Full Azure end-to-end validation requires application deployment. Application deployment is intentionally deferred to B3.E3, which will introduce GitHub Actions deployment with OIDC. At that point, `/api/messages` can be validated against the real Azure Storage Queue.

## Known limitations

- No deployment automation exists yet.
- No live Azure queue enqueue validation has been performed yet.
- RBAC propagation delay can affect first runs.
- No retry or backoff policy has been added yet.
- No poison message handling or queue processor exists yet.

## Next step

B3.E3 - GitHub Actions deployment with OIDC
