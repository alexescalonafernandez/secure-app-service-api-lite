# App Service Operations Runbook

## Purpose

This runbook describes how to perform basic operational validation and troubleshooting for the Secure App Service API Lite application deployed to Azure App Service.

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