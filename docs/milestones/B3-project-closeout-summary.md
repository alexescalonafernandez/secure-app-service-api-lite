# Project B3 closeout summary - Secure App Service API Lite

## Status

Completed.

## Project objective

Project B3 built a lightweight AZ-204-oriented .NET/Azure project that evolved from a local ASP.NET Core Minimal API into an Azure-hosted, asynchronous, observable, identity-based cloud application baseline.

## Final architecture

```text
Client/Postman
  -> Azure App Service API
  -> Azure Storage Queue incoming-messages
  -> Azure Function Queue Trigger
  -> Application Insights
  -> Azure Monitor Alert for queue backlog / poison suspected
```

The final queue topology also includes the poison queue:

```text
incoming-messages-poison
```

## Milestones completed

- B3.E0 - Project foundation + local Minimal API.
- B3.E1 - Azure infrastructure skeleton with Bicep.
- B3.E2 - Managed Identity + RBAC + real Azure Storage Queue integration path.
- B3.E3 - GitHub Actions deployment with OIDC.
- B3.E4 - Operational hardening and observability validation.
- B3.E5 - Queue consumer and resilience baseline.
- B3.E6 - Operational alerts and dashboard baseline.

## Azure services used

- Azure App Service.
- Azure Functions .NET 8 isolated.
- Azure Storage Queue.
- Azure Storage Account.
- Managed Identity.
- Azure RBAC.
- Application Insights.
- Log Analytics.
- Azure Monitor Metric Alert.
- Azure Monitor Action Group.
- GitHub Actions OIDC deployment.
- Bicep.

## Professional skills demonstrated

- .NET Minimal API design.
- Queue-based asynchronous processing.
- Azure Functions queue trigger.
- Managed Identity and RBAC.
- Identity-based storage access.
- Bicep modular infrastructure.
- GitHub Actions deployment with OIDC.
- Application Insights telemetry.
- KQL troubleshooting.
- Retry and poison queue validation.
- Azure Monitor alerting.
- Operational runbook documentation.
- Incident-style troubleshooting.

## Real technical findings

1. Function App Flex Consumption should configure runtime through `functionAppConfig.runtime`, not legacy `FUNCTIONS_WORKER_RUNTIME` / `FUNCTIONS_EXTENSION_VERSION` app settings.
2. Azure Functions queue poison handling required `Storage Queue Data Message Sender` in addition to reader/processor roles.
3. `QueueMessageCount` required hourly evaluation/window and is queue-service-level, not poison-queue-specific.
4. Application Insights was essential to diagnose runtime/RBAC failures.
5. Sanitized telemetry allowed operational evidence without logging message body/payload.

## Portfolio value

- It is not just a CRUD/API sample.
- It demonstrates cloud delivery, identity, deployment, observability, async processing, failure handling, and operations.
- It leaves verifiable evidence in GitHub through milestones, ADRs, runbooks, deployment configuration, and validation notes.

## Current limitations

- No automated poison-message reprocessing.
- No production-grade dashboard.
- No Key Vault/private networking.
- No multi-environment promotion.
- No exact poison queue metric alert.
- No Service Bus/Durable Functions.
- No advanced CI/CD release strategy.

## Recommended next project direction

After B3, good next directions include:

- An AZ-104/AZ-400-oriented infrastructure and deployment project.
- An AZ-305-oriented architecture project.
- A .NET + AI project after the Azure operational base is stronger.

Do not choose the next project in this document. The decision should be made after review with the mentor director.

## Transferable summary for B3 mentor director

```text
B3 completó un proyecto Azure/.NET de portfolio que evolucionó desde una Minimal API local hasta una arquitectura cloud asíncrona con App Service, Storage Queue, Azure Function, Managed Identity, RBAC, GitHub Actions OIDC, Application Insights, retry/poison handling y Azure Monitor alerting. El proyecto dejó evidencia verificable en GitHub, documentación de decisiones, runbooks operativos y validaciones reales en Azure. Durante el delivery se diagnosticaron problemas reales de configuración Flex Consumption, permisos RBAC para poison queue y restricciones de métricas Azure Monitor, lo que refuerza su valor como experiencia práctica de cloud delivery y troubleshooting.
```
