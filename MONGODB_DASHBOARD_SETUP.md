# Quick MongoDB Setup for Dashboard Navigation

## Update Home Menu Item to Navigate to Dashboard

Run this query in your MongoDB to configure the "Home" menu to navigate to the dashboard:

```javascript
// Update the Home menu item path
db.Menus.updateOne(
  { label: "Home" },
  { $set: { 
    path: "/propertyLanding/dashboard1",
    hasDropdown: false,
    isVisible: true,
    order: 1
  }}
)
```

## Verify the Update

```javascript
// Check the Home menu configuration
db.Menus.findOne({ label: "Home" })
```

Expected output:
```javascript
{
  "_id": ObjectId("..."),
  "label": "Home",
  "path": "/propertyLanding/dashboard1",  // ✅ Should point to dashboard
  "order": 1,
  "hasDropdown": false,
  "isVisible": true,
  "createdAt": ISODate("..."),
  "updatedAt": ISODate("...")
}
```

## Verify User Permissions

Make sure users have access to the Home menu:

```javascript
// Check user's menu permissions (replace userId with actual ID)
db.MenuPermissions.find({
  UserId: 1,  // ← Change this to your user ID
  isActive: true,
  AccessLevel: { $ne: "hidden" }
})
```

## Test the Navigation

After updating the database:

1. **Refresh** your application (may need hard refresh: Ctrl+Shift+R)
2. **Login** to your application
3. **Select a property** from the property selector
4. You should see the **dashboard** as the default page
5. **Click "Home"** in the navigation - it should navigate to the dashboard

## Alternative: If You Don't Have a "Home" Menu Yet

If the Home menu doesn't exist, create it:

```javascript
// Create the Home menu item
db.Menus.insertOne({
  label: "Home",
  path: "/propertyLanding/dashboard1",
  order: 1,
  hasDropdown: false,
  subItems: [],
  isVisible: true,
  createdAt: new Date(),
  updatedAt: new Date()
})
```

Then create permissions for your users:

```javascript
// Get the menu ID from the insert result, then create permission
db.MenuPermissions.insertOne({
  UserId: 1,  // ← Your user ID
  MenuID: ObjectId("..."),  // ← Use the _id from the menu you just created
  AccessLevel: "full",
  isActive: true,
  IsActive: true,  // Sometimes it's PascalCase
  createdAt: new Date(),
  updatedAt: new Date()
})
```

## Quick Navigation Test URLs

After logging in, you can test these URLs directly:

- **Property Landing (redirects to dashboard)**: `http://localhost:4200/propertyLanding`
- **Dashboard Direct**: `http://localhost:4200/propertyLanding/dashboard1`
- **Property Master**: `http://localhost:4200/propertyLanding/property-master`
- **Menu Access Map**: `http://localhost:4200/propertyLanding/MenuAccessmapping`

---

**Note**: The application dynamically loads menu items from the `/menu/GetinitialData` API endpoint, which queries MongoDB. Any changes to the `Menus` collection will be reflected in the navigation after a page refresh.
