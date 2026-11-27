// Copy-MenuPermissions-ForUser.js
//
// Idempotent migration script: copies all MenuPermissions documents from a
// source user to a target user, so the target user gets the same menu
// access as the source user (e.g. cloning user 19's menus onto user
// 1821189652).
//
// NOTE: The MenuPermissions collection has inconsistent field casing left
// over from older data (a couple of legacy lowercase docs: userId/menuId/...)
// alongside the current PascalCase schema (UserId/MenuID/AccessLevel/
// IsActive/From/To/CreatedAt/CreatedBy) that the application actually reads
// and writes (see CreateMenuPermissionCommand / GetMenuListQueryByUserId).
// This script reads both casings defensively but always WRITES using the
// current PascalCase schema, since that's what the app queries against.
//
// Usage (mongosh):
//   mongosh "<connection string>" ListingDB Copy-MenuPermissions-ForUser.js
//
// To copy for different users, edit SOURCE_USER_ID / TARGET_USER_ID below,
// or override them via mongosh --eval, e.g.:
//   mongosh "<connection string>" \
//     --eval "var SOURCE_USER_ID=19, TARGET_USER_ID=1821189652" \
//     ListingDB Copy-MenuPermissions-ForUser.js
//
// This script is safe to run multiple times; it will not create a duplicate
// permission if the target user already has a MenuPermissions entry for a
// given menuId.

const sourceUserId = typeof SOURCE_USER_ID !== "undefined" ? SOURCE_USER_ID : 19;
const targetUserId = typeof TARGET_USER_ID !== "undefined" ? TARGET_USER_ID : 1821189652;

const menuPermissionsCollection = db.getCollection("MenuPermissions");
const countersCollection = db.getCollection("counters");

// Helpers to read either casing, since the collection has mixed legacy data.
const getUserId = (doc) => (doc.UserId !== undefined ? doc.UserId : doc.userId);
const getMenuId = (doc) => (doc.MenuID !== undefined ? doc.MenuID : doc.menuId);
const getAccessLevel = (doc) => (doc.AccessLevel !== undefined ? doc.AccessLevel : doc.accessLevel);
const getIsActive = (doc) => (doc.IsActive !== undefined ? doc.IsActive : doc.isActive);
const getFrom = (doc) => (doc.From !== undefined ? doc.From : doc.from);
const getTo = (doc) => (doc.To !== undefined ? doc.To : doc.to);
const getCreatedBy = (doc) => (doc.CreatedBy !== undefined ? doc.CreatedBy : doc.createdBy);

const sourcePermissions = menuPermissionsCollection
  .find({ $or: [{ UserId: sourceUserId }, { userId: sourceUserId }] })
  .toArray();

if (sourcePermissions.length === 0) {
  print(`ERROR: No MenuPermissions documents found for source userId ${sourceUserId}. No changes made.`);
  quit(1);
}

// Clean up any legacy lowercase-cased documents previously written for the
// target user (e.g. from an earlier bad run of this script). The app only
// queries the PascalCase schema (UserId/MenuID/...), so lowercase docs are
// dead data that would otherwise block re-creating the entry correctly.
const legacyTargetDocsRemoved = menuPermissionsCollection.deleteMany({
  userId: targetUserId,
  UserId: { $exists: false }
});
if (legacyTargetDocsRemoved.deletedCount > 0) {
  print(`Removed ${legacyTargetDocsRemoved.deletedCount} legacy lowercase-cased document(s) for target userId ${targetUserId}.`);
}

const existingTargetMenuIds = new Set(
  menuPermissionsCollection
    .find({ $or: [{ UserId: targetUserId }, { userId: targetUserId }] })
    .toArray()
    .map(getMenuId)
);

// Make sure the counters doc is in sync with the highest _id actually present
// in MenuPermissions before we start handing out new ids. The counters
// collection can fall out of sync (e.g. seeded/imported data, manual inserts,
// or a corrupted/non-numeric 'seq' field), which otherwise causes errors on
// insert/increment. Force it to a correct numeric value regardless of its
// current type or value.
const highestExisting = menuPermissionsCollection
  .find({}, { _id: 1 })
  .sort({ _id: -1 })
  .limit(1)
  .toArray();
const highestExistingId = highestExisting.length > 0 ? highestExisting[0]._id : 0;

const existingCounter = countersCollection.findOne({ _id: "MenuPermissions" });
const currentSeq = existingCounter && typeof existingCounter.seq === "number" ? existingCounter.seq : 0;

countersCollection.updateOne(
  { _id: "MenuPermissions" },
  { $set: { seq: Math.max(currentSeq, highestExistingId) } },
  { upsert: true }
);

let insertedCount = 0;
let skippedCount = 0;

sourcePermissions.forEach((permission) => {
  const menuId = getMenuId(permission);

  if (existingTargetMenuIds.has(menuId)) {
    skippedCount++;
    return;
  }

  // Reserve the next _id from the shared counters collection, matching the
  // behaviour of CounterService.GetNextSequenceValue("MenuPermissions").
  const counter = countersCollection.findOneAndUpdate(
    { _id: "MenuPermissions" },
    { $inc: { seq: 1 } },
    { upsert: true, returnDocument: "after" }
  );
  const nextId = counter.seq;

  const now = new Date();

  menuPermissionsCollection.insertOne({
    _id: nextId,
    UserId: targetUserId,
    MenuID: menuId,
    AccessLevel: getAccessLevel(permission),
    IsActive: getIsActive(permission),
    From: getFrom(permission),
    To: getTo(permission),
    CreatedAt: now,
    CreatedBy: getCreatedBy(permission) ?? 0,
    UpdatedAt: null,
    UpdatedBy: null
  });

  insertedCount++;
});

print(`Source userId: ${sourceUserId}, Target userId: ${targetUserId}`);
print(`Inserted: ${insertedCount}, Skipped (already existed): ${skippedCount}, Total source permissions: ${sourcePermissions.length}`);
