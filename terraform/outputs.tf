output "api_url" {
  value = "https://${azurerm_linux_web_app.api.default_hostname}"
}

output "api_app_name" {
  description = "Use as app-name in the deploy-API workflow."
  value       = azurerm_linux_web_app.api.name
}

output "frontend_url" {
  value = "https://${azurerm_static_web_app.web.default_host_name}"
}

output "static_web_app_api_token" {
  description = "Deployment token for the Angular workflow (GitHub secret). Run: terraform output -raw static_web_app_api_token"
  value       = azurerm_static_web_app.web.api_key
  sensitive   = true
}
