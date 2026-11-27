variable "subscription_id" {
  description = "Azure subscription id to deploy into."
  type        = string
}

variable "location" {
  description = "Azure region for the resource group and API. Pick the one closest to your MongoDB Atlas cluster."
  type        = string
  default     = "francecentral"
}

variable "swa_location" {
  description = "Static Web Apps are only available in a few regions (westus2, centralus, eastus2, westeurope, eastasia)."
  type        = string
  default     = "westeurope"
}

variable "name_prefix" {
  description = "Short prefix for resource names. Web app names must be globally unique."
  type        = string
  default     = "propertymaster"
}

variable "dotnet_version" {
  description = "Runtime for the API. WebApi currently targets net6.0 (end of support); move to 8.0 when the project is upgraded."
  type        = string
  default     = "6.0"
}

variable "app_service_sku" {
  description = "F1 is free (no always-on, cold starts). Use B1 (~USD 13/month) to avoid cold starts."
  type        = string
  default     = "F1"
}

variable "budget_amount" {
  description = "Monthly budget in USD for the resource group. Alerts only, it does not stop spending."
  type        = number
  default     = 5
}

variable "budget_alert_emails" {
  description = "Email addresses that receive budget alerts."
  type        = list(string)
}

variable "start_date" {
  description = "First day of the budget period, must be the first of a month (YYYY-MM-01T00:00:00Z)."
  type        = string
  default     = "2026-12-01T00:00:00Z"
}

# ---- App settings for the API (secrets: pass via terraform.tfvars or TF_VAR_*, never commit) ----

variable "mongodb_connection_string" {
  type      = string
  sensitive = true
}

variable "jwt_signing_key_base64" {
  type      = string
  sensitive = true
}

variable "resend_api_key" {
  type      = string
  sensitive = true
  default   = ""
}

variable "guest_password" {
  type      = string
  sensitive = true
  default   = ""
}

variable "email_from" {
  type    = string
  default = "onboarding@resend.dev"
}

variable "tags" {
  type = map(string)
  default = {
    project    = "property-master"
    managed_by = "terraform"
  }
}

variable "app_location" {
  description = "Region for the App Service plan and API. Separate from `location` because free-tier (F1) quota is per region and many regions have a quota of 0 on new subscriptions."
  type        = string
  default     = "belgiumcentral"
}
