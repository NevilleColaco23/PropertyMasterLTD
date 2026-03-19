// ============================================
// TEST AGGREGATION PIPELINE
// Test the exact MongoDB aggregation the C# code generates
// ============================================

print("🧪 Testing Room Planner Aggregation Pipeline\n");

// First, check what we have
print("📊 Current Room Collection State:");
const totalRooms = db.Room.countDocuments({});
print(`   Total rooms: ${totalRooms}`);

const prop1Count = db.Room.countDocuments({ PropertyId: 1 });
print(`   Rooms with PropertyId=1: ${prop1Count}`);

const activeCount = db.Room.countDocuments({ Active: true });
print(`   Rooms with Active=true: ${activeCount}`);

const nonDeletedCount = db.Room.countDocuments({
  $or: [
    { IsDeleted: { $ne: true } },
    { IsDeleted: { $exists: false } }
  ]
});
print(`   Non-deleted rooms: ${nonDeletedCount}\n`);

// Show sample room with PropertyId=1
print("📋 Sample room with PropertyId=1:");
const sampleRoom = db.Room.findOne({ PropertyId: 1 });
if (sampleRoom) {
  print(JSON.stringify(sampleRoom, null, 2));
} else {
  print("   ⚠️ No rooms found with PropertyId=1");
}
print("\n");

// Now test the aggregation pipeline (exactly as C# generates it)
print("🔧 Testing Aggregation Pipeline (PropertyId=1, ActiveOnly=true):\n");

const pipeline = [
  // Stage 1: $match with $and combining all conditions
  {
    $match: {
      $and: [
        // Active filter (PascalCase or camelCase)
        {
          $or: [
            { Active: true },
            { isActive: true }
          ]
        },
        // PropertyId filter (PascalCase or camelCase)
        {
          $or: [
            { PropertyId: { $in: [1] } },
            { propertyId: { $in: [1] } }
          ]
        },
        // Not deleted filter
        {
          $or: [
            { IsDeleted: { $ne: true } },
            { IsDeleted: { $exists: false } }
          ]
        }
      ]
    }
  },
  // Stage 2: $lookup property details
  {
    $lookup: {
      from: "Property",
      localField: "PropertyId",
      foreignField: "PropertyId",
      as: "propertyInfo"
    }
  },
  // Stage 3: $project fields
  {
    $project: {
      _id: "$_id",
      roomId: { $ifNull: ["$RoomId", "$_id"] },
      roomNumber: { $ifNull: ["$roomNumber", "$RoomCode"] },
      roomName: { 
        $ifNull: ["$RoomName", "$roomName", "$RoomCode", "$roomNumber"] 
      },
      roomType: { $ifNull: ["$roomType", "Standard"] },
      propertyId: { $ifNull: ["$PropertyId", "$propertyId"] },
      propertyName: { 
        $ifNull: [
          { $arrayElemAt: ["$propertyInfo.PropertyName", 0] },
          { $arrayElemAt: ["$propertyInfo.propertyName", 0] },
          "Unknown Property"
        ] 
      },
      floor: "$floor",
      capacity: "$capacity",
      status: { $ifNull: ["$status", "Available"] },
      amenities: { $ifNull: ["$amenities", []] },
      pricePerNight: "$pricePerNight",
      isActive: { $ifNull: ["$isActive", "$Active", true] }
    }
  },
  // Stage 4: $sort
  {
    $sort: {
      propertyName: 1,
      floor: 1,
      roomNumber: 1
    }
  }
];

print("Pipeline stages:");
print(JSON.stringify(pipeline, null, 2));
print("\n");

// Execute the aggregation
const results = db.Room.aggregate(pipeline).toArray();

print(`✅ Aggregation Result: ${results.length} rooms found\n`);

if (results.length > 0) {
  print("📋 Rooms returned:");
  results.forEach(room => {
    print(`   - Room ${room.roomNumber} (${room.roomName}) - PropertyId: ${room.propertyId}`);
  });
  print("\n");
  print("Sample room document:");
  print(JSON.stringify(results[0], null, 2));
} else {
  print("⚠️ No rooms returned from aggregation!");
  print("\n");
  print("🔍 Debugging each stage:\n");
  
  // Test just the $match stage
  print("Stage 1: Testing $match only:");
  const matchResults = db.Room.aggregate([pipeline[0]]).toArray();
  print(`   Matched ${matchResults.length} rooms`);
  
  if (matchResults.length > 0) {
    print("   ✅ $match stage is working");
    print(`   Sample matched room: ${JSON.stringify(matchResults[0], null, 2)}`);
  } else {
    print("   ❌ $match stage returned 0 results - this is the problem!");
    print("\n   Testing individual conditions:\n");
    
    // Test Active condition only
    const activeTest = db.Room.aggregate([
      {
        $match: {
          $or: [
            { Active: true },
            { isActive: true }
          ]
        }
      }
    ]).toArray();
    print(`   Active filter only: ${activeTest.length} rooms`);
    
    // Test PropertyId condition only
    const propTest = db.Room.aggregate([
      {
        $match: {
          $or: [
            { PropertyId: { $in: [1] } },
            { propertyId: { $in: [1] } }
          ]
        }
      }
    ]).toArray();
    print(`   PropertyId filter only: ${propTest.length} rooms`);
    
    // Test IsDeleted condition only
    const deletedTest = db.Room.aggregate([
      {
        $match: {
          $or: [
            { IsDeleted: { $ne: true } },
            { IsDeleted: { $exists: false } }
          ]
        }
      }
    ]).toArray();
    print(`   IsDeleted filter only: ${deletedTest.length} rooms`);
    
    // Test combining Active + PropertyId
    const combinedTest = db.Room.aggregate([
      {
        $match: {
          $and: [
            {
              $or: [
                { Active: true },
                { isActive: true }
              ]
            },
            {
              $or: [
                { PropertyId: { $in: [1] } },
                { propertyId: { $in: [1] } }
              ]
            }
          ]
        }
      }
    ]).toArray();
    print(`   Active + PropertyId combined: ${combinedTest.length} rooms`);
  }
  
  // Test $lookup stage
  print("\n");
  print("Stage 2: Testing $lookup:");
  const lookupResults = db.Room.aggregate([pipeline[0], pipeline[1]]).toArray();
  print(`   After $lookup: ${lookupResults.length} rooms`);
  
  // Test $project stage
  print("\n");
  print("Stage 3: Testing $project:");
  const projectResults = db.Room.aggregate([pipeline[0], pipeline[1], pipeline[2]]).toArray();
  print(`   After $project: ${projectResults.length} rooms`);
}

print("\n✅ Test complete!");
