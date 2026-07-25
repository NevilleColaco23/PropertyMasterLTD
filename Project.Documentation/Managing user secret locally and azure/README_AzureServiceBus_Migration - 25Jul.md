# AccessLogWorker — Azure Service Bus Migration Summary

## What Was Built

A fully event-driven activity logging system using **Azure Service Bus** as the message broker.  
When a user performs any action in the app, instead of writing directly to MongoDB, the API publishes  
a message to Azure Service Bus. The **AccessLogWorker** (a background worker service) picks it up  
and saves it to MongoDB Atlas.

---

## Architecture

```
User Action (login, create, update, delete...)
		↓
Controller Action  [ActivityLog] attribute
		↓
ActivityLoggingActionFilter  (Infrastructure/Filters)
		↓
   Is ServiceBus configured?
   ┌─── YES ──────────────────────────────────────────────────────┐
   │   IServiceBusPublisher.SendAsync<UserActivityEvent>()        │
   │                  ↓                                           │
   │   Azure Service Bus Queue: "app-queue"                       │
   │                  ↓                                           │
   │   AccessLogWorker (BackgroundService)                        │
   │   Worker.OnMessageReceivedAsync()                            │
   │                  ↓                                           │
   │   AccessLogMessageProcessor.ProcessAsync()                   │
   │   Maps UserActivityEvent → UserActivityLog                   │
   │                  ↓                                           │
   │   IUserActivityRepository.LogActivityAsync()                 │
   │                  ↓                                           │
   │   MongoDB Atlas → ListingDB → UserActivityLogs               │
   └──────────────────────────────────────────────────────────────┘
   └─── NO (Service Bus not configured — local dev fallback) ─────┐
		UserActivityService.LogActivityAsync()                    │
		→ Saves directly to MongoDB Atlas                         │
   └──────────────────────────────────────────────────────────────┘
```

---

## Projects Involved

| Project | Role |
|---|---|
| `WebApi` | Publishes `UserActivityEvent` to Azure Service Bus |
| `testAngularAPI.Server` | Same — publishes activity events (secondary API) |
| `Messaging.Shared` | Shared contracts: `UserActivityEvent`, `IServiceBusPublisher`, `ServiceBusOptions` |
| `AccessLogWorker` | Consumes messages from Service Bus, saves to MongoDB |
| `classfiles/Infrastructure` | `ActivityLoggingActionFilter` — intercepts all controller actions |
| `classfiles/Application` | `UserActivityService` — fallback direct DB write |

---

## Key Files Created / Modified

### New Files
| File | Purpose |
|---|---|
| `Messaging.Shared/Models/UserActivityEvent.cs` | Message contract sent to/from Service Bus |
| `Messaging.Shared/IServiceBusPublisher.cs` | Interface: `SendAsync<T>(message, queueName)` |
| `Messaging.Shared/ServiceBusOptions.cs` | Config binding: `ConnectionString` + `AccessLogQueueName` |
| `AccessLogWorker/Worker.cs` | Listens to Service Bus, deserializes messages |
| `AccessLogWorker/Services/IAccessLogMessageProcessor.cs` | Interface for processing messages |
| `AccessLogWorker/Services/AccessLogMessageProcessor.cs` | Maps event → domain entity → MongoDB |
| `WebApi/Messaging Queue/ServiceBusPublisher.cs` | Sends JSON messages to Service Bus |
| `testAngularAPI.Server/Messaging/ServiceBusPublisher.cs` | Same for secondary API project |

### Modified Files
| File | What Changed |
|---|---|
| `classfiles/Infrastructure/Filters/ActivityLoggingActionFilter.cs` | Inject `IServiceBusPublisher`, publish to queue instead of direct DB write. Falls back to direct write if Service Bus not configured |
| `classfiles/Infrastructure/Infrastructure.csproj` | Added `Messaging.Shared` project reference |
| `WebApi/Program.cs` + `Startup.cs` | Removed RabbitMQ, registered `IServiceBusPublisher` |
| `testAngularAPI.Server/Startup.cs` + `ApiStartup.cs` | Removed RabbitMQ, registered filter + publisher |
| `testAngularAPI.Server/testAngularAPI.Server.csproj` | Replaced `RabbitMQ.Client` with `Azure.Messaging.ServiceBus` |
| `classfiles/Infrastructure/.../UserActivityRepositoryMongo.cs` | Wrapped `CreateIndexes()` in try-catch — no longer crashes app on MongoDB timeout |

