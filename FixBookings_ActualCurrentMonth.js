// ========================================
// FIX: Create Bookings for ACTUAL Current Month (2024/2025)
// ========================================


print("========================================");
print("CREATING BOOKINGS FOR ACTUAL CURRENT MONTH");
print("========================================");

// Force current year and month (update these if needed)
const today = new Date();
const currentYear = 2024; // <<< CHANGE THIS if you're in 2025
const currentMonth = 2;   // <<< March = 2 (0-indexed), April = 3, etc.

print(`Today's date: ${today.toLocaleDateString()}`);
print(`Creating bookings for: ${new Date(currentYear, currentMonth, 1).toLocaleDateString()}`);

// Delete existing test bookings first
print("\nDeleting old CURRENT_MONTH_ bookings...");
const deleteResult = db.Bookings.deleteMany({ bookingId: /^CURRENT_MONTH_/ });
print(`Deleted ${deleteResult.deletedCount} old bookings`);

// Insert new bookings with CORRECT dates
print("\nInserting new bookings...");
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

print(`✅ SUCCESS! Inserted ${Object.keys(result.insertedIds).length} bookings`);
print("\nBookings created:");

db.Bookings.find({ bookingId: /^CURRENT_MONTH_/ }).forEach((booking) => {
    print(`- ${booking.bookingId}: Room ${booking.roomNumber}, ${booking.checkInDate.toLocaleDateString()} - ${booking.checkOutDate.toLocaleDateString()}`);
});

// Verify dates
print("\n========================================");
print("VERIFICATION:");
print("========================================");
const monthStart = new Date(currentYear, currentMonth, 1);
const monthEnd = new Date(currentYear, currentMonth + 1, 0);

print(`Looking for bookings between ${monthStart.toLocaleDateString()} and ${monthEnd.toLocaleDateString()}`);

const foundBookings = db.Bookings.find({
    $or: [
        {
            checkInDate: { $gte: monthStart, $lte: monthEnd }
        },
        {
            checkOutDate: { $gte: monthStart, $lte: monthEnd }
        }
    ],
    propertyId: 1
}).count();

print(`Found ${foundBookings} bookings in this date range`);

print("\n========================================");
print("NEXT STEPS:");
print("========================================");
print("1. Make sure Room Planner is showing March 2024");
print("2. Refresh your browser (Ctrl+F5)");
print("3. Check browser console (F12) for logs");
print("4. You should now see booking bars!");
