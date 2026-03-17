# Centralized Activity Message Builder

## 🎯 Overview

All activity display message logic is now centralized in **`ActivityDisplayMessageBuilder`** service. This makes it:

- ✅ **Reusable** - Use from filters, background jobs, manual logging, etc.
- ✅ **Testable** - Easy to unit test message building independently
- ✅ **Maintainable** - All message formats in one place
- ✅ **Consistent** - Same message format everywhere
- ✅ **Extensible** - Easy to add new message types

## 📂 File Location

```
classfiles/Application/UserActivity/Services/ActivityDisplayMessageBuilder.cs
```

## 🏗️ Architecture

### Before (Scattered)
```
ActivityLoggingActionFilter.cs (300+ lines of message logic)
├─ BuildDisplayMessage()
├─ GetUserDisplayName()
├─ GetActivityVerb()
├─ GetEntityName()
├─ FormatEntityType()
├─ GetPropertyContext()
├─ ResolvePropertyName()
└─ GetSearchContext()

❌ Hard to reuse
❌ Hard to test
❌ Hard to maintain
```

### After (Centralized)
```
ActivityDisplayMessageBuilder.cs (All message logic)
├─ BuildMessageAsync() - Main entry point
├─ IsImportantOperation()
├─ GetActivityVerb()
├─ FormatEntityType()
├─ GetPropertyContextAsync()
├─ ResolvePropertyNameAsync()
├─ GetSearchContext()
└─ BuildSimpleMessage() - For quick logging

ActivityLoggingActionFilter.cs (Clean!)
└─ Uses _messageBuilder.BuildMessageAsync()

✅ Reusable everywhere
✅ Easy to test
✅ Single source of truth
```

## 🔧 Registration (Already Done)

The service is registered in `classfiles/Infrastructure/ApplicationDependencies/Startup.cs`:

```csharp
services.AddScoped<ActivityDisplayMessageBuilder>();
```

## 📖 Usage Examples

### 1. **From ActivityLoggingActionFilter (Current)**

```csharp
public class ActivityLoggingActionFilter : IAsyncActionFilter
{
    private readonly ActivityDisplayMessageBuilder _messageBuilder;

    public ActivityLoggingActionFilter(ActivityDisplayMessageBuilder messageBuilder)
    {
        _messageBuilder = messageBuilder;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // ... extract metadata ...

        // Build display message
        var displayMessage = await _messageBuilder.BuildMessageAsync(
            activityType,
            entityType,
            entityId,
            username,
            metadata);

        // Log activity with displayMessage
    }
}
```

### 2. **From Manual Logging (Background Jobs, Services)**

```csharp
public class BookingService
{
    private readonly ActivityDisplayMessageBuilder _messageBuilder;
    private readonly UserActivityService _activityService;

    public BookingService(
        ActivityDisplayMessageBuilder messageBuilder,
        UserActivityService activityService)
    {
        _messageBuilder = messageBuilder;
        _activityService = activityService;
    }

    public async Task ProcessBooking(Booking booking, string username)
    {
        // ... process booking ...

        // Build metadata
        var metadata = new Dictionary<string, object>
        {
            { "response_Name", booking.GuestName },
            { "selected_PropertyId", booking.PropertyId.ToString() }
        };

        // Build human-readable message
        var displayMessage = await _messageBuilder.BuildMessageAsync(
            ActivityType.Create,
            "Booking",
            booking.Id,
            username,
            metadata);

        // Log: "john created 'Smith Family Booking' in Sunset Villa"
        await _activityService.LogActivityAsync(
            userId: booking.CreatedBy,
            username: username,
            activityType: ActivityType.Create,
            entityType: "Booking",
            entityId: booking.Id,
            action: "Created Booking",
            description: $"Created booking #{booking.Id}",
            metadata: metadata,
            displayMessage: displayMessage  // ← Human-readable!
        );
    }
}
```

### 3. **Simple Message (No Async, No Property Resolution)**

```csharp
// For quick logging where property context doesn't matter
var simpleMessage = _messageBuilder.BuildSimpleMessage(
    ActivityType.View,
    "Reports",
    "john@example.com"
);
// Result: "john viewed Reports"
```

### 4. **Custom Message Building**

```csharp
// Build custom metadata
var metadata = new Dictionary<string, object>
{
    { "response_Name", "Annual Revenue Report" },
    { "selected_PropertyId", "5" },
    { "query_SearchItem", "revenue" }
};

var message = await _messageBuilder.BuildMessageAsync(
    ActivityType.Export,
    "Report",
    null,
    "sarah@example.com",
    metadata
);
// Result: "sarah exported 'Annual Revenue Report' in Sunset Villa"
```

## 🎨 Public Methods

