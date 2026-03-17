# Activity Logging - Quick Start Guide for Developers 🚀

## TL;DR

**Want to log user activities?** Use attributes on controllers. Messages are automatic.
**Want custom messages?** Use centralized templates in one of two files.

---

## For Backend Developers (C#)

### 1. Automatic Logging (Recommended)
Just add attributes to your controller methods:

```csharp
[HttpGet("{id}")]
[LogView(EntityType = "Property")]
public async Task<ActionResult<PropertyDto>> GetById(int id)
{
    // Your code here
    // ActivityLoggingActionFilter automatically logs this as:
    // "john.doe viewed Property #123 in Grand Hotel"
}

[HttpPost]
[LogCreate(EntityType = "Room", EntityIdProperty = "Id")]
public async Task<ActionResult<int>> CreateRoom(CreateRoomCommand command)
{
    // Your code here
    // Automatically logs:
    // "jane.smith created Room #456 in Sunset Villa"
}

[HttpPut("{id}")]
[LogUpdate(EntityType = "Booking")]
public async Task<ActionResult> UpdateBooking(int id, UpdateBookingCommand command)
{
    // Your code here
    // Automatically logs:
    // "admin updated Booking #789 in Grand Hotel"
}

[HttpDelete("{id}")]
[LogDelete(EntityType = "Room")]
public async Task<ActionResult> DeleteRoom(int id)
{
    // Your code here
    // Automatically logs:
    // "manager deleted Room #101 in Ocean Resort"
}
```

**That's it!** The filter automatically:
- ✅ Extracts username from JWT token
- ✅ Captures entity ID from route or response
- ✅ Adds property context from X-Selected-Property header
- ✅ Builds human-readable message
- ✅ Saves to MongoDB with metadata
- ✅ Never crashes your request (wrapped in try-catch)

### 2. Custom Messages (If Needed)

**Where?** → `classfiles/Application/UserActivity/Constants/ActivityMessageTemplates.cs`

**Add your template:**
```csharp
public const string RoomStatusChanged = "{0} changed room '{1}' status to {2}";
```

**Use in ActivityDisplayMessageBuilder:**
```csharp
var message = string.Format(
    ActivityMessageTemplates.RoomStatusChanged,
    username,      // "john.doe"
    roomName,      // "Ocean View Suite"
    newStatus      // "Available"
);
// Result: "john.doe changed room 'Ocean View Suite' status to Available"
```

### 3. Available Attributes

| Attribute | Use Case | Example |
|-----------|----------|---------|
| `[LogView]` | GET requests, viewing data | View property details, dashboard |
| `[LogCreate]` | POST requests, creating data | Create room, add booking |
| `[LogUpdate]` | PUT/PATCH, updating data | Update property, modify room |
| `[LogDelete]` | DELETE requests, removing data | Delete booking, remove user |
| `[LogSearch]` | Search operations | Search properties, filter rooms |
| `[LogExport]` | Export/download operations | Export to Excel, generate PDF |

### 4. Optional Attribute Parameters

```csharp
[LogCreate(
    EntityType = "Room",                // Default: Controller name
    EntityIdProperty = "RoomId",        // Default: "Id"
    Module = "RoomManagement",          // Default: Controller name
    Description = "Created new room",   // Default: Auto-generated
    LogOnFailure = true                 // Default: false (only log success)
)]
```

---

## For Frontend Developers (TypeScript/Angular)

### 1. Automatic Context (Already Working)
Two interceptors automatically add context to all HTTP requests:
- **property-context.interceptor** → Adds `X-Selected-Property` header
- **activity-message.interceptor** → Adds `X-Activity-Message` header (if set)

**You don't need to do anything** - backend automatically logs most operations.

### 2. Custom Messages (When You Want Specific Wording)

**Where?** → `app/src/app/constants/activity-messages.ts`

**Import and use:**
```typescript
import { ActivityMessages } from '../../constants/activity-messages';
import { ActivityMessageService } from '../../services/activity-message.service';

export class MyComponent {
  private activityMessage = inject(ActivityMessageService);

  onPropertySelect(property: Property): void {
    // Set custom message
    const message = ActivityMessages.PROPERTY_SELECTED(property.name);
    const headers = this.activityMessage.createHeaders(message);
    
    // Make API call with custom message
    this.http.post('/api/property/select', { id: property.id }, { headers })
      .subscribe(...);
  }

  onRoomCreate(room: Room): void {
    const message = ActivityMessages.ROOM_CREATED(room.name, room.floorNumber);
    const headers = this.activityMessage.createHeaders(message);
    
    this.http.post('/api/rooms', room, { headers })
      .subscribe(...);
  }

  onBookingCancel(bookingId: number, reason: string): void {
    const message = ActivityMessages.BOOKING_CANCELLED(bookingId, reason);
    const headers = this.activityMessage.createHeaders(message);
    
    this.http.delete(`/api/bookings/${bookingId}`, { headers })
      .subscribe(...);
  }
}
```

### 3. Available Message Functions

