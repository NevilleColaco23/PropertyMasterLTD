# Activity Logging with Azure Service Bus — Simple Guide (25 Jul)

## What Does This Do?

Whenever a user does something in the app (like login, create a listing, etc.), we want to **record that activity**.

Instead of the API saving it directly to the database, we now do it in 3 simple steps:
1. The **API drops a message** into an Azure Service Bus queue (think of it like dropping a letter in a postbox 📬)
2. A background app called **AccessLogWorker** picks up that message (like a postman collecting letters)
3. The **worker saves the activity** to MongoDB (the database)

This keeps the API fast — it does not wait for the database write to finish.

---

## Simple Flow

```
User does something (login, update listing, etc.)
		↓
API Controller handles the request
		↓
ActivityLoggingActionFilter fires automatically on every request
		↓
   Is Azure Service Bus configured?

   YES → API sends a message to the "app-queue" in Azure Service Bus
			  ↓
		 AccessLogWorker (running in the background) picks it up
			  ↓
		 Worker saves the activity to MongoDB
		 (Database: ListingDB → Collection: UserActivityLogs)

   NO  → API saves the activity directly to MongoDB
		 (This is the fallback for local development without Service Bus)
```

> **Why is there a fallback?**  
> So the app still works on your local machine even if you have not set up Azure Service Bus yet.

---

## Projects and Their Roles

| Project | What it does |
|---|---|
| `WebApi` | The main API. Sends activity messages to Service Bus when a user does something. |
| `testAngularAPI.Server` | A secondary API. Does the same as WebApi. |
| `Messaging.Shared` | A shared library. Holds the message shape and the publisher interface used by both APIs. |
| `AccessLogWorker` | A background service. Reads messages from Service Bus and saves them to MongoDB. |
| `classfiles/Infrastructure` | Contains the filter that automatically captures every API call. |
| `classfiles/Application` | Contains the fallback service that writes directly to MongoDB. |

---

## Key Files

### New Files Added
| File | What it does in simple terms |
|---|---|
| `Messaging.Shared/Models/UserActivityEvent.cs` | The message shape — what data gets sent (user, action, timestamp, etc.) |
| `Messaging.Shared/IServiceBusPublisher.cs` | A contract: "any publisher must have a Send method" |
| `Messaging.Shared/ServiceBusOptions.cs` | Holds config values: connection string and queue name |
| `AccessLogWorker/Worker.cs` | Listens to the Service Bus queue, gets each message, and triggers saving |
| `AccessLogWorker/Services/IAccessLogMessageProcessor.cs` | Interface for the message processing step |
| `AccessLogWorker/Services/AccessLogMessageProcessor.cs` | Converts the message into a MongoDB log entry and saves it |
| `WebApi/Messaging Queue/ServiceBusPublisher.cs` | Sends messages to Azure Service Bus from WebApi |
| `testAngularAPI.Server/Messaging/ServiceBusPublisher.cs` | Same but for the secondary API |

### Files Changed
| File | What changed |
|---|---|
| `ActivityLoggingActionFilter.cs` | Now sends to Service Bus. Falls back to direct DB write if not configured. |
| `Infrastructure.csproj` | Added a reference to `Messaging.Shared` so the filter can use shared types. |
| `WebApi/Program.cs` + `Startup.cs` | Removed RabbitMQ. Registered the new Service Bus publisher. |
| `testAngularAPI.Server/Startup.cs` + `ApiStartup.cs` | Same — removed RabbitMQ, added Service Bus publisher and logging filter. |
| `testAngularAPI.Server.csproj` | Swapped the RabbitMQ NuGet package for the Azure Service Bus package. |
| `UserActivityRepositoryMongo.cs` | Fixed a crash — if MongoDB is slow to connect on startup, the app now logs a warning instead of crashing. |

### Files Removed
| File | Why |
|---|---|
| `testAngularAPI.Server/Messaging/RabbitMqPublisher.cs` | Replaced by the new Service Bus publisher. |

---

## What Data Is in the Message?

When an activity happens, this data is packaged and sent to the queue:

