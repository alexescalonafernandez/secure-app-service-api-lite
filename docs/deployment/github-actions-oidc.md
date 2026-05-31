# GitHub Actions OIDC setup

## Purpose

This setup allows GitHub Actions to authenticate to Azure with OpenID Connect (OIDC) and deploy to the existing Azure App Service without storing Azure client secrets in GitHub.

## Identity model

The application uses two separate identities with different responsibilities:

- **Runtime identity:** the Azure App Service system-assigned Managed Identity is used by the deployed application to access the Azure Storage Queue through RBAC.
- **Deployment identity:** the GitHub Actions OIDC identity is used by the deployment pipeline to authenticate to Azure and deploy the application to App Service. The OIDC token is trusted through a Microsoft Entra App Registration, its associated Service Principal, and a federated credential.

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
- creates or reuses the Microsoft Entra App Registration;
- creates or reuses the associated Service Principal;
- creates or validates the federated credential;
- creates or reuses the `Contributor` RBAC assignment at Web App scope; and
- prints the values needed for the manual GitHub Environment setup.

The script is designed to be rerunnable-safe and idempotent. Existing resources are reused, and an existing federated credential must match the expected configuration.

## What the validation script checks

[`infra/scripts/07-validate-github-oidc.ps1`](../../infra/scripts/07-validate-github-oidc.ps1) verifies that:

- the Web App exists;
- the App Registration exists;
- the Service Principal exists;
- the federated credential exists;
- the federated credential subject matches;
- the federated credential issuer matches;
- the federated credential audience contains `api://AzureADTokenExchange`; and
- the `Contributor` role assignment exists at Web App scope.

## Federated credential subject

The federated credential trusts this GitHub Actions subject:

```text
repo:alexescalonafernandez/secure-app-service-api-lite:environment:dev
```

The GitHub Actions deployment job must therefore use:

```yaml
environment: dev
```

The subject is GitHub Environment-based. A branch-based subject such as `ref:refs/heads/main` would not match when the workflow uses a GitHub Environment-based OIDC subject. Azure Login will fail if the token subject does not match the configured federated credential subject.

## Azure RBAC scope

The deployment identity receives the `Contributor` role. Its role assignment is scoped only to the Web App resource:

```text
/subscriptions/<subscription-id>/resourceGroups/rg-b3-secure-api-dev-we-01/providers/Microsoft.Web/sites/app-b3-secure-api-dev-we-01
```

The role assignment is intentionally not scoped to the full resource group. Web App scope follows least privilege more closely for the current deployment goal.

## Manual GitHub Environment setup

1. Open the GitHub repository.
2. Go to **Settings**.
3. Go to **Environments**.
4. Create a new environment named `dev` if it does not already exist.
5. Configure the required environment secrets.
6. Configure the required environment variables.

## Required GitHub Environment secrets

Configure these secrets in the `dev` GitHub Environment:

- `AZURE_CLIENT_ID`
- `AZURE_TENANT_ID`
- `AZURE_SUBSCRIPTION_ID`

Use the values printed by `infra/scripts/06-bootstrap-github-oidc.ps1`. Do not commit the printed values to the repository.

## Required GitHub Environment variables

Configure these variables in the `dev` GitHub Environment:

| Variable | Value |
| --- | --- |
| `AZURE_WEBAPP_NAME` | `app-b3-secure-api-dev-we-01` |
| `DOTNET_VERSION` | `8.0.x` |
| `PROJECT_PATH` | `src/SecureAppServiceApiLite.Api/SecureAppServiceApiLite.Api.csproj` |

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
- **Insufficient permissions:** use an Azure identity that can create or reuse the App Registration and Service Principal, configure federated credentials, and create the Web App-scoped role assignment.
- **Federated credential subject mismatch:** ensure the deployment job uses `environment: dev`. The expected subject is `repo:alexescalonafernandez/secure-app-service-api-lite:environment:dev`.
- **RBAC propagation delay:** if authentication succeeds but deployment authorization fails immediately after bootstrapping, wait for Azure RBAC propagation and retry.

## Security notes

- Do not use publish profiles.
- Do not use Azure client secrets.
- Do not commit script output values to the repository.
- Keep deployment RBAC scoped to the Web App resource.
- Keep the deployment identity and runtime identity separate.

## What is not automated yet

- GitHub Environment creation is manual.
- GitHub Environment secrets and variables are configured manually.
- Workflow creation is not part of this documentation task.
- Application deployment is not yet implemented in this task.

## Next step

**B3.E3.2 - GitHub Actions build and test workflow**
