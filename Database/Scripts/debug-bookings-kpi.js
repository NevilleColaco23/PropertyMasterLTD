// Debug Script for Bookings Today KPI
// Run this in MongoDB Compass to diagnose why KPI shows 0

use ListingDB

console.log('🔍 Debugging "Bookings Today" KPI - checking database...\n');

// Check collection names (case-sensitive!)
console.log('📚 Available collections:');
db.getCollectionNames().forEach(name => console.log('  - ' + name));

console.log('\n');

// Check lowercase "bookings" collection
const bookingsLower = db.bookings.countDocuments();
console.log(`📊 Documents in "bookings" (lowercase): ${bookingsLower}`);

// Check uppercase "Bookings" collection
const bookingsUpper = db.Bookings.countDocuments();
console.log(`📊 Documents in "Bookings" (uppercase): ${bookingsUpper}`);

console.log('\n');

// Check for today's bookings with CreatedAt field
const today = new Date();
today.setHours(0, 0, 0, 0);
const tomorrow = new Date(today);
tomorrow.setDate(tomorrow.getDate() + 1);

console.log(`📅 Today's date range: ${today.toISOString()} to ${tomorrow.toISOString()}\n`);

// Check lowercase collection
const todayBookingsLower = db.bookings.countDocuments({
    CreatedAt: { 
        $gte: today,
        $lt: tomorrow
    }
});
console.log(`✅ Today's bookings in "bookings" (lowercase): ${todayBookingsLower}`);

// Check uppercase collection
const todayBookingsUpper = db.Bookings.countDocuments({
    CreatedAt: { 
        $gte: today,
        $lt: tomorrow
    }
});
console.log(`✅ Today's bookings in "Bookings" (uppercase): ${todayBookingsUpper}`);

console.log('\n');

// Check test bookings by ID range
const testBookings = db.bookings.countDocuments({
    _id: { 
        $gte: NumberLong("4100000000000001"), 
        $lte: NumberLong("4100000000000005") 
    }
});
console.log(`🧪 Test bookings found (ID range 4100000000000001-5): ${testBookings}`);

if (testBookings > 0) {
    console.log('\n📄 Sample test booking:');
    const sample = db.bookings.findOne({
        _id: NumberLong("4100000000000001")
    });
    
    if (sample) {
        console.log(JSON.stringify({
            _id: sample._id,
            bookingId: sample.bookingId,
            bookingDate: sample.bookingDate,
            CreatedAt: sample.CreatedAt,
            hasCreatedAt: sample.CreatedAt !== undefined,
            createdAtType: typeof sample.CreatedAt,
            propertyId: sample.propertyId
        }, null, 2));
    }
}

console.log('\n');

// Check all bookings with CreatedAt field
const bookingsWithCreatedAt = db.bookings.countDocuments({ CreatedAt: { $exists: true } });
console.log(`📌 Total bookings with CreatedAt field: ${bookingsWithCreatedAt}`);

// Check all bookings with bookingDate field
const bookingsWithBookingDate = db.bookings.countDocuments({ bookingDate: { $exists: true } });
console.log(`📌 Total bookings with bookingDate field: ${bookingsWithBookingDate}`);

console.log('\n');
console.log('🔧 DIAGNOSIS:');
console.log('==============');

if (bookingsLower === 0 && bookingsUpper === 0) {
    console.log('❌ No bookings collection found! Run seed-bookings-data.js first');
} else if (bookingsLower > 0 && bookingsUpper === 0) {
    console.log('⚠️  ISSUE FOUND: Bookings are in lowercase "bookings" collection');
    console.log('   Backend is looking for uppercase "Bookings" collection!');
    console.log('   Solution: Copy data to "Bookings" collection (see below)');
} else if (todayBookingsLower === 0 && todayBookingsUpper === 0) {
    console.log('⚠️  No bookings found for today');
    console.log('   Solution: Run test-bookings-today.js script');
} else if (bookingsWithCreatedAt === 0) {
    console.log('⚠️  Bookings exist but missing "CreatedAt" field');
    console.log('   Solution: Update existing bookings to add CreatedAt field (see below)');
} else {
    console.log('✅ Everything looks good! Check backend logs for errors');
    console.log('   Backend endpoint: GET /api/v1/dashboard/kpi/bookings-today?userId=YOUR_USER_ID');
}

console.log('\n');
console.log('🔧 SOLUTIONS:');
console.log('==============\n');

if (bookingsLower > 0 && bookingsUpper === 0) {
    console.log('// Copy bookings to uppercase collection:');
    console.log('db.bookings.find().forEach(doc => db.Bookings.insertOne(doc));');
    console.log('');
}

if (bookingsWithCreatedAt === 0 && bookingsWithBookingDate > 0) {
    console.log('// Add CreatedAt field to existing bookings:');
    console.log('db.bookings.updateMany(');
    console.log('    { CreatedAt: { $exists: false } },');
    console.log('    { $set: { CreatedAt: "$bookingDate" } }');
    console.log(');');
    console.log('// Or use bookingDate as CreatedAt:');
    console.log('db.bookings.find({ CreatedAt: { $exists: false } }).forEach(doc => {');
    console.log('    db.bookings.updateOne(');
    console.log('        { _id: doc._id },');
    console.log('        { $set: { CreatedAt: doc.bookingDate } }');
    console.log('    );');
    console.log('});');
}

console.log('\n✨ Run solutions above, then refresh your dashboard!');
