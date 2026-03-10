// ============================================
// Add Demo Property Access to Existing User
// ============================================
// Run this script AFTER running seed-demo-property.mongodb.js
// This adds demo property access to users who signed up before demo property existed
// ============================================

use('ListingDB'); // Change to your database name

// ============================================
// Configuration
// ============================================
const DEMO_PROPERTY_ID = -1;
const userEmail = "nevillecolaco19@gmail.com"; // 👈 CHANGE THIS to your email

print('\n========================================');
print('🔧 Adding Demo Property Access to User');
print('========================================\n');

// ============================================
// 1. Check if demo property exists
// ============================================
const demoProperty = db.Properties.findOne({ _id: DEMO_PROPERTY_ID });

if (!demoProperty) {
  print('❌ Error: Demo property does not exist!');
  print('   Please run seed-demo-property.mongodb.js first.');
  quit();
}

print(`✅ Demo property found: "${demoProperty.Name}"`);

// ============================================
// 2. Find the user
// ============================================
const user = db.Users.findOne({ Email: userEmail });

if (!user) {
  print(`❌ Error: User with email "${userEmail}" not found!`);
  print('   Available users:');
  db.Users.find({}, { _id: 1, Email: 1, UserName: 1 }).forEach(u => {
    print(`   - ID: ${u._id}, Email: ${u.Email}, Username: ${u.UserName}`);
  });
  quit();
}

print(`✅ User found: ${user.UserName} (ID: ${user._id})`);

// ============================================
// 3. Check current property access
// ============================================
print('\n📋 Current Property Access:');

if (user.PropertyAccessList && user.PropertyAccessList.length > 0) {
  print(`   User has access to ${user.PropertyAccessList.length} properties:`);
  user.PropertyAccessList.forEach((access, index) => {
    const prop = db.Properties.findOne({ _id: access.Id });
    const propName = prop ? prop.Name : 'Unknown Property';
    print(`   ${index + 1}. Property ID: ${access.Id} - ${propName} (Active: ${access.IsActive})`);
  });
  
  // Check if already has demo access
  const hasDemoAccess = user.PropertyAccessList.some(p => p.Id === DEMO_PROPERTY_ID);
  if (hasDemoAccess) {
    print('\n⚠️  User already has access to demo property!');
    print('   No changes needed.');
    quit();
  }
} else {
  print('   User has no property access yet.');
}

// ============================================
// 4. Add demo property access
// ============================================
print('\n📝 Adding demo property access...');

const result = db.Users.updateOne(
  { _id: user._id },
  {
    $push: {
      PropertyAccessList: {
        Id: DEMO_PROPERTY_ID,              // Property._id reference
        IsActive: true,                     // PropertyAccessList.IsActive
        From: new Date(),                   // PropertyAccessList.From
        To: new Date(Date.now() + 365*24*60*60*1000), // 1 year access
        CreatedDate: new Date(),            // PropertyAccessList.CreatedDate
        CreatedBy: user._id,                // PropertyAccessList.CreatedBy
        IsDemo: true                        // Custom flag (not in domain model but useful)
      }
    }
  }
);

if (result.modifiedCount > 0) {
  print('✅ Demo property access added successfully!');
} else {
  print('❌ Failed to add demo property access.');
}

// ============================================
// 5. Verification
// ============================================
print('\n========================================');
print('✨ VERIFICATION');
print('========================================\n');

const updatedUser = db.Users.findOne({ _id: user._id });

print(`User: ${updatedUser.UserName} (${updatedUser.Email})`);
print(`Properties Access: ${updatedUser.PropertyAccessList ? updatedUser.PropertyAccessList.length : 0}`);

if (updatedUser.PropertyAccessList && updatedUser.PropertyAccessList.length > 0) {
  print('\n📌 Property Access List:');
  updatedUser.PropertyAccessList.forEach((access, index) => {
    const prop = db.Properties.findOne({ _id: access.Id });
    const propName = prop ? prop.Name : 'Unknown';
    const isDemo = access.Id === DEMO_PROPERTY_ID ? '🎪 DEMO' : '';
    print(`   ${index + 1}. Property: ${propName} (ID: ${access.Id}) ${isDemo}`);
    print(`      - Active: ${access.IsActive}`);
    print(`      - From: ${access.From}`);
    print(`      - To: ${access.To}`);
  });
}

print('\n========================================');
print('🎉 User Setup Complete!');
print('========================================\n');
