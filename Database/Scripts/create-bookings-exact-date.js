// Fix: Create Bookings for Backend's EXACT Date
// Backend shows: 2026-03-17T20:02:03Z
// We need bookings for March 17, 2026


console.log('🎯 Creating bookings for EXACT backend date...\n');

// Backend's exact date from API response: 2026-03-17
const backendToday = new Date('2026-03-17');
backendToday.setHours(0, 0, 0, 0);

console.log(`📅 Backend "today": ${backendToday.toISOString()}`);
console.log(`   Date: ${backendToday.toLocaleDateString()}`);
console.log('');

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

// Check if we already have bookings for this date
const existing = db.Bookings.countDocuments({
    CreatedAt: { 
        $gte: backendToday,
        $lt: new Date(backendToday.getTime() + 24 * 60 * 60 * 1000)
    }
});

console.log(`📊 Existing bookings for ${backendToday.toLocaleDateString()}: ${existing}`);
console.log('');

if (existing > 0) {
    console.log('✅ Bookings already exist for this date!');
    console.log('   Try refreshing your dashboard again.');
    console.log('   If still 0, check:');
    console.log('   1. Browser cache (Ctrl+Shift+R)');
    console.log('   2. Backend logs for errors');
    console.log('   3. API endpoint directly in browser');
} else {
    console.log('💡 Creating 15 bookings for March 17, 2026...\n');

    // Create check-in date (3 days from booking)
    const checkInDate = new Date(backendToday);
    checkInDate.setDate(checkInDate.getDate() + 3);

    // Create check-out date (3 days after check-in)
    const checkOutDate = new Date(checkInDate);
    checkOutDate.setDate(checkOutDate.getDate() + 3);

    const exactDateBookings = [];
    let bookingIdStart = 4300000000000001; // New ID range

    for (let i = 0; i < 15; i++) {
        const booking = {
            _id: NumberLong(bookingIdStart.toString()),
            bookingId: generateBookingId(),
            guestId: NumberLong((1000000000003700 + i).toString()),
            staffId: NumberLong((2000000000000080 + i).toString()),
            roomNumber: generateRoomNumber(),
            bookingDate: backendToday,
            CreatedAt: backendToday,  // CRITICAL: Must match backend's today exactly!
            checkInDate: checkInDate,
            checkOutDate: checkOutDate,
            numberOfGuests: Math.floor(Math.random() * 4) + 1,
            totalPrice: parseFloat((Math.random() * 1000 + 200).toFixed(2)),
            paymentStatusId: NumberLong("5000000000000011"),
            bookingSourceId: NumberLong("5000000000000001"),
            specialRequests: ['Backend exact date test'],
            isConfirmed: true,
            lastModified: backendToday,
            propertyId: -1,
            Status: "Active"
        };
        
        exactDateBookings.push(booking);
        bookingIdStart++;
    }

    // Insert into Bookings collection
    db.Bookings.insertMany(exactDateBookings);

    console.log(`✅ Successfully inserted ${exactDateBookings.length} bookings!`);
    console.log(`   Booking date: ${backendToday.toISOString()}`);
    console.log(`   Check-in: ${checkInDate.toISOString()}`);
    console.log(`   Check-out: ${checkOutDate.toISOString()}`);
    console.log('');

    // Verify insertion
    const verifyCount = db.Bookings.countDocuments({
        CreatedAt: { 
            $gte: backendToday,
            $lt: new Date(backendToday.getTime() + 24 * 60 * 60 * 1000)
        }
    });

    console.log(`📊 Total bookings for ${backendToday.toLocaleDateString()}: ${verifyCount}`);
    console.log('');

    // Show sample booking
    const sample = db.Bookings.findOne({
        _id: NumberLong("4300000000000001")
    });

    if (sample) {
        console.log('📄 Sample booking:');
        console.log(JSON.stringify({
            bookingId: sample.bookingId,
            bookingDate: sample.bookingDate,
            CreatedAt: sample.CreatedAt,
            roomNumber: sample.roomNumber,
            totalPrice: sample.totalPrice
        }, null, 2));
        console.log('');
    }
}

console.log('🎯 NEXT STEPS:');
console.log('==============');
console.log('1. Refresh your dashboard (Ctrl+Shift+R for hard refresh)');
console.log('2. KPI should now show: 15 bookings');
console.log('3. If still 0, open DevTools → Console and run:');
console.log('   fetch("/api/v1/dashboard/kpi/bookings-today?userId=1")');
console.log('   .then(r => r.json()).then(d => console.log(d))');
console.log('');
console.log('✅ Expected result: "value": 15');
console.log('');
console.log('🧹 To clean up test bookings later:');
console.log('db.Bookings.deleteMany({ _id: { $gte: NumberLong("4300000000000001"), $lte: NumberLong("4300000000000015") } })');
