# Email Activation System - Quick Reference

## Architecture Overview

```
┌─────────────────┐      ┌──────────────────┐      ┌─────────────────┐
│  Angular App    │────▶ │   ASP.NET API    │────▶ │    MongoDB      │
│  (Frontend)     │      │   (WebAPI)       │      │   (Database)    │
└─────────────────┘      └──────────────────┘      └─────────────────┘
                                  │
                                  ▼
                         ┌──────────────────┐
                         │    RabbitMQ      │
                         │  (Message Queue) │
                         └──────────────────┘
                                  │
                                  ▼
                         ┌──────────────────┐      ┌─────────────────┐
                         │   EmailWorker    │────▶ │   Resend API    │
                         │  (Background)    │      │ (Email Service) │
                         └──────────────────┘      └─────────────────┘
```

## Complete Flow Diagram

```mermaid
graph LR
    A[User Signup] --> B[Create User]
    B --> C[Generate Token]
    C --> D[Hash & Store Token]
    D --> E[Queue Email]
    E --> F[RabbitMQ]
    F --> G[EmailWorker]
    G --> H[Send via Resend]
    H --> I[User Receives Email]
    I --> J[Click Activation Link]
    J --> K[Validate Token]
    K --> L{Token Valid?}
    L -->|Yes| M[Set EmailConfirmed=true]
    L -->|No| N[Show Error]
    M --> O[User Can Login]
    N --> P[Resend Activation]
```

## File Locations

### Backend
- **AccountController**: `WebApi/API/V1/AccountController.cs` (Endpoints)
- **UserService**: `classfiles/Infrastructure/Authentication/Core/Services/UserService.cs` (Logic)
- **User Entity**: `classfiles/Domain/Users/Users.cs` (Model)
- **Identity Config**: `classfiles/Infrastructure/Identity/Startup.cs` (Configuration)
- **Email Queue**: `classfiles/Infrastructure/Services/EmailQueueService.cs` (RabbitMQ)
- **EmailWorker**: `EmailWorker/Worker.cs` (Consumer)

### Frontend
- **Login**: `app/src/app/core/auth/login-form/login-form.ts`
- **Signup**: `app/src/app/core/auth/create-user/create-user.ts`
- **Activation**: `app/src/app/core/auth/activate-account/activate-account.component.ts`
- **Routes**: `app/src/app/app.routes.ts`

## Quick Commands

### Backend
```bash
# Run WebAPI
dotnet run --project WebApi

# Run EmailWorker
dotnet run --project EmailWorker

# Build solution
dotnet build
```

### Frontend
```bash
# Run dev server
ng serve

# Build for production
ng build --configuration production
```

### Database
```javascript
// Check user
db.Users.findOne({ Email: "user@example.com" })

// Activate user manually
db.Users.updateOne(
  { _id: 1 },
  { $set: { EmailConfirmed: true, EmailConfirmationTokenHash: "" }}
)
```

## Environment Variables

### WebAPI (`appsettings.json`)
```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://user:pass@host:port/db"
  },
  "RabbitMq": {
    "Host": "raccoon.lmq.cloudamqp.com",
    "Port": 5672,
    "Username": "xxx",
    "Password": "xxx"
  },
  "AuthenticationSettings": {
    "JwtSigningKeyBase64": "your-secret"
  }
}
```

### EmailWorker (`appsettings.json`)
```json
{
  "RabbitMq": {
    "Host": "raccoon.lmq.cloudamqp.com"
  },
  "Resend": {
    "ApiKey": "re_xxxx"
  },
  "MongoDB": {
    "ConnectionString": "mongodb://..."
  }
}
```

### Frontend (`environment.ts`)
```typescript
export const environment = {
  apiUrl: 'https://localhost:44346/api/v1'
};
```

## Key Code Snippets

### Token Generation
```csharp
var token = Convert.ToBase64String(
    RandomNumberGenerator.GetBytes(32))
    .Replace("+", "-")
    .Replace("/", "_")
    .Replace("=", "");

var hash = Convert.ToBase64String(
    SHA256.HashData(Encoding.UTF8.GetBytes(token)));
```

### Email Confirmation Check
```csharp
options.SignIn.RequireConfirmedEmail = true;
```

### Normalized Email Setting
```csharp
NormalizedEmail = email.ToUpperInvariant();
```

## 📖 Full Documentation

**See:** [docs/EMAIL_ACTIVATION_SYSTEM.md](./docs/EMAIL_ACTIVATION_SYSTEM.md) for:
- Complete architecture
- Sequence diagrams
- Database schema
- API documentation
- Security details
- Testing procedures
- Troubleshooting guide
