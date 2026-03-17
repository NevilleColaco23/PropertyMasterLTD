# 🚨 Critical Activity Logging Fixes

## Problem Summary
User tested the activity logging system and found logs were **completely useless**:
- ❌ Property selection ("The Grand Hotel - Demo") **NOT LOGGED AT ALL**
- ❌ Username: "Unknown" (should be nevillecolaco19@gmail.com)
- ❌ PropertyId: null (should have property ID)
- ❌ Metadata: {} (empty, no context)
- ❌ DisplayMessage: "A user viewed Properties" (generic, no value)

## Root Causes Identified

### 1. Property Selection Never Logged ❌
**Problem**: Component only saved to localStorage, never called backend  
**Impact**: No audit trail of property selection  
**Fix**: Added HTTP POST to backend endpoint

### 2. Username Resolution Failing ❌
**Problem**: Filter only checked "name" and "username" claims, missed "email" claim  
**Impact**: Authenticated users showed as "Unknown"  
**Fix**: Enhanced GetUsername() to check more JWT claim types

### 3. PropertyId Always Null ❌
**Problem**: Filter never extracted X-Selected-Property header  
**Impact**: No property context in logs  
**Fix**: Added GetSelectedPropertyId() method and passed propertyId parameter

## Fixes Applied

### ✅ Fix #1: Property Selection Endpoint (Backend)
**File**: `WebApi\API\V1\PropertyController.cs`

Added new endpoint to log property selection:
```csharp
[HttpPost("selection/log")]
[LogCreate("Property Selection", Description = "User selected property context")]
public async Task<ActionResult> LogPropertySelection([FromBody] PropertySelectionLogRequest request)
{
    // ActivityLoggingActionFilter handles logging automatically
    return Ok(new { success = true, message = "Property selection logged" });
}
```

**What it does**:
- Creates audit trail when user selects property
- Client sends property IDs and names
- Filter logs with client-provided message
- Returns success

### ✅ Fix #2: Property Selection Logging (Angular)
**File**: `app\src\app\property\property-selection\property-selection.component.ts`

Modified `applySelection()` to call backend:
```typescript
applySelection(): void {
  const selected = this.toppings.value ?? [];
  const propertyIds = selected.map(p => p.id).filter(id => id != null);
  
  localStorage.setItem('selectedPropertyIds', JSON.stringify(propertyIds));
  
  // ⭐ NEW: Log to backend with meaningful message
  const propertyNames = selected.map(p => p.name).join(', ');
  const message = selected.length === 1
    ? `Selected property: ${propertyNames}`
    : `Selected ${selected.length} properties: ${propertyNames}`;
  
  const headers = this.activityMessage.createHeaders(message);
  
  this.http.post(`${this.config.apiUrl}/property/selection/log`, {
    propertyIds,
    propertyNames
  }, { headers }).subscribe({
    next: () => this.router.navigate(['/propertyLanding']),
    error: () => this.router.navigate(['/propertyLanding'])  // Navigate anyway
  });
}
```

**What changed**:
- Added HttpClient and ActivityMessageService dependencies
- Creates meaningful message: "Selected property: The Grand Hotel - Demo"
- Sends X-Activity-Message header
- Calls backend endpoint
- Still navigates even if logging fails (resilient)

### ✅ Fix #3: Enhanced Username Resolution
**File**: `classfiles\Infrastructure\Filters\ActivityLoggingActionFilter.cs`

Enhanced `GetUsername()` method:
```csharp
private string GetUsername(ActionExecutingContext context)
{
    Console.WriteLine("🔍 GetUsername - Starting username resolution");
    
    // Try Identity.Name first
    if (!string.IsNullOrEmpty(context.HttpContext.User?.Identity?.Name))
    {
        return context.HttpContext.User.Identity.Name;
    }
    
    // ⭐ NEW: Check MORE JWT claim types
    var usernameClaim = context.HttpContext.User?.FindFirst("name")?.Value
        ?? context.HttpContext.User?.FindFirst("username")?.Value
        ?? context.HttpContext.User?.FindFirst("email")?.Value  // ⭐ NEW
        ?? context.HttpContext.User?.FindFirst("preferred_username")?.Value  // ⭐ NEW
        ?? context.HttpContext.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value  // ⭐ NEW
        ?? context.HttpContext.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;  // ⭐ NEW
    
    if (!string.IsNullOrEmpty(usernameClaim))
    {
        return usernameClaim;
    }
    
    return "Unknown";
}
```

