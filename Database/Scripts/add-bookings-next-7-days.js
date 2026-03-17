// Add Bookings for Next 7 Days (March 18-24, 2026)
// Creates 15 bookings per day for the next week


console.log('🎯 Adding bookings for next 7 days...\n');

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

function getRandomElement(arr) {
    return arr[Math.floor(Math.random() * arr.length)];
}

// Special requests pool
const specialRequestsPool = [
    ['Late check-in requested'],
    ['Early check-out'],
    ['Extra towels needed'],
    ['Quiet room preferred'],
    ['High floor requested'],
    ['Near elevator'],
    ['Away from elevator'],
    ['Ocean view preferred'],
    ['Hypoallergenic bedding'],
    ['Extra pillows'],
    [],
    []
];

// Base date: March 17, 2026 (today)
const baseDate = new Date(Date.UTC(2026, 2, 17, 0, 0, 0, 0));

console.log(`📅 Base date (today): ${baseDate.toISOString()}`);
console.log('   Creating bookings for next 7 days...\n');

let bookingIdStart = 4600000000000001; // New ID range for next 7 days
let guestIdStart = 1000000000004000;
let staffIdStart = 2000000000000200;

const allBookings = [];

// Create bookings for next 7 days (March 18-24, 2026)
for (let dayOffset = 1; dayOffset <= 7; dayOffset++) {
    const bookingDate = new Date(Date.UTC(2026, 2, 17 + dayOffset, 0, 0, 0, 0));
    const bookingsPerDay = Math.floor(Math.random() * 6) + 12; // 12-17 bookings per day
    
    console.log(`📅 Day ${dayOffset}: ${bookingDate.toISOString().split('T')[0]} - Creating ${bookingsPerDay} bookings`);
    
    for (let i = 0; i < bookingsPerDay; i++) {
        // Check-in: 2-7 days after booking
        const checkInOffset = Math.floor(Math.random() * 6) + 2;
        const checkInDate = new Date(Date.UTC(2026, 2, 17 + dayOffset + checkInOffset));
        
        // Check-out: 1-5 days after check-in
        const stayDuration = Math.floor(Math.random() * 5) + 1;
        const checkOutDate = new Date(checkInDate);
        checkOutDate.setUTCDate(checkOutDate.getUTCDate() + stayDuration);
        
        const booking = {
            _id: NumberLong(bookingIdStart.toString()),
            bookingId: generateBookingId(),
            guestId: NumberLong(guestIdStart.toString()),
            staffId: NumberLong(staffIdStart.toString()),
            roomNumber: generateRoomNumber(),
            bookingDate: bookingDate,
            CreatedAt: bookingDate,  // UTC midnight for each day
            checkInDate: checkInDate,
            checkOutDate: checkOutDate,
            numberOfGuests: Math.floor(Math.random() * 4) + 1,
            totalPrice: parseFloat((Math.random() * 1500 + 150).toFixed(2)),
            paymentStatusId: NumberLong("5000000000000011"),
            bookingSourceId: NumberLong(["5000000000000001", "5000000000000002", "5000000000000003"][Math.floor(Math.random() * 3)]),
            specialRequests: getRandomElement(specialRequestsPool),
            isConfirmed: Math.random() > 0.1, // 90% confirmed
            lastModified: bookingDate,
            propertyId: Math.random() > 0.5 ? -1 : 1, // Mix of both properties
            Status: "Active"
        };
        
        allBookings.push(booking);
        bookingIdStart++;
        guestIdStart++;
        staffIdStart++;
    }
}

console.log(`\n💡 Total bookings to insert: ${allBookings.length}`);
console.log('   Inserting into Bookings collection...\n');

// Insert all bookings
const result = db.Bookings.insertMany(allBookings);
console.log(`✅ Successfully inserted ${Object.keys(result.insertedIds).length} bookings!\n`);

// Verify bookings for each day
console.log('📊 VERIFICATION - Bookings per day:');
console.log('=====================================');

for (let dayOffset = 0; dayOffset <= 7; dayOffset++) {
    const dayStart = new Date(Date.UTC(2026, 2, 17 + dayOffset, 0, 0, 0, 0));
    const dayEnd = new Date(Date.UTC(2026, 2, 18 + dayOffset, 0, 0, 0, 0));
    
    const count = db.Bookings.countDocuments({
        CreatedAt: { 
            $gte: dayStart,
            $lt: dayEnd
        }
    });
    
    const dayName = dayStart.toISOString().split('T')[0];
    const label = dayOffset === 0 ? '(TODAY)' : `(+${dayOffset} day${dayOffset > 1 ? 's' : ''})`;
    console.log(`   ${dayName} ${label}: ${count} bookings`);
}

console.log('\n📈 Total bookings in range:');
const totalStart = new Date(Date.UTC(2026, 2, 17, 0, 0, 0, 0));
const totalEnd = new Date(Date.UTC(2026, 2, 25, 0, 0, 0, 0));
const totalCount = db.Bookings.countDocuments({
    CreatedAt: { 
        $gte: totalStart,
        $lt: totalEnd
    }
});
console.log(`   March 17-24, 2026: ${totalCount} bookings total`);

// Show sample bookings from different days
console.log('\n📄 Sample bookings from different days:');
console.log('========================================');

for (let dayOffset of [1, 4, 7]) {
    const sampleDate = new Date(Date.UTC(2026, 2, 17 + dayOffset, 0, 0, 0, 0));
    const nextDate = new Date(Date.UTC(2026, 2, 18 + dayOffset, 0, 0, 0, 0));
    
    const sample = db.Bookings.findOne({
        CreatedAt: { 
            $gte: sampleDate,
            $lt: nextDate
        }
    });
    
    if (sample) {
        console.log(`\n   Day +${dayOffset} (${sampleDate.toISOString().split('T')[0]}):`);
        console.log(`   - Booking ID: ${sample.bookingId}`);
        console.log(`   - Room: ${sample.roomNumber}`);
        console.log(`   - Guests: ${sample.numberOfGuests}`);
        console.log(`   - Price: $${sample.totalPrice}`);
        console.log(`   - Check-in: ${sample.checkInDate.toISOString().split('T')[0]}`);
    }
}

console.log('\n\n🎯 SUMMARY:');
console.log('===========');
console.log(`✅ Successfully added ${allBookings.length} bookings for next 7 days`);
console.log('   Date range: March 18-24, 2026');
console.log('   Booking IDs: 4600000000000001 - ' + (bookingIdStart - 1));
console.log('');
console.log('📊 This data is perfect for:');
console.log('   - Calendar widget (shows booking distribution)');
console.log('   - Chart widget (shows daily trends)');
console.log('   - Testing date range queries');
console.log('');
console.log('🔄 Next steps:');
console.log('   1. Refresh your dashboard');
console.log('   2. Calendar widget should show bookings spread across the week');
console.log('   3. Chart widgets can display daily trends');
console.log('');
console.log('🧹 To clean up these test bookings later:');
console.log('   db.Bookings.deleteMany({ _id: { $gte: NumberLong("4600000000000001"), $lte: NumberLong("' + (bookingIdStart - 1) + '") } })');
