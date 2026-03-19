// ============================================
// SAMPLE DATA FOR ROOM PLANNER
// MongoDB Script to insert sample rooms and bookings
// ============================================

// Connect to your database
// use PropertyMaster

print("🏨 Inserting sample rooms...");

// Clear existing test rooms to avoid duplicate key errors
print("🧹 Cleaning up old test data...");
db.Room.deleteMany({ RoomId: { $in: [1001, 1002, 1003, 2001, 2002, 3001, 3002] } });
db.Bookings.deleteMany({ 
  $or: [
    { bookingId: { $regex: /^TEST/ } },
    { _id: { $in: [
      NumberLong("4000000000001001"),
      NumberLong("4000000000001002"),
      NumberLong("4000000000001003"),
      NumberLong("4000000000001004"),
      NumberLong("4000000000001005"),
      NumberLong("4000000000001006")
    ] } }
  ]
});
print("✅ Old data cleaned");

// Insert sample rooms for Property 1
db.Room.insertMany([
  {
    _id: NumberLong("1001"),
    PropertyId: 1,
    RoomId: 1001,
    roomNumber: "101",
    RoomCode: "101",
    RoomName: "Deluxe Room 101",
    roomType: "Deluxe",
    floor: 1,
    capacity: 2,
    status: "Available",
    amenities: ["WiFi", "TV", "Air Conditioning", "Mini Bar"],
    pricePerNight: 120.00,
    Active: true,
    isActive: true,
    CompanyLogoURL: "",
    CreatedAt: new Date(),
    CreatedBy: 1,
    UpdatedAt: null,
    UpdatedBy: null,
    IsDeleted: false,
    DeletedAt: null,
    DeletedBy: null
  },
  {
    _id: NumberLong("1002"),
    PropertyId: 1,
    RoomId: 1002,
    roomNumber: "102",
    RoomCode: "102",
    RoomName: "Standard Room 102",
    roomType: "Standard",
    floor: 1,
    capacity: 2,
    status: "Available",
    amenities: ["WiFi", "TV", "Air Conditioning"],
    pricePerNight: 80.00,
    Active: true,
    isActive: true,
    CompanyLogoURL: "",
    CreatedAt: new Date(),
    CreatedBy: 1,
    UpdatedAt: null,
    UpdatedBy: null,
    IsDeleted: false,
    DeletedAt: null,
    DeletedBy: null
  },
  {
    _id: NumberLong("1003"),
    PropertyId: 1,
    RoomId: 1003,
    roomNumber: "103",
    RoomCode: "103",
    RoomName: "Standard Room 103",
    roomType: "Standard",
    floor: 1,
    capacity: 2,
    status: "Available",
    amenities: ["WiFi", "TV"],
    pricePerNight: 80.00,
    Active: true,
    isActive: true,
    CompanyLogoURL: "",
    CreatedAt: new Date(),
    CreatedBy: 1,
    UpdatedAt: null,
    UpdatedBy: null,
    IsDeleted: false,
    DeletedAt: null,
    DeletedBy: null
  },
  {
    _id: NumberLong("2001"),
    PropertyId: 1,
    RoomId: 2001,
    roomNumber: "201",
    RoomCode: "201",
    RoomName: "Suite 201",
    roomType: "Suite",
    floor: 2,
    capacity: 4,
    status: "Available",
    amenities: ["WiFi", "TV", "Air Conditioning", "Mini Bar", "Balcony", "Kitchen"],
    pricePerNight: 250.00,
    Active: true,
    isActive: true,
    CompanyLogoURL: "",
    CreatedAt: new Date(),
    CreatedBy: 1,
    UpdatedAt: null,
    UpdatedBy: null,
    IsDeleted: false,
    DeletedAt: null,
    DeletedBy: null
  },
  {
    _id: NumberLong("2002"),
    PropertyId: 1,
    RoomId: 2002,
    roomNumber: "202",
    RoomCode: "202",
    RoomName: "Deluxe Room 202",
    roomType: "Deluxe",
    floor: 2,
    capacity: 2,
    status: "Available",
    amenities: ["WiFi", "TV", "Air Conditioning", "Mini Bar"],
    pricePerNight: 120.00,
    Active: true,
    isActive: true,
    CompanyLogoURL: "",
    CreatedAt: new Date(),
    CreatedBy: 1,
    UpdatedAt: null,
    UpdatedBy: null,
    IsDeleted: false,
    DeletedAt: null,
    DeletedBy: null
  },
  {
    _id: NumberLong("3001"),
    PropertyId: 1,
    RoomId: 3001,
    roomNumber: "301",
    RoomCode: "301",
    RoomName: "Presidential Suite",
    roomType: "Presidential Suite",
    floor: 3,
    capacity: 6,
    status: "Available",
    amenities: ["WiFi", "TV", "Air Conditioning", "Mini Bar", "Balcony", "Kitchen", "Jacuzzi", "Ocean View"],
    pricePerNight: 500.00,
    Active: true,
    isActive: true,
    CompanyLogoURL: "",
    CreatedAt: new Date(),
    CreatedBy: 1,
    UpdatedAt: null,
    UpdatedBy: null,
    IsDeleted: false,
    DeletedAt: null,
    DeletedBy: null
  },
  {
    _id: NumberLong("3002"),
    PropertyId: 1,
    RoomId: 3002,
    roomNumber: "302",
    RoomCode: "302",
    RoomName: "Deluxe Room 302",
    roomType: "Deluxe",
    floor: 3,
    capacity: 2,
    status: "Maintenance",
    amenities: ["WiFi", "TV", "Air Conditioning", "Mini Bar"],
    pricePerNight: 120.00,
    Active: true,
    isActive: true,
    CompanyLogoURL: "",
    CreatedAt: new Date(),
    CreatedBy: 1,
    UpdatedAt: null,
    UpdatedBy: null,
    IsDeleted: false,
    DeletedAt: null,
    DeletedBy: null
  }
]);

