// ========================================
// QUICK INSERT: Create Bookings for Current Month
// ========================================
// This script ACTUALLY inserts bookings (not just prints them)


print("========================================");
print("INSERTING BOOKINGS FOR CURRENT MONTH");
print("========================================");

const now = new Date();
const currentYear = now.getFullYear();
const currentMonth = now.getMonth();

print(`Creating bookings for ${new Date(currentYear, currentMonth, 1).toLocaleDateString()}`);

// First, check if we already have bookings for this month
const existingBookings = db.Bookings.countDocuments({
    bookingId: /^CURRENT_MONTH_/
});

if (existingBookings > 0) {
    print(`⚠️ Found ${existingBookings} existing CURRENT_MONTH_ bookings`);
    print("Do you want to delete them first? If yes, run:");
    print('db.Bookings.deleteMany({ bookingId: /^CURRENT_MONTH_/ });');
    print("\nThen run this script again.");
} else {
    // Insert bookings
    const result = db.Bookings.insertMany([
        {
            bookingId: "CURRENT_MONTH_001",
            roomNumber: "101",
            propertyId: 1,
            checkInDate: new Date(currentYear, currentMonth, 5),
            checkOutDate: new Date(currentYear, currentMonth, 8),
            status: "confirmed",
            guestId: "G001"
        },
        {
            bookingId: "CURRENT_MONTH_002",
            roomNumber: "102",
            propertyId: 1,
            checkInDate: new Date(currentYear, currentMonth, 10),
            checkOutDate: new Date(currentYear, currentMonth, 15),
            status: "confirmed",
            guestId: "G002"
        },
        {
            bookingId: "CURRENT_MONTH_003",
            roomNumber: "103",
            propertyId: 1,
            checkInDate: new Date(currentYear, currentMonth, 12),
            checkOutDate: new Date(currentYear, currentMonth, 18),
            status: "confirmed",
            guestId: "G003"
        },
        {
            bookingId: "CURRENT_MONTH_004",
            roomNumber: "201",
            propertyId: 1,
            checkInDate: new Date(currentYear, currentMonth, 15),
            checkOutDate: new Date(currentYear, currentMonth, 20),
            status: "confirmed",
            guestId: "G001"
        },
        {
            bookingId: "CURRENT_MONTH_005",
            roomNumber: "202",
            propertyId: 1,
            checkInDate: new Date(currentYear, currentMonth, 1),
            checkOutDate: new Date(currentYear, currentMonth, 5),
            status: "confirmed",
            guestId: "G002"
        },
        {
            bookingId: "CURRENT_MONTH_006",
            roomNumber: "301",
            propertyId: 1,
            checkInDate: new Date(currentYear, currentMonth, 20),
            checkOutDate: new Date(currentYear, currentMonth, 25),
            status: "confirmed",
            guestId: "G003"
        }
    ]);

    print(`✅ SUCCESS! Inserted ${result.insertedIds.length} bookings`);
    print("\nBookings created:");
    
    db.Bookings.find({ bookingId: /^CURRENT_MONTH_/ }).forEach((booking, index) => {
        print(`${index + 1}. ${booking.bookingId}: Room ${booking.roomNumber}, ${booking.checkInDate.toLocaleDateString()} - ${booking.checkOutDate.toLocaleDateString()}`);
    });
    
    print("\n========================================");
    print("NEXT STEPS:");
    print("========================================");
    print("1. Refresh your browser (F5)");
    print("2. Navigate to Room Planner tab");
    print("3. You should now see booking bars in the grid!");
}
