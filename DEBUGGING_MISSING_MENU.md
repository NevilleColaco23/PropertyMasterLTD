# Debugging Missing Navigation Menu

## Quick Diagnosis

### Step 1: Check Browser Console

Press **F12** to open Developer Tools, then refresh the dashboard page. Look for these messages:

**✅ Expected (Working):**
```
🚀 PropertyLandingComponent initialized
📍 API URL: http://your-api-url
✅ Menu data received: {results: Array(3), ...}
✅ Loaded 3 menu items
```

**❌ Problem Indicators:**
```
⚠️ WARNING: No menu items received from API!     → Database has no permissions for your user
❌ ERROR: Failed to fetch menu items              → API call failed
Status: 401                                       → Token expired, logout/login
Status: 500                                       → Backend error, check API logs
```

### Step 2: Check Network Tab

1. Open Network tab in Developer Tools (F12)
2. Refresh page
3. Find `/menu/GetinitialData` request
4. Check **Status** and **Response**

| Status | Problem | Solution |
|--------|---------|----------|
| 200 with empty results | No menu permissions in database | Add permissions in MongoDB |
| 401 Unauthorized | Token expired | Logout and login again |
| 403 Forbidden | No access rights | Check user permissions |
| 500 Server Error | Backend issue | Check API server logs |

### Step 3: Check Your User ID (Optional)

If you need to verify which user you're logged in as:

1. Open browser console (F12)
2. Copy/paste contents of `check-user-id.js`
3. Press Enter
4. Note your User ID

Use this to verify you have menu permissions in MongoDB:
```javascript
db.MenuPermissions.find({ UserId: YOUR_USER_ID })
```

## Common Solutions

### Solution 1: Token Expired
**Symptoms:** 401 error in Network tab
**Fix:** 
1. Logout
2. Login again
3. Select property
4. Check if menus appear

### Solution 2: No Menu Permissions
**Symptoms:** 200 response but empty results
**Fix:** 
1. Connect to MongoDB
2. Verify permissions exist:
   ```javascript
   db.MenuPermissions.find({ UserId: YOUR_USER_ID })
   ```
3. If empty, your user needs menu permissions added in the database

### Solution 3: Backend Error
**Symptoms:** 500 error in Network tab
**Fix:**
1. Check API server logs
2. Verify database connection is working
3. Check MongoDB query syntax in `GetMenuListQueryByUserId.cs`

### Solution 4: Database Empty
**Symptoms:** Menus collection is empty
**Fix:**
1. Verify menus exist:
   ```javascript
   db.Menus.countDocuments()
   ```
2. If 0, populate the Menus collection with menu items

## Quick Test: Temporary Hard-Code

To verify the component works, temporarily hard-code menus in `property-landing.component.ts`:

```typescript
ngOnInit(): void {
  // TEMPORARY - Remove after testing
  this.navItems = [
    { label: 'Home', path: '/propertyLanding/dashboard1', order: 1, hasDropdown: false },
    { label: 'Settings', path: '/propertyLanding/MenuAccessmapping', order: 2, hasDropdown: false }
  ];
  this.cdr.detectChanges();
  
  // Continue with API call...
  console.log('🚀 PropertyLandingComponent initialized');
  // ...
}
```

**If menus appear:** Problem is API/database, not component
**If menus don't appear:** Check browser console for component errors

## Files Reference

- **check-user-id.js** - Run in browser console to check your user ID and token status
- **property-landing.component.ts** - Component that loads and displays menus
- **GetMenuListQueryByUserId.cs** - Backend MongoDB query for menu permissions

## What to Share for Help

If you need assistance, share:
1. Console messages (from Step 1)
2. Network tab status code (from Step 2)
3. Your user ID (from check-user-id.js)
4. Whether you have MenuPermissions in database

---

**Most Common Issue:** User has no MenuPermissions in the database. Check MongoDB and add permissions if missing.
