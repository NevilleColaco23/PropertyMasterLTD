# 🚀 GitHub Actions CI/CD Pipeline Documentation

This document details the GitHub Actions workflows configured for the **Property Master** repository. These workflows automate the build, containerization, and deployment processes across frontend, backend, and background worker microservices.

---

## 📌 Repository Overview & Architecture

| Service Component | Tech Stack | Hosting / Infrastructure | GitHub Workflow File |
| :--- | :--- | :--- | :--- |
| **Frontend UI** | Angular 21 | **Azure Static Web Apps** | `.github/workflows/deploy-angular-frontend.yml` |
| **Main Web API** | .NET 6 ASP.NET Core | **Azure App Service** (`propertymaster-api-de09`) | `.github/workflows/azure_web_app_deployment_propertymaster-api-de09.yml` |
| **Worker Microservices**<br>*(EmailWorker & AccessLogWorker)* | .NET 6 Background Services | **GitHub Container Registry (GHCR)** & **Azure Container Apps** | `.github/workflows/docker-build-push.yml` |

---

## 🛠️ Detailed Workflow Specifications

### 1. Main Web API Workflow
* **File:** `.github/workflows/azure_web_app_deployment_propertymaster-api-de09.yml`
* **Trigger:** Push to branch `Azure_web_App_Deployment` (or manual trigger via `workflow_dispatch`)
* **Primary Target:** Azure App Service (`propertymaster-api-de09`)

#### 🔐 Authentication Mechanism
This workflow uses **OpenID Connect (OIDC) / Workload Identity Federation** via an Azure User-Assigned Managed Identity (`oidc-msi-87a2`). It eliminates the need to store long-lived credentials or publish profiles in GitHub Secrets.

#### ⚙️ Pipeline Steps
1. **Checkout Code:** Retrieves repository files.
2. **Azure Login:** Authenticates using OIDC federated credentials (`azure/login@v2`).
3. **Setup .NET:** Configures .NET 6 SDK.
4. **Build & Publish:** Executes `dotnet build` and `dotnet publish -c Release -o ./publish`.
5. **Deploy:** Uploads published binaries to Azure App Service (`azure/webapps-deploy@v2`).

---

### 2. Angular Frontend Workflow
* **File:** `.github/workflows/deploy-angular-frontend.yml`
* **Trigger:** Pushes or Pull Requests to target deployment branches
* **Primary Target:** Azure Static Web Apps

#### 🔐 Authentication Mechanism
Uses an Azure Static Web Apps deployment API token stored as a repository secret (`AZURE_STATIC_WEB_APPS_API_TOKEN...`).

#### ⚙️ Pipeline Steps
1. **Checkout Code:** Pulls the repository.
2. **Build & Deploy:** Executes `Azure/static-web-apps-deploy@v1` to compile the Angular single-page application and deploy compiled static assets directly to Azure's global CDN network.

*Note: Angular builds are served as static assets directly and are NOT stored as packages under GitHub Packages.*

---

### 3. Worker Microservices Workflow
* **File:** `.github/workflows/docker-build-push.yml`
* **Trigger:** Pushes to deployment branches
* **Primary Target:** GitHub Container Registry (GHCR) & Azure Container Apps

#### 🔐 Authentication Mechanism
Uses `secrets.GITHUB_TOKEN` to authenticate against GitHub Container Registry (`ghcr.io`).

#### ⚙️ Pipeline Steps
1. **Docker Build:** Builds Docker container images for:
   * `propertymaster-emailworker` (`EmailWorker/Dockerfile`)
   * `propertymaster-accesslogworker` (`AccessLogWorker/Dockerfile`)
2. **Registry Push:** Pushes built images to GitHub Container Registry (`ghcr.io/nevillecolaco23/...`).
3. **Azure Container Apps Deployment:** Triggers container revision updates on Azure.

*Note: Container images published by this workflow appear directly under **Packages** on the repository landing page.*

---

## 📂 File System Location Rules

> ⚠️ **CRITICAL REQUIREMENT:**
> All workflow `.yml` files MUST reside in the repository root at:
> ```text
> PropertyMasterLTD/
>  └── .github/
>       └── workflows/
>            ├── azure_web_app_deployment_propertymaster-api-de09.yml
>            ├── deploy-angular-frontend.yml
>            └── docker-build-push.yml
> ```

### IDE Solution Explorer Mapping
To view workflow files inside Visual Studio project folders without altering their physical disk locations:
1. Right-click the project (e.g., `WebApi` or `EmailWorker`) $ightarrow$ **Add** $ightarrow$ **Existing Item...**
2. Browse to `.github/workflows/` and select the `.yml` file.
3. Click the dropdown arrow on **Add** and select **Add As Link**.

---

## 🔒 Required GitHub Secrets & Variables

| Name | Source / Type | Used By |
| :--- | :--- | :--- |
| `AZURECLIENTID_...` | Azure OIDC Identity | `azure_web_app_deployment_propertymaster-api-de09.yml` |
| `AZURETENANTID_...` | Azure OIDC Identity | `azure_web_app_deployment_propertymaster-api-de09.yml` |
| `AZURESUBSCRIPTIONID_...` | Azure OIDC Identity | `azure_web_app_deployment_propertymaster-api-de09.yml` |
| `AZURE_STATIC_WEB_APPS_API_TOKEN_...` | Azure Static Web Apps | `deploy-angular-frontend.yml` |
| `GITHUB_TOKEN` | Automatic GitHub Token | `docker-build-push.yml` |