### 1. `BuildMessageAsync()` - Full Featured

```csharp
/// <summary>
/// Builds a human-readable activity message with all features
/// </summary>
public async Task<string> BuildMessageAsync(
    ActivityType activityType,
    string? entityType,
    int? entityId,
    string username,
    Dictionary<string, object>? metadata = null)
```

**Returns:**
- Views: `"john viewed Dashboards"`
- Important: `"sarah created 'Ocean Suite' in Marina Bay Resort"`
- Search: `"mike searched Bookings for 'conference'"`

### 2. `BuildSimpleMessage()` - Quick & Synchronous

```csharp
/// <summary>
/// Builds a simple message without async property resolution
/// </summary>
public string BuildSimpleMessage(
    ActivityType activityType,
    string? entityType,
    string username)
```

**Returns:** `"john viewed Reports"`

### 3. `IsImportantOperation()` - Classification

```csharp
/// <summary>
/// Determines if an activity type requires detailed logging
/// </summary>
public bool IsImportantOperation(ActivityType activityType)
```

**Important Operations:**
- Create, Update, Delete
- BulkCreate, BulkUpdate, BulkDelete
- StatusChange, Approve, Reject

**Not Important:**
- View, PageView, Login, Logout, Export, Download

### 4. `GetActivityVerb()` - Verb Mapping

```csharp
/// <summary>
/// Gets appropriate verb for activity type
/// </summary>
public string GetActivityVerb(ActivityType activityType)
```

**Examples:**
- `Create` → `"created"`
- `Update` → `"updated"`
- `View` → `"viewed"`
- `Search` → `"searched"`

### 5. `FormatEntityType()` - Entity Formatting

```csharp
/// <summary>
/// Formats entity type into readable text
/// </summary>
public string FormatEntityType(string entityType, bool isImportantOperation)
```

**Examples:**
- Views: `"Dashboards"`, `"Properties"`
- Important: `"Property"`, `"Room"`, `"Booking"`

### 6. `GetPropertyContextAsync()` - Property Resolution

```csharp
/// <summary>
/// Gets property context message with name resolution
/// </summary>
public async Task<string> GetPropertyContextAsync(
    Dictionary<string, object> metadata,
    bool shortFormat = false)
```

**Examples:**
- Short: `"in Sunset Villa"`
- Long: `"while working on 'Sunset Villa' property"`

### 7. `ResolvePropertyNameAsync()` - Name Lookup

```csharp
/// <summary>
/// Resolves property ID to property name from database
/// </summary>
public async Task<string?> ResolvePropertyNameAsync(int propertyId)
```

**Returns:** Property name or null if not found

### 8. `GetSearchContext()` - Search Term Extraction

```csharp
/// <summary>
/// Gets search/filter context from metadata
/// </summary>
public string GetSearchContext(Dictionary<string, object> metadata)
```

**Returns:** `"for 'conference'"` or empty string

## 🧪 Unit Testing Examples

### Test Message Building

```csharp
[TestClass]
public class ActivityDisplayMessageBuilderTests
{
    private Mock<IPropertyRepository> _mockPropertyRepo;
    private ActivityDisplayMessageBuilder _builder;

    [TestInitialize]
    public void Setup()
    {
        _mockPropertyRepo = new Mock<IPropertyRepository>();
        _builder = new ActivityDisplayMessageBuilder(_mockPropertyRepo.Object);
    }

    [TestMethod]
    public async Task BuildMessage_ViewOperation_ReturnsShortMessage()
    {
        // Arrange
        var metadata = new Dictionary<string, object>();

        // Act
        var message = await _builder.BuildMessageAsync(
            ActivityType.View,
            "Dashboards",
            null,
            "john@example.com",
            metadata
        );

        // Assert
        Assert.AreEqual("john viewed Dashboards", message);
    }

    [TestMethod]
    public async Task BuildMessage_CreateWithProperty_ReturnsDetailedMessage()
    {
        // Arrange
        _mockPropertyRepo.Setup(x => x.GetByIdAsync(5))
            .ReturnsAsync(new Property("Sunset Villa", true, new List<Room>()));

        var metadata = new Dictionary<string, object>
        {
            { "response_Name", "Ocean View Suite" },
            { "selected_PropertyId", "5" }
        };

        // Act
        var message = await _builder.BuildMessageAsync(
            ActivityType.Create,
            "Room",
            null,
            "sarah@example.com",
            metadata
        );

        // Assert
        Assert.AreEqual("sarah created 'Ocean View Suite' in Sunset Villa", message);
    }

    [TestMethod]
    public void IsImportantOperation_Create_ReturnsTrue()
    {
        Assert.IsTrue(_builder.IsImportantOperation(ActivityType.Create));
    }

    [TestMethod]
    public void IsImportantOperation_View_ReturnsFalse()
    {
        Assert.IsFalse(_builder.IsImportantOperation(ActivityType.View));
    }

    [TestMethod]
    public void GetActivityVerb_Create_ReturnsCreated()
    {
        Assert.AreEqual("created", _builder.GetActivityVerb(ActivityType.Create));
    }
}
```

