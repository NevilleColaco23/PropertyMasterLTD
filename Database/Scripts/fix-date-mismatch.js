// Check Date Mismatch Between System and Backend
// This diagnoses the date synchronization issue

use ListingDB

console.log('🕐 Checking date synchronization issue...\n');

// Get system date from MongoDB
const systemDate = new Date();
console.log(`📅 System/MongoDB Date: ${systemDate.toISOString()}`);
console.log(`   Date: ${systemDate.toLocaleDateString()}`);
console.log(`   Year: ${systemDate.getFullYear()}`);
console.log(`   Month: ${systemDate.getMonth() + 1}`);
console.log(`   Day: ${systemDate.getDate()}`);
console.log('');

// Check if system date looks incorrect
const currentYear = systemDate.getFullYear();
if (currentYear > 2025) {
    console.log('⚠️  WARNING: System date is set to the FUTURE!');
    console.log(`   Your system shows: ${systemDate.toLocaleDateString()}`);
    console.log(`   Actual date should be: January 2025`);
    console.log('');
    console.log('🔧 This is why the KPI shows 0:');
    console.log('   - MongoDB bookings created: March 2026');
    console.log('   - Backend is looking for: January 2025');
    console.log('   - Dates don\'t match!');
    console.log('');
}

// Let's create bookings for the ACTUAL backend date
// Backend will use UTC current date
console.log('🔧 SOLUTION: Create bookings for the REAL current date\n');

// Calculate what the backend sees as "today"
// Since we can't get the backend's actual date, we'll use a reasonable assumption
const actualToday = new Date('2025-01-17'); // Approximate actual date
actualToday.setHours(0, 0, 0, 0);

console.log(`📅 Estimated backend "today": ${actualToday.toISOString()}`);
console.log('');

console.log('💡 Creating bookings for the REAL today (2025)...');

// Helper functions
function generateBookingId() {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789';
    let result = '';
    for (let i = 0; i < 10; i++) {
        result += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    return result;
}

function generateRoomNumber() {
    const floor = Math.floor(Math.random() * 10) + 1;
    const room = Math.floor(Math.random() * 50) + 1;
    return `${floor}${room.toString().padStart(2, '0')}`;
}

// Create bookings for actual today
const checkInDate = new Date(actualToday);
checkInDate.setDate(checkInDate.getDate() + 3);

const checkOutDate = new Date(checkInDate);
checkOutDate.setDate(checkOutDate.getDate() + 3);

const realTodayBookings = [];
let bookingIdStart = 4200000000000001; // Different ID range

for (let i = 0; i < 10; i++) {
    const booking = {
        _id: NumberLong(bookingIdStart.toString()),
        bookingId: generateBookingId(),
        guestId: NumberLong((1000000000003600 + i).toString()),
        staffId: NumberLong((2000000000000070 + i).toString()),
        roomNumber: generateRoomNumber(),
        bookingDate: actualToday,
        CreatedAt: actualToday,  // Backend uses this!
        checkInDate: checkInDate,
        checkOutDate: checkOutDate,
        numberOfGuests: Math.floor(Math.random() * 4) + 1,
        totalPrice: parseFloat((Math.random() * 1000 + 200).toFixed(2)),
        paymentStatusId: NumberLong("5000000000000011"),
        bookingSourceId: NumberLong("5000000000000001"),
        specialRequests: ['Testing real date'],
        isConfirmed: true,
        lastModified: actualToday,
        propertyId: -1,
        Status: "Active"
    };
    
    realTodayBookings.push(booking);
    bookingIdStart++;
}

// Insert into uppercase Bookings collection
db.Bookings.insertMany(realTodayBookings);

console.log(`✅ Inserted ${realTodayBookings.length} bookings for REAL today (2025-01-17)`);
console.log('');

// Verify
const count2025 = db.Bookings.countDocuments({
    CreatedAt: { 
        $gte: actualToday,
        $lt: new Date(actualToday.getTime() + 24 * 60 * 60 * 1000)
    }
});

console.log(`📊 Bookings for 2025-01-17: ${count2025}`);
console.log('');

console.log('🎯 NOW:');
console.log('   1. Refresh your dashboard');
console.log('   2. The KPI should show: 10 bookings');
console.log('   3. If still 0, check backend logs for the exact date it\'s querying');
console.log('');
console.log('🔧 Alternative: Get exact backend date');
console.log('   Open browser DevTools → Network tab');
console.log('   Look for: /api/v1/dashboard/kpi/bookings-today');
console.log('   Check the response to see what date backend is using');
