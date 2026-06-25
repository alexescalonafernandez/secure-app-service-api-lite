# ADR-0002 - Queue consumer and resilience baseline

## Status

Accepted

## Context

B3.E5 introduces the queue consumer and resilience baseline for Secure App Service API Lite. The existing producer path is:

```text
Client
  -> Azure App Service API
  -> Azure Storage Queue: incoming-messages
```

The API currently serializes a `QueuedMessage` JSON envelope into `incoming-messages`. The current API contract contains `Id`, `Subject`, `Body`, `Priority`, and `CreatedAtUtc`.

This milestone is a minimum reliability baseline, not a complete messaging platform. The decisions below describe the intended consumer architecture, identity boundaries, telemetry expectations, and planned resilience settings. They do not claim that the Function, RBAC, telemetry, retry, poison-message, or deployment behavior has already been implemented or validated.

## Decision

The consumer will be a separate Azure Functions .NET 8 isolated worker that consumes messages from `incoming-messages`.

The API remains the producer. The Function becomes the consumer. The consumer will not run as a long-lived worker inside the existing App Service API.

A future project named `SecureAppServiceApiLite.Contracts` will hold the shared `QueuedMessage` contract. The API and Function will reference that shared project. During that extraction, the serialized contract shape must remain unchanged.

The Function must never log the `Body` property.

## Architecture

The planned architecture is:

```text
Client
  -> Azure App Service API
  -> Azure Storage Queue: incoming-messages
  -> Azure Functions .NET 8 isolated worker
  -> sanitized telemetry
```

This separates the synchronous API request path from asynchronous message processing. The API owns request validation and enqueueing. The Function owns dequeueing, deserialization, validation of queue content, processing outcomes, retry behavior, poison-message observation, and sanitized telemetry.

No Function project, infrastructure resource, workflow, script, test, or configuration is introduced by this ADR.

## Queue message contract

The shared message envelope remains:

```text
Id
Subject
Body
Priority
CreatedAtUtc
```

The future `SecureAppServiceApiLite.Contracts` project will contain the shared `QueuedMessage` type so the API producer and Function consumer use one contract definition.

The serialized contract shape must remain unchanged during extraction to avoid breaking messages already produced by the API. The consumer may log operational metadata such as message identifiers and outcomes, but it must never log `Body`.

## Queue encoding decision

The producer currently sends raw JSON to `incoming-messages`.

The Function will initially receive queue content as `string`, then explicitly deserialize and validate the message. The planned queue binding configuration will use raw-message handling through `messageEncoding: none`.

This is intentional. Malformed JSON inserted into the queue must be observable and testable so retry and poison-message behavior can be validated. The implementation must not hide malformed content behind implicit binding deserialization.

## Identity and RBAC model

The identities are separate:

```text
GitHub Actions OIDC identity
  -> deploys workloads.

App Service system-assigned Managed Identity
  -> enqueues messages into incoming-messages.

Function App system-assigned Managed Identity
  -> consumes messages at runtime.
```

Deployment identity and runtime identities are separate. Deployment permissions must stay resource-scoped.

Exact runtime RBAC assignments for the producer queue, Function host storage, telemetry, and related Azure resources will be validated later during B3.E5.4.

No storage keys, connection strings, secrets, or publish profiles must be added to source control or GitHub.

## Function host storage

The Function App will use a separate Storage Account for Function host storage. It must not reuse the producer queue storage account for all Function host responsibilities.

Exact naming, Bicep implementation, identity-based configuration, and required RBAC will be determined and validated in B3.E5.4.

## Telemetry model

The Function will emit sanitized telemetry to the existing Application Insights resource.

Only operational metadata such as `MessageId`, `Priority`, outcome, and trigger metadata may be logged. Message body, secrets, connection strings, and sensitive content must never be logged.

Exact package and startup configuration will be implemented later in B3.E5.2 and B3.E5.3.

## Resilience baseline

The planned resilience baseline is subject to Azure validation:

- Low concurrency suitable for a lab.
- Batch size of 1.
- Retry visibility timeout of 30 seconds.
- Maximum dequeue count of 3 before poison handling.
- Poison queue expected name: `incoming-messages-poison`.

These settings and the resulting behavior are planned decisions, not yet validated evidence. Retry and poison-message behavior must be tested in Azure before being treated as proven.

## Validation scenarios

Planned validation scenarios are:

1. Valid API message
   - API -> `incoming-messages` -> Function -> sanitized successful telemetry.
2. Invalid API request
   - API returns HTTP 400 and no message reaches the queue.
3. Malformed JSON inserted directly into `incoming-messages`
   - Function cannot deserialize -> retry behavior -> poison handling.
4. Telemetry safety
   - Function telemetry includes `MessageId`, `Priority`, and outcome, but never includes `Body`.

## Consequences

Positive:

- Clear producer/consumer separation.
- Real asynchronous processing practice.
- Explicit identity boundaries.
- Controlled retry and poison-message learning path.
- Better operational evidence.

Trade-offs:

- New Function App and host storage resource add infrastructure complexity and possible cost.
- More RBAC design and validation are required.
- No idempotency store or real business processing exists yet.
- Retry and poison behavior must be tested in Azure, not assumed locally.

## Deferred concerns

The following concerns are deferred:

- Real business processing action.
- Database or external integrations.
- Idempotency store.
- Alert rules and dashboards.
- Advanced observability.
- Queue scaling tuning.
- Multiple consumers.
- Service Bus or other messaging products.
- Deployment slots, private endpoints, containers, and Kubernetes.
