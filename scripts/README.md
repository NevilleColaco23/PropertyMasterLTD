# Demo Property MongoDB Seeding Scripts

## 🎯 Overview

This folder contains MongoDB scripts to seed demo property data for new users.

**Two scripts:**
1. ✅ `seed-demo-property.mongodb.js` - Creates the demo property
2. ✅ `add-demo-access-to-user.mongodb.js` - Grants demo access to existing users

---

## 🚀 Quick Setup

### Step 1: Create Demo Property

**Using MongoDB Compass:**

1. **Open MongoDB Compass**
2. **Connect** to your MongoDB instance
3. **Select** database: `ListingDB`
4. **Click** "Mongosh" tab (bottom of screen)
5. **Copy** entire contents of `seed-demo-property.mongodb.js`
6. **Paste** into Mongosh
7. **Press Enter**

**Expected Output:**
```
📝 Step 1: Creating Demo Property...
✅ Demo property created successfully with 6 rooms!

========================================
✨ VERIFICATION SUMMARY
========================================
✅ Demo Property: "The Grand Hotel - Demo" (ID: -1)
   - Rooms: 6
   - Property Code: DEMO0001
   - Active: true
```

---

### Step 2: Add Demo Access to Your User

**Edit the script first:**

1. Open `add-demo-access-to-user.mongodb.js`
2. **Line 13:** Change the email to yours:
```javascript
const userEmail = "your-email@example.com"; // 👈 CHANGE THIS
```

**Run the script:**

1. **Copy** entire contents of `add-demo-access-to-user.mongodb.js`
2. **Paste** into Mongosh
3. **Press Enter**

**Expected Output:**
```
✅ Demo property found: "The Grand Hotel - Demo"
✅ User found: YourUsername (ID: 18)
📝 Adding demo property access...
✅ Demo property access added successfully!

========================================
✨ VERIFICATION
========================================
User: YourUsername (your-email@example.com)
Properties Access: 1

📌 Property Access List:
   1. Property: The Grand Hotel - Demo (ID: -1) 🎪 DEMO
      - Active: true
      - From: 2024-03-15...
      - To: 2025-03-15...
```

---

## 📋 What Gets Created

### Demo Property Structure

Based on `classfiles/Domain/Property/Property.cs`:

```javascript
{
  "_id": -1,                    // Property.Id (int)
  "Name": "The Grand Hotel - Demo",  // Property.Name (string)
  "Active": true,               // Property.Active (bool)
  "PropertyCode": "DEMO0001",   // Property.PropertyCode (string)
  "CompanyLogoURL": "https://placehold.co/200x200/4CAF50/white?text=DEMO",
  "Rooms": [                    // Property.Rooms (List<Room>)
    {
      "Id": "room-demo-101",    // Room.Id (string) - BsonId
      "RoomCode": "101",        // Room.RoomCode (string)
      "RoomName": "Deluxe King Room",  // Room.RoomName (string)
      "Active": true,           // Room.Active (bool)
      "CompanyLogoURL": "..."   // Room.CompanyLogoURL (string)
    }
    // ... 5 more rooms
  ],
  "AccessList": [],             // Empty initially
  "IsDemo": true                // Custom flag (not in domain model)
}
```

### 6 Demo Rooms

| Room Code | Room Name | Type |
|-----------|-----------|------|
| 101 | Deluxe King Room | Standard |
| 102 | Deluxe Queen Room | Standard |
| 201 | Executive Suite | Suite |
| 202 | Presidential Suite | Suite |
| 301 | Family Room | Family |
| 302 | Ocean View Room | Premium |

---

## 🔍 Schema Comparison

Your existing property structure vs demo property:

### Your Existing Property (ID: 100)
```javascript
{
  "_id": 100,
  "Name": "The Grand Meredian",
  "Active": true,
  "PropertCode": "123AAAA",  // ⚠️ Typo: "PropertCode" instead of "PropertyCode"
  "CompanyLogoURL": "https://i.postimg.cc/W174BkHS/hotel.jpg",
  "Rooms": [
    {
      "RoomName": "R1",
      "RoomCode": "xxxco10",
      "Id": 1,
      "Active": true
    }
  ],
  "AccessList": [1, 100]
}
```

### Demo Property (ID: -1)
```javascript
{
  "_id": -1,
  "Name": "The Grand Hotel - Demo",
  "Active": true,
  "PropertyCode": "DEMO0001",  // ✅ Correct spelling
  "CompanyLogoURL": "https://placehold.co/200x200/4CAF50/white?text=DEMO",
  "Rooms": [
    {
      "Id": "room-demo-101",
      "RoomCode": "101",
      "RoomName": "Deluxe King Room",
      "Active": true,
      "CompanyLogoURL": "..."
    }
    // ... 5 more rooms
  ],
  "AccessList": [],
  "IsDemo": true
}
```

---

## 🔧 Troubleshooting

