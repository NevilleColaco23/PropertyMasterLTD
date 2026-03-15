// ============================================
// Debug Menu Data - Find what's in the database
// ============================================
use('PropertyMaster');

// 1. Check all menus in the collection
print("====== ALL MENUS ======");
db.getCollection('Menus').find({}).toArray().forEach(menu => {
  printjson({
    _id: menu._id,
    label: menu.label,
    path: menu.path,
    MenuName: menu.MenuName
  });
});

// 2. Check Home menu specifically
print("\n====== HOME MENU (ID=1) ======");
const homeMenu = db.getCollection('Menus').findOne({ _id: 1 });
printjson(homeMenu);

// 3. Check menu permissions
print("\n====== MENU PERMISSIONS FOR USER 1 ======");
db.getCollection('MenuPermissions').find({
  UserId: 1,
  isActive: true
}).toArray().forEach(perm => {
  printjson({
    MenuID: perm.MenuID,
    AccessLevel: perm.AccessLevel,
    isActive: perm.isActive
  });
});

// 4. Run the actual aggregation that the API uses
print("\n====== AGGREGATION RESULTS (What API returns) ======");
const menuResults = db.getCollection("MenuPermissions").aggregate([
  {
    "$match": {
      "UserId": 1,
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
      "MenuID": "$MenuID",
      "label": "$label",
      "path": "$path",
      "order": "$order",
      "hasDropdown": "$hasDropdown",
      "subItems": "$subItems",
      "isVisible": "$isVisible"
    }
  },
  { "$match": { "isVisible": true } },
  { "$sort": { "order": 1 } }
]).toArray();

menuResults.forEach(item => {
  printjson({
    MenuID: item.MenuID,
    label: item.label,
    path: item.path,
    order: item.order
  });
});
