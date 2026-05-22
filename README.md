# Secure App Service API Lite

## Project goal
Secure App Service API Lite is **Project B3** of the Azure Projects + AI Delivery Lab roadmap.
Its goal is to build a lightweight, AZ-204-oriented ASP.NET Core Minimal API that can evolve from local development to Azure-hosted production patterns.

## Current milestone
**B3.E1 - Azure infrastructure skeleton with Bicep (complete)**

## Milestone status
- **B3.E0 - Project foundation + local Minimal API**: complete.
- **B3.E1 - Azure infrastructure skeleton with Bicep**: complete.

B3.E1 adds Bicep-based Azure infrastructure for Storage Queue, Application Insights, Log Analytics, Linux App Service Plan, Linux Web App, and a system-assigned Managed Identity. Deployment remains manual through `infra/scripts/*.azcli`, while application deployment and RBAC assignment are intentionally deferred.

## Current implemented scope
At this stage, the repository includes:
- ASP.NET Core Minimal API on .NET 8.
- `GET /health` endpoint.
- `POST /api/messages` endpoint.
- `CreateMessageRequest` and `CreateMessageResponse` contracts.
- `IMessageQueue` abstraction.
- `InMemoryMessageQueue` fake/local queue implementation.
- `QueuedMessage` model.
- FluentValidation-based request validation.
- Swagger/OpenAPI in Development.
- Basic endpoint and validator tests.

## Endpoints
- `GET /health`
  - Returns service health payload.
- `POST /api/messages`
  - Validates and accepts a message for queueing.
  - Returns HTTP `202 Accepted` with a message id and accepted status.

## Example: POST /api/messages
Request:
```json
{
  "subject": "Order update",
  "body": "Order #123 has shipped.",
  "priority": "Normal"
}
```

Successful response (`202 Accepted`):
```json
{
  "messageId": "6b865f24-b0f4-47bf-b616-a0f1ea8fbb75",
  "status": "Accepted"
}
```

## Validation rules
`POST /api/messages` enforces:
- `subject`: required, max length 120.
- `body`: required, max length 2000.
- `priority`: optional; if provided, must be one of `Low`, `Normal`, `High` (case-insensitive).
- If `priority` is omitted/blank, server defaults queued message priority to `Normal`.

## How to run locally
From repository root:
```bash
dotnet restore
dotnet run --project src/SecureAppServiceApiLite.Api
```

Default local URLs are shown in terminal output by ASP.NET Core. In Development, Swagger UI is available.

## How to run tests
From repository root:
```bash
dotnet test
```

## Still out of scope
The following are intentionally not included yet after B3.E1:
- Application deployment to Azure App Service.
- Azure SDK integration in application code.
- RBAC role assignments (including Managed Identity to Storage Queue).
- Key Vault integration.
- GitHub Actions CI/CD.
- Production networking hardening (private endpoints, custom domains, advanced network controls).

## Next milestone preview
B3.E2 - Managed Identity + RBAC + real Azure Storage Queue integration path.

---

### AI delivery guideline
Issues are useful for traceability, but AI implementation prompts should be **self-contained, explicit, and verifiable** instead of relying on the model to read a specific issue comment.
