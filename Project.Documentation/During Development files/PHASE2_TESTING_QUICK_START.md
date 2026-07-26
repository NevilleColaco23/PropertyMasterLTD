# 🧪 PHASE 2 - QUICK TESTING GUIDE

## 🎯 Test Your Automatic Activity Logging in 5 Minutes

### Prerequisites
✅ Backend API running  
✅ MongoDB running  
✅ Angular app running  
✅ You have a test user account

---

## 🚀 Step 1: Restart Backend (2 minutes)

The attributes are compiled into the DLLs, so restart is required:

```bash
# Stop current API (Ctrl+C in terminal)

# Navigate to WebApi folder
cd C:\Users\nevil\OneDrive\Desktop\Projects to learn\workspace\PropertyMasterV4.0\WebApi

# Clean and rebuild
dotnet clean
dotnet build

# Run
dotnet run
```

**Expected Output**:
```
✅ ActivityLoggingActionFilter registered
✅ API listening on https://localhost:5001
```

---

## 🧪 Step 2: Test Property Creation (1 minute)

### Option A: Using Postman/Swagger

**Endpoint**:
```
POST https://localhost:5001/api/v1/property
```

**Headers**:
```
Content-Type: application/json
Authorization: Bearer {your_jwt_token}
```

**Body**:
```json
{
  "propertyName": "Test Activity Hotel",
  "address": "123 Test Street",
  "city": "Test City",
  "country": "Test Country"
}
```

**Expected Response**:
```json
{
  "id": 42
}
```

### Option B: Using Angular App
1. Login to app
2. Go to Property Master
3. Click "Add Property"
4. Fill form and save
5. Property created!

---

## 🔍 Step 3: Verify Activity Logged (1 minute)

### Method 1: MongoDB Compass

**Query**:
```javascript
// In MongoSH tab:
use ListingDB
db.UserActivityLogs.find().sort({ Timestamp: -1 }).limit(1).pretty()
```

**Expected Output**:
```javascript
{
  "ActivityId": 42,
  "UserId": 1,
  "Username": "admin@test.com",
  "ActivityType": "PropertyCreated",
  "EntityType": "Property",
  "EntityId": "42",
  "Action": "Create",
  "Description": "Created Property #42",
  "IPAddress": "::1",
  "UserAgent": "PostmanRuntime/7.32.0",
  "Timestamp": ISODate("2025-01-31T10:45:30.123Z"),
  "Duration": 156.5,
  "Status": "Success"
}
```

### Method 2: REST API

**Endpoint**:
```
GET https://localhost:5001/api/v1/activity/recent?limit=5
```

**Expected Response**:
```json
[
  {
    "activityId": 42,
    "activityType": "PropertyCreated",
    "entityType": "Property",
    "entityId": "42",
    "description": "Created Property #42",
    "timestamp": "2025-01-31T10:45:30.123Z",
    "duration": 156.5,
    "status": "Success"
  }
]
```

---

## 📊 Step 4: View in Activity Widget (1 minute)

1. Open Angular app: `http://localhost:4200`
2. Login with your account
3. Navigate to **Dashboard**
4. Find **Activity Stream Widget**
5. See your property creation at the top! 🎉

**Expected Widget Display**:
```
📊 ACTIVITY SUMMARY

Total Activities: 1
Most Active User: admin@test.com

────────────────────────────────

🟢 Just now
admin@test.com created Property #42
Duration: 157ms | ✅ Success

────────────────────────────────
```

---

## 🔒 Step 5: Test Failed Login (Security Test)

### Test Failed Login:

**Endpoint**:
```
POST https://localhost:5001/api/v1/account/login
```

**Body** (wrong password):
```json
{
  "username": "admin@test.com",
  "password": "wrong_password_123"
}
```

**Expected Response**:
```json
{
  "error": "Username or password incorrect."
}
```

### Verify Failed Attempt Logged:

**MongoDB Query**:
```javascript
db.UserActivityLogs.find({ 
  ActivityType: "UserLoggedIn",
  Status: "Failed"
}).sort({ Timestamp: -1 }).limit(1).pretty()
```

**Expected Output**:
```javascript
{
  "ActivityType": "UserLoggedIn",
  "EntityType": "User Session",
  "Status": "Failed",
  "ErrorMessage": "Username or password incorrect.",
  "IPAddress": "::1",
  "Timestamp": ISODate("2025-01-31T10:50:00Z")
}
```

**Security Feature**: Failed login attempts are tracked for security audit! 🔒

---

## ✅ Quick Verification Checklist

| Test | Expected Result | ✅ |
|------|-----------------|---|
| Restart backend | ActivityLoggingActionFilter registered | ⬜ |
| Create property | Returns property ID | ⬜ |
| Check MongoDB | Activity logged in UserActivityLogs | ⬜ |
| Call /activity/recent | Returns your activity | ⬜ |
| View Activity Widget | Shows property creation | ⬜ |
| Failed login | Logged with Status="Failed" | ⬜ |

