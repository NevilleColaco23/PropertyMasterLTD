# 📋 ATTRIBUTE QUICK REFERENCE CARD

## 🎯 Available Attributes

### 1. [LogCreate] - Log Entity Creation
```csharp
[HttpPost]
[LogCreate("Property")]
public async Task<ActionResult<int>> Create(CreatePropertyCommand command)
```

**Logged ActivityType**: `PropertyCreated`  
**Auto-extracts**: Entity ID from response  
**Use for**: POST endpoints that create new entities

---

### 2. [LogUpdate] - Log Entity Updates
```csharp
[HttpPut("{id}")]
[LogUpdate("Property")]
public async Task<ActionResult> Update(int id, UpdatePropertyCommand command)
```

**Logged ActivityType**: `PropertyUpdated`  
**Auto-extracts**: Entity ID from route parameter `{id}`  
**Use for**: PUT/PATCH endpoints that modify entities

---

### 3. [LogDelete] - Log Entity Deletions
```csharp
[HttpDelete("{id}")]
[LogDelete("Property")]
public async Task<ActionResult> Delete(int id)
```

**Logged ActivityType**: `PropertyDeleted`  
**Auto-extracts**: Entity ID from route parameter `{id}`  
**Use for**: DELETE endpoints

---

### 4. [LogView] - Log Entity Views
```csharp
[HttpGet("{id}")]
[LogView("Property")]
public async Task<ActionResult<PropertyDto>> GetById(int id)
```

**Logged ActivityType**: `PropertyViewed`  
**Auto-extracts**: Entity ID from route parameter `{id}`  
**Use for**: GET endpoints that return single entity

---

### 5. [LogList] - Log List/Collection Views
```csharp
[HttpGet]
[LogList("Properties")]
public async Task<ActionResult<List<PropertyDto>>> GetList()
```

**Logged ActivityType**: `PropertiesListed`  
**Auto-extracts**: Nothing (no entity ID for lists)  
**Use for**: GET endpoints that return collections

---

### 6. [LogExport] - Log Data Exports
```csharp
[HttpGet("export")]
[LogExport("Properties")]
public async Task<IActionResult> ExportToExcel()
```

**Logged ActivityType**: `PropertiesExported`  
**Auto-extracts**: Nothing  
**Use for**: Export endpoints (Excel, CSV, PDF)

---

### 7. [LogSearch] - Log Search Operations
```csharp
[HttpGet("search")]
[LogSearch("Properties")]
public async Task<ActionResult<List<PropertyDto>>> Search([FromQuery] string query)
```

**Logged ActivityType**: `PropertiesSearched`  
**Auto-extracts**: Nothing  
**Use for**: Search endpoints

---

## 🎨 Advanced Usage

### Custom Description
```csharp
[LogCreate("Property", Description = "User created a new property listing")]
public async Task<ActionResult<int>> Create(...)
```

### Log Failures (Security)
```csharp
[LogCreate("User Session", LogOnFailure = true)]
public async Task<ActionResult> Login(LoginDto dto)
```
**Use for**: Authentication, authorization, critical operations

### Custom Entity ID Property
```csharp
[LogCreate("Dashboard", EntityIdProperty = "DashboardId")]
public async Task<ActionResult<DashboardResponse>> Create(...)
```
**Use when**: Response has custom ID property name

### Multiple Placeholders
```csharp
[LogUpdate("Dashboard", Description = "User {action} {entityType} #{entityId}")]
public async Task<ActionResult> Update(...)
```
**Result**: "User Updated Dashboard #42"

---

## 📊 What Gets Logged Automatically

### Every Attribute Logs:
- ✅ UserId (from JWT)
- ✅ Username (from JWT)
- ✅ ActivityType (from attribute)
- ✅ EntityType (from attribute)
- ✅ EntityId (from response or route)
- ✅ Description (auto-generated or custom)
- ✅ IPAddress (from HttpContext)
- ✅ UserAgent (from request headers)
- ✅ SessionId (from session)
- ✅ TraceId (from Activity.Current)
- ✅ Timestamp (DateTime.UtcNow)
- ✅ Duration (measured with Stopwatch)
- ✅ Status (Success/Failed)
- ✅ ErrorMessage (if exception)

**20+ fields with ZERO manual code!**

---

## 🔥 Common Patterns

### Standard CRUD Controller
```csharp
using MyWarehouse.Application.UserActivity.Attributes;

[ApiController]
[Route("api/v1/properties")]
public class PropertyController : ControllerBase
{
    [HttpGet]
    [LogList("Properties")]
    public async Task<ActionResult<List<PropertyDto>>> GetList() { }

    [HttpGet("{id}")]
    [LogView("Property")]
    public async Task<ActionResult<PropertyDto>> GetById(int id) { }

    [HttpPost]
    [LogCreate("Property")]
    public async Task<ActionResult<int>> Create(CreatePropertyCommand command) { }

    [HttpPut("{id}")]
    [LogUpdate("Property")]
    public async Task<ActionResult> Update(int id, UpdatePropertyCommand command) { }

    [HttpDelete("{id}")]
    [LogDelete("Property")]
    public async Task<ActionResult> Delete(int id) { }
}
```

