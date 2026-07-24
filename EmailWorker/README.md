# Email System with Resend Integration

This document explains how the EmailWorker project is configured to send emails using Resend API, deployed as a containerised service on **Microsoft Azure**.

## Architecture Overview

The email system uses a queue-based architecture with MongoDB:

1. **WebApi** - Queues emails to MongoDB
2. **EmailWorker** - Background worker that processes emails from MongoDB using Resend API

## Components

### 1. EmailWorker Project (Background Service)

Located in: `EmailWorker/`

**Key Files:**
- `ResendEmailSender.cs` - Implements IEmailSender using Resend API
- `Worker.cs` - Background service that polls MongoDB and sends emails
- `EmailOutboxMessage.cs` - Email message data model
- `Program.cs` - Worker service configuration

**Configuration (`appsettings.json`):**
```json
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://mongo:EcMPWsskZQhygpalucMlhpEwbxaapZFL@gondola.proxy.rlwy.net:31267"
  },
  "AppSettings": {
    "MongoDbDatabaseName": "ListingDB"
  },
  "Resend": {
    "ApiKey": "re_9WEmbhnZ_QFeCfA1MgmSuULRkPsYHVTQd"
  },
  "Email": {
    "From": "onboarding@resend.dev"
  }
}
```

### 2. WebApi Project (Email Queueing)

Located in: `classfiles/Infrastructure/Services/EmailQueueService.cs`

**How to Queue Emails:**
```csharp
// Inject IEmailQueueService in your service
private readonly IEmailQueueService _emailQueueService;

public YourService(IEmailQueueService emailQueueService)
{
    _emailQueueService = emailQueueService;
}

// Queue an email
await _emailQueueService.QueueEmailAsync(
    to: "user@example.com",
    subject: "Welcome!",
    bodyHtml: "<h1>Welcome to our app!</h1>",
    type: "activation" // optional, defaults to "activation"
);
```

## MongoDB Collection

The system uses MongoDB collection: `EmailOutbox`

**Document Structure:**
```json
{
  "_id": 1,
  "type": "activation",
  "to": "user@example.com",
  "subject": "Activate Your Account",
  "bodyHtml": "<html>...</html>",
  "status": 0, // 0=Pending, 1=Processing, 2=Sent, 3=Failed
  "attempts": 0,
  "nextRunAtUtc": "2024-01-01T00:00:00Z",
  "lockedUntilUtc": null,
  "lastError": null,
  "createdAtUtc": "2024-01-01T00:00:00Z",
  "sentAtUtc": null
}
```

## How It Works

1. **Queue Email**: Your WebApi calls `IEmailQueueService.QueueEmailAsync()` which inserts a document into MongoDB
2. **Worker Polls**: The EmailWorker background service polls MongoDB every 2 seconds for pending emails
3. **Claim & Lock**: Worker claims an email by setting status to "Processing" and locks it for 2 minutes
4. **Send Email**: Worker sends email via Resend API using `ResendEmailSender`
5. **Update Status**: On success, sets status to "Sent". On failure, retries with exponential backoff (up to 5 attempts)

## Retry Logic

- **Max Attempts**: 5
- **Backoff**: Exponential (2^attempts minutes, max 60 minutes)
- **Lock Duration**: 2 minutes (prevents duplicate processing if worker crashes)
- **Final Status**: After 5 failed attempts, status is set to "Failed"

## Running the EmailWorker

### Development
```bash
cd EmailWorker
dotnet run
```

### Azure Deployment

1. Build the Docker image:
   ```bash
   docker build -f Dockerfile -t propertymaster-emailworker .
   ```
2. Push to Azure Container Registry:
   ```bash
   az acr build --registry <your-registry> --image propertymaster-emailworker .
   ```
3. Deploy to **Azure Container Apps** or **Azure App Service (Linux container)**
4. Add Application Settings (environment variables):
   ```
   RESEND_API_KEY=your_actual_resend_api_key
   EMAIL_FROM=your_verified_domain@example.com
   ConnectionStrings__MongoDb=your_mongodb_connection_string
   ```
5. Azure will start the .NET Worker Service container automatically

## Configuration for Production

### Resend API Key

1. Go to [Resend Dashboard](https://resend.com/api-keys)
2. Create a new API key
3. Update `appsettings.json` or use environment variable `RESEND_API_KEY`

### Email From Address

For testing:
- Use `onboarding@resend.dev` (Resend's test sender)

For production:
- Verify your domain in Resend
- Use `noreply@yourdomain.com`

### MongoDB Connection

Update `ConnectionStrings:MongoDb` in `appsettings.json` or use your Azure Cosmos DB for MongoDB / Atlas connection string.

## Example: Sending Activation Email

Already implemented in `UserService.cs`:

```csharp
public async Task<(SignUpResult result, SignUpResultData? data)> SignUp(...)
{
    // ... user creation code ...

    // Queue activation email
    try
    {
        var activationLink = $"https://yourapp.com/activate?userId={userId}&token=placeholder";
        var htmlBody = $@"
            <html>
            <body>
                <h2>Welcome to Property Master!</h2>
                <p>Hi {username},</p>
                <p>Thank you for signing up. Please activate your account by clicking the link below:</p>
                <p><a href='{activationLink}'>Activate Account</a></p>
                <p>If you did not sign up for this account, please ignore this email.</p>
                <p>Best regards,<br/>Property Master Team</p>
            </body>
            </html>";

        await _emailQueueService.QueueEmailAsync(email, "Activate Your Account", htmlBody, "activation");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to queue activation email for user {UserId}", userId);
        // Don't fail the signup if email queueing fails
    }

    return (SignUpResult.Success, ...);
}
```

## Monitoring

Check EmailWorker logs to monitor email sending:
```
EmailWorker started.
Sent outbox email 1 -> user@example.com
Failed outbox email 2 attempt 1/5
```

Query MongoDB to check email status:
```javascript
db.EmailOutbox.find({ status: 3 }) // Find failed emails
db.EmailOutbox.find({ status: 2 }) // Find sent emails
db.EmailOutbox.find({ status: 0 }) // Find pending emails
```

## Troubleshooting

### Emails not sending?

1. **Check EmailWorker is running**: Look for "EmailWorker started." in logs
2. **Check MongoDB connection**: Verify connection string is correct
3. **Check Resend API key**: Make sure it's valid and not rate-limited
4. **Check email queue**: Query MongoDB EmailOutbox collection
5. **Check from address**: Make sure it's verified in Resend (or use onboarding@resend.dev for testing)

### Common Issues

1. **"Missing Resend:ApiKey"**: Add the API key to appsettings.json or environment variable
2. **"Missing Email:From"**: Add the from address to appsettings.json
3. **"Resend did not return a message id"**: Check Resend API key and from address validity
4. **Emails stuck in Processing**: Worker may have crashed - they will auto-retry after lock expires (2 min)

## Next Steps

1. **Deploy EmailWorker to Azure** as a separate container service
2. **Update activation link** to your actual frontend URL
3. **Create email templates** for different email types (welcome, password reset, etc.)
4. **Verify your domain** in Resend for production emails
5. **Add monitoring/alerts** for failed emails

## Security Notes

- **Never commit API keys**: Use environment variables in production
- **Use verified domains**: Don't use `onboarding@resend.dev` in production
- **Rate limiting**: Resend has rate limits - monitor your usage
- **Email validation**: Always validate email addresses before queueing
