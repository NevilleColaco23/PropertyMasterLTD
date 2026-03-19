// ============================================
// DEBUG SCRIPT FOR ROOM PLANNER
// Run this in MongoDB Compass or mongosh to verify data
// ============================================

print("🔍 ROOM PLANNER DEBUG REPORT");
print("=" .repeat(60));

// Check Rooms
print("\n📦 CHECKING ROOMS COLLECTION:");
const roomCount = db.Room.countDocuments({ PropertyId: 1, IsDeleted: false });
print(`  Total rooms for Property 1: ${roomCount}`);

if (roomCount > 0) {
  print("\n  Sample rooms:");
  db.Room.find({ PropertyId: 1, IsDeleted: false })
    .limit(3)
    .forEach(room => {
      print(`    - ${room.RoomName} (${room.roomNumber}) - ${room.status}`);
    });
} else {
  print("  ❌ NO ROOMS FOUND! Run SampleData_RoomPlanner.js first!");
}

// Check Bookings
print("\n📅 CHECKING BOOKINGS COLLECTION:");
const bookingCount = db.Bookings.countDocuments({ propertyId: 1, Status: "Active" });
print(`  Total active bookings for Property 1: ${bookingCount}`);

if (bookingCount > 0) {
  print("\n  Sample bookings:");
  db.Bookings.find({ propertyId: 1, Status: "Active" })
    .limit(3)
    .forEach(booking => {
      const checkIn = booking.checkInDate.toISOString().split('T')[0];
      const checkOut = booking.checkOutDate.toISOString().split('T')[0];
      print(`    - ${booking.bookingId}: Room ${booking.roomNumber} (${checkIn} to ${checkOut})`);
    });
} else {
  print("  ❌ NO BOOKINGS FOUND! Run SampleData_RoomPlanner.js first!");
}

// Check Property
print("\n🏨 CHECKING PROPERTY COLLECTION:");
const property = db.Property.findOne({ PropertyId: 1 });
if (property) {
  print(`  ✅ Property ID 1 exists: ${property.PropertyName || property.Name || 'Unknown'}`);
} else {
  print("  ❌ Property ID 1 NOT FOUND!");
  print("  Note: If Property collection uses different ID field, rooms may not load");
}

// Check Room Query Compatibility
print("\n🔧 TESTING QUERY COMPATIBILITY:");
const testQuery = db.Room.aggregate([
  { $match: { 
    $or: [
      { PropertyId: 1 },
      { propertyId: 1 }
    ],
    $or: [
      { IsDeleted: { $ne: true } },
      { isDeleted: { $ne: true } }
    ]
  }},
  { $project: {
    roomId: { $ifNull: ["$RoomId", "$roomId"] },
    roomNumber: { $ifNull: ["$RoomCode", "$roomNumber"] },
    roomName: { $ifNull: ["$RoomName", "$roomName"] },
    roomType: { $ifNull: ["$roomType", "Standard"] },
    propertyId: { $ifNull: ["$PropertyId", "$propertyId"] },
    floor: { $ifNull: ["$floor", null] },
    capacity: { $ifNull: ["$capacity", null] },
    status: { $ifNull: ["$status", "Available"] },
    amenities: { $ifNull: ["$amenities", []] },
    pricePerNight: { $ifNull: ["$pricePerNight", null] },
    isActive: { $ifNull: ["$Active", "$isActive"] }
  }},
  { $sort: { floor: 1, roomNumber: 1 } }
]).toArray();

print(`  Query returned ${testQuery.length} rooms`);
if (testQuery.length > 0) {
  print("  ✅ Query works! Sample result:");
  const sample = testQuery[0];
  print(`    - Room ID: ${sample.roomId}`);
  print(`    - Room Number: ${sample.roomNumber}`);
  print(`    - Room Name: ${sample.roomName}`);
  print(`    - Property ID: ${sample.propertyId}`);
  print(`    - Status: ${sample.status}`);
} else {
  print("  ❌ Query returned no results!");
}

print("\n" + "=".repeat(60));
print("🎯 TROUBLESHOOTING STEPS:");
print("1. Make sure Property 1 is selected in the UI (Change Properties button)");
print("2. Check browser console for API errors (F12 → Console tab)");
print("3. Verify API endpoint: GET /api/v1/dashboard/rooms?userId=1&propertyIds=1");
print("4. If no data shows, try clicking 'Refresh' button in Room Planner");
print("5. Switch to 'Room Planner' tab to trigger data load");
print("=".repeat(60));