---

## 🎯 Test All Attributes

### Test [LogCreate] - Property Creation
```http
POST /api/v1/property
{ "propertyName": "Test Hotel" }
```
**Expected ActivityType**: `PropertyCreated`

### Test [LogUpdate] - Property Update
```http
PUT /api/v1/property/42
{ "id": 42, "propertyName": "Updated Hotel" }
```
**Expected ActivityType**: `PropertyUpdated`

### Test [LogDelete] - Property Deletion
```http
DELETE /api/v1/property/42
```
**Expected ActivityType**: `PropertyDeleted`

### Test [LogView] - Property Detail View
```http
GET /api/v1/property/42
```
**Expected ActivityType**: `PropertyViewed`

### Test [LogList] - Property List View
```http
GET /api/v1/property
```
**Expected ActivityType**: `PropertiesListed`

---

## 📈 Advanced Testing

### Test Dashboard Operations:

**Save Dashboard**:
```http
POST /api/v1/dashboard
{ "userId": 1, "dashboardName": "My Test Dashboard" }
```
**Expected**: `DashboardCreated`

**Delete Dashboard**:
```http
DELETE /api/v1/dashboard/{id}?userId=1
```
**Expected**: `DashboardDeleted`

### Test Partner Operations:

**Create Partner**:
```http
POST /api/v1/partners
{ "partnerName": "Test Partner" }
```
**Expected**: `PartnerCreated`

### Test Product Operations:

**Create Product**:
```http
POST /api/v1/products
{ "productName": "Test Product" }
```
**Expected**: `ProductCreated`

---

## 🔍 Debugging

### If activity not logged:

1. **Check filter is registered**:
   - Look in API startup logs for: `ActivityLoggingActionFilter registered`
   
2. **Check MongoDB connection**:
   ```csharp
   // Should be in appsettings.json
   "MongoDb": {
     "ConnectionString": "mongodb://localhost:27017",
     "Database": "ListingDB"
   }
   ```

3. **Check attribute is present**:
   ```csharp
   // Should see attribute above action
   [HttpPost]
   [LogCreate("Property")]
   public async Task<ActionResult<int>> Create(...)
   ```

4. **Check UserActivityService registered**:
   ```csharp
   // In Startup.cs
   services.AddScoped<UserActivityService>();
   ```

5. **Check MongoDB collection**:
   ```javascript
   use ListingDB
   show collections  // Should show UserActivityLogs
   db.UserActivityLogs.countDocuments()  // Should be > 0
   ```

---

## 🎉 Success Indicators

✅ **API Starts Successfully**: No errors in console  
✅ **Property Created**: Returns ID  
✅ **MongoDB Has Entry**: Query returns activity  
✅ **REST API Returns Data**: /activity/recent works  
✅ **Widget Displays Activity**: Shows in Angular app  
✅ **Failed Logins Tracked**: Security audit working  

If all ✅ checked, **Phase 2 is working perfectly!** 🚀

---

## 📊 Sample Activity Data

After running all tests, you should have:

```javascript
// MongoDB UserActivityLogs
db.UserActivityLogs.aggregate([
  { $group: { 
      _id: "$ActivityType", 
      count: { $sum: 1 } 
  }},
  { $sort: { count: -1 } }
])

// Expected Output:
[
  { "_id": "PropertyCreated", "count": 1 },
  { "_id": "PropertyUpdated", "count": 1 },
  { "_id": "PropertyDeleted", "count": 1 },
  { "_id": "PropertyViewed", "count": 1 },
  { "_id": "PropertiesListed", "count": 1 },
  { "_id": "UserLoggedIn", "count": 2 },  // 1 success, 1 failure
  { "_id": "DashboardCreated", "count": 1 }
]
```

---

## 🆘 Common Issues

### Issue: "ActivityLoggingActionFilter not found"
**Solution**: 
```bash
dotnet clean
dotnet build
# Make sure Infrastructure project builds successfully
```

### Issue: "UserActivityService not registered"
**Solution**: Check `Startup.cs` has:
```csharp
services.AddScoped<UserActivityService>();
```

### Issue: "Activities not appearing in widget"
**Solution**: 
1. Check widget is calling `/api/v1/activity/summary`
2. Check widget auto-refresh is enabled
3. Hard refresh browser (Ctrl+F5)

### Issue: "MongoDB connection failed"
**Solution**:
```bash
# Start MongoDB
net start MongoDB

# Or check connection string in appsettings.json
```

---

## 🎯 Next Steps After Testing

✅ **Everything Works**: Move to advanced features (real-time, analytics)  
⚠️ **Partial Success**: Review DEBUGGING section above  
❌ **Not Working**: Check Prerequisites and Startup.cs configuration  

---

**Testing Time**: ~5 minutes  
**Status**: Ready to test! 🧪  
**Difficulty**: Easy ⭐  
