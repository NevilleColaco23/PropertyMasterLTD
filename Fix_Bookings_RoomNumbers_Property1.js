// ========================================
// Fix Bookings Room Numbers for Property 1
// ========================================
// This script updates all bookings for property 1 to use valid room numbers
// from the existing rooms in the Rooms collection

use('mywarehouse');

// Step 1: Get valid room numbers for property 1
const validRooms = db.rooms.find({ propertyId: 1 }).toArray();
const validRoomNumbers = validRooms.map(r => r.roomNumber);

print('========================================');
print('📊 Valid Rooms for Property 1:');
print(validRoomNumbers);
print('========================================\n');

// Step 2: Get all bookings for property 1 with invalid room numbers
const invalidBookings = db.bookings.find({
  propertyId: 1,
  roomNumber: { $nin: validRoomNumbers }
}).toArray();

print(`Found ${invalidBookings.length} bookings with invalid room numbers\n`);

// Step 3: Distribute bookings evenly across valid rooms
let roomIndex = 0;
let updatedCount = 0;

invalidBookings.forEach((booking, index) => {
  // Use round-robin to distribute bookings across rooms
  const newRoomNumber = validRoomNumbers[roomIndex % validRoomNumbers.length];
  
  // Update the booking
  const result = db.bookings.updateOne(
    { _id: booking._id },
    { 
      $set: { 
        roomNumber: newRoomNumber,
        lastModified: new Date()
      } 
    }
  );
  
  if (result.modifiedCount > 0) {
    updatedCount++;
    if (updatedCount <= 10) {
      print(`✅ Updated booking ${booking.bookingId}: ${booking.roomNumber} → ${newRoomNumber}`);
    }
  }
  
  roomIndex++;
});

print(`\n========================================`);
print(`✅ Updated ${updatedCount} bookings`);
print(`========================================\n`);

// Step 4: Verify - show distribution
print('📊 Bookings per room after update:');
validRoomNumbers.forEach(roomNum => {
  const count = db.bookings.countDocuments({ 
    propertyId: 1, 
    roomNumber: roomNum,
    checkInDate: { $gte: new Date('2026-03-01') },
    checkOutDate: { $lte: new Date('2026-03-31') }
  });
  print(`   Room ${roomNum}: ${count} bookings`);
});

print('\n✅ Done! Refresh your Room Planner to see the bookings.');