print("✅ Inserted 7 sample rooms");

print("📅 Inserting sample bookings...");

// Get current date and create bookings for the current month
const today = new Date();
const currentYear = today.getFullYear();
const currentMonth = today.getMonth();

// Helper function to create date
function createDate(day) {
  return new Date(currentYear, currentMonth, day);
}

// Clear existing test bookings (optional)
// db.Bookings.deleteMany({ bookingId: { $in: ["TEST001", "TEST002", "TEST003", "TEST004", "TEST005"] } });

db.Bookings.insertMany([
  // Booking 1: Room 101 - Occupied (check-in 5 days ago, checkout in 2 days)
  {
    _id: NumberLong("4000000000001001"),
    bookingId: "TEST001",
    guestId: NumberLong("1000000000003392"),
    staffId: NumberLong("2000000000000056"),
    roomNumber: "101",
    propertyId: 1,
    bookingDate: new Date(currentYear, currentMonth, today.getDate() - 10),
    checkInDate: new Date(currentYear, currentMonth, today.getDate() - 5),
    checkOutDate: new Date(currentYear, currentMonth, today.getDate() + 2),
    numberOfGuests: 2,
    totalPrice: 840.00, // 7 nights * $120
    paymentStatusId: NumberLong("5000000000000012"),
    bookingSourceId: NumberLong("5000000000000003"),
    specialRequests: ["Late checkout"],
    isConfirmed: true,
    Status: "Active",
    lastModified: new Date()
  },
  // Booking 2: Room 102 - Check-in today
  {
    _id: NumberLong("4000000000001002"),
    bookingId: "TEST002",
    guestId: NumberLong("1000000000003393"),
    staffId: NumberLong("2000000000000056"),
    roomNumber: "102",
    propertyId: 1,
    bookingDate: new Date(currentYear, currentMonth, today.getDate() - 7),
    checkInDate: new Date(currentYear, currentMonth, today.getDate()),
    checkOutDate: new Date(currentYear, currentMonth, today.getDate() + 3),
    numberOfGuests: 2,
    totalPrice: 240.00, // 3 nights * $80
    paymentStatusId: NumberLong("5000000000000012"),
    bookingSourceId: NumberLong("5000000000000003"),
    specialRequests: ["Early check-in"],
    isConfirmed: true,
    Status: "Active",
    lastModified: new Date()
  },
  // Booking 3: Room 103 - Check-out today
  {
    _id: NumberLong("4000000000001003"),
    bookingId: "TEST003",
    guestId: NumberLong("1000000000003394"),
    staffId: NumberLong("2000000000000056"),
    roomNumber: "103",
    propertyId: 1,
    bookingDate: new Date(currentYear, currentMonth, today.getDate() - 14),
    checkInDate: new Date(currentYear, currentMonth, today.getDate() - 7),
    checkOutDate: new Date(currentYear, currentMonth, today.getDate()),
    numberOfGuests: 1,
    totalPrice: 560.00, // 7 nights * $80
    paymentStatusId: NumberLong("5000000000000012"),
    bookingSourceId: NumberLong("5000000000000003"),
    specialRequests: [],
    isConfirmed: true,
    Status: "Active",
    lastModified: new Date()
  },
  // Booking 4: Room 201 - Future booking (check-in in 5 days)
  {
    _id: NumberLong("4000000000001004"),
    bookingId: "TEST004",
    guestId: NumberLong("1000000000003395"),
    staffId: NumberLong("2000000000000056"),
    roomNumber: "201",
    propertyId: 1,
    bookingDate: new Date(currentYear, currentMonth, today.getDate() - 2),
    checkInDate: new Date(currentYear, currentMonth, today.getDate() + 5),
    checkOutDate: new Date(currentYear, currentMonth, today.getDate() + 12),
    numberOfGuests: 4,
    totalPrice: 1750.00, // 7 nights * $250
    paymentStatusId: NumberLong("5000000000000012"),
    bookingSourceId: NumberLong("5000000000000003"),
    specialRequests: ["Crib needed"],
    isConfirmed: true,
    Status: "Active",
    lastModified: new Date()
  },
  // Booking 5: Room 301 - Multi-week booking (started 10 days ago, ends in 18 days)
  {
    _id: NumberLong("4000000000001005"),
    bookingId: "TEST005",
    guestId: NumberLong("1000000000003396"),
    staffId: NumberLong("2000000000000056"),
    roomNumber: "301",
    propertyId: 1,
    bookingDate: new Date(currentYear, currentMonth, today.getDate() - 15),
    checkInDate: new Date(currentYear, currentMonth, today.getDate() - 10),
    checkOutDate: new Date(currentYear, currentMonth, today.getDate() + 18),
    numberOfGuests: 6,
    totalPrice: 14000.00, // 28 nights * $500
    paymentStatusId: NumberLong("5000000000000012"),
    bookingSourceId: NumberLong("5000000000000003"),
    specialRequests: ["VIP treatment", "Champagne on arrival"],
    isConfirmed: true,
    Status: "Active",
    lastModified: new Date()
  },
  // Booking 6: Room 202 - Future booking next week
  {
    _id: NumberLong("4000000000001006"),
    bookingId: "TEST006",
    guestId: NumberLong("1000000000003397"),
    staffId: NumberLong("2000000000000056"),
    roomNumber: "202",
    propertyId: 1,
    bookingDate: new Date(),
    checkInDate: new Date(currentYear, currentMonth, today.getDate() + 10),
    checkOutDate: new Date(currentYear, currentMonth, today.getDate() + 14),
    numberOfGuests: 2,
    totalPrice: 480.00, // 4 nights * $120
    paymentStatusId: NumberLong("5000000000000012"),
    bookingSourceId: NumberLong("5000000000000003"),
    specialRequests: [],
    isConfirmed: true,
    Status: "Active",
    lastModified: new Date()
  }
]);

print("✅ Inserted 6 sample bookings");
print("🎉 Sample data insertion complete!");
print("");
print("📊 Summary:");
print("  - 7 Rooms (Property ID: 1)");
print("    • 3 Standard rooms");
print("    • 3 Deluxe rooms");
print("    • 1 Suite");
print("    • 1 Presidential Suite");
print("    • 1 Room in maintenance");
print("");
print("  - 6 Bookings spanning current month");
print("    • 1 Occupied (mid-stay)");
print("    • 1 Check-in today");
print("    • 1 Check-out today");
print("    • 3 Future bookings");
print("");
print("🏨 Room Planner should now display data!");
print("💡 Tip: If propertyId filter is active, make sure Property 1 is selected");
