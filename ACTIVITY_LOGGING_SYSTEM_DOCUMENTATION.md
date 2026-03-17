# Activity Logging System - Complete Documentation

## 📋 Table of Contents
1. [System Overview](#system-overview)
2. [Architecture Diagram](#architecture-diagram)
3. [Request Flow](#request-flow)
4. [Client-Side Components (Angular)](#client-side-components-angular)
5. [Server-Side Components (.NET)](#server-side-components-net)
6. [Database Layer (MongoDB)](#database-layer-mongodb)
7. [How to Add Logging to New Endpoints](#how-to-add-logging-to-new-endpoints)
8. [Examples](#examples)

---

## 🎯 System Overview

The Activity Logging System is a comprehensive, automated solution for tracking all user actions across the PropertyMaster application. It captures:

- **Who**: User ID and username
- **What**: Activity type (View, Create, Update, Delete, etc.)
- **When**: Timestamp (UTC)
- **Where**: Property context, IP address, user agent
- **How Long**: Request duration in milliseconds
- **Result**: Success/failure status
- **Details**: Rich metadata and human-readable messages

### Key Features
✅ **Automatic Logging** - No manual logging code needed in controllers  
✅ **Property Context Tracking** - Knows which property user is working on  
✅ **Client-Provided Messages** - Angular can send custom messages  
✅ **Fallback Server Messages** - Auto-generated if client doesn't provide  
✅ **Rich Metadata** - Route params, query strings, request/response data  
✅ **Performance Metrics** - Tracks request duration  
✅ **Error Tracking** - Logs failed operations with error messages  
✅ **MongoDB Storage** - Optimized with indexes for fast queries  

---

## 🏗️ Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         ANGULAR CLIENT                               │
├─────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  User Action (e.g., "Select Property", "View Dashboard")     │  │
│  └─────────────────────────────┬────────────────────────────────┘  │
│                                 │                                    │
│  ┌──────────────────────────────▼───────────────────────────────┐  │
│  │  Activity Message Service                                     │  │
│  │  setMessage("Selected 'Sunset Villa' property")              │  │
│  └─────────────────────────────┬────────────────────────────────┘  │
│                                 │                                    │
│  ┌──────────────────────────────▼───────────────────────────────┐  │
│  │  HTTP Request                                                 │  │
│  │  GET /api/v1/property/all                                     │  │
│  └─────────────────────────────┬────────────────────────────────┘  │
│                                 │                                    │
│  ┌──────────────────────────────▼───────────────────────────────┐  │
│  │  HTTP Interceptors (Run in Order)                            │  │
│  │  1. authInterceptor      → Add JWT token                     │  │
│  │  2. propertyContextInterceptor → Add X-Selected-Property: 3  │  │
│  │  3. activityMessageInterceptor → Add X-Activity-Message      │  │
│  └─────────────────────────────┬────────────────────────────────┘  │
│                                 │                                    │
└─────────────────────────────────┼────────────────────────────────────┘
                                  │
                                  │ HTTP Request with Headers:
                                  │ - Authorization: Bearer eyJ...
                                  │ - X-Selected-Property: 3
                                  │ - X-Activity-Message: "Selected..."
                                  │
┌─────────────────────────────────▼────────────────────────────────────┐
│                         .NET API SERVER                               │
├─────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌──────────────────────────────────────────────────────────────┐  │
│  │  JWT Authentication Middleware                                │  │
│  │  - Validates token                                            │  │
│  │  - Sets HttpContext.User with claims                          │  │
│  └─────────────────────────────┬────────────────────────────────┘  │
│                                 │                                    │
│  ┌──────────────────────────────▼───────────────────────────────┐  │
│  │  Controller Action Invoked                                    │  │
│  │  [LogView("Properties")]                                      │  │
│  │  public async Task<ActionResult> GetAll()                     │  │
│  └─────────────────────────────┬────────────────────────────────┘  │
│                                 │                                    │
│  ┌──────────────────────────────▼───────────────────────────────┐  │
│  │  ActivityLoggingActionFilter (Intercepts Request)            │  │
│  │                                                               │  │
│  │  BEFORE ACTION:                                               │  │
│  │  - Check if [ActivityLog] attribute exists                   │  │
│  │  - Start stopwatch                                            │  │
│  │                                                               │  │
│  │  EXECUTE ACTION → Controller method runs → Returns result    │  │
│  │                                                               │  │
│  │  AFTER ACTION:                                                │  │
│  │  - Stop stopwatch (measure duration)                          │  │
│  │  - Extract userId from JWT claims ("sub")                     │  │
│  │  - Extract username from JWT claims ("unique_name")           │  │
│  │  - Extract property ID from X-Selected-Property header        │  │
│  │  - Extract metadata (route/query/body/response)               │  │
│  │  - Check for X-Activity-Message header (client message)       │  │
│  │  - If no client message, call ActivityDisplayMessageBuilder  │  │
│  └─────────────────────────────┬────────────────────────────────┘  │
│                                 │                                    │
│  ┌──────────────────────────────▼───────────────────────────────┐  │
│  │  ActivityDisplayMessageBuilder                                │  │
│  │  BuildMessageAsync()                                          │  │
│  │                                                               │  │
│  │  - Determine if important operation (Create/Update/Delete)    │  │
│  │  - Format username (nevillecolaco19@gmail.com → nevillecolaco19) │
│  │  - Get verb ("viewed", "created", "updated")                  │  │
│  │  - Get entity name ("Properties", "Dashboard")                │  │
│  │  - Resolve property name from PropertyId (if important op)    │  │
│  │  - Build message: "nevillecolaco viewed Properties"           │  │
│  └─────────────────────────────┬────────────────────────────────┘  │
│                                 │                                    │
│  ┌──────────────────────────────▼───────────────────────────────┐  │
│  │  UserActivityService                                          │  │
│  │  LogActivityAsync()                                           │  │
│  │                                                               │  │
│  │  Creates UserActivityLog object:                              │  │
│  │  {                                                            │  │
│  │    UserId: 19,                                                │  │
│  │    Username: "nevillecolaco19@gmail.com",                     │  │
│  │    ActivityType: View,                                        │  │
│  │    EntityType: "Properties",                                  │  │
│  │    Action: "View Properties",                                 │  │
│  │    DisplayMessage: "nevillecolaco viewed Properties",         │  │
│  │    PropertyId: 3,                                             │  │
│  │    Metadata: { selected_PropertyId: "3", ... },               │  │
│  │    Timestamp: "2024-01-15T10:30:00Z",                         │  │
│  │    IPAddress: "192.168.1.100",                                │  │
│  │    DurationMs: 245                                            │  │
│  │  }                                                            │  │
│  └─────────────────────────────┬────────────────────────────────┘  │
│                                 │                                    │
│  ┌──────────────────────────────▼───────────────────────────────┐  │
│  │  UserActivityRepositoryMongo                                  │  │
│  │  LogActivityAsync()                                           │  │
│  │                                                               │  │
│  │  - Generate next ID from CounterService                       │  │
│  │  - Insert document into MongoDB collection                    │  │
│  └─────────────────────────────┬────────────────────────────────┘  │
│                                 │                                    │
└─────────────────────────────────┼────────────────────────────────────┘
                                  │
┌─────────────────────────────────▼────────────────────────────────────┐
│                         MONGODB DATABASE                              │
├─────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  Database: ListingDB                                                  │
│  Collection: UserActivityLogs                                         │
│                                                                       │
│  Document Saved:                                                      │
│  {                                                                    │
│    "_id": 1234,                                                       │
│    "UserId": 19,                                                      │
│    "Username": "nevillecolaco19@gmail.com",                           │
│    "ActivityType": "View",                                            │
│    "EntityType": "Properties",                                        │
│    "EntityId": null,                                                  │
│    "Action": "View Properties",                                       │
│    "Description": "View Properties",                                  │
│    "DisplayMessage": "nevillecolaco viewed Properties",               │
│    "PropertyId": 3,                                                   │
│    "Metadata": {                                                      │
│      "selected_PropertyId": "3",                                      │
│      "response_TotalCount": "15"                                      │
│    },                                                                 │
│    "Timestamp": ISODate("2024-01-15T10:30:00Z"),                     │
│    "IPAddress": "192.168.1.100",                                      │
│    "UserAgent": "Mozilla/5.0...",                                     │
│    "SessionId": "abc123...",                                          │
│    "TraceId": "trace-456...",                                         │
│    "IsSuccess": true,                                                 │
│    "ErrorMessage": null,                                              │
│    "DurationMs": 245,                                                 │
│    "Module": "Property"                                               │
│  }                                                                    │
│                                                                       │
│  Indexes:                                                             │
│  - Timestamp (descending) - for recent activities                     │
│  - UserId + Timestamp - for user activity history                     │
│  - EntityType + EntityId + Timestamp - for entity audit trail         │
│  - ActivityType + Timestamp - for filtering by type                   │
│                                                                       │
└───────────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Request Flow

### Step-by-Step Execution

#### 1️⃣ **User Performs Action (Angular)**
```typescript
// User selects a property
applySelection() {
  // Set custom message
  this.activityMessage.setMessage(`Selected '${property.Name}' property`);
  
  // Make API call (interceptors will add headers automatically)
  this.http.post('/api/v1/property/selection/log', data).subscribe();
}
```

#### 2️⃣ **HTTP Interceptors Add Headers**
```typescript
// property-context.interceptor.ts
const selectedProperty = localStorage.getItem('selectedProperty');
if (selectedProperty) {
  const property = JSON.parse(selectedProperty);
  request = request.clone({
    setHeaders: {
      'X-Selected-Property': property.Id.toString()
    }
  });
}

// activity-message.interceptor.ts
const message = this.activityMessageService.getMessage();
if (message) {
  request = request.clone({
    setHeaders: {
      'X-Activity-Message': message
    }
  });
}
```

**Resulting HTTP Request:**
```http
POST /api/v1/property/selection/log HTTP/1.1
Host: localhost:5000
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
X-Selected-Property: 3
X-Activity-Message: Selected 'Sunset Villa' property
Content-Type: application/json

{
  "PropertyIds": [3],
  "PropertyNames": ["Sunset Villa"]
}
```

#### 3️⃣ **Controller Receives Request**
```csharp
[HttpPost("selection/log")]
[LogCreate("Property")] // 👈 This attribute triggers activity logging
public async Task<ActionResult> LogPropertySelection(
    [FromBody] PropertySelectionLogRequest request)
{
    return Ok(new { success = true });
}
```

#### 4️⃣ **ActivityLoggingActionFilter Intercepts**

**BEFORE Action Execution:**
```csharp
public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
{
    // Check for [ActivityLog] attribute
    var attribute = context.ActionDescriptor.EndpointMetadata
        .OfType<ActivityLogAttribute>()
        .FirstOrDefault();
    
    if (attribute == null) return; // No logging needed
    
    // Start timing
    var stopwatch = Stopwatch.StartNew();
    
    // Execute the action
    var executedContext = await next();
    
    // Stop timing
    stopwatch.Stop();
    
    // ... logging continues below
}
```

**AFTER Action Execution:**
```csharp
// Extract user info from JWT token
var userId = GetUserId(context);        // From "sub" claim → 19
var username = GetUsername(context);     // From "unique_name" claim → "nevillecolaco19@gmail.com"

// Extract property context
var propertyId = GetSelectedPropertyId(context); // From X-Selected-Property header → 3

// Extract metadata
var metadata = ExtractMetadata(context, executedContext);
// {
//   "selected_PropertyId": "3",
//   "param_request_PropertyIds": "[3]",
//   "param_request_PropertyNames": "[\"Sunset Villa\"]"
// }

// Check for client-provided message
var displayMessage = GetClientProvidedMessage(context); 
// From X-Activity-Message header → "Selected 'Sunset Villa' property"

// If no client message, generate one
if (string.IsNullOrEmpty(displayMessage)) {
    displayMessage = await _messageBuilder.BuildMessageAsync(
        activityType: ActivityType.Create,
        entityType: "Property",
        entityId: null,
        username: "nevillecolaco19@gmail.com",
        metadata: metadata
    );
    // Result: "nevillecolaco created Property"
}
```

#### 5️⃣ **ActivityDisplayMessageBuilder Generates Message**

*Only runs if client didn't provide message*

```csharp
public async Task<string> BuildMessageAsync(
    ActivityType activityType,
    string? entityType,
    int? entityId,
    string username,
    Dictionary<string, object>? metadata = null)
{
    // Determine importance (Create/Update/Delete = important)
    var isImportant = IsImportantOperation(activityType); // true for Create
    
    // Format username (remove @domain.com)
    var userDisplayName = GetUserDisplayName(username, metadata);
    // "nevillecolaco19@gmail.com" → "nevillecolaco19"
    
    // Get verb
    var verb = GetActivityVerb(activityType); // "created"
    
    // Get entity name
    var entityName = GetEntityName(entityType, entityId, metadata, isImportant);
    // "Property"
    
    // Build base message
    var message = $"{userDisplayName} {verb} {entityName}";
    // "nevillecolaco19 created Property"
    
    // Add property context (for important operations only)
    if (isImportant) {
        var propertyContext = await GetPropertyContextAsync(metadata, shortFormat: true);
        // Looks up property name from PropertyRepository
        // Returns: "in Sunset Villa"
        
        if (!string.IsNullOrEmpty(propertyContext)) {
            message += $" {propertyContext}";
            // Final: "nevillecolaco19 created Property in Sunset Villa"
        }
    }
    
    return message;
}
```

#### 6️⃣ **UserActivityService Saves Log**

```csharp
public async Task<int> LogActivityAsync(
    int userId,
    string username,
    ActivityType activityType,
    string action,
    string description,
    string? entityType = null,
    int? entityId = null,
    Dictionary<string, object>? metadata = null,
    string? displayMessage = null,
    int? propertyId = null,
    string? ipAddress = null,
    string? userAgent = null,
    string? sessionId = null,
    string? traceId = null,
    bool isSuccess = true,
    string? errorMessage = null,
    long? durationMs = null,
    string? module = null)
{
    var activity = new UserActivityLog
    {
        UserId = userId,                    // 19
        Username = username,                // "nevillecolaco19@gmail.com"
        ActivityType = activityType,        // ActivityType.Create
        EntityType = entityType,            // "Property"
        EntityId = entityId,                // null
        Action = action,                    // "Create Property"
        Description = description,          // "Create Property"
        Metadata = metadata,                // { "selected_PropertyId": "3", ... }
        DisplayMessage = displayMessage,    // "Selected 'Sunset Villa' property"
        Timestamp = DateTime.UtcNow,        // 2024-01-15T10:30:00Z
        IPAddress = ipAddress,              // "192.168.1.100"
        UserAgent = userAgent,              // "Mozilla/5.0..."
        SessionId = sessionId,              // "session-abc123"
        TraceId = traceId,                  // "trace-456"
        IsSuccess = isSuccess,              // true
        ErrorMessage = errorMessage,        // null
        DurationMs = durationMs,            // 245
        Module = module,                    // "Property"
        PropertyId = propertyId             // 3
    };
    
    return await _repository.LogActivityAsync(activity);
}
```

#### 7️⃣ **UserActivityRepositoryMongo Saves to MongoDB**

```csharp
public async Task<int> LogActivityAsync(UserActivityLog activity)
{
    // Generate next sequential ID
    activity.Id = await _counterService.GetNextSequenceValue("UserActivityLogs");
    // e.g., 1234
    
    // Insert into MongoDB
    await _collection.InsertOneAsync(activity);
    
    return activity.Id;
}
```

**MongoDB Document Created:**
```json
{
  "_id": 1234,
  "UserId": 19,
  "Username": "nevillecolaco19@gmail.com",
  "ActivityType": "Create",
  "EntityType": "Property",
  "EntityId": null,
  "Action": "Create Property",
  "Description": "Create Property",
  "DisplayMessage": "Selected 'Sunset Villa' property",
  "PropertyId": 3,
  "Metadata": {
    "selected_PropertyId": "3",
    "param_request_PropertyIds": "[3]",
    "param_request_PropertyNames": "[\"Sunset Villa\"]"
  },
  "Timestamp": "2024-01-15T10:30:00.000Z",
  "IPAddress": "192.168.1.100",
  "UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64)...",
  "SessionId": "session-abc123",
  "TraceId": "trace-456",
  "IsSuccess": true,
  "ErrorMessage": null,
  "DurationMs": 245,
  "Module": "Property"
}
```

---

## 🖥️ Client-Side Components (Angular)

### 1. Property Context Interceptor

**File:** `app/src/app/core/interceptors/property-context.interceptor.ts`

**Purpose:** Automatically adds `X-Selected-Property` header to every HTTP request

**Code:**
```typescript
export const propertyContextInterceptor: HttpInterceptorFn = (req, next) => {
  const selectedPropertyStr = localStorage.getItem('selectedProperty');
  
  if (selectedPropertyStr) {
    try {
      const selectedProperty = JSON.parse(selectedPropertyStr);
      if (selectedProperty && selectedProperty.Id) {
        req = req.clone({
          setHeaders: {
            'X-Selected-Property': selectedProperty.Id.toString()
          }
        });
      }
    } catch (error) {
      console.error('Error parsing selected property:', error);
    }
  }
  
  return next(req);
};
```

**When it runs:** On **every** HTTP request  
**What it does:** Reads selected property from localStorage and adds header  
**Result:** Server knows which property context user is in

---

### 2. Activity Message Interceptor

**File:** `app/src/app/core/interceptors/activity-message.interceptor.ts`

**Purpose:** Adds `X-Activity-Message` header when Angular sets a custom message

**Code:**
```typescript
export const activityMessageInterceptor: HttpInterceptorFn = (req, next) => {
  const activityMessageService = inject(ActivityMessageService);
  const message = activityMessageService.getMessage();
  
  if (message) {
    req = req.clone({
      setHeaders: {
        'X-Activity-Message': message
      }
    });
    
    // Clear message after use (one-time use)
    activityMessageService.clearMessage();
  }
  
  return next(req);
};
```

**When it runs:** After property context interceptor  
**What it does:** Adds custom message if available  
**Result:** Server receives human-readable message from client

---

### 3. Activity Message Service

**File:** `app/src/app/services/activity-message.service.ts`

**Purpose:** Helper service to set activity messages before making requests

**Code:**
```typescript
@Injectable({
  providedIn: 'root'
})
export class ActivityMessageService {
  private currentMessage: string | null = null;

  setMessage(message: string): void {
    this.currentMessage = message;
  }

  getMessage(): string | null {
    return this.currentMessage;
  }

  clearMessage(): void {
    this.currentMessage = null;
  }
}
```

**Usage Example:**
```typescript
// In a component
constructor(
  private activityMessage: ActivityMessageService,
  private http: HttpClient
) {}

createRoom() {
  // Set custom message
  this.activityMessage.setMessage(`Added '${this.roomName}' to 3rd floor`);
  
  // Make request (interceptor will add message header)
  this.http.post('/api/v1/rooms', roomData).subscribe();
}
```

---

### 4. Property Selection Component

**File:** `app/src/app/property/property-selection/property-selection.component.ts`

**Purpose:** Example of explicit backend logging for property selection

**Code:**
```typescript
applySelection(): void {
  if (this.selectedProperties.length === 0) {
    return;
  }

  // Build meaningful message
  const propertyNames = this.selectedProperties
    .map(p => p.Name)
    .join(', ');
  const message = this.selectedProperties.length === 1
    ? `Selected '${propertyNames}' property`
    : `Selected ${this.selectedProperties.length} properties: ${propertyNames}`;

  // Set message for the API call
  this.activityMessage.setMessage(message);

  // Call backend to log the selection
  const logPayload = {
    PropertyIds: this.selectedProperties.map(p => p.Id),
    PropertyNames: this.selectedProperties.map(p => p.Name)
  };

  this.http.post(`${this.config.apiUrl}/api/v1/property/selection/log`, logPayload)
    .subscribe({
      next: () => {
        // Save to localStorage
        localStorage.setItem('selectedProperty', 
          JSON.stringify(this.selectedProperties[0]));
        
        // Navigate to dashboard
        this.router.navigate(['/dashboard']);
      },
      error: (error) => {
        console.error('Failed to log property selection:', error);
      }
    });
}
```

**Why explicit call:** Property selection is a critical action that deserves its own dedicated log entry

---

## ⚙️ Server-Side Components (.NET)

### 1. ActivityLoggingActionFilter

**File:** `classfiles/Infrastructure/Filters/ActivityLoggingActionFilter.cs`

**Purpose:** MVC Action Filter that intercepts requests with `[ActivityLog]` attributes

**Registration:**
```csharp
// Startup.cs
services.AddControllers(options =>
{
    options.Filters.Add<ActivityLoggingActionFilter>();
});
```

**Key Methods:**

#### OnActionExecutionAsync
Main entry point that wraps action execution
```csharp
public async Task OnActionExecutionAsync(
    ActionExecutingContext context, 
    ActionExecutionDelegate next)
{
    // 1. Check for [ActivityLog] attribute
    var attribute = context.ActionDescriptor.EndpointMetadata
        .OfType<ActivityLogAttribute>()
        .FirstOrDefault();
    
    if (attribute == null) {
        await next(); // No logging needed
        return;
    }
    
    // 2. Start timing
    var stopwatch = Stopwatch.StartNew();
    
    // 3. Execute action
    var executedContext = await next();
    
    // 4. Stop timing
    stopwatch.Stop();
    
    // 5. Extract data and log
    // ... (see full implementation)
}
```

#### GetUsername
Extracts username from JWT claims
```csharp
private string GetUsername(ActionExecutingContext context)
{
    // Try Identity.Name first
    if (!string.IsNullOrEmpty(context.HttpContext.User?.Identity?.Name)) {
        return context.HttpContext.User.Identity.Name;
    }
    
    // Try JWT claims (in priority order)
    var usernameClaim = context.HttpContext.User?.FindFirst("unique_name")?.Value
        ?? context.HttpContext.User?.FindFirst("name")?.Value
        ?? context.HttpContext.User?.FindFirst("email")?.Value;
    
    return usernameClaim ?? "Unknown";
}
```

#### GetSelectedPropertyId
Extracts property ID from custom header
```csharp
private int? GetSelectedPropertyId(ActionExecutingContext context)
{
    if (context.HttpContext.Request.Headers.TryGetValue(
        "X-Selected-Property", out var propertyHeader))
    {
        if (int.TryParse(propertyHeader.ToString(), out var propertyId)) {
            return propertyId;
        }
    }
    return null;
}
```

#### ExtractMetadata
Captures rich context from request/response
```csharp
private Dictionary<string, object> ExtractMetadata(
    ActionExecutingContext executingContext, 
    ActionExecutedContext executedContext)
{
    var metadata = new Dictionary<string, object>();
    
    // Route parameters
    foreach (var routeParam in executingContext.RouteData.Values) {
        if (routeParam.Key != "controller" && routeParam.Key != "action") {
            metadata[$"route_{routeParam.Key}"] = routeParam.Value?.ToString() ?? "";
        }
    }
    
    // Query string
    foreach (var queryParam in executingContext.HttpContext.Request.Query) {
        metadata[$"query_{queryParam.Key}"] = queryParam.Value.ToString();
    }
    
    // Selected property header
    if (executingContext.HttpContext.Request.Headers
        .TryGetValue("X-Selected-Property", out var selectedPropertyHeader)) {
        metadata["selected_PropertyId"] = selectedPropertyHeader.ToString();
    }
    
    // Response data
    if (executedContext.Result is ObjectResult objectResult && objectResult.Value != null) {
        var responseType = objectResult.Value.GetType();
        
        // Extract Id
        var idProp = responseType.GetProperty("Id");
        if (idProp != null) {
            metadata["response_Id"] = idProp.GetValue(objectResult.Value)?.ToString() ?? "";
        }
        
        // Extract Name
        var nameProp = responseType.GetProperty("Name");
        if (nameProp != null) {
            metadata["response_Name"] = nameProp.GetValue(objectResult.Value)?.ToString() ?? "";
        }
        
        // Extract count for list results
        var resultsProperty = responseType.GetProperty("Results");
        if (resultsProperty != null) {
            var results = resultsProperty.GetValue(objectResult.Value) as IEnumerable;
            if (results != null) {
                int count = 0;
                foreach (var _ in results) count++;
                metadata["response_Count"] = count;
            }
        }
    }
    
    return metadata;
}
```

---

### 2. ActivityDisplayMessageBuilder

**File:** `classfiles/Application/UserActivity/Services/ActivityDisplayMessageBuilder.cs`

**Purpose:** Builds human-readable activity messages when client doesn't provide one

**Key Methods:**

#### BuildMessageAsync
Main message building logic
```csharp
public async Task<string> BuildMessageAsync(
    ActivityType activityType,
    string? entityType,
    int? entityId,
    string username,
    Dictionary<string, object>? metadata = null)
{
    metadata ??= new Dictionary<string, object>();
    
    // 1. Determine if important operation
    var isImportant = IsImportantOperation(activityType);
    
    // 2. Format username
    var userDisplayName = GetUserDisplayName(username, metadata);
    
    // 3. Get verb
    var verb = GetActivityVerb(activityType);
    
    // 4. Get entity name
    var entityName = GetEntityName(entityType, entityId, metadata, isImportant);
    
    // 5. Build base message
    var message = $"{userDisplayName} {verb} {entityName}";
    
    // 6. Add property context (for important operations only)
    if (isImportant) {
        var propertyContext = await GetPropertyContextAsync(metadata, shortFormat: true);
        if (!string.IsNullOrEmpty(propertyContext)) {
            message += $" {propertyContext}";
        }
    }
    
    return message;
}
```

#### IsImportantOperation
Determines which operations need detailed messages
```csharp
public bool IsImportantOperation(ActivityType activityType)
{
    return activityType switch
    {
        ActivityType.Create => true,
        ActivityType.Update => true,
        ActivityType.Delete => true,
        ActivityType.BulkCreate => true,
        ActivityType.BulkUpdate => true,
        ActivityType.BulkDelete => true,
        ActivityType.StatusChange => true,
        ActivityType.Approve => true,
        ActivityType.Reject => true,
        _ => false // View, PageView, Login, etc.
    };
}
```

#### GetActivityVerb
Maps activity types to human-readable verbs
```csharp
public string GetActivityVerb(ActivityType activityType)
{
    return activityType switch
    {
        ActivityType.Create => "created",
        ActivityType.Update => "updated",
        ActivityType.Delete => "deleted",
        ActivityType.View => "viewed",
        ActivityType.PageView => "viewed",
        ActivityType.Search => "searched",
        ActivityType.Export => "exported",
        ActivityType.Import => "imported",
        ActivityType.Login => "logged into",
        ActivityType.Logout => "logged out of",
        ActivityType.Download => "downloaded",
        ActivityType.Upload => "uploaded",
        ActivityType.Approve => "approved",
        ActivityType.Reject => "rejected",
        ActivityType.StatusChange => "changed status of",
        _ => "interacted with"
    };
}
```

---

### 3. UserActivityService

**File:** `classfiles/Application/UserActivity/Services/UserActivityService.cs`

**Purpose:** Service layer for logging activities (abstracts repository)

**Main Method:**
```csharp
public async Task<int> LogActivityAsync(
    int userId,
    string username,
    ActivityType activityType,
    string action,
    string description,
    string? entityType = null,
    int? entityId = null,
    Dictionary<string, object>? metadata = null,
    bool isSuccess = true,
    string? errorMessage = null,
    long? durationMs = null,
    string? module = null,
    int? propertyId = null,
    string? ipAddress = null,
    string? userAgent = null,
    string? sessionId = null,
    string? traceId = null,
    string? displayMessage = null)
{
    var activity = new UserActivityLog
    {
        UserId = userId,
        Username = username,
        ActivityType = activityType,
        EntityType = entityType,
        EntityId = entityId,
        Action = action,
        Description = description,
        Metadata = metadata,
        DisplayMessage = displayMessage,
        Timestamp = DateTime.UtcNow,
        IPAddress = ipAddress,
        UserAgent = userAgent,
        SessionId = sessionId,
        TraceId = traceId,
        IsSuccess = isSuccess,
        ErrorMessage = errorMessage,
        DurationMs = durationMs,
        Module = module,
        PropertyId = propertyId
    };
    
    return await _repository.LogActivityAsync(activity);
}
```

**Helper Methods:**
```csharp
// For CRUD operations
public async Task LogCrudOperationAsync(
    int userId,
    string username,
    ActivityType activityType,
    string entityType,
    int entityId,
    string entityName,
    Dictionary<string, object>? metadata = null)

// For page views
public async Task LogPageViewAsync(
    int userId,
    string username,
    string pageName,
    string pageUrl)

// For login/logout
public async Task LogLoginAsync(
    int userId,
    string username,
    bool isSuccess,
    string? errorMessage = null)

// For exports
public async Task LogExportAsync(
    int userId,
    string username,
    string exportType,
    int recordCount,
    string? entityType = null)

// For searches
public async Task LogSearchAsync(
    int userId,
    string username,
    string searchTerm,
    string? entityType = null,
    int? resultCount = null)
```

---

### 4. Activity Log Attributes

**File:** `MyWarehouse.Application.UserActivity.Attributes/ActivityLogAttribute.cs`

**Purpose:** Decorators to mark controller actions for logging

**Available Attributes:**
```csharp
[LogView("EntityType")]          // For GET requests (view operations)
[LogCreate("EntityType")]        // For POST requests (create)
[LogUpdate("EntityType")]        // For PUT/PATCH requests (update)
[LogDelete("EntityType")]        // For DELETE requests
[LogPageView("PageName")]        // For page/dashboard views
[LogSearch("EntityType")]        // For search operations
[LogExport("EntityType")]        // For export operations
```

**Usage:**
```csharp
[HttpGet]
[LogView("Properties")]
public async Task<ActionResult<List<Property>>> GetAll()
{
    var properties = await _propertyService.GetAllAsync();
    return Ok(properties);
}

[HttpPost]
[LogCreate("Room")]
public async Task<ActionResult<Room>> CreateRoom([FromBody] CreateRoomCommand command)
{
    var room = await _mediator.Send(command);
    return Ok(room);
}
```

---

## 💾 Database Layer (MongoDB)

### 1. UserActivityLog Entity

**File:** `classfiles/Domain/UserActivity/UserActivityLog.cs`

**Purpose:** Domain model representing an activity log entry

**Schema:**
```csharp
public class UserActivityLog : IEntity<int>
{
    [BsonId]
    public int Id { get; set; }                      // Sequential ID
    
    // Who
    public int UserId { get; set; }                  // User's ID
    public string Username { get; set; }             // Email/username
    
    // What
    public ActivityType ActivityType { get; set; }   // Create, View, Update, etc.
    public string? EntityType { get; set; }          // "Property", "Room", "Booking"
    public int? EntityId { get; set; }               // Specific entity ID
    public string Action { get; set; }               // "View Properties"
    public string Description { get; set; }          // Detailed description
    
    // Context
    public Dictionary<string, object>? Metadata { get; set; }  // Technical metadata
    public string? DisplayMessage { get; set; }                // Human-readable message
    public int? PropertyId { get; set; }                       // Property context
    
    // When
    public DateTime Timestamp { get; set; }          // UTC timestamp
    
    // Where
    public string? IPAddress { get; set; }           // Client IP
    public string? UserAgent { get; set; }           // Browser/device info
    public string? SessionId { get; set; }           // Session identifier
    public string? TraceId { get; set; }             // Distributed tracing
    
    // Result
    public bool IsSuccess { get; set; }              // true/false
    public string? ErrorMessage { get; set; }        // Error details if failed
    public long? DurationMs { get; set; }            // Request duration
    
    // Organization
    public string? Module { get; set; }              // "Property", "Booking", etc.
}
```

---

### 2. ActivityType Enum

**File:** `classfiles/Domain/UserActivity/ActivityType.cs`

**Purpose:** Standardized activity types

```csharp
public enum ActivityType
{
    // Data Operations
    Create,
    Update,
    Delete,
    View,
    
    // Bulk Operations
    BulkCreate,
    BulkUpdate,
    BulkDelete,
    
    // User Actions
    Login,
    Logout,
    Search,
    Export,
    Import,
    Download,
    Upload,
    
    // Workflow
    Approve,
    Reject,
    StatusChange,
    
    // Navigation
    PageView
}
```

---

### 3. UserActivityRepositoryMongo

**File:** `classfiles/Infrastructure/.../UserActivityRepositoryMongo.cs`

**Purpose:** MongoDB implementation of activity logging repository

#### LogActivityAsync
Saves log to database
```csharp
public async Task<int> LogActivityAsync(UserActivityLog activity)
{
    // Generate sequential ID
    activity.Id = await _counterService.GetNextSequenceValue(
        "UserActivityLogs");
    
    // Insert into MongoDB collection
    await _collection.InsertOneAsync(activity);
    
    return activity.Id;
}
```

#### CreateIndexes
Optimizes query performance
```csharp
private void CreateIndexes()
{
    // For recent activities
    var timestampIndex = Builders<UserActivityLog>.IndexKeys
        .Descending(x => x.Timestamp);
    _collection.Indexes.CreateOne(
        new CreateIndexModel<UserActivityLog>(timestampIndex));
    
    // For user activity history
    var userTimestampIndex = Builders<UserActivityLog>.IndexKeys
        .Ascending(x => x.UserId)
        .Descending(x => x.Timestamp);
    _collection.Indexes.CreateOne(
        new CreateIndexModel<UserActivityLog>(userTimestampIndex));
    
    // For entity audit trail
    var entityIndex = Builders<UserActivityLog>.IndexKeys
        .Ascending(x => x.EntityType)
        .Ascending(x => x.EntityId)
        .Descending(x => x.Timestamp);
    _collection.Indexes.CreateOne(
        new CreateIndexModel<UserActivityLog>(entityIndex));
    
    // For filtering by activity type
    var activityTypeIndex = Builders<UserActivityLog>.IndexKeys
        .Ascending(x => x.ActivityType)
        .Descending(x => x.Timestamp);
    _collection.Indexes.CreateOne(
        new CreateIndexModel<UserActivityLog>(activityTypeIndex));
}
```

#### Query Methods
```csharp
// Get recent activities across all users
public async Task<List<UserActivityLog>> GetRecentActivitiesAsync(
    int count = 50, int skip = 0)

// Get activities for specific user
public async Task<List<UserActivityLog>> GetUserActivitiesAsync(
    int userId, DateTime? from = null, DateTime? to = null)

// Get activities by type
public async Task<List<UserActivityLog>> GetActivitiesByTypeAsync(
    ActivityType activityType, DateTime? from = null, DateTime? to = null)

// Get activities for specific entity (audit trail)
public async Task<List<UserActivityLog>> GetEntityActivitiesAsync(
    string entityType, int entityId, DateTime? from = null, DateTime? to = null)

// Get activity statistics
public async Task<Dictionary<string, object>> GetActivityStatisticsAsync(
    DateTime? from = null, DateTime? to = null)

// Get most active users
public async Task<List<(string Username, int ActivityCount)>> 
    GetMostActiveUsersAsync(int count = 10, DateTime? from = null, DateTime? to = null)

// Paginated query with filters
public async Task<(List<UserActivityLog> Activities, long TotalCount)> 
    GetActivitiesPagedAsync(
        int? userId = null,
        ActivityType? activityType = null,
        string? entityType = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 50,
        string sortBy = "Timestamp",
        bool sortDescending = true)
```

---

### 4. MongoDB Collection Structure

**Database:** `ListingDB`  
**Collection:** `UserActivityLogs`

**Sample Document:**
```json
{
  "_id": 1234,
  "UserId": 19,
  "Username": "nevillecolaco19@gmail.com",
  "ActivityType": "View",
  "EntityType": "Properties",
  "EntityId": null,
  "Action": "View Properties",
  "Description": "View Properties",
  "DisplayMessage": "nevillecolaco viewed Properties",
  "PropertyId": 3,
  "Metadata": {
    "selected_PropertyId": "3",
    "response_TotalCount": "15",
    "query_pageSize": "50",
    "query_pageNumber": "1"
  },
  "Timestamp": "2024-01-15T10:30:00.000Z",
  "IPAddress": "192.168.1.100",
  "UserAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
  "SessionId": "session-abc123",
  "TraceId": "trace-456",
  "IsSuccess": true,
  "ErrorMessage": null,
  "DurationMs": 245,
  "Module": "Property"
}
```

**Indexes:**
```javascript
// Index 1: Recent activities
{ "Timestamp": -1 }

// Index 2: User activity history
{ "UserId": 1, "Timestamp": -1 }

// Index 3: Entity audit trail
{ "EntityType": 1, "EntityId": 1, "Timestamp": -1 }

// Index 4: Filter by type
{ "ActivityType": 1, "Timestamp": -1 }
```

---

## 📝 How to Add Logging to New Endpoints

### Step 1: Add Attribute to Controller Action

```csharp
[HttpGet("{id}")]
[LogView("Room")]  // 👈 Add this attribute
public async Task<ActionResult<RoomDto>> GetRoomById(int id)
{
    var room = await _roomService.GetByIdAsync(id);
    return Ok(room);
}
```

**That's it!** The system will automatically:
- ✅ Extract user info from JWT token
- ✅ Extract property context from header
- ✅ Capture request/response metadata
- ✅ Generate human-readable message
- ✅ Save log to MongoDB

### Step 2 (Optional): Add Client-Provided Message

If you want a more specific message from Angular:

```typescript
// In Angular component
createRoom() {
  // Set custom message
  this.activityMessage.setMessage(
    `Added '${this.roomForm.value.name}' room to 3rd floor`
  );
  
  // Make request (message will be added automatically)
  this.http.post('/api/v1/rooms', roomData).subscribe();
}
```

---

## 🧪 Examples

### Example 1: View All Properties

**Angular:**
```typescript
// No special code needed - just make the request
this.http.get('/api/v1/property/all').subscribe();
```

**Controller:**
```csharp
[HttpGet("all")]
[LogView("Properties")]
public async Task<ActionResult<List<PropertyDto>>> GetAll()
{
    var properties = await _propertyService.GetAllAsync();
    return Ok(properties);
}
```

**MongoDB Log:**
```json
{
  "_id": 1234,
  "UserId": 19,
  "Username": "nevillecolaco19@gmail.com",
  "ActivityType": "View",
  "EntityType": "Properties",
  "Action": "View Properties",
  "DisplayMessage": "nevillecolaco viewed Properties",
  "PropertyId": 3,
  "Timestamp": "2024-01-15T10:30:00Z",
  "DurationMs": 120,
  "IsSuccess": true
}
```

---

### Example 2: Create Room with Client Message

**Angular:**
```typescript
createRoom() {
  const roomName = this.roomForm.value.name;
  const floor = this.roomForm.value.floor;
  
  // Set custom message
  this.activityMessage.setMessage(
    `Added '${roomName}' room to ${floor}th floor`
  );
  
  // Make request
  this.http.post('/api/v1/rooms', this.roomForm.value).subscribe();
}
```

**Controller:**
```csharp
[HttpPost]
[LogCreate("Room")]
public async Task<ActionResult<RoomDto>> CreateRoom([FromBody] CreateRoomCommand command)
{
    var room = await _mediator.Send(command);
    return Ok(room);
}
```

**MongoDB Log:**
```json
{
  "_id": 1235,
  "UserId": 19,
  "Username": "nevillecolaco19@gmail.com",
  "ActivityType": "Create",
  "EntityType": "Room",
  "EntityId": 42,
  "Action": "Create Room",
  "DisplayMessage": "Added 'Ocean View Suite' room to 3rd floor",
  "PropertyId": 3,
  "Metadata": {
    "selected_PropertyId": "3",
    "param_command_Name": "Ocean View Suite",
    "param_command_Floor": "3",
    "response_Id": "42",
    "response_Name": "Ocean View Suite"
  },
  "Timestamp": "2024-01-15T10:35:00Z",
  "DurationMs": 320,
  "IsSuccess": true
}
```

---

### Example 3: Property Selection

**Angular:**
```typescript
selectProperty(property: Property) {
  // Set meaningful message
  this.activityMessage.setMessage(
    `Selected '${property.Name}' property`
  );
  
  // Call backend to log
  this.http.post('/api/v1/property/selection/log', {
    PropertyIds: [property.Id],
    PropertyNames: [property.Name]
  }).subscribe(() => {
    // Save to localStorage
    localStorage.setItem('selectedProperty', JSON.stringify(property));
    
    // Navigate
    this.router.navigate(['/dashboard']);
  });
}
```

**Controller:**
```csharp
[HttpPost("selection/log")]
[LogCreate("Property")]
public async Task<ActionResult> LogPropertySelection(
    [FromBody] PropertySelectionLogRequest request)
{
    return Ok(new { success = true });
}
```

**MongoDB Log:**
```json
{
  "_id": 1236,
  "UserId": 19,
  "Username": "nevillecolaco19@gmail.com",
  "ActivityType": "Create",
  "EntityType": "Property",
  "Action": "Create Property",
  "DisplayMessage": "Selected 'Sunset Villa' property",
  "PropertyId": 3,
  "Metadata": {
    "selected_PropertyId": "3",
    "param_request_PropertyIds": "[3]",
    "param_request_PropertyNames": "[\"Sunset Villa\"]"
  },
  "Timestamp": "2024-01-15T10:25:00Z",
  "DurationMs": 85,
  "IsSuccess": true
}
```

---

### Example 4: Failed Operation

**Controller:**
```csharp
[HttpDelete("{id}")]
[LogDelete("Room", LogOnFailure = true)]  // Log even on failure
public async Task<ActionResult> DeleteRoom(int id)
{
    try {
        await _roomService.DeleteAsync(id);
        return Ok();
    } catch (Exception ex) {
        return BadRequest(ex.Message);
    }
}
```

**MongoDB Log (on failure):**
```json
{
  "_id": 1237,
  "UserId": 19,
  "Username": "nevillecolaco19@gmail.com",
  "ActivityType": "Delete",
  "EntityType": "Room",
  "EntityId": 99,
  "Action": "Delete Room",
  "DisplayMessage": "nevillecolaco deleted Room #99",
  "PropertyId": 3,
  "IsSuccess": false,
  "ErrorMessage": "Room not found",
  "Timestamp": "2024-01-15T10:40:00Z",
  "DurationMs": 45
}
```

---

## 🎯 Key Takeaways

### What Makes This System Powerful

1. **Zero Boilerplate** - Add one attribute, get full logging
2. **Property Context Tracking** - Always knows which property user is in
3. **Client-Provided Messages** - Angular can send meaningful messages
4. **Fallback Server Messages** - Auto-generated if client doesn't provide
5. **Rich Metadata** - Captures route, query, request, response data
6. **Performance Metrics** - Tracks request duration automatically
7. **Error Tracking** - Logs failures with error messages
8. **MongoDB Optimized** - Indexed for fast queries
9. **Non-Intrusive** - Never crashes requests even if logging fails

### The Magic Formula

```
User Action (Angular)
  ↓
HTTP Interceptors (Add Headers)
  ↓
Controller with [ActivityLog] Attribute
  ↓
ActivityLoggingActionFilter (Intercepts)
  ↓
ActivityDisplayMessageBuilder (Generates Message if Needed)
  ↓
UserActivityService (Creates Log Object)
  ↓
UserActivityRepositoryMongo (Saves to Database)
  ↓
MongoDB Collection (Indexed for Performance)
```

### When to Use Client Messages vs Server Messages

**Use Client Messages When:**
- ✅ Action has specific UI context (e.g., "Added to 3rd floor")
- ✅ Message needs business logic from UI (e.g., "Selected 5 items")
- ✅ You want to highlight specific details

**Let Server Generate When:**
- ✅ Standard CRUD operations (View, Create, Update, Delete)
- ✅ Generic actions without special context
- ✅ You want consistency across similar operations

---

## 📊 Querying Logs

### Get Recent Activities
```csharp
var recent = await _userActivityRepository.GetRecentActivitiesAsync(count: 50);
```

### Get User Activity History
```csharp
var userActivities = await _userActivityRepository.GetUserActivitiesAsync(
    userId: 19,
    from: DateTime.UtcNow.AddDays(-7),
    limit: 100
);
```

### Get Entity Audit Trail
```csharp
var roomHistory = await _userActivityRepository.GetEntityActivitiesAsync(
    entityType: "Room",
    entityId: 42,
    from: DateTime.UtcNow.AddMonths(-1)
);
```

### Get Activity Statistics
```csharp
var stats = await _userActivityRepository.GetActivityStatisticsAsync(
    from: DateTime.UtcNow.AddDays(-30)
);
```

### MongoDB Direct Query
```javascript
// Most active users in last 7 days
db.UserActivityLogs.aggregate([
  {
    $match: {
      Timestamp: { $gte: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000) }
    }
  },
  {
    $group: {
      _id: "$Username",
      count: { $sum: 1 }
    }
  },
  {
    $sort: { count: -1 }
  },
  {
    $limit: 10
  }
]);

// Activities by type (last 30 days)
db.UserActivityLogs.aggregate([
  {
    $match: {
      Timestamp: { $gte: new Date(Date.now() - 30 * 24 * 60 * 60 * 1000) }
    }
  },
  {
    $group: {
      _id: "$ActivityType",
      count: { $sum: 1 }
    }
  }
]);

// Property-specific activities
db.UserActivityLogs.find({
  PropertyId: 3,
  Timestamp: { $gte: ISODate("2024-01-01T00:00:00Z") }
}).sort({ Timestamp: -1 }).limit(50);
```

---

## 🎉 Conclusion

This activity logging system provides **enterprise-grade audit tracking** with minimal developer effort. Simply add attributes to controller actions, and the system handles the rest:

- ✅ Automatic user identification
- ✅ Property context tracking
- ✅ Metadata extraction
- ✅ Human-readable messages
- ✅ Performance metrics
- ✅ Error tracking
- ✅ Optimized database storage

**Result:** Complete visibility into all user actions across your application! 🚀
