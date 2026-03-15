// ============================================
// Fix Home Menu Path - Set correct dashboard path
// ============================================
use('PropertyMaster');

print("====== FIXING HOME MENU PATH ======");

// Update the Home menu to have the correct path
const result = db.getCollection('Menus').updateOne(
  { _id: 1 },
  {
    $set: {
      MenuName: "Home",
      label: "Home",
      path: "/propertyLanding/dashboard1",  // ✅ Correct path without property code
      order: 1,
      hasDropdown: false,
      subItems: [],
      isVisible: true,
      isActive: true,
      ParentMenuId: 0,
      updatedAt: new Date()
    },
    $setOnInsert: {
      createdAt: new Date()
    }
  },
  { upsert: true }
);

printjson(result);

// Verify the update
const homeMenu = db.getCollection('Menus').findOne({ _id: 1 });
print("\n✅ Home menu after update:");
printjson(homeMenu);

// Also check if there are any other menus with similar issues
print("\n====== ALL MENU PATHS ======");
db.getCollection('Menus').find({}).forEach(menu => {
  print(`Menu ID ${menu._id}: "${menu.label}" -> Path: "${menu.path}"`);
});