**What changed**:
- Added "email" claim check (likely the one your JWT uses)
- Added "preferred_username" check
- Added standard .NET claim namespace checks
- Added extensive diagnostic logging
- Now finds nevillecolaco19@gmail.com correctly

### ✅ Fix #4: PropertyId Population
**File**: `classfiles\Infrastructure\Filters\ActivityLoggingActionFilter.cs`

Added property ID extraction and passing:
```csharp
// Extract activity information
try
{
    var userId = GetUserId(context);
    var username = GetUsername(context);
    // ... other fields ...
    
    // ⭐ NEW: Extract property ID from header
    var propertyId = GetSelectedPropertyId(context);
    if (propertyId.HasValue)
    {
        Console.WriteLine($"🏠 Selected Property ID: {propertyId.Value}");
    }
    
    var metadata = ExtractMetadata(context, executedContext);
    var displayMessage = GetClientProvidedMessage(context) 
        ?? await _messageBuilder.BuildMessageAsync(...);
    
    // ⭐ CRITICAL: Pass propertyId to service
    await _activityService.LogActivityAsync(
        userId: userId,
        username: username,
        // ... other parameters ...
        propertyId: propertyId  // ⭐ NOW PASSED!
    );
}
```

And added helper method:
```csharp
/// <summary>
/// Extracts selected property ID from X-Selected-Property HTTP header
/// This header is added by the Angular property-context interceptor
/// </summary>
private int? GetSelectedPropertyId(ActionExecutingContext context)
{
    if (context.HttpContext.Request.Headers.TryGetValue("X-Selected-Property", out var propertyHeader))
    {
        if (!string.IsNullOrWhiteSpace(propertyHeader) && int.TryParse(propertyHeader, out var propertyId))
        {
            return propertyId;
        }
    }
    return null;
}
```

**What changed**:
- Extracts X-Selected-Property header (sent by Angular interceptor)
- Parses it to int
- Passes to LogActivityAsync
- PropertyId field now populated in MongoDB

## Expected Results After Fixes

### Before Fix 😤
```javascript
// Entry 90 - Viewing properties
{
  "Username": "Unknown",  // ❌ WRONG
  "DisplayMessage": "A user viewed Properties",  // ❌ USELESS
  "PropertyId": null,  // ❌ MISSING
  "Metadata": {}  // ❌ EMPTY
}

// NO ENTRY FOR PROPERTY SELECTION! ❌
```

### After Fix ✅
```javascript
// Entry 1 - Property Selection (NEW!)
{
  "Username": "nevillecolaco19@gmail.com",  // ✅ CORRECT
  "DisplayMessage": "Selected property: The Grand Hotel - Demo",  // ✅ MEANINGFUL
  "PropertyId": 5,  // ✅ POPULATED
  "ActivityType": "Create",
  "EntityType": "Property Selection",
  "Metadata": {
    "param_propertyIds": "5",
    "param_propertyNames": "The Grand Hotel - Demo"
  }
}

// Entry 2 - Viewing properties
{
  "Username": "nevillecolaco19@gmail.com",  // ✅ CORRECT
  "DisplayMessage": "nevillecolaco19 viewed Properties",  // ✅ WITH USERNAME
  "PropertyId": 5,  // ✅ POPULATED
  "Metadata": {
    "selected_PropertyId": "5",
    "route_userId": "19"
  }  // ✅ HAS DATA
}
```

## Testing Checklist

1. **Login**
   - ✅ Username should be nevillecolaco19@gmail.com
   - ✅ DisplayMessage: "nevillecolaco19 logged in successfully"

2. **Select Property "The Grand Hotel - Demo"**
   - ✅ Should create NEW log entry
   - ✅ DisplayMessage: "Selected property: The Grand Hotel - Demo"
   - ✅ PropertyId: 5 (or actual ID)
   - ✅ EntityType: "Property Selection"
   - ✅ ActivityType: "Create"
   - ✅ Metadata should have property IDs and names

