// ============================================
// DEBUG SCRIPT: Check Property Selection
// Run this to see which properties have rooms
// ============================================

print("🔍 Checking all properties with rooms...\n");

// Get distinct property IDs from Room collection
const propertyIds = db.Room.distinct("PropertyId");

print(`📊 Found ${propertyIds.length} distinct property IDs in Room collection:`);
print(JSON.stringify(propertyIds, null, 2));
print("");

// Check room count for each property
propertyIds.forEach(propId => {
  const count = db.Room.countDocuments({ PropertyId: propId, IsDeleted: { $ne: true } });
  const activeCount = db.Room.countDocuments({ PropertyId: propId, Active: true, IsDeleted: { $ne: true } });
  
  print(`🏨 Property ID: ${propId}`);
  print(`   - Total rooms: ${count}`);
  print(`   - Active rooms: ${activeCount}`);
  
  // Show sample rooms
  const sampleRooms = db.Room.find({ PropertyId: propId, IsDeleted: { $ne: true } })
    .limit(3)
    .toArray();
  
  sampleRooms.forEach(room => {
    print(`   - Room: ${room.RoomCode || room.roomNumber} (${room.RoomName || 'No name'})`);
  });
  print("");
});

// Check if Property -1 exists
print("🔍 Checking for Property ID -1...");
const prop1Rooms = db.Room.countDocuments({ PropertyId: -1 });
print(`   Rooms with PropertyId = -1: ${prop1Rooms}`);

if (prop1Rooms === 0) {
  print("   ⚠️ No rooms found for PropertyId = -1");
  print("   💡 This is why Room Planner shows 'No rooms available'");
}
print("");

// Show Property collection info
print("🏢 Checking Property collection...");
const properties = db.Property.find({}).limit(5).toArray();
print(`   Found ${properties.length} properties in Property collection`);
properties.forEach(prop => {
  print(`   - PropertyId: ${prop.PropertyId || prop._id}, Name: ${prop.Name || prop.PropertyName || 'Unknown'}`);
});
