// ============================================
// MongoDB Menu Collection - Home Button Fix
// ============================================
// This script ensures the "Home" menu item has the correct path
// to navigate to the dashboard

// Use your database
use('PropertyMaster');

// Update or insert the "Home" menu item in the Menus collection
db.getCollection('Menus').updateOne(
  { _id: 1 }, // Assuming Home menu has ID 1
  {
    $set: {
      label: "Home",
      path: "/propertyLanding/dashboard1",  // This is the key fix!
      order: 1,
      hasDropdown: false,
      subItems: [],
      isVisible: true,
      createdAt: new Date(),
      updatedAt: new Date()
    }
  },
  { upsert: true }
);

// Verify the Home menu exists
const homeMenu = db.getCollection('Menus').findOne({ _id: 1 });
print("✅ Home menu:");
printjson(homeMenu);

// Check if user has permission for Home menu
const homePermissions = db.getCollection('MenuPermissions').find({
  MenuID: 1,
  isActive: true
}).toArray();

print("\n✅ Users with Home menu permission:");
printjson(homePermissions);

// If no permissions exist, create one for your user (adjust userId as needed)
if (homePermissions.length === 0) {
  print("\n⚠️ No Home menu permissions found. Creating default permission...");
  
  db.getCollection('MenuPermissions').insertOne({
    _id: NumberInt(1),
    UserId: NumberInt(1), // ⚠️ CHANGE THIS to your actual user ID
    MenuID: NumberInt(1),
    AccessLevel: "full",
    isActive: true,
    From: new Date("2024-01-01"),
    To: new Date("2099-12-31"),
    CreatedAt: new Date(),
    CreatedBy: NumberInt(1),
    UpdatedAt: new Date(),
    UpdatedBy: NumberInt(1)
  });
  
  print("✅ Home menu permission created for UserId 1");
}

// Test the aggregation query to see what data is returned
print("\n✅ Testing aggregation for UserId 1:");
const userId = 1;
const menuResults = db.getCollection("MenuPermissions").aggregate([
  {
    "$match": {
      "UserId": userId,
      "isActive": true,
      "AccessLevel": { "$ne": "hidden" }
    }
  },
  {
    "$lookup": {
      "from": "Menus",
      "localField": "MenuID",
      "foreignField": "_id",
      "as": "menu"
    }
  },
  { "$unwind": "$menu" },
  {
    "$replaceRoot": {
      "newRoot": { "$mergeObjects": ["$$ROOT", "$menu"] }
    }
  },
  {
    "$project": {
      "_id": "$_id",
      "userId": "$UserId",
      "isActive": "$isActive",
      "accessLevel": "$AccessLevel",
      "menuId": "$MenuID",
      "label": "$label",
      "path": "$path",  // ⭐ This is the important field!
      "order": "$order",
      "hasDropdown": "$hasDropdown",
      "subItems": "$subItems",
      "isVisible": "$isVisible"
    }
  },
  { "$match": { "isVisible": true } },
  { "$sort": { "order": 1 } }
]).toArray();

print("\n✅ Menu items returned (should include 'path' field):");
printjson(menuResults);

print("\n✅ Home menu path:", menuResults.find(m => m.label === "Home")?.path);
