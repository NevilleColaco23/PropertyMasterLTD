// ============================================
// Demo Property Seeding Script
// ============================================
// Run this script in MongoDB Compass or mongo shell
// Database: ListingDB (or your database name)
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
} else {
  db.Properties.insertOne({
    "_id": -1,
    "Name": "The Grand Hotel - Demo",
    "Active": true,
    "PropertyCode": "DEMO0001",
    "CompanyLogoURL": "https://placehold.co/200x200/4CAF50/white?text=DEMO",
    "Rooms": [
      {
        "_id": "room-101",
        "RoomCode": "101",
        "RoomName": "Deluxe King Room",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/2196F3/white?text=Room+101"
      },
      {
        "_id": "room-102",
        "RoomCode": "102",
        "RoomName": "Deluxe Queen Room",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/2196F3/white?text=Room+102"
      },
      {
        "_id": "room-201",
        "RoomCode": "201",
        "RoomName": "Executive Suite",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/9C27B0/white?text=Suite+201"
      },
      {
        "_id": "room-202",
        "RoomCode": "202",
        "RoomName": "Presidential Suite",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/9C27B0/white?text=Suite+202"
      },
      {
        "_id": "room-301",
        "RoomCode": "301",
        "RoomName": "Family Room",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/FF9800/white?text=Family+301"
      },
      {
        "_id": "room-302",
        "RoomCode": "302",
        "RoomName": "Ocean View Room",
        "Active": true,
        "CompanyLogoURL": "https://placehold.co/400x300/00BCD4/white?text=Ocean+302"
      }
    ],
    "CreatedAt": new Date(),
    "IsDemo": true
  });
  print('✅ Demo property created successfully!');
}

// ============================================
// 2. CREATE DEMO BOOKINGS
// ============================================
print('\n📝 Step 2: Creating Demo Bookings...');

// Check if demo bookings already exist
const existingBookings = db.Bookings.countDocuments({ PropertyID: -1 });

if (existingBookings > 0) {
  print(`⚠️  ${existingBookings} demo bookings already exist. Skipping creation.`);
} else {
  db.Bookings.insertMany([
    {
      "_id": -1,
      "PropertyID": -1,
      "RoomCode": "101",
      "GuestName": "John Smith",
      "GuestEmail": "john.smith@example.com",
      "CheckIn": new Date(Date.now() - 10*24*60*60*1000), // 10 days ago
      "CheckOut": new Date(Date.now() - 7*24*60*60*1000),  // 7 days ago
      "Status": "Completed",
      "TotalAmount": 450.00,
      "CreatedAt": new Date(Date.now() - 15*24*60*60*1000),
      "IsDemo": true
    },
    {
      "_id": -2,
      "PropertyID": -1,
      "RoomCode": "201",
      "GuestName": "Sarah Johnson",
      "GuestEmail": "sarah.j@example.com",
      "CheckIn": new Date(Date.now() - 2*24*60*60*1000),  // 2 days ago
      "CheckOut": new Date(Date.now() + 2*24*60*60*1000),  // 2 days from now
      "Status": "Active",
      "TotalAmount": 800.00,
      "CreatedAt": new Date(Date.now() - 5*24*60*60*1000),
      "IsDemo": true
    },
    {
      "_id": -3,
      "PropertyID": -1,
      "RoomCode": "302",
      "GuestName": "Michael Brown",
      "GuestEmail": "m.brown@example.com",
      "CheckIn": new Date(Date.now() + 5*24*60*60*1000),  // 5 days from now
      "CheckOut": new Date(Date.now() + 12*24*60*60*1000), // 12 days from now
      "Status": "Confirmed",
      "TotalAmount": 1200.00,
      "CreatedAt": new Date(Date.now() - 3*24*60*60*1000),
      "IsDemo": true
    },
    {
      "_id": -4,
      "PropertyID": -1,
      "RoomCode": "102",
      "GuestName": "Emma Davis",
      "GuestEmail": "emma.davis@example.com",
      "CheckIn": new Date(Date.now() + 10*24*60*60*1000), // 10 days from now
      "CheckOut": new Date(Date.now() + 14*24*60*60*1000), // 14 days from now
      "Status": "Pending",
      "TotalAmount": 600.00,
      "CreatedAt": new Date(),
      "IsDemo": true
    }
  ]);
  print('✅ 4 demo bookings created successfully!');
}

// ============================================
// 3. CREATE DEMO MENUS
// ============================================
print('\n📝 Step 3: Creating Demo Menus...');

// Check if demo menus already exist
const existingMenus = db.Menus.countDocuments({ PropertyID: -1 });

