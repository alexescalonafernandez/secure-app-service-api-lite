# GitHub Actions OIDC setup

## Purpose

This setup allows GitHub Actions to authenticate to Azure with OpenID Connect (OIDC) and deploy to the existing Azure App Service and Queue Consumer Azure Function App without storing Azure client secrets in GitHub.

The Function App deployment path reuses the existing OIDC app registration, service principal, and federated credential used by the API Web App deployment.

## Identity model

The application uses separate identities with different responsibilities:

- **Runtime identity:** Azure runtime managed identities are used by deployed workloads to access Azure resources through RBAC.
- **Deployment identity:** the GitHub Actions OIDC identity is used by deployment pipelines to authenticate to Azure and deploy application code. The OIDC token is trusted through a Microsoft Entra App Registration, its associated Service Principal, and a federated credential.

## Why OIDC

OIDC avoids storing long-lived deployment credentials:

- No Azure client secret is stored in GitHub.
- No publish profile is used.
- GitHub receives a short-lived OIDC token for the workflow job.
- Azure trusts that token through a federated credential.

## What the bootstrap script creates

[`infra/scripts/06-bootstrap-github-oidc.ps1`](../../infra/scripts/06-bootstrap-github-oidc.ps1) prepares the Azure-side deployment identity. It:

- checks Azure CLI availability;
- checks the active Azure login session;
- verifies that the target Web App exists;
- verifies that the target Function App exists;
- creates or reuses the Microsoft Entra App Registration;
- creates or reuses the associated Service Principal;
- creates or validates the federated credential;
- creates or reuses the `Contributor` RBAC assignment at Web App scope;
- creates or reuses the `Contributor` RBAC assignment at Function App scope; and
- prints the values needed for the manual GitHub Environment setup.

The script is designed to be rerunnable-safe and idempotent. Existing resources are reused, and an existing federated credential must match the expected configuration.

## What the validation script checks

[`infra/scripts/07-validate-github-oidc.ps1`](../../infra/scripts/07-validate-github-oidc.ps1) verifies that:

- the Web App exists;
- the Function App exists;
- the App Registration exists;
- the Service Principal exists;
- the federated credential exists;
- the federated credential subject matches;
- the federated credential issuer matches;
- the federated credential audience contains `api://AzureADTokenExchange`;
- the `Contributor` role assignment exists at Web App scope; and
- the `Contributor` role assignment exists at Function App scope.

## Federated credential subject

The federated credential trusts this GitHub Actions subject:

```text
repo:alexescalonafernandez/secure-app-service-api-lite:environment:dev
```

The GitHub Actions deployment jobs must therefore use:

```yaml
environment: dev
```

The subject is GitHub Environment-based. A branch-based subject such as `ref:refs/heads/main` would not match when the workflow uses a GitHub Environment-based OIDC subject. Azure Login will fail if the token subject does not match the configured federated credential subject.

## Azure RBAC scope

The deployment identity receives the `Contributor` role. Its role assignments are scoped only to the deployable app resources:

```text
/subscriptions/<subscription-id>/resourceGroups/rg-b3-secure-api-dev-we-01/providers/Microsoft.Web/sites/app-b3-secure-api-dev-we-01
/subscriptions/<subscription-id>/resourceGroups/rg-b3-secure-api-dev-we-01/providers/Microsoft.Web/sites/func-b3-secure-api-dev-we-01
```

The role assignments are intentionally not scoped to the full resource group. App-level scope follows least privilege more closely for the current deployment goals.

## Manual GitHub Environment setup

1. Open the GitHub repository.
2. Go to **Settings**.
3. Go to **Environments**.
4. Create a new environment named `dev` if it does not already exist.
5. Configure the required environment secrets.
6. Configure the required environment variables.

## Required GitHub Environment secrets

Configure these secrets in the `dev` GitHub Environment for both the API Web App and Function App deployment workflows:

- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`

Use the values printed by `infra/scripts/06-bootstrap-github-oidc.ps1`. Do not commit the printed values to the repository.

## Required GitHub Environment variables

Configure these variables in the `dev` GitHub Environment.

Existing API deployment variables remain:

| Variable | Value |
| --- | --- |
| `AZURE_WEBAPP_NAME` | `app-b3-secure-api-dev-we-01` |
| `PROJECT_PATH` | `src/SecureAppServiceApiLite.Api/SecureAppServiceApiLite.Api.csproj` |
| `DOTNET_VERSION` | `8.0.x` |

New Function App deployment variables are:

| Variable | Value |
| --- | --- |
| `AZURE_FUNCTIONAPP_NAME` | `func-b3-secure-api-dev-we-01` |
| `FUNCTION_PROJECT_PATH` | `src/SecureAppServiceApiLite.QueueConsumer/SecureAppServiceApiLite.QueueConsumer.csproj` |

## Deployment workflows

### API Web App

The API Web App workflow deploys the API project to the existing App Service using OIDC and the GitHub Environment variables above.

### Queue Consumer Function App

The Queue Consumer Function App workflow is manual-only through `workflow_dispatch`. It restores, builds, tests, publishes the Function project, authenticates to Azure with OIDC, and deploys the published Function code with the official Azure Functions GitHub Action.

The Function workflow deploys code only. It does not provision or update Azure infrastructure, retry settings, poison queue behavior, alerts, dashboards, or application behavior.

Flex Consumption runtime configuration is managed in Bicep, not by setting `FUNCTIONS_WORKER_RUNTIME` in the workflow. Do not configure `WEBSITE_RUN_FROM_PACKAGE` for this deployment path.

## How to run the bootstrap

Run the bootstrap script from the repository root after signing in to Azure CLI:

```bash
pwsh ./infra/scripts/06-bootstrap-github-oidc.ps1
```

## How to validate the bootstrap

Run the validation script from the repository root:

```bash
pwsh ./infra/scripts/07-validate-github-oidc.ps1
```

## Troubleshooting

- **Azure CLI is not installed:** install Azure CLI and ensure the `az` command is available in `PATH`.
- **User is not logged in to Azure CLI:** run `az login`, select the correct subscription if necessary, and rerun the script.
- **Web App is not deployed yet:** deploy the existing infrastructure first. The scripts expect `app-b3-secure-api-dev-we-01` in `rg-b3-secure-api-dev-we-01`.
- **Function App is not deployed yet:** deploy the existing infrastructure first. The scripts expect `func-b3-secure-api-dev-we-01` in `rg-b3-secure-api-dev-we-01`.
- **Insufficient permissions:** use an Azure identity that can create or reuse the App Registration and Service Principal, configure federated credentials, and create the app-scoped role assignments.
- **Federated credential subject mismatch:** ensure deployment jobs use `environment: dev`. The expected subject is `repo:alexescalonafernandez/secure-app-service-api-lite:environment:dev`.
- **RBAC propagation delay:** if authentication succeeds but deployment authorization fails immediately after bootstrapping, wait for Azure RBAC propagation and retry.

## Security notes

- Do not use publish profiles.
- Do not use Azure client secrets.
- Do not use storage connection strings for deployment authentication.
- Do not commit script output values to the repository.
- Keep deployment RBAC scoped to individual app resources.
- Keep deployment identity and runtime identities separate.