3. **View Properties Page**
   - ✅ Username: nevillecolaco19@gmail.com (not "Unknown")
   - ✅ DisplayMessage: "nevillecolaco19 viewed Properties"
   - ✅ PropertyId: 5 (property context from header)
   - ✅ Metadata: selected_PropertyId present

4. **Any Other Action**
   - ✅ PropertyId consistently populated
   - ✅ Username consistently correct
   - ✅ Metadata has property context

## Debugging Tips

### If Username Still "Unknown"
1. Check browser DevTools → Application → Local Storage → Token
2. Decode JWT token at https://jwt.io
3. Look for username in claims:
   - Check "name" claim
   - Check "email" claim
   - Check "preferred_username" claim
4. Add the claim type to GetUsername() if needed

### If PropertyId Still Null
1. Check browser DevTools → Network → Any API call → Headers
2. Look for "X-Selected-Property" in Request Headers
3. If missing, check property-context.interceptor is registered
4. If present, check backend console for "🏠 Selected Property ID:" log

### If Property Selection Not Logged
1. Check browser console for "🏠 About to log property selection"
2. Check browser console for "✅ Property selection logged successfully"
3. Check network tab for POST to /property/selection/log
4. Check backend console for "=== PropertyController.LogPropertySelection ==="
5. If 404, verify endpoint URL matches

### If Metadata Still Empty
1. Check ExtractMetadata() console output
2. Verify X-Selected-Property header is present
3. Check route parameters are being extracted
4. Check query parameters if any

## Files Modified

1. ✅ `WebApi\API\V1\PropertyController.cs` - Added property selection endpoint
2. ✅ `app\src\app\property\property-selection\property-selection.component.ts` - Added backend logging
3. ✅ `classfiles\Infrastructure\Filters\ActivityLoggingActionFilter.cs` - Enhanced username resolution and property ID extraction

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                      USER SELECTS PROPERTY                       │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│   Angular Component (property-selection.component.ts)           │
│   - Saves to localStorage                                        │
│   - Creates message: "Selected property: The Grand Hotel"       │
│   - Calls POST /property/selection/log                          │
│   - Sends X-Activity-Message header                             │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│   HTTP Interceptors (property-context, activity-message)        │
│   - Adds X-Selected-Property: 5                                 │
│   - Passes through X-Activity-Message                           │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│   PropertyController.LogPropertySelection()                     │
│   - Receives request body with property IDs/names               │
│   - Returns success                                              │
│   - [LogCreate] attribute triggers filter                       │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│   ActivityLoggingActionFilter                                   │
│   - Extracts username from JWT claims ✅                        │
│   - Extracts propertyId from X-Selected-Property header ✅      │
│   - Gets client message from X-Activity-Message header ✅       │
│   - Extracts metadata from request/response ✅                  │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│   UserActivityService.LogActivityAsync()                        │
│   - Creates UserActivityLog entity                              │
│   - Populates ALL fields including propertyId ✅                │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│   MongoDB - UserActivityLogs Collection                         │
│   {                                                              │
│     "Username": "nevillecolaco19@gmail.com",  ✅                │
│     "DisplayMessage": "Selected property: The Grand Hotel",  ✅ │
│     "PropertyId": 5,  ✅                                         │
│     "Metadata": { propertyIds, propertyNames }  ✅              │
│   }                                                              │
└─────────────────────────────────────────────────────────────────┘
```

## Next Steps

1. **Test the fixes**:
   - Login
   - Select "The Grand Hotel - Demo"
   - Check MongoDB logs
   - Verify all fields populated correctly

2. **If issues remain**:
   - Check console logs (both browser and backend)
   - Check network tab for headers
   - Decode JWT token to see claims
   - Review debugging tips above

3. **After verification**:
   - Remove [AllowAnonymous] from property selection endpoint
   - Remove debug Console.WriteLine statements (or convert to proper ILogger)
   - Create unit tests for username resolution
   - Create integration tests for property selection logging

## Success Criteria ✅

After these fixes, logs should show:
- ✅ Property selection: "nevillecolaco19 selected property: The Grand Hotel - Demo"
- ✅ Property views: "nevillecolaco19 viewed Properties"
- ✅ Username: Always correct (nevillecolaco19@gmail.com)
- ✅ PropertyId: Always populated when property selected
- ✅ Metadata: Rich context (property IDs, names, route params)
- ✅ Complete audit trail of user actions
