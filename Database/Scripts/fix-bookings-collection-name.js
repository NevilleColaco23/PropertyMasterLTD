// Fix Script: Copy bookings to uppercase collection
// This fixes the "Bookings Today" KPI showing 0

use ListingDB

console.log('🔧 Fixing collection name case issue...\n');

// Check current state
const bookingsLower = db.bookings.countDocuments();
const bookingsUpper = db.Bookings.countDocuments();

console.log(`📊 Current state:`);
console.log(`   - "bookings" (lowercase): ${bookingsLower} documents`);
console.log(`   - "Bookings" (uppercase): ${bookingsUpper} documents`);
console.log('');

if (bookingsUpper > 0) {
    console.log('⚠️  WARNING: "Bookings" collection already has data!');
    console.log('   This script will ADD to existing data, not replace it.');
    console.log('   To start fresh, run: db.Bookings.drop()');
    console.log('');
}

if (bookingsLower === 0) {
    console.log('❌ ERROR: No bookings found in lowercase collection!');
    console.log('   Run seed-bookings-data.js first.');
} else {
    console.log('🚀 Copying all bookings to uppercase "Bookings" collection...');
    console.log('   This may take a moment for large datasets...\n');
    
    let copied = 0;
    let errors = 0;
    
    // Use bulk operations for better performance
    const bulkOps = [];
    
    db.bookings.find().forEach(doc => {
        bulkOps.push({ insertOne: { document: doc } });
        copied++;
        
        // Process in batches of 1000
        if (bulkOps.length >= 1000) {
            try {
                db.Bookings.bulkWrite(bulkOps);
                bulkOps.length = 0; // Clear array
                console.log(`   ✅ Processed ${copied} documents...`);
            } catch (e) {
                console.log(`   ⚠️  Error in batch: ${e.message}`);
                errors++;
            }
        }
    });
    
    // Process remaining documents
    if (bulkOps.length > 0) {
        try {
            db.Bookings.bulkWrite(bulkOps);
            console.log(`   ✅ Processed final ${bulkOps.length} documents...`);
        } catch (e) {
            console.log(`   ⚠️  Error in final batch: ${e.message}`);
            errors++;
        }
    }
    
    console.log('');
    console.log('✅ Copy complete!');
    console.log(`   - Documents copied: ${copied}`);
    console.log(`   - Errors: ${errors}`);
    console.log('');
    
    // Verify the copy
    const newCount = db.Bookings.countDocuments();
    console.log(`📊 Verification:`);
    console.log(`   - "Bookings" now has: ${newCount} documents`);
    
    // Check today's bookings
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);
    
    const todayCount = db.Bookings.countDocuments({
        CreatedAt: { 
            $gte: today,
            $lt: tomorrow
        }
    });
    
    console.log(`   - Today's bookings: ${todayCount}`);
    console.log('');
    
    if (newCount === bookingsLower) {
        console.log('✨ SUCCESS! All bookings copied successfully!');
        console.log('   Now refresh your dashboard to see updated KPI.');
    } else {
        console.log('⚠️  WARNING: Document count mismatch!');
        console.log(`   Expected: ${bookingsLower}, Got: ${newCount}`);
    }
}

console.log('');
console.log('📝 Note: The lowercase "bookings" collection is still intact.');
console.log('   You can keep both or drop the lowercase one later.');
console.log('');
console.log('🧹 To remove lowercase collection later (optional):');
console.log('   db.bookings.drop()');
