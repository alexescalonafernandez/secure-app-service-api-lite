# ADR-0001 - Start with local Minimal API and fake queue abstraction

- **Status:** Accepted

## Context
Project B3 needs an incremental start that supports rapid learning, local development, and verifiable progress before introducing Azure infrastructure complexity.

At B3.E0 time:
- Core API behavior must be demonstrable locally.
- Team needs endpoint, validation, and testing foundations.
- Azure resources, SDK dependencies, and IaC are intentionally deferred.

## Decision
Adopt a local-first implementation for B3.E0:
1. Build the service as ASP.NET Core Minimal API on .NET 8.
2. Implement health and message endpoints (`/health`, `/api/messages`).
3. Introduce `IMessageQueue` abstraction as the queue boundary.
4. Use `InMemoryMessageQueue` fake implementation for local execution.
5. Use FluentValidation for request rules.
6. Add baseline endpoint/validator tests.

## Consequences
Positive:
- Fast setup and local feedback loop.
- Clear, testable API contract early.
- Queue abstraction ready for later Azure implementation.
- Reduced early-stage cost and operational overhead.

Trade-offs:
- No durable queue behavior yet.
- No cloud identity/secrets/telemetry path yet.
- Some architecture concerns postponed to later milestones.

## Alternatives considered
1. **Start directly with Azure resources and SDKs in E0**
   - Rejected due to higher setup complexity and slower first validation cycle.
2. **Hard-code queue behavior in endpoint with no abstraction**
   - Rejected because it would increase refactoring cost when moving to Azure queues.
3. **Use a full layered architecture before first endpoint**
   - Rejected for E0 to avoid over-engineering before functional baseline proof.

---

### AI delivery guideline
Issues are useful for traceability, but AI implementation prompts should be **self-contained, explicit, and verifiable** instead of relying on the model to read a specific issue comment.