**Property Messages:**
```typescript
ActivityMessages.PROPERTY_SELECTED(name)
ActivityMessages.PROPERTIES_BULK_SELECTED(count, names)
ActivityMessages.PROPERTY_CREATED(name)
ActivityMessages.PROPERTY_UPDATED(name)
ActivityMessages.PROPERTY_DELETED(name)
```

**Room Messages:**
```typescript
ActivityMessages.ROOM_CREATED(name, floor)
ActivityMessages.ROOM_CREATED_SIMPLE(name)
ActivityMessages.ROOM_UPDATED(name, changes)
ActivityMessages.ROOM_UPDATED_SIMPLE(name)
ActivityMessages.ROOM_DELETED(name)
```

**Booking Messages:**
```typescript
ActivityMessages.BOOKING_CREATED(guestName, dates)
ActivityMessages.BOOKING_CREATED_SIMPLE(guestName)
ActivityMessages.BOOKING_UPDATED(bookingId)
ActivityMessages.BOOKING_CANCELLED(bookingId, reason)
ActivityMessages.BOOKING_CANCELLED_SIMPLE(bookingId)
ActivityMessages.BOOKING_DELETED(bookingId)
```

**Dashboard & Widget Messages:**
```typescript
ActivityMessages.DASHBOARD_VIEWED(dashboardName)
ActivityMessages.DASHBOARDS_VIEWED()
ActivityMessages.WIDGET_ADDED(widgetName)
ActivityMessages.WIDGET_REMOVED(widgetName)
ActivityMessages.WIDGET_LIBRARY_VIEWED()
```

**Export & Search Messages:**
```typescript
ActivityMessages.DATA_EXPORTED(type, count, format)
ActivityMessages.DATA_EXPORTED_SIMPLE(type, count)
ActivityMessages.SEARCH_PERFORMED(term)
ActivityMessages.SEARCH_IN_ENTITY(term, entityType)
```

**User & Settings Messages:**
```typescript
ActivityMessages.USER_CREATED(username)
ActivityMessages.USER_UPDATED(username)
ActivityMessages.USER_DELETED(username)
ActivityMessages.SETTINGS_UPDATED(settingName)
ActivityMessages.SETTINGS_VIEWED(settingName)
ActivityMessages.REPORT_GENERATED(reportName)
ActivityMessages.REPORT_VIEWED(reportName)
```

### 4. When to Use Custom Messages vs Automatic?

**Use Automatic (Default):**
- ✅ Standard CRUD operations (most cases)
- ✅ Simple actions where auto-generated message is good enough
- ✅ When you don't care about exact wording

**Use Custom Messages:**
- ✅ Important business operations (booking cancellations, approvals)
- ✅ When you want specific wording for reports
- ✅ Multi-step operations (e.g., "Bulk imported 50 rooms")
- ✅ Operations with additional context (e.g., cancellation reason)

---

## Common Scenarios

### Scenario 1: Simple CRUD Controller
```csharp
// Backend - Just add attributes, that's it!
[ApiController]
[Route("api/v1/properties")]
public class PropertyController : ControllerBase
{
    [HttpGet]
    [LogView(EntityType = "Property")]
    public async Task<ActionResult<List<PropertyDto>>> GetAll() { ... }
    
    [HttpGet("{id}")]
    [LogView(EntityType = "Property")]
    public async Task<ActionResult<PropertyDto>> GetById(int id) { ... }
    
    [HttpPost]
    [LogCreate(EntityType = "Property", EntityIdProperty = "Id")]
    public async Task<ActionResult<int>> Create(CreatePropertyCommand cmd) { ... }
    
    [HttpPut("{id}")]
    [LogUpdate(EntityType = "Property")]
    public async Task<ActionResult> Update(int id, UpdatePropertyCommand cmd) { ... }
    
    [HttpDelete("{id}")]
    [LogDelete(EntityType = "Property")]
    public async Task<ActionResult> Delete(int id) { ... }
}

// Frontend - Nothing needed, interceptors handle it!
// Logs are automatic for all CRUD operations
```

### Scenario 2: Custom Message for Important Action
```typescript
// Frontend
onBookingCancel(booking: Booking): void {
  const reason = prompt('Cancellation reason:');
  
  // Custom message with reason
  const message = ActivityMessages.BOOKING_CANCELLED(booking.id, reason);
  const headers = this.activityMessage.createHeaders(message);
  
  this.http.post(`/api/bookings/${booking.id}/cancel`, 
    { reason }, 
    { headers }
  ).subscribe(() => {
    alert('Booking cancelled and logged!');
  });
}
```

```csharp
// Backend
[HttpPost("{id}/cancel")]
[LogUpdate(EntityType = "Booking")] // Logs with client message!
public async Task<ActionResult> CancelBooking(int id, CancelBookingCommand cmd)
{
    // Client message in header overrides auto-generated one
    // MongoDB will store: "admin cancelled booking #123: Customer request"
    return Ok();
}
```

