// ============================================
// Clean Up Duplicate Property Access
// ============================================
// This script removes duplicate PropertyAccessList entries for users
// Run this in MongoDB Compass after the duplicate property issue
// ============================================

use('ListingDB'); // Change to your database name

print('\n========================================');
print('🧹 Cleaning Up Duplicate Property Access');
print('========================================\n');

// ============================================
// 1. Find users with duplicate property access
// ============================================
const users = db.Users.find({ PropertyAccessList: { $exists: true, $ne: null } }).toArray();

print(`📊 Found ${users.length} users with property access lists\n`);

let totalCleaned = 0;
let totalDuplicatesRemoved = 0;

users.forEach((user, index) => {
  if (!user.PropertyAccessList || user.PropertyAccessList.length === 0) {
    return;
  }

  print(`\n👤 User ${index + 1}: ${user.UserName} (${user.Email})`);
  print(`   Current property access count: ${user.PropertyAccessList.length}`);

  // Show current access
  user.PropertyAccessList.forEach((access, i) => {
    print(`   ${i + 1}. Property ID: ${access.Id}, Active: ${access.IsActive}`);
  });

  // Find duplicates (group by Id)
  const seen = new Map();
  const uniqueAccess = [];
  const duplicates = [];

  user.PropertyAccessList.forEach(access => {
    if (!seen.has(access.Id)) {
      seen.set(access.Id, access);
      uniqueAccess.push(access);
    } else {
      duplicates.push(access);
    }
  });

  if (duplicates.length > 0) {
    print(`   ⚠️  Found ${duplicates.length} duplicate(s)!`);
    duplicates.forEach(dup => {
      print(`      - Duplicate Property ID: ${dup.Id}`);
    });

    // Update user with unique access list
    db.Users.updateOne(
      { _id: user._id },
      { $set: { PropertyAccessList: uniqueAccess } }
    );

    print(`   ✅ Cleaned! Removed ${duplicates.length} duplicate(s)`);
    print(`   New property access count: ${uniqueAccess.length}`);
    
    totalCleaned++;
    totalDuplicatesRemoved += duplicates.length;
  } else {
    print(`   ✅ No duplicates found`);
  }
});

// ============================================
// 2. Summary
// ============================================
print('\n\n========================================');
print('📊 CLEANUP SUMMARY');
print('========================================');
print(`Total users checked: ${users.length}`);
print(`Users with duplicates: ${totalCleaned}`);
print(`Total duplicates removed: ${totalDuplicatesRemoved}`);

if (totalCleaned > 0) {
  print('\n✅ Duplicate property access entries have been removed!');
  print('   Please log out and log back in to see the changes.');
} else {
  print('\n✅ No duplicates found. Your data is clean!');
}

print('\n========================================');
print('🎉 Cleanup Complete!');
print('========================================\n');

// ============================================
// 3. Verification - Show all users' property access
// ============================================
print('\n📋 Current Property Access (After Cleanup):\n');

db.Users.find({ PropertyAccessList: { $exists: true, $ne: null } }).forEach(user => {
  if (user.PropertyAccessList && user.PropertyAccessList.length > 0) {
    print(`👤 ${user.UserName} (${user.Email}):`);
    user.PropertyAccessList.forEach((access, i) => {
      const prop = db.Property.findOne({ _id: access.Id });
      const propName = prop ? prop.Name : 'Unknown Property';
      print(`   ${i + 1}. Property: ${propName} (ID: ${access.Id}, Active: ${access.IsActive})`);
    });
    print('');
  }
});

print('========================================');
print('✅ Script Execution Complete!');
print('========================================\n');
