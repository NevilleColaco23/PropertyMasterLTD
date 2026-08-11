// Add-GroupMaster-MenuItem.js
//
// Idempotent migration script: adds a "Group Master" sub-item to the
// "Settings" entry in the navigation Menus collection, so it appears in the
// Settings dropdown alongside "Menu Access mapping" and "Property Master".
//
// Usage (mongosh):
//   mongosh "<connection string>" PropertyMaster Add-GroupMaster-MenuItem.js
//
// This script is safe to run multiple times; it will not create a duplicate
// entry if "Group Master" is already present in Settings' subItems.

const menusCollection = db.getCollection("Menus");

// Locate the "Settings" top-level menu document.
const settingsMenu = menusCollection.findOne({ label: "Settings" });

if (!settingsMenu) {
  print("ERROR: Could not find a Menus document with label 'Settings'. No changes made.");
  quit(1);
}

const existingSubItems = settingsMenu.subItems || [];

const alreadyExists = existingSubItems.some(
  (sub) => sub.subLabel === "Group Master" || sub.subPath === "/propertyLanding/group-master"
);

if (alreadyExists) {
  print("Settings menu already contains a 'Group Master' entry. No changes made.");
  quit(0);
}

const nextSubOrder =
  existingSubItems.reduce((max, sub) => Math.max(max, sub.subOrder ?? 0), 0) + 1;

const result = menusCollection.updateOne(
  { _id: settingsMenu._id },
  {
    $push: {
      subItems: {
        subLabel: "Group Master",
        subPath: "/propertyLanding/group-master",
        subOrder: nextSubOrder
      }
    }
  }
);

print(`Matched: ${result.matchedCount}, Modified: ${result.modifiedCount}`);
print("Added 'Group Master' to Settings menu subItems.");