### Scenario 3: Property Context Automatically Included
```typescript
// Frontend - Property context interceptor automatically adds header
// No extra code needed!

this.http.get('/api/rooms')
  .subscribe(rooms => {
    // Backend automatically logs:
    // "john.doe viewed Rooms in Grand Hotel"
    // PropertyId is captured from X-Selected-Property header
  });
```

---

## Adding New Message Templates

### Backend (C#)

**File:** `classfiles/Application/UserActivity/Constants/ActivityMessageTemplates.cs`

```csharp
// Add your template
public const string InvoiceGenerated = "{0} generated invoice #{1} for {2}";

// Use in builder or controller
var message = string.Format(
    ActivityMessageTemplates.InvoiceGenerated,
    username,
    invoiceId,
    customerName
);
```

### Frontend (TypeScript)

**File:** `app/src/app/constants/activity-messages.ts`

```typescript
// Add your function
/**
 * When user generates an invoice
 * @param invoiceId - Invoice ID
 * @param customerName - Customer name
 */
static readonly INVOICE_GENERATED = (invoiceId: number, customerName: string) => 
  `Generated invoice #${invoiceId} for ${customerName}`;

// Use in component
const message = ActivityMessages.INVOICE_GENERATED(123, 'Acme Corp');
```

---

## Viewing Logs

### MongoDB Query
```javascript
// Find all activities for a property
db.UserActivities.find({ PropertyId: 123 })

// Find all activities by user
db.UserActivities.find({ Username: "john.doe" })

// Find all room creations
db.UserActivities.find({ ActivityType: "Create", EntityType: "Room" })

// Find activities with display message
db.UserActivities.find({ DisplayMessage: /created room/i })

// Search metadata
db.UserActivities.find({ "Metadata.param_Name": "Ocean View Suite" })
```

### Example Log Document
```json
{
  "_id": ObjectId("..."),
  "UserId": 42,
  "Username": "john.doe",
  "ActivityType": "Create",
  "EntityType": "Room",
  "EntityId": 456,
  "PropertyId": 123,
  "Action": "Create Room",
  "Description": "Create Room #456",
  "DisplayMessage": "john.doe added 'Ocean View Suite' room to 3rd floor in Grand Hotel",
  "Module": "Room",
  "Timestamp": ISODate("2024-01-15T10:30:00Z"),
  "IpAddress": "192.168.1.100",
  "UserAgent": "Mozilla/5.0...",
  "IsSuccess": true,
  "DurationMs": 245,
  "Metadata": {
    "param_Name": "Ocean View Suite",
    "param_FloorNumber": "3",
    "response_Id": "456",
    "selected_PropertyId": "123"
  }
}
```

---

## Troubleshooting

### "Activity not logging"
✅ Check attribute is on controller method
✅ Verify JWT token has valid username claim
✅ Check MongoDB connection is working
✅ Look for errors in console (should never crash, but logs error)

### "Wrong username logged"
✅ Check JWT token contains `unique_name` claim
✅ Verify token is valid and not expired
✅ Check authentication is working

### "PropertyId is null"
✅ Ensure property is selected in frontend
✅ Check localStorage has 'selectedPropertyIds'
✅ Verify property-context.interceptor is registered
✅ Confirm X-Selected-Property header is sent

### "Custom message not used"
✅ Check message is set before API call
✅ Verify X-Activity-Message header is in request
✅ Confirm activity-message.interceptor is registered
✅ Check message is not empty/null

---

## Best Practices

### DO ✅
- Use attributes for standard CRUD operations
- Use custom messages for important business actions
- Keep messages short and descriptive
- Include entity names in messages
- Add custom metadata for searchability
- Test that logs appear in MongoDB

### DON'T ❌
- Don't log sensitive data (passwords, tokens, etc.)
- Don't add attributes to internal/helper methods
- Don't make logging synchronous/blocking
- Don't throw exceptions in logging code
- Don't hardcode strings in components (use templates)

---

## Need Help?

**Documentation:**
- Full system docs: `ACTIVITY_LOGGING_SYSTEM_DOCUMENTATION.md`
- Implementation details: `CENTRALIZED_TEMPLATES_IMPLEMENTATION.md`
- Old system removal: `OLD_ACCESS_LOG_REMOVAL_PLAN.md`

**Code Files:**
- Server templates: `classfiles/Application/UserActivity/Constants/ActivityMessageTemplates.cs`
- Client templates: `app/src/app/constants/activity-messages.ts`
- Filter: `classfiles/Infrastructure/Filters/ActivityLoggingActionFilter.cs`
- Builder: `classfiles/Application/UserActivity/Services/ActivityDisplayMessageBuilder.cs`

**Questions?** Check the comprehensive documentation or search existing logs for examples!

---

## Summary

1. **For most operations**: Just add `[LogView]`, `[LogCreate]`, `[LogUpdate]`, or `[LogDelete]` attributes
2. **For custom messages**: Use templates from `ActivityMessageTemplates.cs` (backend) or `activity-messages.ts` (frontend)
3. **Property context**: Automatically captured by interceptors
4. **View logs**: Query MongoDB `UserActivities` collection
5. **Add new messages**: Update the centralized template files

**That's it! Happy logging! 🎉**
