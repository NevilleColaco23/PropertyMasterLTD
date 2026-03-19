// ============================================
// QUICK INSERT - MINIMAL SAMPLE DATA
// Copy and paste into MongoDB Shell
// ============================================

// Insert 3 rooms
db.Room.insertMany([
  {
    _id: 1001,
    PropertyId: 1,
    RoomId: 1001,
    roomNumber: "101",
    RoomCode: "101",
    RoomName: "Room 101",
    roomType: "Deluxe",
    floor: 1,
    capacity: 2,
    amenities: ["WiFi", "TV"],
    pricePerNight: 120,
    Active: true,
    isActive: true,
    IsDeleted: false,
    CreatedAt: new Date(),
    CreatedBy: 1
  },
  {
    _id: 1002,
    PropertyId: 1,
    RoomId: 1002,
    roomNumber: "102",
    RoomCode: "102",
    RoomName: "Room 102",
    roomType: "Standard",
    floor: 1,
    capacity: 2,
    amenities: ["WiFi"],
    pricePerNight: 80,
    Active: true,
    isActive: true,
    IsDeleted: false,
    CreatedAt: new Date(),
    CreatedBy: 1
  },
  {
    _id: 1003,
    PropertyId: 1,
    RoomId: 1003,
    roomNumber: "103",
    RoomCode: "103",
    RoomName: "Room 103",
    roomType: "Suite",
    floor: 1,
    capacity: 4,
    amenities: ["WiFi", "TV", "Kitchen"],
    pricePerNight: 200,
    Active: true,
    isActive: true,
    IsDeleted: false,
    CreatedAt: new Date(),
    CreatedBy: 1
  }
]);

// Insert 2 bookings (current month)
db.Bookings.insertMany([
  {
    _id: NumberLong("4000000000099001"),
    bookingId: "QUICK001",
    roomNumber: "101",
    propertyId: 1,
    checkInDate: new Date(new Date().setDate(new Date().getDate() - 3)),
    checkOutDate: new Date(new Date().setDate(new Date().getDate() + 4)),
    numberOfGuests: 2,
    totalPrice: 840,
    isConfirmed: true,
    Status: "Active",
    bookingDate: new Date(),
    lastModified: new Date()
  },
  {
    _id: NumberLong("4000000000099002"),
    bookingId: "QUICK002",
    roomNumber: "103",
    propertyId: 1,
    checkInDate: new Date(new Date().setDate(new Date().getDate() + 2)),
    checkOutDate: new Date(new Date().setDate(new Date().getDate() + 7)),
    numberOfGuests: 4,
    totalPrice: 1000,
    isConfirmed: true,
    Status: "Active",
    bookingDate: new Date(),
    lastModified: new Date()
  }
]);

print("✅ Inserted 3 rooms and 2 bookings for Property 1");
print("🎉 Go to Dashboard → Room Planner to see the data!");