### Deleted Files
| File | Reason |
|---|---|
| `testAngularAPI.Server/Messaging/RabbitMqPublisher.cs` | Replaced by ServiceBusPublisher |

---

## Message Contract

```csharp
// Messaging.Shared/Models/UserActivityEvent.cs
public class UserActivityEvent
{
	public int UserId { get; set; }
	public string Username { get; set; }
	public int ActivityType { get; set; }   // int to avoid enum serialization issues
	public string? EntityType { get; set; }
	public int? EntityId { get; set; }
	public string Action { get; set; }
	public string? DisplayMessage { get; set; }
	public string? IPAddress { get; set; }
	public DateTime Timestamp { get; set; }
	public Dictionary<string, string>? Metadata { get; set; }
}
```

---

## Azure Resources

| Resource | Name | Purpose |
|---|---|---|
| Service Bus Namespace | `namespace-servicebus-accesslog` | Message broker |
| Service Bus Queue | `app-queue` | Holds activity events |
| MongoDB Atlas Cluster | `pmcluster0.yegbwxt.mongodb.net` | Stores UserActivityLogs |
| MongoDB Database | `ListingDB` | Used by AccessLogWorker |
| MongoDB Collection | `UserActivityLogs` | Final destination for activity logs |

---

## Configuration

### appsettings.json (all projects — placeholder only, safe for git)
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

### Local Development — dotnet user-secrets (never committed to git)
```powershell
# WebApi
cd WebApi
dotnet user-secrets set "ServiceBus:ConnectionString" "Endpoint=sb://namespace-servicebus-accesslog.servicebus.windows.net/;..."
dotnet user-secrets set "ConnectionStrings:MongoDb" "mongodb+srv://..."

# AccessLogWorker
cd AccessLogWorker
dotnet user-secrets set "ServiceBus:ConnectionString" "Endpoint=sb://..."
dotnet user-secrets set "ConnectionStrings:MongoDb" "mongodb+srv://..."
```

### Production — Azure App Service Environment Variables
```
ServiceBus__ConnectionString    = Endpoint=sb://namespace-servicebus-accesslog...
ServiceBus__AccessLogQueueName  = app-queue
ConnectionStrings__MongoDb      = mongodb+srv://...
```
> Note: Azure uses `__` (double underscore) which maps to `:` in config hierarchy.

---

## How the Fallback Works

The filter checks if Service Bus is configured before publishing:

```csharp
var isServiceBusConfigured = !string.IsNullOrWhiteSpace(_serviceBusOptions.ConnectionString)
	&& !_serviceBusOptions.ConnectionString.StartsWith("<");

if (isServiceBusConfigured)
	// Publish to Azure Service Bus → AccessLogWorker saves to MongoDB
else
	// Write directly to MongoDB (local dev without Service Bus)
```

This means:
- **With Service Bus configured** → full async event-driven flow ✅
- **Without Service Bus** → app still works, logs go directly to MongoDB ✅

---

## What Was NOT Migrated Yet

| Item | Status | Notes |
|---|---|---|
| `EmailWorker` | ⏳ Pending | Still uses RabbitMQ for email sending |
| `WebApi/Messaging Queue/RabbitMqPublisher.cs` | ⏳ Pending | Old file still exists, not registered but not deleted |
| `Messaging.Shared/IRabbitMqPublisher.cs` | ⏳ Pending | Old interface still exists |
| `Messaging.Shared/RabbitMqOptions.cs` | ⏳ Pending | Old options class still exists |

---

## Security

| Secret | Local | Production | Git |
|---|---|---|---|
| MongoDB URI | `dotnet user-secrets` | Azure App Settings | Placeholder `<AZURE_MONGODB_CONNECTION_STRING>` |
| Service Bus Key | `dotnet user-secrets` | Azure App Settings | Placeholder `<AZURE_SERVICE_BUS_CONNECTION_STRING>` |

**No real credentials are ever committed to the repository.**
