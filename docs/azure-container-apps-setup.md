# Azure Container Apps — One-Time Setup Guide

This guide walks you through the **one-time** Azure infrastructure setup needed before GitHub Actions can automatically deploy the workers.

You only run these commands **once**. After that, every push to `Azure_web_App_Deployment` deploys automatically.

---

## Prerequisites

- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) installed
- Logged in: `az login`
- Azure subscription active

---

## Step 1 — Register the Container Apps provider

```bash
az provider register --namespace Microsoft.App
az provider register --namespace Microsoft.OperationalInsights
```

> Wait ~1 minute for registration to complete before continuing.

---

## Step 2 — Create the Container Apps Environment

The **Environment** is the shared network boundary where both workers will live.

```bash
az containerapp env create --name propertymaster-env --resource-group rg-propertymaster-dev --location francecentral
```

---

## Step 3 — Create the EmailWorker Container App

```bash

C:\Windows\System32>az containerapp create --name propertymaster-emailworker --resource-group rg-propertymaster-dev --environment propertymaster-env --image ghcr.io/nevillecolaco23/propertymaster-emailworker:latest --cpu 0.25 --memory 0.5Gi --min-replicas 0 --max-replicas 1 --secrets mongodb-conn="<YOUR_MONGODB_CONNECTION_STRING>" resend-apikey="<YOUR_RESEND_API_KEY>" --env-vars ConnectionStrings__MongoDb=secretref:mongodb-conn Resend__ApiKey=secretref:resend-apikey AppSettings__MongoDbDatabaseName="ListingDB" Email__From="noreply@masterproperty.site" ASPNETCORE_ENVIRONMENT="Production"
\ Running ..Failed to provision revision for container app 'propertymaster-emailworker'. Error details: The following field(s) are either invalid or missing. Field 'template.containers.propertymaster-emailworker.image' is invalid with details: 'Invalid value: "ghcr.io/nevillecolaco23/propertymaster-emailworker:latest": GET https:?scope=repository%3Anevillecolaco23%2Fpropertymaster-emailworker%3Apull&service=ghcr.io: DENIED: requested access to the resource is denied';..

C:\Windows\System32>

```

> `--min-replicas 0` = **scales to zero when idle = free** ✅

---

## Step 4 — Create the AccessLogWorker Container App

```bash
az containerapp create \
  --name propertymaster-accesslogworker \
  --resource-group rg-propertymaster-dev \
  --environment propertymaster-env \
  --image ghcr.io/nevillecolaco23/propertymaster-accesslogworker:latest \
  --cpu 0.25 \
  --memory 0.5Gi \
  --min-replicas 0 \
  --max-replicas 1 \
  --secrets \
	  mongodb-conn="<YOUR_MONGODB_CONNECTION_STRING>" \
	  rabbitmq-host="<YOUR_CLOUDAMQP_HOST>" \
	  rabbitmq-user="<YOUR_CLOUDAMQP_USERNAME>" \
	  rabbitmq-pass="<YOUR_CLOUDAMQP_PASSWORD>" \
  --env-vars \
	  ConnectionStrings__MongoDb=secretref:mongodb-conn \
	  RabbitMq__Host=secretref:rabbitmq-host \
	  RabbitMq__Username=secretref:rabbitmq-user \
	  RabbitMq__Password=secretref:rabbitmq-pass \
	  RabbitMq__UseSsl="true" \
	  RabbitMq__Port="5671" \
	  AppSettings__MongoDbDatabaseName="ListingDB" \
	  ASPNETCORE_ENVIRONMENT="Production"
```

---

## Step 5 — Create a Service Principal for GitHub Actions

GitHub Actions needs permission to deploy to your Azure subscription.
This command creates a **service principal** and outputs a JSON credential.

```bash
az ad sp create-for-rbac \
  --name "propertymaster-github-deployer" \
  --role contributor \
  --scopes /subscriptions/<YOUR_SUBSCRIPTION_ID>/resourceGroups/rg-propertymaster-dev \
  --sdk-auth
```

> Replace `<YOUR_SUBSCRIPTION_ID>` with your actual subscription ID.  
> Find it with: `az account show --query id -o tsv`

The command outputs a JSON block like this:

```json
{
  "clientId": "...",
  "clientSecret": "...",
  "subscriptionId": "...",
  "tenantId": "...",
  ...
}
```

**Copy the entire JSON output.**

---

## Step 6 — Add the Secret to GitHub

1. Go to your GitHub repo: `https://github.com/NevilleColaco23/PropertyMasterLTD`
2. Click **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret**
4. Name: `AZURE_CREDENTIALS`
5. Value: paste the **entire JSON** from Step 5
6. Click **Add secret**

---

## Step 7 — Test the pipeline

Push any change to `Azure_web_App_Deployment` branch, then watch:

```
https://github.com/NevilleColaco23/PropertyMasterLTD/actions
```

You should see 5 jobs run in order:

```
Build WebApi          ──────────────────────── (image pushed to ghcr.io)
Build EmailWorker     ──┐
						├── Deploy EmailWorker     → Azure Container Apps ✅
Build AccessLogWorker ──┘
						└── Deploy AccessLogWorker → Azure Container Apps ✅
```

---

## Verify deployment in Azure Portal

1. Go to [portal.azure.com](https://portal.azure.com)
2. Navigate to **Resource Groups** → `rg-propertymaster-dev`
3. You should see:
   - `propertymaster-env` (Container Apps Environment)
   - `propertymaster-emailworker` (Container App)
   - `propertymaster-accesslogworker` (Container App)
4. Click each Container App → **Log stream** to see live logs

---

## Update secrets later (if connection strings change)

```bash
# Update a secret value
az containerapp secret set \
  --name propertymaster-emailworker \
  --resource-group rg-propertymaster-dev \
  --secrets mongodb-conn="<NEW_CONNECTION_STRING>"

# Restart the container to pick up the new secret
az containerapp revision restart \
  --name propertymaster-emailworker \
  --resource-group rg-propertymaster-dev
```

---

## Cost reminder

| Resource | Free tier allowance |
|---|---|
| Container Apps vCPU | 180,000 vCPU-seconds/month |
| Container Apps memory | 360,000 GiB-seconds/month |
| Requests | 2 million/month |
| **min-replicas: 0** | **Scales to zero = $0 when idle** ✅ |
