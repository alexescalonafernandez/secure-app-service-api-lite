# ADR-0003: Function App infrastructure and identity baseline

## Status

Accepted for B3.E5.4 planning.

## Context

The project already has:

* An API App Service that produces messages into Azure Storage Queue.
* A Storage Account containing the `incoming-messages` queue.
* A Log Analytics workspace and Application Insights resource.
* A Queue Consumer implemented as an Azure Functions .NET 8 isolated worker project.
* Local validation with Azurite for valid messages, invalid JSON, invalid envelope validation, retries, and poison queue behavior.
* OpenTelemetry instrumentation in the Function project, with Azure Monitor export enabled only when `APPLICATIONINSIGHTS_CONNECTION_STRING` exists.

B3.E5 now needs Azure infrastructure for the Queue Consumer Function App.

## Decision

Use a separate Azure Function App for the Queue Consumer.

Recommended baseline:

* Function App name: `func-b3-secure-api-dev-we-01`
* Hosting model: Azure Functions Flex Consumption
* Runtime: .NET 8 isolated worker
* Operating system: Linux
* Managed identity: system-assigned identity
* Host storage account: separate Function host storage account, proposed name `stfuncb3sapidevwe01`
* Source queue storage account: existing producer storage account, currently used for `incoming-messages`
* Queue trigger connection prefix: `IncomingMessagesStorage`
* Telemetry target: existing Application Insights resource
* Deployment automation: out of scope for this ADR
* Function code deployment: out of scope for this ADR

## App settings design

Flex Consumption configures the Functions language runtime through `functionAppConfig.runtime`, not through legacy runtime app settings. For this Function App, keep `functionAppConfig.runtime.name=dotnet-isolated` and `functionAppConfig.runtime.version=8.0`.

Deployment validation found that Flex Consumption sites must not define either of these app settings:

* `FUNCTIONS_WORKER_RUNTIME`
* `FUNCTIONS_EXTENSION_VERSION`

The Function App should define these app settings:

* `APPLICATIONINSIGHTS_CONNECTION_STRING=<existing Application Insights connection string>`
* `AzureWebJobsStorage__credential=managedidentity`
* `AzureWebJobsStorage__blobServiceUri=https://<function-host-storage-account>.blob.core.windows.net`
* `AzureWebJobsStorage__queueServiceUri=https://<function-host-storage-account>.queue.core.windows.net`
* `AzureWebJobsStorage__tableServiceUri=https://<function-host-storage-account>.table.core.windows.net`
* `IncomingMessagesStorage__credential=managedidentity`
* `IncomingMessagesStorage__queueServiceUri=https://<producer-storage-account>.queue.core.windows.net`

Do not use Storage Account connection strings for the Queue trigger.

Azure-hosted identity-based connection settings explicitly set `__credential=managedidentity`. Do not add client ID or managed identity resource ID settings because the Function App uses its system-assigned managed identity.

Local development should still avoid `__credential=managedidentity` unless explicitly testing identity-based cloud connections locally.

## Identity and RBAC design

The Queue Consumer Function App must have its own system-assigned managed identity.

Do not reuse the API App Service identity.

The Function App identity needs access to two storage concerns:

### 1. Function host storage

Scope: Function host Storage Account.

Required baseline role:

* `Storage Blob Data Owner`

Reason: Azure Functions host storage uses blobs for runtime coordination and host data when `AzureWebJobsStorage` is identity-based.

Do not add Table permissions in the first implementation. Diagnostic table events can be revisited later if startup diagnostics require it.

### 2. Source queue storage

Scope: producer Storage Account that contains `incoming-messages`.

Required roles:

* `Storage Queue Data Reader`
* `Storage Queue Data Message Processor`
* `Storage Queue Data Message Sender`

Reason: the Function queue trigger must be able to read, process, update, and delete queue messages according to Azure Functions queue trigger behavior.

Initial least-privilege validation showed that `Storage Queue Data Reader` and `Storage Queue Data Message Processor` are sufficient for normal trigger processing. Azure retry and poison-message validation then showed that poison queue handling also requires enqueue permission: after retry exhaustion, the Functions runtime copies failed messages to the poison queue. The design explicitly provisions `incoming-messages-poison` and grants `Storage Queue Data Message Sender` to the Function managed identity on the source storage account. This avoids granting the broader `Storage Queue Data Contributor` role while still allowing the Functions runtime to copy failed messages to the poison queue.

Use storage-account scope for the first implementation to support the source queue and runtime poison queue behavior. More granular scoping can be evaluated later.

## Separation of identities

Keep three identities conceptually separate:

1. API runtime identity: enqueues messages.
2. Function runtime identity: consumes messages.
3. GitHub Actions deployment identity: deploys application code later.

Do not grant resource group Contributor to the runtime identities.

## Bicep implications for the next slice

The next Bicep slice should add:

* a new Function host Storage Account module or parameterized storage module usage;
* a new Function App module;
* a new Flex Consumption hosting plan or equivalent required resource;
* system-assigned identity on the Function App;
* identity-based app settings for `AzureWebJobsStorage` and `IncomingMessagesStorage`;
* role assignments for host storage and source queue storage;
* outputs for Function App name, default hostname, principal ID, host storage name, and role assignment names.

The Bicep implementation must not modify queue processing logic or deployment workflows.

## Validation plan for the future Bicep slice

Before deployment:

* run Bicep build;
* run Bicep validate;
* run what-if.

After deployment:

* confirm Function App exists;
* confirm system-assigned identity exists;
* confirm app settings are present without storage connection strings;
* confirm host storage exists;
* confirm RBAC assignments exist;
* do not require successful function execution until code deployment is implemented in a later slice.

## Risks and tradeoffs

* Flex Consumption has a different deployment model from classic App Service plans.
* Flex Consumption currently does not support deployment slots.
* Azure Functions host storage identity requires correct RBAC; missing permissions may cause startup issues.
* Queue trigger identity permissions must be validated in Azure, not only locally.
* Application Insights connection string is not treated as a secret in this project, but it should still not be duplicated unnecessarily.
* Separate host storage increases resource count slightly but keeps Function runtime concerns separated from producer queue storage.

## Deferred

* Function code deployment.
* GitHub Actions deployment workflow.
* Retry tuning.
* Poison queue processing.
* Alerts and dashboards.
* Key Vault.
* Private networking.
* User-assigned managed identity.
* Deployment slots.
* More granular queue-level RBAC scoping.
* Diagnostic table storage permissions unless required by troubleshooting.

## Done criteria

This ADR is complete when:

* the infrastructure design is documented;
* no code or Bicep has changed;
* identity separation is explicit;
* app settings are defined;
* RBAC requirements are defined;
* deferred items are clearly listed.
