// FINAL FIX: Create Bookings Using UTC Midnight
// Backend queries for: 2026-03-17T00:00:00.000Z to 2026-03-18T00:00:00.000Z
// We must create bookings with CreatedAt in this exact range

use ListingDB

console.log('🎯 FINAL FIX: Creating bookings with correct UTC timestamp...\n');

// Create March 17, 2026 at MIDNIGHT UTC (not local time!)
const utcMidnight = new Date(Date.UTC(2026, 2, 17, 0, 0, 0, 0)); // Month is 0-indexed: 2 = March
console.log(`✅ Target date (UTC midnight): ${utcMidnight.toISOString()}`);
console.log(`   This matches backend query: >= 2026-03-17T00:00:00.000Z\n`);

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

// First, clean up any old test bookings that have wrong timestamps
console.log('🧹 Cleaning up old test bookings with incorrect timestamps...');
const deleted = db.Bookings.deleteMany({
    $or: [
        { _id: { $gte: NumberLong("4100000000000001"), $lte: NumberLong("4100000000000005") } },
        { _id: { $gte: NumberLong("4200000000000001"), $lte: NumberLong("4200000000000010") } },
        { _id: { $gte: NumberLong("4300000000000001"), $lte: NumberLong("4300000000000015") } }
    ]
});
console.log(`   Deleted ${deleted.deletedCount} old test bookings\n`);

// Create 25 bookings with CORRECT UTC timestamp
console.log('💡 Creating 25 bookings with UTC midnight timestamp...\n');

const checkInDate = new Date(Date.UTC(2026, 2, 20)); // March 20, 2026 UTC
const checkOutDate = new Date(Date.UTC(2026, 2, 23)); // March 23, 2026 UTC

const correctBookings = [];
let bookingIdStart = 4500000000000001; // New clean ID range

for (let i = 0; i < 25; i++) {
    const booking = {
        _id: NumberLong(bookingIdStart.toString()),
        bookingId: generateBookingId(),
        guestId: NumberLong((1000000000003900 + i).toString()),
        staffId: NumberLong((2000000000000100 + i).toString()),
        roomNumber: generateRoomNumber(),
        bookingDate: utcMidnight,
        CreatedAt: utcMidnight,  // ← THIS IS THE KEY! Must be UTC midnight!
        checkInDate: checkInDate,
        checkOutDate: checkOutDate,
        numberOfGuests: Math.floor(Math.random() * 4) + 1,
        totalPrice: parseFloat((Math.random() * 1000 + 200).toFixed(2)),
        paymentStatusId: NumberLong("5000000000000011"),
        bookingSourceId: NumberLong("5000000000000001"),
        specialRequests: ['UTC midnight fix'],
        isConfirmed: true,
        lastModified: utcMidnight,
        propertyId: -1,
        Status: "Active"
    };
    
    correctBookings.push(booking);
    bookingIdStart++;
}

// Insert bookings
const result = db.Bookings.insertMany(correctBookings);
console.log(`✅ Inserted ${Object.keys(result.insertedIds).length} bookings!\n`);

// Verify with exact backend query
const backendToday = new Date(Date.UTC(2026, 2, 17, 0, 0, 0, 0));
const backendTomorrow = new Date(Date.UTC(2026, 2, 18, 0, 0, 0, 0));

const count = db.Bookings.countDocuments({
    CreatedAt: { 
        $gte: backendToday,
        $lt: backendTomorrow
    }
});

console.log('📊 VERIFICATION (using backend\'s exact query):');
console.log(`   CreatedAt >= ${backendToday.toISOString()}`);
console.log(`   CreatedAt <  ${backendTomorrow.toISOString()}`);
console.log(`   Result: ${count} bookings found\n`);

if (count >= 25) {
    console.log('✅ SUCCESS! Bookings are in the correct date range!');
    console.log('   Backend should now return value: ' + count);
} else {
    console.log('⚠️  WARNING: Only found ' + count + ' bookings');
    console.log('   Expected at least 25');
}

// Show sample booking to verify timestamp
const sample = db.Bookings.findOne({ _id: NumberLong("4500000000000001") });
if (sample) {
    console.log('\n📄 Sample booking (verify CreatedAt):');
    console.log(JSON.stringify({
        _id: sample._id,
        bookingId: sample.bookingId,
        CreatedAt: sample.CreatedAt,
        createdAtISO: sample.CreatedAt.toISOString ? sample.CreatedAt.toISOString() : 'N/A',
        roomNumber: sample.roomNumber
    }, null, 2));
}

console.log('\n🎯 FINAL STEPS:');
console.log('===============');
console.log('1. Hard refresh your dashboard (Ctrl+Shift+R)');
console.log('2. Wait 5 seconds for backend to process');
console.log('3. Check the "Bookings Today" KPI');
console.log('4. Expected value: 25 (or more if you ran this multiple times)');
console.log('');
console.log('🔍 If still 0, run this in browser console:');
console.log('   fetch("/api/v1/dashboard/kpi/bookings-today?userId=1")');
console.log('     .then(r => r.json()).then(console.log)');
console.log('');
console.log('✅ You should see: { "value": 25, ... }');
