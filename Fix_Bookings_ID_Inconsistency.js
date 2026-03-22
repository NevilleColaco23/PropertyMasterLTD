// ============================================
// Fix Bookings Collection _id Inconsistency
// ============================================
// Problem: Some bookings have Int64 _id, some have ObjectId _id
// Solution: Standardize all bookings to use Int64 auto-increment IDs
// ============================================

use PropertyMaster; // Change to your database name

print("🔍 Analyzing Bookings collection _id types...\n");

// Count bookings by _id type
var objectIdCount = db.Bookings.countDocuments({ _id: { $type: "objectId" } });
var int64Count = db.Bookings.countDocuments({ _id: { $type: "long" } });
var intCount = db.Bookings.countDocuments({ _id: { $type: "int" } });

print(`📊 Current state:`);
print(`   - ObjectId IDs: ${objectIdCount}`);
print(`   - Int64 IDs: ${int64Count}`);
print(`   - Int32 IDs: ${intCount}`);
print(`   - Total: ${db.Bookings.countDocuments({})}\n`);

if (objectIdCount === 0) {
    print("✅ All bookings already have integer IDs. No fix needed!");
} else {
    print(`⚠️  Found ${objectIdCount} bookings with ObjectId that need conversion.\n`);
    print("🔧 Starting conversion process...\n");

    // Get the highest existing integer ID
    var maxId = db.Bookings.find({ _id: { $type: ["long", "int"] } })
        .sort({ _id: -1 })
        .limit(1)
        .toArray();
    
    var nextId = maxId.length > 0 ? NumberLong(maxId[0]._id + 1) : NumberLong(1);
    print(`📍 Starting ID assignment from: ${nextId}\n`);

    // Find all bookings with ObjectId
    var bookingsWithObjectId = db.Bookings.find({ _id: { $type: "objectId" } }).toArray();
    
    var converted = 0;
    var failed = 0;

    bookingsWithObjectId.forEach(function(booking) {
        try {
            var oldId = booking._id;
            var newId = nextId;
            
            // Create new document with integer ID
            var newBooking = Object.assign({}, booking);
            newBooking._id = newId;
            
            // Insert new document
            db.Bookings.insertOne(newBooking);
            
            // Delete old document
            db.Bookings.deleteOne({ _id: oldId });
            
            print(`✅ Converted booking ${booking.bookingId}: ObjectId(${oldId}) → ${newId}`);
            
            nextId = NumberLong(nextId + 1);
            converted++;
        } catch (e) {
            print(`❌ Failed to convert booking ${booking.bookingId}: ${e.message}`);
            failed++;
        }
    });

    print(`\n📊 Conversion complete:`);
    print(`   - Successfully converted: ${converted}`);
    print(`   - Failed: ${failed}`);
    
    // Update KeyCounter to match
    print(`\n🔄 Updating KeyCounter for Bookings...`);
    db.KeyCounter.updateOne(
        { _id: "Bookings" },
        { $set: { seq: nextId - 1 } },
        { upsert: true }
    );
    print(`✅ KeyCounter updated to: ${nextId - 1}`);
}

print("\n🔍 Final verification:");
var finalObjectIdCount = db.Bookings.countDocuments({ _id: { $type: "objectId" } });
var finalInt64Count = db.Bookings.countDocuments({ _id: { $type: "long" } });
print(`   - ObjectId IDs: ${finalObjectIdCount}`);
print(`   - Int64 IDs: ${finalInt64Count}`);

if (finalObjectIdCount === 0) {
    print("\n🎉 Success! All bookings now have Int64 IDs!");
} else {
    print("\n⚠️  Warning: Some ObjectId entries still remain. Manual intervention may be required.");
}

print("\n✅ Script complete!");