### Issue: "Demo property not created"

**Check:**
```javascript
db.Properties.findOne({ _id: -1 })
```

**If null, run:**
```javascript
// Delete if exists
db.Properties.deleteOne({ _id: -1 })

// Run seed-demo-property.mongodb.js again
```

---

### Issue: "User doesn't have demo access after signup"

**Check user's PropertyAccessList:**
```javascript
db.Users.findOne(
  { Email: "your-email@example.com" },
  { PropertyAccessList: 1, UserName: 1 }
)
```

**If PropertyAccessList is empty or missing demo (-1):**
- Run `add-demo-access-to-user.mongodb.js`

---

### Issue: "PropertyAccessList structure mismatch"

Your `PropertyAccessList` schema (from `Domain/Users/Users.cs`):
```javascript
{
  "Id": -1,          // int (Property ID reference)
  "IsActive": true,  // bool
  "From": ISODate("..."),      // DateTime
  "To": ISODate("..."),        // DateTime
  "CreatedDate": ISODate("..."), // DateTime
  "CreatedBy": 18    // int (User ID)
}
```

**Note:** Field is `Id` (not `PropertyID`)!

---

## 📊 Verify Everything Works

### 1. Check Demo Property Exists
```javascript
db.Properties.find({ _id: -1 })
```

### 2. Check User Has Access
```javascript
db.Users.findOne(
  { Email: "your-email@example.com" },
  { UserName: 1, PropertyAccessList: 1 }
)

// Should show:
// {
//   "PropertyAccessList": [
//     { "Id": -1, "IsActive": true, ... }
//   ]
// }
```

### 3. Count Properties
```javascript
// Your properties (ID >= 0)
db.Properties.countDocuments({ _id: { $gte: 0 } })

// Demo property (ID = -1)
db.Properties.countDocuments({ _id: -1 })

// Total
db.Properties.countDocuments()
```

---

## 🗑️ Cleanup Commands

### Remove Demo Property
```javascript
db.Properties.deleteOne({ _id: -1 })
print('✅ Demo property deleted');
```

### Remove Demo Access from All Users
```javascript
db.Users.updateMany(
  {},
  { $pull: { PropertyAccessList: { Id: -1 } } }
)
print('✅ Demo access removed from all users');
```

### Complete Cleanup
```javascript
// Remove demo property
db.Properties.deleteOne({ _id: -1 })

// Remove demo access from all users
db.Users.updateMany(
  {},
  { $pull: { PropertyAccessList: { Id: -1 } } }
)

print('✅ All demo data removed');
```

---

## 🎓 Understanding PropertyAccessList

### Your Domain Model
```csharp
// classfiles/Domain/Users/Users.cs
public class PropertyAccessList
{
    public int Id { get; set; }          // Property ID (references Property._id)
    public bool IsActive { get; set; }
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public DateTime CreatedDate { get; set; }
    public int CreatedBy { get; set; }   // User ID who granted access
}
```

### MongoDB Document
```javascript
{
  "_id": 18,
  "Email": "user@example.com",
  "PropertyAccessList": [
    {
      "Id": 100,           // Your real property
      "IsActive": true,
      "From": ISODate("..."),
      "To": ISODate("..."),
      "CreatedDate": ISODate("..."),
      "CreatedBy": 1
    },
    {
      "Id": -1,            // Demo property
      "IsActive": true,
      "From": ISODate("..."),
      "To": ISODate("..."),
      "CreatedDate": ISODate("..."),
      "CreatedBy": 18,
      "IsDemo": true       // Custom flag (not in domain model)
    }
  ]
}
```

---

## ⚠️ Important Notes

### Field Name: "Id" not "PropertyID"

Your domain model uses `Id`:
```csharp
public class PropertyAccessList
{
    public int Id { get; set; }  // ← Not "PropertyID"!
}
```

So MongoDB documents must use:
```javascript
{ "Id": -1 }  // ✅ Correct
// NOT:
{ "PropertyID": -1 }  // ❌ Wrong
```

### PropertyCode Typo

Your existing property has:
```javascript
"PropertCode": "123AAAA"  // ⚠️ Typo
```

But your domain model uses:
```csharp
public string PropertyCode { get; set; }  // ✅ Correct
```

**Recommendation:** Fix the typo in your existing property:
```javascript
db.Properties.updateOne(
  { _id: 100 },
  {
    $set: { PropertyCode: "123AAAA" },
    $unset: { PropertCode: "" }
  }
)
```

---

## 📞 Support

If you encounter issues:

1. **Check logs**: Look at Serilog output in console
2. **Verify schema**: Run `seed-demo-property.mongodb.js` - it shows schema comparison
3. **Check DemoPropertyService**: Line 174 uses `"Id"` (not `"PropertyID"`)

---

**Created:** March 2024  
**Based on:** Your actual Property and PropertyAccessList domain models  
**Last Updated:** March 2024
