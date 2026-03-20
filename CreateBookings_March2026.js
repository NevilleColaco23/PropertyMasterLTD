// ========================================
// FIX: Create Bookings for March 2026
// ========================================


print("========================================");
print("CREATING BOOKINGS FOR MARCH 2026");
print("========================================");

// Delete existing test bookings
print("\nDeleting old CURRENT_MONTH_ bookings...");
const deleteResult = db.Bookings.deleteMany({ bookingId: /^CURRENT_MONTH_/ });
print(`Deleted ${deleteResult.deletedCount} old bookings`);

// Create bookings for March 2026
const year = 2026;
const month = 2; // March (0-indexed, so 2 = March)

print(`\nInserting bookings for March 2026...`);

// Helper function to generate realistic booking IDs like "V4IPXC5TRJ"
function generateBookingId() {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
    let result = '';
    for (let i = 0; i < 10; i++) {
        result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return result;
}

const result = db.Bookings.insertMany([
    {
        bookingId: generateBookingId(),  // Realistic alphanumeric booking ID
        roomNumber: "101",
        propertyId: 1,
        checkInDate: new Date(year, month, 5),   // March 5, 2026
        checkOutDate: new Date(year, month, 8),   // March 8, 2026
        status: "confirmed",
        guestId: 1000000000003039,  // Use numeric guestId like real data
        numberOfGuests: 2,
        totalPrice: 450.00,
        bookingDate: new Date(),
        isConfirmed: true
    },
    {
        bookingId: generateBookingId(),  // Realistic alphanumeric booking ID
        roomNumber: "102",
        propertyId: 1,
        checkInDate: new Date(year, month, 10),  // March 10, 2026
        checkOutDate: new Date(year, month, 15),  // March 15, 2026
        status: "confirmed",
        guestId: 1000000000003039,  // Use numeric guestId like real data
        numberOfGuests: 1,
        totalPrice: 750.00,
        bookingDate: new Date(),
        isConfirmed: true
    },
    {
        bookingId: generateBookingId(),  // Realistic alphanumeric booking ID
        roomNumber: "103",
        propertyId: 1,
        checkInDate: new Date(year, month, 12),  // March 12, 2026
        checkOutDate: new Date(year, month, 18),  // March 18, 2026
        status: "confirmed",
        guestId: 1000000000003039,  // Use numeric guestId like real data
        numberOfGuests: 3,
        totalPrice: 900.00,
        bookingDate: new Date(),
        isConfirmed: true
    },
    {
        bookingId: generateBookingId(),  // Realistic alphanumeric booking ID
        roomNumber: "201",
        propertyId: 1,
        checkInDate: new Date(year, month, 15),  // March 15, 2026
        checkOutDate: new Date(year, month, 20),  // March 20, 2026
        status: "confirmed",
        guestId: 1000000000003039,  // Use numeric guestId like real data
        numberOfGuests: 2,
        totalPrice: 600.00,
        bookingDate: new Date(),
        isConfirmed: true
    },
    {
        bookingId: generateBookingId(),  // Realistic alphanumeric booking ID
        roomNumber: "202",
        propertyId: 1,
        checkInDate: new Date(year, month, 1),   // March 1, 2026
        checkOutDate: new Date(year, month, 5),   // March 5, 2026
        status: "confirmed",
        guestId: 1000000000003039,  // Use numeric guestId like real data
        numberOfGuests: 2,
        totalPrice: 480.00,
        bookingDate: new Date(),
        isConfirmed: true
    },
    {
        bookingId: generateBookingId(),  // Realistic alphanumeric booking ID
        roomNumber: "301",
        propertyId: 1,
        checkInDate: new Date(year, month, 20),  // March 20, 2026
        checkOutDate: new Date(year, month, 25),  // March 25, 2026
        status: "confirmed",
        guestId: 1000000000003039,  // Use numeric guestId like real data
        numberOfGuests: 4,
        totalPrice: 1000.00,
        bookingDate: new Date(),
        isConfirmed: true
    }
]);

print(`✅ SUCCESS! Inserted ${Object.keys(result.insertedIds).length} bookings`);
print("\nBookings created:");

db.Bookings.find({ bookingId: /^CURRENT_MONTH_/ }).forEach((booking) => {
    print(`- ${booking.bookingId}: Room ${booking.roomNumber}`);
    print(`  Check-in:  ${booking.checkInDate.toISOString()}`);
    print(`  Check-out: ${booking.checkOutDate.toISOString()}`);
});

// Verify the query will find them
print("\n========================================");
print("VERIFICATION:");
print("========================================");

const start = new Date(2026, 2, 1);
const end = new Date(2026, 2, 31);

print(`Query date range: ${start.toISOString()} to ${end.toISOString()}`);

const foundBookings = db.Bookings.find({
    $and: [
        {
            $or: [
                {
                    $and: [
                        { checkInDate: { $lte: end } },
                        { checkOutDate: { $gte: start } }
                    ]
                },
                {
                    $and: [
                        { CheckInDate: { $lte: end } },
                        { CheckOutDate: { $gte: start } }
                    ]
                }
            ]
        },
        {
            $or: [
                { propertyId: 1 },
                { PropertyId: 1 }
            ]
        }
    ]
}).count();

print(`Found ${foundBookings} bookings that match the query`);

if (foundBookings === 0) {
    print("\n❌ ERROR: Query found 0 bookings!");
    print("This means there's a field name mismatch or data issue.");
} else {
    print(`\n✅ SUCCESS! Query will return ${foundBookings} bookings`);
    print("\nNext steps:");
    print("1. Refresh your browser (Ctrl+F5)");
    print("2. Navigate to Room Planner tab");
    print("3. You should see booking bars in the grid!");
}
