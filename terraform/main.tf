resource "azurerm_resource_group" "rg" {
  name     = "rg-${var.name_prefix}"
  location = var.location
  tags     = var.tags
}

# ---------------- API (WebApi) ----------------

resource "azurerm_service_plan" "api" {
  name                = "plan-${var.name_prefix}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = var.app_location
  os_type             = "Linux"
  sku_name            = var.app_service_sku
  tags                = var.tags
}

resource "azurerm_linux_web_app" "api" {
  name                = "${var.name_prefix}-api"
  resource_group_name = azurerm_resource_group.rg.name
  location            = var.app_location
  service_plan_id     = azurerm_service_plan.api.id
  https_only          = true
  tags                = var.tags

  site_config {
    always_on           = var.app_service_sku == "F1" || var.app_service_sku == "D1" ? false : true
    minimum_tls_version = "1.2"

    application_stack {
      dotnet_version = var.dotnet_version
    }
  }

  # Same keys the deploy-API workflow sets today. Service Bus is intentionally NOT configured:
  # with no ServiceBus__ConnectionString the API writes activity logs directly to MongoDB.
  app_settings = {
    "ConnectionStrings__MongoDb"                  = var.mongodb_connection_string
    "AuthenticationSettings__JwtSigningKeyBase64" = var.jwt_signing_key_base64
    "Resend__ApiKey"                              = var.resend_api_key
    "GuestSettings__Password"                     = var.guest_password
    "Email__From"                                 = var.email_from
    "FrontendSettings__BaseUrl"                   = "https://${azurerm_static_web_app.web.default_host_name}"
    # The new Static Web App hostname is not in appsettings.json's CORS list. Index 20 appends to it
    # without overwriting the existing entries (localhost, vercel, ...).
    "CorsSettings__AllowedOrigins__20" = "https://${azurerm_static_web_app.web.default_host_name}"
    "ASPNETCORE_ENVIRONMENT"           = "Production"
  }
}

# ---------------- Frontend (Angular SPA) ----------------

resource "azurerm_static_web_app" "web" {
  name                = "${var.name_prefix}-web"
  resource_group_name = azurerm_resource_group.rg.name
  location            = var.swa_location
  sku_tier            = "Free"
  sku_size            = "Free"
  tags                = var.tags
}

# ---------------- Cost guardrail ----------------

resource "azurerm_consumption_budget_resource_group" "budget" {
  name              = "budget-${var.name_prefix}"
  resource_group_id = azurerm_resource_group.rg.id
  amount            = var.budget_amount
  time_grain        = "Monthly"

  time_period {
    start_date = var.start_date
  }

  dynamic "notification" {
    for_each = [50, 80, 100]
    content {
      enabled        = true
      threshold      = notification.value
      operator       = "GreaterThanOrEqualTo"
      threshold_type = "Actual"
      contact_emails = var.budget_alert_emails
    }
  }
}