if (existingMenus > 0) {
  print(`⚠️  ${existingMenus} demo menus already exist. Skipping creation.`);
} else {
  db.Menus.insertMany([
    {
      "_id": -1,
      "PropertyID": -1,
      "MenuName": "Breakfast Menu",
      "MenuType": "Breakfast",
      "Active": true,
      "AvailableFrom": "06:00",
      "AvailableTo": "11:00",
      "Items": [
        {
          "ItemName": "Continental Breakfast",
          "Description": "Croissant, jam, butter, coffee/tea",
          "Price": 12.99,
          "Category": "Breakfast",
          "Available": true
        },
        {
          "ItemName": "Full English Breakfast",
          "Description": "Eggs, bacon, sausage, beans, toast",
          "Price": 18.99,
          "Category": "Breakfast",
          "Available": true
        },
        {
          "ItemName": "Pancake Stack",
          "Description": "Stack of 3 with maple syrup",
          "Price": 14.99,
          "Category": "Breakfast",
          "Available": true
        }
      ],
      "CreatedAt": new Date(),
      "IsDemo": true
    },
    {
      "_id": -2,
      "PropertyID": -1,
      "MenuName": "Lunch Menu",
      "MenuType": "Lunch",
      "Active": true,
      "AvailableFrom": "12:00",
      "AvailableTo": "15:00",
      "Items": [
        {
          "ItemName": "Caesar Salad",
          "Description": "Romaine lettuce, parmesan, croutons",
          "Price": 16.99,
          "Category": "Salads",
          "Available": true
        },
        {
          "ItemName": "Grilled Chicken Sandwich",
          "Description": "With fries and coleslaw",
          "Price": 19.99,
          "Category": "Sandwiches",
          "Available": true
        },
        {
          "ItemName": "Fish & Chips",
          "Description": "Battered cod with chunky chips",
          "Price": 22.99,
          "Category": "Mains",
          "Available": true
        }
      ],
      "CreatedAt": new Date(),
      "IsDemo": true
    },
    {
      "_id": -3,
      "PropertyID": -1,
      "MenuName": "Dinner Menu",
      "MenuType": "Dinner",
      "Active": true,
      "AvailableFrom": "18:00",
      "AvailableTo": "22:00",
      "Items": [
        {
          "ItemName": "Ribeye Steak",
          "Description": "12oz ribeye with vegetables",
          "Price": 34.99,
          "Category": "Steaks",
          "Available": true
        },
        {
          "ItemName": "Grilled Salmon",
          "Description": "Atlantic salmon with lemon butter",
          "Price": 28.99,
          "Category": "Seafood",
          "Available": true
        },
        {
          "ItemName": "Vegetarian Pasta",
          "Description": "Penne with roasted vegetables",
          "Price": 22.99,
          "Category": "Pasta",
          "Available": true
        }
      ],
      "CreatedAt": new Date(),
      "IsDemo": true
    }
  ]);
  print('✅ 3 demo menus created successfully!');
}

// ============================================
// 4. VERIFICATION
// ============================================
print('\n\n========================================');
print('✨ VERIFICATION SUMMARY');
print('========================================');

// Check demo property
const demoProperty = db.Properties.findOne({ _id: -1 });
if (demoProperty) {
  print(`✅ Demo Property: "${demoProperty.Name}" (ID: ${demoProperty._id})`);
  print(`   - Rooms: ${demoProperty.Rooms.length}`);
  print(`   - Property Code: ${demoProperty.PropertyCode}`);
} else {
  print('❌ Demo Property: NOT FOUND');
}

// Check demo bookings
const bookingCount = db.Bookings.countDocuments({ PropertyID: -1 });
print(`${bookingCount > 0 ? '✅' : '❌'} Demo Bookings: ${bookingCount}`);

if (bookingCount > 0) {
  const bookingStatuses = db.Bookings.aggregate([
    { $match: { PropertyID: -1 } },
    { $group: { _id: "$Status", count: { $sum: 1 } } }
  ]).toArray();
  
  bookingStatuses.forEach(status => {
    print(`   - ${status._id}: ${status.count}`);
  });
}

// Check demo menus
const menuCount = db.Menus.countDocuments({ PropertyID: -1 });
print(`${menuCount > 0 ? '✅' : '❌'} Demo Menus: ${menuCount}`);

if (menuCount > 0) {
  const menus = db.Menus.find({ PropertyID: -1 }, { MenuName: 1, MenuType: 1 }).toArray();
  menus.forEach(menu => {
    print(`   - ${menu.MenuName} (${menu.MenuType})`);
  });
}

print('\n========================================');
print('🎉 Demo Property Setup Complete!');
print('========================================\n');

// ============================================
// 5. SHOW SAMPLE DATA
// ============================================
print('📊 Sample Demo Property Data:\n');
print(JSON.stringify(db.Properties.findOne({ _id: -1 }), null, 2));