## 🔄 Migration from Old Code

If you have existing code using the old filter methods directly, update it to use the centralized builder:

### Before
```csharp
// Old: Message logic scattered in filter
var message = await BuildDisplayMessage(activityType, entityType, entityId, username, metadata);
```

### After
```csharp
// New: Use centralized builder
var message = await _messageBuilder.BuildMessageAsync(activityType, entityType, entityId, username, metadata);
```

## 🎯 Benefits Recap

### 1. **Single Source of Truth**
All message formatting rules in one place. Want to change how entity types are formatted? Update `FormatEntityType()` once, applies everywhere.

### 2. **Reusability**
Use from:
- ✅ ActivityLoggingActionFilter
- ✅ Background jobs
- ✅ Manual logging in services
- ✅ Bulk import/export operations
- ✅ Migration scripts
- ✅ Admin tools

### 3. **Testability**
```csharp
// Easy to unit test without ActionFilter complexity
var builder = new ActivityDisplayMessageBuilder(mockPropertyRepo.Object);
var message = await builder.BuildMessageAsync(...);
Assert.AreEqual("expected message", message);
```

### 4. **Maintainability**
```csharp
// Add new activity type? Just update the verb mapping
public string GetActivityVerb(ActivityType activityType)
{
    return activityType switch
    {
        // ... existing mappings ...
        ActivityType.Archive => "archived",  // ← Add new!
        _ => "interacted with"
    };
}
```

### 5. **Consistency**
Same message format whether logging from:
- Web API requests
- Background jobs
- Console applications
- Migration scripts

## 📊 Before & After Comparison

### Code Size Reduction

**Before:**
```
ActivityLoggingActionFilter.cs: ~800 lines
├─ Filter logic: 400 lines
└─ Message logic: 400 lines  ← Duplicated if used elsewhere
```

**After:**
```
ActivityLoggingActionFilter.cs: ~400 lines (just filter logic)
ActivityDisplayMessageBuilder.cs: ~350 lines (reusable!)
Total: 750 lines (50 lines saved, but more importantly: reusable!)
```

### Dependency Injection

**Before:**
```csharp
public ActivityLoggingActionFilter(
    UserActivityService activityService,
    IPropertyRepository propertyRepository  // ← Direct dependency
)
```

**After:**
```csharp
public ActivityLoggingActionFilter(
    UserActivityService activityService,
    ActivityDisplayMessageBuilder messageBuilder  // ← Clean abstraction
)
```

## 🚀 Future Enhancements

### 1. **Message Templates**
```csharp
public class ActivityDisplayMessageBuilder
{
    private readonly Dictionary<ActivityType, string> _templates = new()
    {
        { ActivityType.Create, "{user} created {entity} in {property}" },
        { ActivityType.Update, "{user} updated {entity} in {property}" },
        // ...
    };
}
```

### 2. **Localization Support**
```csharp
public async Task<string> BuildMessageAsync(
    ActivityType activityType,
    string? entityType,
    int? entityId,
    string username,
    Dictionary<string, object>? metadata = null,
    string culture = "en-US")  // ← Add culture parameter
{
    // Use resource files for translations
}
```

### 3. **Rich HTML Messages**
```csharp
public async Task<string> BuildRichMessageAsync(...)
{
    return $"<span class='user'>{userName}</span> " +
           $"<span class='verb'>{verb}</span> " +
           $"<a href='/entity/{entityId}'>{entityName}</a>";
}
```

### 4. **Message Caching**
```csharp
private readonly IMemoryCache _cache;

public async Task<string> BuildMessageAsync(...)
{
    var cacheKey = $"property_{propertyId}";
    var propertyName = await _cache.GetOrCreateAsync(cacheKey, ...);
}
```

## ✅ Summary

You now have a **centralized, reusable, testable message builder** that:

1. ✅ Lives in one place: `ActivityDisplayMessageBuilder.cs`
2. ✅ Is registered in DI: `services.AddScoped<ActivityDisplayMessageBuilder>()`
3. ✅ Can be used anywhere: Filters, services, background jobs, etc.
4. ✅ Is easy to test: Mock `IPropertyRepository`, test messages
5. ✅ Is easy to maintain: One place to update message formats
6. ✅ Is consistent: Same messages everywhere

**No more scattered message logic!** 🎉
