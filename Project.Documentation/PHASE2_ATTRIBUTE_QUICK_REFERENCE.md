# ⚡ PHASE 2 QUICK REFERENCE - Automatic Logging Attributes

## 🚀 Quick Start (30 seconds)

### **Step 1: Add Attribute to Controller Action**

```csharp
[HttpPost]
[LogCreate("Property")]
public async Task<IActionResult> CreateProperty(CreatePropertyCommand command)
{
    var result = await _mediator.Send(command);
    return Ok(result);
}
```

### **Step 2: Test It**

```bash
# Make API call
curl -X POST https://localhost:44346/api/v1/property \
  -H "Authorization: Bearer TOKEN" \
  -d '{"propertyName":"Test"}'

# Check activity logged
curl https://localhost:44346/api/v1/activity/recent?count=1
```

**✅ Done! Activity logged automatically!**

---

## 📚 Convenience Attributes Reference

### **Create Operations**

```csharp
[HttpPost]
[LogCreate("Property")]  // Logs: "Created Property #123"
public async Task<IActionResult> CreateProperty(...)
```

---

### **Update Operations**

```csharp
[HttpPut("{id}")]
[LogUpdate("Property")]  // Logs: "Updated Property #123"
public async Task<IActionResult> UpdateProperty(...)
```

---

### **Delete Operations**

```csharp
[HttpDelete("{id}")]
[LogDelete("Property")]  // Logs: "Deleted Property #123"
public async Task<IActionResult> DeleteProperty(...)
```

---

### **View Operations**

```csharp
[HttpGet("{id}")]
[LogView("Property")]  // Logs: "Viewed Property #123"
public async Task<IActionResult> GetProperty(...)
```

---

### **List Operations**

```csharp
[HttpGet]
[LogList("Properties")]  // Logs: "Listed Properties records"
public async Task<IActionResult> GetAllProperties(...)
```

---

### **Export Operations**

```csharp
[HttpGet("export")]
[LogExport("Properties")]  // Logs: "Exported Properties data"
public async Task<IActionResult> ExportProperties(...)
```

---

### **Search Operations**

```csharp
[HttpPost("search")]
[LogSearch("Properties")]  // Logs: "Searched Properties records"
public async Task<IActionResult> SearchProperties(...)
```

---

## 🎯 Common Patterns

### **Pattern 1: Auto-Infer Entity Type**

```csharp
// In PropertyController.cs
[HttpPost]
[LogCreate]  // Entity type auto-inferred as "Property"
public async Task<IActionResult> CreateProperty(...)
```

---

### **Pattern 2: Custom Description**

```csharp
[HttpPost]
[LogCreate("Property", Description = "Created new property {entityType} #{entityId}")]
public async Task<IActionResult> CreateProperty(...)
```

---

### **Pattern 3: Custom Entity ID Property**

```csharp
[HttpPost]
[LogCreate("Property", EntityIdProperty = "PropertyId")]
public async Task<IActionResult> CreateProperty(...)
// Uses result.PropertyId instead of result.Id
```

---

### **Pattern 4: Log on Failure**

```csharp
[HttpDelete("{id}")]
[LogDelete("Property", LogOnFailure = true)]
public async Task<IActionResult> DeleteProperty(...)
// Logs even if delete fails
```

---

### **Pattern 5: Generic ActivityLog**

```csharp
[HttpPost("dashboard")]
[ActivityLog("DashboardCreate", "Dashboard", "Created new dashboard")]
public async Task<IActionResult> CreateDashboard(...)
```

---

## 📋 Full Controller Example

```csharp
using Microsoft.AspNetCore.Mvc;
using Application.UserActivity.Attributes;

namespace WebApi.API.V1
{
    [ApiController]
    [Route("api/v1/property")]
    public class PropertyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PropertyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // CREATE
        [HttpPost]
        [LogCreate]  // Auto-logs "Created Property #123"
        public async Task<IActionResult> CreateProperty(CreatePropertyCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // READ (Single)
        [HttpGet("{id}")]
        [LogView]  // Auto-logs "Viewed Property #123"
        public async Task<IActionResult> GetProperty(int id)
        {
            var result = await _mediator.Send(new GetPropertyQuery { Id = id });
            return Ok(result);
        }

        // READ (All)
        [HttpGet]
        [LogList]  // Auto-logs "Listed Property records"
        public async Task<IActionResult> GetAllProperties()
        {
            var result = await _mediator.Send(new GetAllPropertiesQuery());
            return Ok(result);
        }

        // UPDATE
        [HttpPut("{id}")]
        [LogUpdate]  // Auto-logs "Updated Property #123"
        public async Task<IActionResult> UpdateProperty(int id, UpdatePropertyCommand command)
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        // DELETE
        [HttpDelete("{id}")]
        [LogDelete]  // Auto-logs "Deleted Property #123"
        public async Task<IActionResult> DeleteProperty(int id)
        {
            await _mediator.Send(new DeletePropertyCommand { Id = id });
            return NoContent();
        }

        // SEARCH
        [HttpPost("search")]
        [LogSearch]  // Auto-logs "Searched Property records"
        public async Task<IActionResult> SearchProperties(SearchPropertiesQuery query)
        {
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        // EXPORT
        [HttpGet("export")]
        [LogExport]  // Auto-logs "Exported Property data"
        public async Task<IActionResult> ExportProperties()
        {
            var data = await _mediator.Send(new ExportPropertiesQuery());
            return File(data, "application/vnd.ms-excel", "properties.xlsx");
        }
    }
}
```

