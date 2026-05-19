# Infrastructure

## Purpose

B3.E1 infrastructure baseline for Secure App Service API Lite.

## Current state

This milestone defines naming, parameters, and repeatable Azure CLI scripts.

No Azure application resources are created by `main.bicep` yet. The `00-account-context.azcli` script includes an explicit resource group creation command that should be executed intentionally when needed.

## Files

- `main.bicep`
- `dev.bicepparam`
- `scripts/00-account-context.azcli`
- `scripts/01-build.azcli`
- `scripts/02-validate.azcli`
- `scripts/03-what-if.azcli`

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

## Notes

- No Key Vault in V1.
- No GitHub Actions yet.
- No Azure SDK integration yet.
- No application deployment yet.
- No Azure application resources are deployed in this baseline commit.