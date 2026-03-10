# Demo Property MongoDB Seeding Scripts

## Quick Setup

### Option 1: MongoDB Compass (GUI)

1. **Open MongoDB Compass**
2. **Connect** to your MongoDB instance
3. **Select** your database (e.g., `ListingDB`)
4. **Open Mongosh tab** (bottom of the screen)
5. **Copy & paste** the contents of `seed-demo-property.mongodb.js`
6. **Press Enter** to execute

---

### Option 2: Mongo Shell (CLI)

```sh
# Navigate to scripts directory
cd scripts

# Run the script
mongosh "your-connection-string" < seed-demo-property.mongodb.js

# Or connect first, then load
mongosh "your-connection-string"
> use ListingDB
> load('seed-demo-property.mongodb.js')
```

---

### Option 3: VS Code MongoDB Extension

1. **Install**: [MongoDB for VS Code](https://marketplace.visualstudio.com/items?itemName=mongodb.mongodb-vscode)
2. **Connect** to your MongoDB
3. **Open** `scripts/seed-demo-property.mongodb.js`
4. **Right-click** → "Run File"

---

## What Gets Created

### 1. Demo Property (ID: -1)
```javascript
{
  "_id": -1,
  "Name": "The Grand Hotel - Demo",
  "PropertyCode": "DEMO0001",
  "Rooms": [/* 6 rooms */],
  "IsDemo": true
}
```

**Rooms:**
- 101 - Deluxe King Room
- 102 - Deluxe Queen Room
- 201 - Executive Suite
- 202 - Presidential Suite
- 301 - Family Room
- 302 - Ocean View Room

---

### 2. Demo Bookings (4 bookings)
```javascript
[
  { "_id": -1, "GuestName": "John Smith", "Status": "Completed", ... },
  { "_id": -2, "GuestName": "Sarah Johnson", "Status": "Active", ... },
  { "_id": -3, "GuestName": "Michael Brown", "Status": "Confirmed", ... },
  { "_id": -4, "GuestName": "Emma Davis", "Status": "Pending", ... }
]
```

**Timeline:**
- **Past**: John Smith (Completed - checked out)
- **Current**: Sarah Johnson (Active - currently staying)
- **Future**: Michael Brown (Confirmed - upcoming)
- **Future**: Emma Davis (Pending - awaiting confirmation)

---

### 3. Demo Menus (3 menus)
```javascript
[
  { "_id": -1, "MenuName": "Breakfast Menu", "Items": [/* 3 items */] },
  { "_id": -2, "MenuName": "Lunch Menu", "Items": [/* 3 items */] },
  { "_id": -3, "MenuName": "Dinner Menu", "Items": [/* 3 items */] }
]
```

**Menu Items (9 total):**
- **Breakfast**: Continental, Full English, Pancakes
- **Lunch**: Caesar Salad, Chicken Sandwich, Fish & Chips
- **Dinner**: Ribeye Steak, Grilled Salmon, Vegetarian Pasta

---

## Verification

After running the script, you should see:

```
========================================
✨ VERIFICATION SUMMARY
========================================
✅ Demo Property: "The Grand Hotel - Demo" (ID: -1)
   - Rooms: 6
   - Property Code: DEMO0001
✅ Demo Bookings: 4
   - Completed: 1
   - Active: 1
   - Confirmed: 1
   - Pending: 1
✅ Demo Menus: 3
   - Breakfast Menu (Breakfast)
   - Lunch Menu (Lunch)
   - Dinner Menu (Dinner)

========================================
🎉 Demo Property Setup Complete!
========================================
```

---

## Troubleshooting

### Issue: "Script fails to run"

**Check:**
1. Are you connected to the correct database?
2. Do you have write permissions?
3. Is the database name correct in the script?

**Fix:**
```javascript
// Line 7 in seed-demo-property.mongodb.js
use('ListingDB'); // ← Change to your database name
```

---

### Issue: "Duplicate key error"

**Cause:** Demo data already exists

**Solution:**
```javascript
// Delete existing demo data first
db.Properties.deleteOne({ _id: -1 })
db.Bookings.deleteMany({ PropertyID: -1 })
db.Menus.deleteMany({ PropertyID: -1 })

// Then run the seed script again
```

---

### Issue: "Property created but not showing for users"

**Check user's PropertyAccessList:**
```javascript
db.Users.findOne(
  { Email: "your-email@example.com" },
  { PropertyAccessList: 1 }
)

// Should show:
// {
//   "PropertyAccessList": [
//     { "PropertyID": -1, "IsDemo": true, ... }
//   ]
// }
```

**If missing, manually add:**
```javascript
db.Users.updateOne(
  { Email: "your-email@example.com" },
  {
    $push: {
      PropertyAccessList: {
        PropertyID: -1,
        IsActive: true,
        From: new Date(),
        To: new Date(Date.now() + 365*24*60*60*1000), // 1 year
        CreatedDate: new Date(),
        IsDemo: true,
        Permissions: ["ViewOnly", "CanExplore"]
      }
    }
  }
)
```

---

## Manual Cleanup (If Needed)

### Remove Demo Property & All Related Data
```javascript
// WARNING: This deletes all demo data!

db.Properties.deleteOne({ _id: -1 })
db.Bookings.deleteMany({ PropertyID: -1 })
db.Menus.deleteMany({ PropertyID: -1 })

// Remove demo access from all users
db.Users.updateMany(
  {},
  {
    $pull: {
      PropertyAccessList: { PropertyID: -1 }
    }
  }
)

print('✅ All demo data removed');
```

### Reset Demo Property (Keep Structure, Reset Data)
```javascript
// Keep property but reset bookings/menus to original state

db.Bookings.deleteMany({ PropertyID: -1 })
db.Menus.deleteMany({ PropertyID: -1 })

// Then run seed-demo-property.mongodb.js again
```

---

## Next Steps

After running this script:

1. ✅ **Sign up a new user** (without property code)
2. ✅ **Check MongoDB**: User should have PropertyAccessList with PropertyID: -1
3. ✅ **Login** with the user
4. ✅ **Verify**: User should see "The Grand Hotel - Demo" in property selector

---

## Production Deployment

### Railway MongoDB

If deploying to Railway, update the connection string in the script:

```javascript
// For Railway MongoDB
use('railway'); // Railway default database name

// Or use your custom database
use('YourProductionDBName');
```

Run the script **once** on production database after first deployment.

---

## Automation (Future)

**Option A: Run on Application Startup**
```csharp
// In WebApi/Program.cs or Startup.cs
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DemoPropertySeeder>();
    await seeder.SeedDemoPropertyIfNotExistsAsync();
}
```

**Option B: Admin API Endpoint**
```csharp
[HttpPost("admin/seed-demo")]
[Authorize(Roles = "Admin")]
public async Task<IActionResult> SeedDemoProperty()
{
    await _demoPropertyService.EnsureDemoPropertyExistsAsync();
    return Ok("Demo property seeded");
}
```

---

**Created:** March 2024  
**Last Updated:** March 2024  
**Script Version:** 1.0.0