---

## 🎨 Custom Activity Types

For non-CRUD operations, use `[ActivityLog]`:

```csharp
// Dashboard operations
[ActivityLog("DashboardCreate", "Dashboard")]
[ActivityLog("DashboardUpdate", "Dashboard")]
[ActivityLog("DashboardDelete", "Dashboard")]
[ActivityLog("WidgetAdd", "Widget")]
[ActivityLog("WidgetRemove", "Widget")]

// Status changes
[ActivityLog("StatusChange", "Booking")]

// Approvals
[ActivityLog("Approve", "Booking")]
[ActivityLog("Reject", "Booking")]

// Notifications
[ActivityLog("SendEmail", "Notification")]
[ActivityLog("SendNotification", "Alert")]

// Bulk operations
[ActivityLog("BulkUpdate", "Property")]
[ActivityLog("BulkDelete", "Booking")]
```

---

## ⚙️ What Gets Logged Automatically

| Field | Source | Example |
|-------|--------|---------|
| **UserId** | JWT Claims (`UserId`, `sub`, `id`) | 1 |
| **Username** | JWT Claims (`name`, `username`, `Identity.Name`) | "admin" |
| **Entity ID** | Response property or route `{id}` | 123 |
| **Entity Type** | Attribute or controller name | "Property" |
| **Module** | Controller name | "Property" |
| **IP Address** | `HttpContext.Connection.RemoteIpAddress` | "192.168.1.100" |
| **User Agent** | `User-Agent` header | "Mozilla/5.0..." |
| **Session ID** | `HttpContext.Session.Id` | "abc-123" |
| **Trace ID** | `HttpContext.TraceIdentifier` | "xyz-789" |
| **Duration** | Measured with Stopwatch | 245ms |
| **Success** | HTTP status code | true/false |
| **Error** | Exception message | null or error text |

---

## 🔧 Attribute Properties

### **ActivityLogAttribute**

```csharp
[ActivityLog(
    activityType: "Create",           // Required
    entityType: "Property",            // Optional (auto-inferred)
    description: "Created {entityType} #{entityId}"  // Optional (template)
)]
```

### **Additional Properties**

```csharp
[LogCreate(
    "Property",
    Description = "Custom message",
    Module = "PropertyManagement",
    EntityIdProperty = "PropertyId",
    LogOnFailure = true
)]
```

---

## 🧪 Testing Checklist

- [ ] Add `[LogCreate]` to Create action
- [ ] Call API to create entity
- [ ] Check `/api/v1/activity/recent`
- [ ] Verify activity logged with correct type
- [ ] Verify entity ID extracted correctly
- [ ] Verify username and user ID correct
- [ ] Verify execution time captured
- [ ] Repeat for Update, Delete, View operations

---

## 📦 Import Statement

Add at top of controller:

```csharp
using Application.UserActivity.Attributes;
```

---

## 🎉 Benefits

| Before | After |
|--------|-------|
| 45+ lines per action | 1 line attribute |
| Manual timing | Automatic |
| Manual error handling | Automatic |
| Repetitive code | DRY principle |
| Easy to forget | Can't forget (attribute visible) |
| Hard to maintain | Centralized logic |

---

## 💡 Pro Tips

### **Tip 1: Use Auto-Inference**
```csharp
// In PropertyController
[LogCreate]  // Better than [LogCreate("Property")]
```

### **Tip 2: Descriptive for Important Actions**
```csharp
[LogDelete("Property", Description = "Permanently deleted property #{entityId}")]
```

### **Tip 3: Group Related Attributes**
```csharp
// Dashboard controller
[ActivityLog("DashboardView", "Dashboard")]
[ActivityLog("DashboardCreate", "Dashboard")]
[ActivityLog("DashboardUpdate", "Dashboard")]
```

### **Tip 4: Don't Log Health Checks**
```csharp
[HttpGet("health")]
// No attribute = no logging (good for health checks)
public IActionResult HealthCheck()
```

---

## 🐛 Common Issues

### **Issue:** Entity ID always null

**Fix:** Specify EntityIdProperty
```csharp
[LogCreate("Property", EntityIdProperty = "PropertyId")]
```

---

### **Issue:** User ID always 0

**Fix:** Check JWT token contains `UserId`, `sub`, or `id` claim

---

### **Issue:** Activities not logged

**Fix:**
1. Check attribute present on action
2. Restart backend API
3. Check `ApiStartup.cs` has filter registered

---

## 📖 Documentation

**Full Guide:** `PHASE2_AUTO_LOGGING_COMPLETE.md`

**Master README:** `ACTIVITY_TRACKING_README.md`

---

## ✅ Quick Deployment

```bash
# 1. Build project
dotnet build

# 2. Restart API
dotnet run

# 3. Add attributes to controllers

# 4. Test!
curl https://localhost:44346/api/v1/property \
  -X POST \
  -H "Authorization: Bearer TOKEN" \
  -d '{"propertyName":"Test"}'

# 5. Verify
curl https://localhost:44346/api/v1/activity/recent?count=1
```

---

**⚡ Phase 2: Automatic Logging - Ready to Use! ⚡**

*One attribute = Complete activity tracking* ✨