```csharp
public class UserActivityEvent
{
	public int UserId { get; set; }             // Who did it
	public string Username { get; set; }        // Their username
	public int ActivityType { get; set; }       // Type of action (number, e.g. 1 = Login)
	public string? EntityType { get; set; }     // What was affected (e.g. "Listing")
	public int? EntityId { get; set; }          // Which record was affected
	public string Action { get; set; }          // Action name (e.g. "Create", "Login")
	public string? DisplayMessage { get; set; } // Human-readable description
	public string? IPAddress { get; set; }      // Where the request came from
	public DateTime Timestamp { get; set; }     // When it happened
	public Dictionary<string, string>? Metadata { get; set; } // Any extra info
}
```

---

## Azure Resources Used

| Resource | Name | What it is |
|---|---|---|
| Service Bus Namespace | `namespace-servicebus-accesslog` | The Service Bus account in Azure |
| Service Bus Queue | `app-queue` | Where messages wait until the worker picks them up |
| MongoDB Cluster | `pmcluster0.yegbwxt.mongodb.net` | The database server (MongoDB Atlas) |
| MongoDB Database | `ListingDB` | The database the worker writes to |
| MongoDB Collection | `UserActivityLogs` | Where the activity logs are stored |

---

## Configuration

### appsettings.json (safe to commit — placeholders only, no real secrets)
```json
{
  "ServiceBus": {
	"ConnectionString": "<AZURE_SERVICE_BUS_CONNECTION_STRING>",
	"AccessLogQueueName": "app-queue"
  },
  "ConnectionStrings": {
	"MongoDb": "<AZURE_MONGODB_CONNECTION_STRING>"
  }
}
```

### Running Locally — use dotnet user-secrets (never committed to git)

This stores real secrets on your machine only. Run these once per project:

```powershell
# For WebApi
cd WebApi
dotnet user-secrets set "ServiceBus:ConnectionString" "Endpoint=sb://namespace-servicebus-accesslog.servicebus.windows.net/;..."
dotnet user-secrets set "ConnectionStrings:MongoDb" "mongodb+srv://..."

# For AccessLogWorker
cd AccessLogWorker
dotnet user-secrets set "ServiceBus:ConnectionString" "Endpoint=sb://..."
dotnet user-secrets set "ConnectionStrings:MongoDb" "mongodb+srv://..."
```

> These values override the placeholders in `appsettings.json` at runtime. They are stored in a folder on your local machine — not in the project folder — so they are never accidentally committed.

### Running in Production — Azure App Settings

Add these in the Azure Portal under your App Service → **Configuration → Application Settings**:

```
ServiceBus__ConnectionString    = Endpoint=sb://namespace-servicebus-accesslog...
ServiceBus__AccessLogQueueName  = app-queue
ConnectionStrings__MongoDb      = mongodb+srv://...
```

> Azure uses double underscore `__` to separate config sections. For example:  
> `ServiceBus__ConnectionString` in Azure = `ServiceBus:ConnectionString` in your code.

---

## How the Fallback Works

The filter checks whether a real Service Bus connection string is set before trying to use it:

```csharp
var isServiceBusConfigured = !string.IsNullOrWhiteSpace(_serviceBusOptions.ConnectionString)
	&& !_serviceBusOptions.ConnectionString.StartsWith("<");

if (isServiceBusConfigured)
	// Send to Azure Service Bus → worker saves to MongoDB
else
	// Write directly to MongoDB (works without Service Bus)
```

In short:
- **Service Bus configured** → full async flow via the queue ✅
- **Service Bus not configured** → direct MongoDB write, app still works ✅

---

## What Is Still Not Migrated

These items still use the old RabbitMQ approach and have not been changed yet:

| Item | Notes |
|---|---|
| `EmailWorker` | Still uses RabbitMQ for sending emails |
| `WebApi/Messaging Queue/RabbitMqPublisher.cs` | Old file still in the project — not in use but not deleted yet |
| `Messaging.Shared/IRabbitMqPublisher.cs` | Old interface still exists |
| `Messaging.Shared/RabbitMqOptions.cs` | Old options class still exists |

---

## Secret Management — Quick Reference

| Secret | On your local machine | In Azure (production) | In the git repository |
|---|---|---|---|
| MongoDB URI | `dotnet user-secrets` | Azure App Settings | Placeholder only |
| Service Bus Key | `dotnet user-secrets` | Azure App Settings | Placeholder only |

**No real credentials are ever committed to the repository.**
