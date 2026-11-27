terraform {
  required_version = ">= 1.5.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
  }

  # State is kept LOCAL on purpose (terraform.tfstate next to these files, gitignored).
  # It must survive the Azure account, so do not move it into an Azure storage account.
}

provider "azurerm" {
  features {}
  subscription_id = var.subscription_id
}
