# JWT Security Configuration

## ⚠️ SECURITY WARNING

The JWT signing key in `appsettings.json` is a **development placeholder** and should **NEVER** be used in production!

## Production Setup

### Option 1: Environment Variables (Recommended for Docker/Cloud)

```bash
# Set environment variable
export Jwt__Key="YourActualProductionSecretKeyHere"

# Or in Docker
docker run -e Jwt__Key="YourActualProductionSecretKeyHere" your-image
```

ASP.NET Core automatically overrides appsettings.json values with environment variables.

### Option 2: User Secrets (Development Only)

```bash
# Initialize user secrets
dotnet user-secrets init

# Set JWT key
dotnet user-secrets set "Jwt:Key" "YourDevelopmentSecretKeyHere"
```

### Option 3: Azure Key Vault (Production - Highly Recommended)

```csharp
// In Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### Option 4: AWS Secrets Manager

```csharp
// Add package: AWSSDK.SecretsManager
builder.Configuration.AddSecretsManager();
```

## Configuration Priority

ASP.NET Core loads configuration in this order (later sources override earlier ones):

1. appsettings.json
2. appsettings.{Environment}.json
3. User Secrets (Development only)
4. Environment Variables
5. Command-line arguments

## Best Practices

1. **Never commit real secrets to source control**
2. **Use strong, randomly generated keys** (at least 32 characters for HS256)
3. **Rotate keys regularly** in production
4. **Use different keys** for different environments (dev, staging, production)
5. **Set `IsDevelopment: false`** in production appsettings

## Generate a Secure Key

```bash
# Generate a secure random key (64 bytes)
openssl rand -base64 64

# Or using PowerShell
[Convert]::ToBase64String([System.Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
```

## Current Configuration

The current `appsettings.json` contains:
- **Key**: Development placeholder (CHANGE IN PRODUCTION!)
- **Issuer**: testAngularAPI
- **Audience**: testAngularAPIUsers
- **ExpiryInMinutes**: 60
- **IsDevelopment**: true (Enables test token generation endpoint)

## Test Token Generation Endpoint

The `/api/auth/generate-test-token` endpoint is **only available when `IsDevelopment: true`**.

In production:
- Set `IsDevelopment: false` in appsettings.json or environment variables
- The endpoint will return 404 NotFound
- Implement proper authentication (login with credentials, OAuth, etc.)
