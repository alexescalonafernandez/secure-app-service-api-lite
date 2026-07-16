# Infrastructure

## Purpose

Infrastructure baseline for Secure App Service API Lite.

## Current state

This milestone defines naming, parameters, and repeatable Azure CLI scripts.

`main.bicep` provisions the current infrastructure baseline: producer Storage Queue, observability resources, Linux App Service Plan, Linux Web App, Queue Consumer Function App, Flex Consumption plan, Function host storage, Function managed identity, and Function RBAC assignments. Application deployment, Key Vault, and GitHub Actions are intentionally out of scope for this milestone.

The `00-account-context.azcli` script includes an explicit resource group creation command that should be executed intentionally when needed.

## Files

- `main.bicep`
- `dev.bicepparam`
- `scripts/00-account-context.azcli`
- `scripts/01-build.azcli`
- `scripts/02-validate.azcli`
- `scripts/03-what-if.azcli`
- `scripts/04-deploy.azcli`
- `scripts/05-inspect.azcli`
- `scripts/99-teardown.azcli`
- `modules/function-host-storage.bicep`
- `modules/function-app.bicep`
- `modules/function-rbac.bicep`

## Execution environment

These `.azcli` scripts are written for Azure CLI executed from PowerShell on Windows.

Multi-line commands use PowerShell backticks (`` ` ``) instead of Bash backslashes (`\`).

If you run the same commands from Bash, WSL, Git Bash, or Cloud Shell, replace PowerShell line continuations with Bash-style backslashes.

## Resource group

Expected resource group:

```text
rg-b3-secure-api-dev-we-01
```

The resource group can be created from `scripts/00-account-context.azcli` after verifying the active Azure subscription.

## Script usage

Recommended order:

1. Run `scripts/00-account-context.azcli` to inspect the current Azure CLI context and optionally create the resource group.
2. Run `scripts/01-build.azcli` to compile `main.bicep` and `dev.bicepparam`.
3. Run `scripts/02-validate.azcli` to validate the deployment at resource group scope.
4. Run `scripts/03-what-if.azcli` to preview Azure changes without applying them.
5. Run `scripts/04-deploy.azcli` to deploy the infrastructure.
6. Run `scripts/05-inspect.azcli` to inspect deployed resources.
7. Run `scripts/99-teardown.azcli` to delete the resource group and avoid ongoing cost.

## Notes

- No Key Vault in V1.
- No GitHub Actions yet.
- No Azure SDK integration yet.
- No application deployment yet. The infrastructure includes the Queue Consumer Function App host only; Function code deployment remains out of scope.
- Infrastructure deployment is manual and performed through `.azcli` scripts.
- `99-teardown.azcli` deletes the full dev resource group to avoid ongoing cost.
