// ============================================
// Demo Property Seeding Script
// ============================================
// Run this script in MongoDB Compass or mongo shell
// Database: ListingDB (or your database name)
// ============================================
// IMPORTANT: This script matches YOUR actual Property model schema
// ============================================

use('ListingDB'); // Change to your database name if different

// ============================================
// 1. CREATE DEMO PROPERTY
// ============================================
print('\n📝 Step 1: Creating Demo Property...');

// Check if demo property already exists
// NOTE: Collection name is "Property" (singular), not "Properties"!
const existingProperty = db.Property.findOne({ _id: -1 });

if (existingProperty) {
  print('⚠️  Demo property already exists. Skipping creation.');
  print(`   Current demo property: ${existingProperty.Name}`);
} else {
  db.Property.insertOne({
    "_id": -1,
    "Name": "The Grand Hotel - Demo",
    "Active": true,
    "PropertyCode": "DEMO0001",  // Note: Your existing property has "PropertCode" (typo), but keeping correct spelling
    "CompanyLogoURL": "https://placehold.co/200x200/4CAF50/white?text=DEMO",
    "Rooms": [
      {
        "Id": "room-101",  // Note: Using "Id" to match your schema (not "_id")
        "RoomCode": "101",
        "RoomName": "Deluxe King Room",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/2196F3/white?text=Room+101"
      },
      {
        "Id": "room-102",
        "RoomCode": "102",
        "RoomName": "Deluxe Queen Room",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/2196F3/white?text=Room+102"
      },
      {
        "Id": "room-201",
        "RoomCode": "201",
        "RoomName": "Executive Suite",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/9C27B0/white?text=Suite+201"
      },
      {
        "Id": "room-202",
        "RoomCode": "202",
        "RoomName": "Presidential Suite",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/9C27B0/white?text=Suite+202"
      },
      {
        "Id": "room-301",
        "RoomCode": "301",
        "RoomName": "Family Room",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/FF9800/white?text=Family+301"
      },
      {
        "Id": "room-302",
        "RoomCode": "302",
        "RoomName": "Ocean View Room",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/00BCD4/white?text=Ocean+302"
      }
    ],
    "AccessList": [],  // Empty access list - will be populated as users are granted access
    "CreatedAt": new Date(),
    "IsDemo": true
  });
  print('✅ Demo property created successfully!');
}

// ============================================
// Demo Property Seeding Script
// ============================================
// Run this script in MongoDB Compass or mongo shell
// Database: ListingDB (or your database name)
// ============================================
// IMPORTANT: This script matches YOUR actual Property model schema
// Based on: classfiles/Domain/Property/Property.cs
// Based on: classfiles/Domain/Users/Users.cs (PropertyAccessList)
// ============================================

use('ListingDB'); // Change to your database name if different

// ============================================
// 1. CREATE DEMO PROPERTY
// ============================================
print('\n📝 Step 1: Creating Demo Property...');

// Check if demo property already exists
const existingProperty = db.Properties.findOne({ _id: -1 });

if (existingProperty) {
  print('⚠️  Demo property already exists. Skipping creation.');
  print(`   Current demo property: ${existingProperty.Name}`);
} else {
  db.Properties.insertOne({
    "_id": -1,
    "Name": "The Grand Hotel - Demo",
    "Active": true,
    "PropertyCode": "DEMO0001",
    "CompanyLogoURL": "https://placehold.co/200x200/4CAF50/white?text=DEMO",
    "Rooms": [
      {
        "Id": "room-demo-101",  // Room.Id field (string)
        "RoomCode": "101",       // Room.RoomCode
        "RoomName": "Deluxe King Room",  // Room.RoomName
        "Active": true,          // Room.Active
        "CompanyLogoURL": "https://placehold.co/400x300/2196F3/white?text=Room+101"
      },
      {
        "Id": "room-demo-102",
        "RoomCode": "102",
        "RoomName": "Deluxe Queen Room",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/2196F3/white?text=Room+102"
      },
      {
        "Id": "room-demo-201",
        "RoomCode": "201",
        "RoomName": "Executive Suite",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/9C27B0/white?text=Suite+201"
      },
      {
        "Id": "room-demo-202",
        "RoomCode": "202",
        "RoomName": "Presidential Suite",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/9C27B0/white?text=Suite+202"
      },
      {
        "Id": "room-demo-301",
        "RoomCode": "301",
        "RoomName": "Family Room",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/FF9800/white?text=Family+301"
      },
      {
        "Id": "room-demo-302",
        "RoomCode": "302",
        "RoomName": "Ocean View Room",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/00BCD4/white?text=Ocean+302"
      }
    ],
    "AccessList": [],  // Empty - will be populated as users are granted access
    "IsDemo": true
  });
  print('✅ Demo property created successfully with 6 rooms!');
}

