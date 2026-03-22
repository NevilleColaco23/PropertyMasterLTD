// ========================================
// Add Missing Rooms for Property 1
// ========================================
// This script adds rooms that bookings are referencing but don't exist yet

use('mywarehouse');

// Step 1: Get all unique room numbers from bookings for property 1
const bookingRoomNumbers = db.bookings.distinct('roomNumber', { propertyId: 1 });

print('========================================');
print(`📊 Found ${bookingRoomNumbers.length} unique room numbers in bookings`);
print('========================================\n');

// Step 2: Get existing room numbers
const existingRoomNumbers = db.rooms.distinct('roomNumber', { propertyId: 1 });

print(`📊 Existing rooms in Rooms collection: ${existingRoomNumbers.length}`);
print(existingRoomNumbers);
print('');

// Step 3: Find missing rooms
const missingRoomNumbers = bookingRoomNumbers.filter(rn => !existingRoomNumbers.includes(rn));

print(`📊 Missing rooms that need to be created: ${missingRoomNumbers.length}`);
print(missingRoomNumbers.slice(0, 20)); // Show first 20
print('');

// Step 4: Create missing rooms
const roomTypes = ['Standard', 'Deluxe', 'Suite', 'Executive', 'Presidential'];
const roomsToInsert = [];

missingRoomNumbers.forEach(roomNumber => {
  // Determine room type based on room number
  let roomType = 'Standard';
  const num = parseInt(roomNumber);
  
  if (!isNaN(num)) {
    if (num >= 1000) roomType = 'Presidential';
    else if (num >= 700) roomType = 'Executive';
    else if (num >= 500) roomType = 'Suite';
    else if (num >= 300) roomType = 'Deluxe';
  }
  
  // Determine floor
  const floor = Math.floor(num / 100) || 1;
  
  roomsToInsert.push({
    propertyId: 1,
    roomNumber: roomNumber,
    roomType: roomType,
    floor: floor,
    maxOccupancy: roomType === 'Presidential' ? 6 : roomType === 'Suite' ? 4 : 2,
    pricePerNight: roomType === 'Presidential' ? 500 : roomType === 'Executive' ? 300 : roomType === 'Suite' ? 200 : roomType === 'Deluxe' ? 150 : 100,
    status: 'Available',
    amenities: ['WiFi', 'TV', 'AC'],
    isActive: true,
    createdDate: new Date(),
    lastModified: new Date()
  });
});

print(`\n🚀 Creating ${roomsToInsert.length} new rooms...`);

if (roomsToInsert.length > 0) {
  const result = db.rooms.insertMany(roomsToInsert);
  print(`✅ Created ${result.insertedCount} rooms`);
} else {
  print('ℹ️ No rooms to create - all room numbers already exist');
}

// Step 5: Verify
const totalRooms = db.rooms.countDocuments({ propertyId: 1 });
print(`\n📊 Total rooms for property 1: ${totalRooms}`);

print('\n✅ Done! Refresh your Room Planner to see all bookings.');
