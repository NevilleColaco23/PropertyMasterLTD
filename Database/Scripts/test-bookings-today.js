// MongoDB Script to Insert Test Bookings for Today
// This will test the "Bookings Today" KPI widget
// Run this in MongoDB Compass or mongosh

use ListingDB

// Helper function to generate random booking ID
function generateBookingId() {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
    let result = '';
    for (let i = 0; i < 10; i++) {
        result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return result;
}

// Helper function to generate random room number
function generateRoomNumber() {
    const floor = Math.floor(Math.random() * 10) + 1; // Floors 1-10
    const room = Math.floor(Math.random() * 50) + 1; // Rooms 1-50
    return `${floor}${room.toString().padStart(2, '0')}`;
}

console.log('Creating test bookings for TODAY to test KPI widget...');

const now = new Date();
const today = new Date(now.getFullYear(), now.getMonth(), now.getDate());

// Create a check-in date 3 days from now
const checkInDate = new Date(today);
checkInDate.setDate(checkInDate.getDate() + 3);

// Check-out date 2-5 days after check-in
const checkOutDate = new Date(checkInDate);
checkOutDate.setDate(checkOutDate.getDate() + (Math.floor(Math.random() * 4) + 2));

const testBookingsForToday = [];
let bookingIdStart = 4100000000000001; // Different range from seed data

// Create 5 test bookings for TODAY
for (let i = 0; i < 5; i++) {
    const booking = {
        _id: NumberLong(bookingIdStart.toString()),
        bookingId: generateBookingId(),
        guestId: NumberLong((1000000000003500 + i).toString()),
        staffId: NumberLong((2000000000000060 + i).toString()),
        roomNumber: generateRoomNumber(),
        bookingDate: today,           // TODAY - when booking was made
        CreatedAt: today,              // Add CreatedAt field for backend KPI calculation
        checkInDate: checkInDate,     // Future check-in
        checkOutDate: checkOutDate,   // Future check-out
        numberOfGuests: Math.floor(Math.random() * 4) + 1,
        totalPrice: parseFloat((Math.random() * 1000 + 200).toFixed(2)),
        paymentStatusId: NumberLong("5000000000000011"), // Paid
        bookingSourceId: NumberLong("5000000000000001"), // Website
        specialRequests: ['Testing KPI widget'],
        isConfirmed: true,
        lastModified: today,
        propertyId: -1,
        Status: "Active"  // For occupancy calculations
    };
    
    testBookingsForToday.push(booking);
    bookingIdStart++;
}

// Insert the test bookings
db.bookings.insertMany(testBookingsForToday);

console.log(`✅ Successfully inserted ${testBookingsForToday.length} test bookings for TODAY!`);
console.log(`   - Booking date: ${today.toISOString()}`);
console.log(`   - Check-in date: ${checkInDate.toISOString()}`);
console.log(`   - Check-out date: ${checkOutDate.toISOString()}`);
console.log(`   - Property ID: -1`);

// Verify the data
const todayBookingsCount = db.bookings.countDocuments({
    CreatedAt: { 
        $gte: today,
        $lt: new Date(today.getTime() + 24 * 60 * 60 * 1000) 
    }
});

console.log(`\n📊 Total bookings created today: ${todayBookingsCount}`);

// Show sample booking
console.log('\n📄 Sample booking:');
const sampleBooking = db.bookings.findOne({ 
    _id: { 
        $gte: NumberLong("4100000000000001"), 
        $lte: NumberLong("4100000000000005") 
    } 
});

if (sampleBooking) {
    console.log(JSON.stringify({
        bookingId: sampleBooking.bookingId,
        bookingDate: sampleBooking.bookingDate,
        CreatedAt: sampleBooking.CreatedAt,
        checkInDate: sampleBooking.checkInDate,
        checkOutDate: sampleBooking.checkOutDate,
        roomNumber: sampleBooking.roomNumber,
        totalPrice: sampleBooking.totalPrice,
        propertyId: sampleBooking.propertyId
    }, null, 2));
}

console.log('\n✨ Now refresh your dashboard to see the "Bookings Today" KPI update!');
console.log('   Expected value: 5 bookings');

// Cleanup command (run this later if you want to remove test data)
console.log('\n🧹 To clean up test data later, run:');
console.log('db.bookings.deleteMany({ _id: { $gte: NumberLong("4100000000000001"), $lte: NumberLong("4100000000000005") } })');
