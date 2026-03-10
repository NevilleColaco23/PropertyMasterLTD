// ============================================
// Cleanup Duplicate PropertyAccessList Entries
// ============================================
// This script removes duplicate property access entries from users
// Run this in MongoDB Compass
// ============================================

use('ListingDB');

print('\n========================================');
print('?? Cleaning Up Duplicate Property Access');
print('========================================\n');

// Find all users
const users = db.Users.find({}).toArray();

let cleanedCount = 0;
let totalDuplicatesRemoved = 0;

users.forEach(user => {
  if (!user.PropertyAccessList || user.PropertyAccessList.length === 0) {
    return; // Skip users with no property access
  }

  const originalCount = user.PropertyAccessList.length;
  
  // Remove duplicates based on Id field
  const uniqueProperties = [];
  const seenIds = new Set();
  
  user.PropertyAccessList.forEach(property => {
    if (!seenIds.has(property.Id)) {
      seenIds.add(property.Id);
      uniqueProperties.push(property);
    }
  });
  
  if (uniqueProperties.length < originalCount) {
    // Update user with deduplicated list
    db.Users.updateOne(
      { _id: user._id },
      { $set: { PropertyAccessList: uniqueProperties } }
    );
    
    const duplicatesRemoved = originalCount - uniqueProperties.length;
    print(`? User ${user.Email} (ID: ${user._id})`);
    print(`   - Original: ${originalCount} properties`);
    print(`   - Cleaned: ${uniqueProperties.length} properties`);
    print(`   - Removed: ${duplicatesRemoved} duplicates\n`);
    
    cleanedCount++;
    totalDuplicatesRemoved += duplicatesRemoved;
  }
});

print('\n========================================');
print('?? CLEANUP SUMMARY');
print('========================================');
print(`Users cleaned: ${cleanedCount}`);
print(`Total duplicates removed: ${totalDuplicatesRemoved}`);
print('========================================\n');

// Verify the specific user
const userEmail = "nevillecolaco19@gmail.com";  // ?? Change this to your email
const verifyUser = db.Users.findOne({ Email: userEmail });

if (verifyUser) {
  print(`\n? Verification for ${userEmail}:`);
  print(`   Property Access Count: ${verifyUser.PropertyAccessList ? verifyUser.PropertyAccessList.length : 0}`);
  
  if (verifyUser.PropertyAccessList && verifyUser.PropertyAccessList.length > 0) {
    print('\n   ?? Properties:');
    verifyUser.PropertyAccessList.forEach((access, index) => {
      const prop = db.Property.findOne({ _id: access.Id });
      const propName = prop ? prop.Name : 'Unknown';
      print(`      ${index + 1}. ${propName} (ID: ${access.Id})`);
    });
  }
} else {
  print(`\n??  User ${userEmail} not found`);
}

print('\n?? Cleanup Complete!');
