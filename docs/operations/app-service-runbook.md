# App Service Operations Runbook

## Purpose

This runbook covers basic operational validation and first-response troubleshooting for the deployed Secure App Service API Lite API. For the current minimal API scope, Application Insights is the primary operational evidence source for request success, request failure, result codes, and durations.

## Current environment

- Environment: dev
- Resource group: rg-b3-secure-api-dev-we-01
- App Service: app-b3-secure-api-dev-we-01
- Storage Account: stb3sapidevwe01
- Queue: incoming-messages
- Deployment workflow: Deploy Web App
- Runtime identity: App Service system-assigned Managed Identity
- Deployment identity: GitHub Actions OIDC identity

## Resource map

```text
GitHub Actions
  -> Azure App Service
  -> Managed Identity
  -> Azure Storage Queue
  -> Application Insights / Log Analytics
```

## Health check

Run the health endpoint from PowerShell:

```powershell
Invoke-RestMethod `
  -Method Get `
  -Uri "https://app-b3-secure-api-dev-we-01.azurewebsites.net/health"
```

Expected behavior:

- HTTP 200.
- Healthy response payload.
- Corresponding request visible in Application Insights.

## Valid message submission check

Run a valid message submission from PowerShell:

```powershell
$body = @{
  subject = "Operational validation"
  body = "Valid message submission check"
} | ConvertTo-Json

Invoke-RestMethod `
  -Method Post `
  -Uri "https://app-b3-secure-api-dev-we-01.azurewebsites.net/api/messages" `
  -ContentType "application/json" `
  -Body $body
```

Expected behavior:

- HTTP 202 Accepted.
- Request visible in Application Insights.
- Message expected in `incoming-messages`.

## Invalid request validation check

Run an invalid message submission with an empty subject from PowerShell:

```powershell
$body = @{
  subject = ""
  body = "Invalid message submission check"
} | ConvertTo-Json

Invoke-WebRequest `
  -Method Post `
  -Uri "https://app-b3-secure-api-dev-we-01.azurewebsites.net/api/messages" `
  -ContentType "application/json" `
  -Body $body `
  -SkipHttpErrorCheck
```

Expected behavior:

- HTTP 400 Bad Request.
- Corresponding request visible in Application Insights.

## Application Insights checks

For Application Insights queries used during API, queue, Function, retry, and poison-message validation, see [`application-insights-kql-cheatsheet.md`](./application-insights-kql-cheatsheet.md).

Application Insights request telemetry depends on these prerequisites:

- `APPLICATIONINSIGHTS_CONNECTION_STRING` configured in App Service through Bicep.
- `Microsoft.ApplicationInsights.AspNetCore` referenced by the API.
- `builder.Services.AddApplicationInsightsTelemetry()` registered only when a connection string is available.

Use this Azure Portal navigation:

```text
Application Insights
  -> Transaction search
  -> Select a recent time range
  -> Locate GET /health and POST /api/messages
  -> Review result code and duration
```

Validated Application Insights evidence:

- `GET /health` recorded.
- Valid `POST /api/messages` recorded.
- Invalid `POST /api/messages` recorded with HTTP 400.

## Queue verification

Use the Azure Portal as the primary queue verification method:

```text
Storage Account
  -> stb3sapidevwe01
  -> Queue service
  -> Queues
  -> incoming-messages
  -> Messages
```

A valid message was previously confirmed manually in the Azure Portal under Storage Account `stb3sapidevwe01`, Queue `incoming-messages`.

Optional Azure CLI queue peek example:

```powershell
az storage message peek `
  --account-name stb3sapidevwe01 `
  --queue-name incoming-messages `
  --auth-mode login
```

The CLI command uses the local Azure CLI identity. A local permission failure does not prove that the App Service Managed Identity failed.

## App Service Log Stream

Use this Azure Portal navigation:

```text
App Service
  -> Monitoring
  -> Log stream
```

Observed behavior:

- Runtime Log Stream connected successfully.
- Platform Log Stream connected successfully.
- No logs appeared during normal requests.

Interpretation:

- This is expected for the current minimal API.
- No explicit `ILogger` application events exist.
- App Service file-system logging is not enabled in B3.E4.
- Application Insights remains the primary operational source.

## Identity and permission caveats

Different operational actions use different identities:

```text
Local Azure CLI identity
  -> Used by manual az commands.

GitHub Actions OIDC deployment identity
  -> Used to deploy the application to the Web App.

App Service system-assigned Managed Identity
  -> Used at runtime to enqueue messages in Azure Storage Queue.
```

A permission result from one identity must not be treated as evidence about the other identities.

## Basic troubleshooting flow

1. Did the Deploy Web App workflow complete successfully?
2. Is the App Service running?
3. Does `GET /health` return HTTP 200?
4. Does valid `POST /api/messages` return HTTP 202?
5. Does invalid `POST /api/messages` return HTTP 400?
6. Are the corresponding requests visible in Application Insights?
7. Is the valid message visible in `incoming-messages`?
8. If queue delivery fails, verify `QueueOptions` configuration, App Service Managed Identity, Storage Queue Data Contributor role assignment, and queue existence.
9. Use Log Stream for startup or platform symptoms, understanding that normal application requests do not currently emit explicit logs.

## Evidence checklist

- [x] Deployment workflow completed successfully.
- [x] `/health` returned HTTP 200 in Azure.
- [x] Valid `/api/messages` returned HTTP 202 in Azure.
- [x] Invalid `/api/messages` returned HTTP 400 in Azure.
- [x] Successful requests were found in Application Insights.
- [x] Invalid request was found in Application Insights.
- [x] Queue message was confirmed in Azure Portal.
- [x] Runtime and Platform Log Stream connection behavior was reviewed.
- [x] Identity and permission caveats were documented.
- [x] Basic troubleshooting flow was documented.

## Current limitations

- Deployment remains manual.
- No alert rules or dashboards exist.
- API-side explicit structured application logging remains minimal.
- Queue retry behavior currently relies on Azure Functions runtime defaults; no custom retry tuning is configured.
- Poison-message handling exists through `incoming-messages-poison`, but no automated poison-message reprocessing workflow exists.
- Local Azure CLI queue inspection may require Storage Queue data-plane permissions.
