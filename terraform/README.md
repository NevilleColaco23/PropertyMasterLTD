# Terraform: Property Master on Azure (low cost)

Creates: resource group, App Service plan (F1 free by default) + Linux web app for the API, a free
Static Web App for the Angular app, and a monthly budget with email alerts. No Service Bus.
MongoDB stays on Atlas (not managed here). State is **local** on purpose (`terraform.tfstate`, gitignored).

## First-time setup
```powershell
az login
az account set --subscription <subscription-id>
cd terraform
copy terraform.tfvars.example terraform.tfvars   # fill in values; never commit this file
terraform init
terraform plan -out tfplan                        # review before applying
terraform apply tfplan
```

## After `apply`
1. **API URL**: `terraform output -raw api_url`. Put `<that url>/api/v1` in
   `app/src/app/environments/environment.prod.ts` (`apiUrl`).
2. **GitHub variable** `AZURE_WEBAPP_NAME` = `terraform output -raw api_app_name`.
3. **GitHub secret** `AZURE_WEBAPP_PUBLISH_PROFILE`:
   `az webapp deployment list-publishing-profiles --name <api app name> --resource-group rg-propertymaster --xml`
   (paste the whole XML).
4. **GitHub secret** `AZURE_STATIC_WEB_APPS_API_TOKEN` = `terraform output -raw static_web_app_api_token`.
5. Push to `Azure_web_App_Deployment` (this triggers production deployment, so do it deliberately).
6. Add the new frontend URL (`terraform output -raw frontend_url`) to Atlas / Google OAuth settings if used.

## Switching to another Azure account
Change `subscription_id` (and `name_prefix` if the names are taken), delete the old `terraform.tfstate`,
then repeat the steps above. Data is in Atlas, so nothing is lost.

## Cost guardrails
- Budget alerts (default USD 5/month) only notify; they do not stop spending.
- `terraform destroy` removes everything created here.
