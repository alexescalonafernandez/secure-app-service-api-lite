# Secure App Service API Lite

## Project goal
Secure App Service API Lite is **Project B3** of the Azure Projects + AI Delivery Lab roadmap.
Its goal is to build a lightweight, AZ-204-oriented ASP.NET Core Minimal API that can evolve from local development to Azure-hosted production patterns.

## Current project status
**Project B3 - Secure App Service API Lite: completed.**

## Milestone status
- **B3.E0 - Project foundation + local Minimal API**: complete.
- **B3.E1 - Azure infrastructure skeleton with Bicep**: complete.
- **B3.E2 - Managed Identity + RBAC + real Azure Storage Queue integration path**: complete.
- **B3.E3 - GitHub Actions deployment with OIDC**: complete.
- **B3.E4 - Operational hardening and observability validation**: complete.
- **B3.E5 - Queue consumer and resilience baseline**: complete.
- **B3.E6 - Operational alerts and dashboard baseline**: complete.

B3.E6 completed the project by adding the minimal operational alerts baseline for queue backlog / poison-suspected conditions. See [B3.E6 milestone documentation](docs/milestones/B3.E6-operational-alerts-dashboard-baseline.md), the [Project B3 closeout summary](docs/milestones/B3-project-closeout-summary.md), and the [poison message response runbook](docs/operations/poison-message-runbook.md).

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
- Azure Storage Queue producer path.
- Azure Function queue consumer baseline.
- Application Insights troubleshooting queries for API and Function validation.
- Azure Monitor queue backlog / poison-suspected alert baseline.

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
The following remain intentionally out of scope after Project B3:
- Automated poison-message reprocessing.
- Production-grade dashboards/workbooks.
- Key Vault integration.
- Production networking hardening (private endpoints, custom domains, advanced network controls).

---

### AI delivery guideline
Issues are useful for traceability, but AI implementation prompts should be **self-contained, explicit, and verifiable** instead of relying on the model to read a specific issue comment.