### Authentication Controller
```csharp
[HttpPost("login")]
[LogCreate("User Session", Description = "User logged in", LogOnFailure = true)]
public async Task<ActionResult> Login(LoginDto dto) { }

[HttpPost("signup")]
[LogCreate("User Account", Description = "New user signed up", LogOnFailure = true)]
public async Task<ActionResult> SignUp(SignUpDto dto) { }

[HttpPost("logout")]
[LogDelete("User Session", Description = "User logged out")]
public async Task<ActionResult> Logout() { }
```

### Analytics Controller
```csharp
[HttpGet("dashboard")]
[LogView("Dashboard Analytics")]
public async Task<ActionResult<DashboardStats>> GetDashboard() { }

[HttpGet("reports/monthly")]
[LogExport("Monthly Report")]
public async Task<IActionResult> ExportMonthlyReport() { }
```

---

## ⚡ Quick Tips

### ✅ DO:
- Use descriptive entity type names: `"Property"`, `"User Account"`, `"Dashboard"`
- Add `LogOnFailure = true` for security-critical operations
- Use plural for lists: `[LogList("Properties")]`
- Use singular for entities: `[LogView("Property")]`

### ❌ DON'T:
- Don't use generic names: `"Item"`, `"Object"`, `"Entity"`
- Don't log sensitive data in descriptions
- Don't forget `using MyWarehouse.Application.UserActivity.Attributes;`

---

## 🎯 Entity Naming Convention

| Action | Entity Type | Example |
|--------|-------------|---------|
| Create | Singular | `[LogCreate("Property")]` |
| Update | Singular | `[LogUpdate("Property")]` |
| Delete | Singular | `[LogDelete("Property")]` |
| View (single) | Singular | `[LogView("Property")]` |
| List (many) | Plural | `[LogList("Properties")]` |
| Export | Plural | `[LogExport("Properties")]` |
| Search | Plural | `[LogSearch("Properties")]` |

---

## 📈 Activity Type Naming Pattern

Attribute automatically converts to PascalCase enum:

| Attribute | Entity Type | Result ActivityType |
|-----------|-------------|---------------------|
| `[LogCreate]` | `"Property"` | `PropertyCreated` |
| `[LogUpdate]` | `"Property"` | `PropertyUpdated` |
| `[LogDelete]` | `"Property"` | `PropertyDeleted` |
| `[LogView]` | `"Property"` | `PropertyViewed` |
| `[LogList]` | `"Properties"` | `PropertiesListed` |
| `[LogExport]` | `"Properties"` | `PropertiesExported` |
| `[LogSearch]` | `"Properties"` | `PropertiesSearched` |

---

## 🔍 Troubleshooting

### Attribute not working?
1. Check using statement: `using MyWarehouse.Application.UserActivity.Attributes;`
2. Check filter registered: `options.Filters.Add<ActivityLoggingActionFilter>();`
3. Rebuild solution: `dotnet clean && dotnet build`
4. Restart API

### Entity ID not extracted?
- Check response has `Id` property
- Or use route parameter: `[HttpGet("{id}")]`
- Or specify custom: `EntityIdProperty = "CustomId"`

### Activity not appearing?
1. Check MongoDB connection
2. Check UserActivityService registered: `services.AddScoped<UserActivityService>();`
3. Check UserActivityRepository registered
4. Check MongoDB collection exists: `db.UserActivityLogs.countDocuments()`

---

## 📚 Full Documentation

- **Implementation Guide**: `PHASE2_AUTO_LOGGING_COMPLETE.md`
- **Application Summary**: `PHASE2_ATTRIBUTE_APPLICATION_COMPLETE.md`
- **Testing Guide**: `PHASE2_TESTING_QUICK_START.md`
- **Complete Summary**: `PHASE2_COMPLETE_SUMMARY.md`
- **This Reference**: `PHASE2_QUICK_REFERENCE_CARD.md`

---

## 💡 Examples by Use Case

### E-Commerce
```csharp
[LogCreate("Order")]
[LogUpdate("Order Status")]
[LogView("Product Details")]
[LogList("Products")]
[LogExport("Sales Report")]
```

### CRM
```csharp
[LogCreate("Customer")]
[LogUpdate("Customer Profile")]
[LogView("Customer History")]
[LogList("Customers")]
[LogSearch("Customers")]
```

### Property Management
```csharp
[LogCreate("Property")]
[LogUpdate("Property")]
[LogView("Property Details")]
[LogList("Properties")]
[LogCreate("Booking")]
```

### SaaS Dashboard
```csharp
[LogCreate("Dashboard")]
[LogUpdate("Dashboard Layout")]
[LogView("Widget Library")]
[LogDelete("Dashboard")]
```

---

**Last Updated**: 2025-01-31  
**Version**: 1.0  
**Status**: Production Ready ✅  