// ============================================
// 2. VERIFICATION
// ============================================
print('\n\n========================================');
print('✨ VERIFICATION SUMMARY');
print('========================================');

// Check demo property (using correct collection name)
const demoProperty = db.Property.findOne({ _id: -1 });
if (demoProperty) {
  print(`✅ Demo Property: "${demoProperty.Name}" (ID: ${demoProperty._id})`);
  print(`   - Rooms: ${demoProperty.Rooms.length}`);
  print(`   - Property Code: ${demoProperty.PropertyCode}`);
  print(`   - Active: ${demoProperty.Active}`);
  print(`   - Logo URL: ${demoProperty.CompanyLogoURL}`);

  print('\n   📌 Rooms:');
  demoProperty.Rooms.forEach((room, index) => {
    print(`      ${index + 1}. ${room.RoomCode} - ${room.RoomName} (${room.Active ? 'Active' : 'Inactive'})`);
  });
} else {
  print('❌ Demo Property: NOT FOUND');
}

print('\n========================================');
print('🎉 Demo Property Setup Complete!');
print('========================================');

// ============================================
// 3. SHOW COMPLETE STRUCTURE (for debugging)
// ============================================
print('\n📊 Complete Demo Property Document:\n');
const demoProp = db.Property.findOne({ _id: -1 });
if (demoProp) {
  print(JSON.stringify(demoProp, null, 2));
} else {
  print('❌ Demo property not found!');
}

// ============================================
// 4. COMPARISON WITH YOUR EXISTING PROPERTY
// ============================================
print('\n\n========================================');
print('📋 SCHEMA COMPARISON');
print('========================================');

const yourProperty = db.Property.findOne({ _id: 100 });
if (yourProperty) {
  print('\n✅ Your Existing Property Structure:');
  print(JSON.stringify(yourProperty, null, 2));

  print('\n📝 Schema Fields:');
  print(`   - _id: ${typeof yourProperty._id} (${yourProperty._id})`);
  print(`   - Name: ${typeof yourProperty.Name} ("${yourProperty.Name}")`);
  print(`   - Active: ${typeof yourProperty.Active} (${yourProperty.Active})`);
  print(`   - PropertyCode: ${typeof yourProperty.PropertyCode} (${yourProperty.PropertyCode || 'undefined'})`);
  print(`   - PropertCode (typo?): ${typeof yourProperty.PropertCode} (${yourProperty.PropertCode || 'undefined'})`);
  print(`   - Rooms: Array with ${yourProperty.Rooms.length} items`);
  print(`   - AccessList: ${yourProperty.AccessList ? `Array with ${yourProperty.AccessList.length} items` : 'undefined'}`);

  if (yourProperty.Rooms && yourProperty.Rooms.length > 0) {
    print('\n   📌 Room Structure:');
    const room = yourProperty.Rooms[0];
    Object.keys(room).forEach(key => {
      print(`      - ${key}: ${typeof room[key]}`);
    });
  }
} else {
  print('⚠️  No property found with ID: 100');
}

print('\n========================================');
print('✅ Script Execution Complete!');
print('========================================\n');
