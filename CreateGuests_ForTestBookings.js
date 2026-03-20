// ========================================
// CREATE GUEST RECORDS FOR TEST BOOKINGS
// ========================================

print("========================================");
print("CREATING TEST GUEST RECORDS");
print("========================================");

// First, check if Guests collection exists and has data
const guestsCollectionExists = db.getCollectionNames().includes("Guests");
print(`\nGuests collection exists: ${guestsCollectionExists}`);

if (guestsCollectionExists) {
    const guestCount = db.Guests.countDocuments();
    print(`Existing guests in collection: ${guestCount}`);
}

// Check if test guest already exists
const existingGuest = db.Guests.findOne({ guestId: 1000000000003039 });
if (existingGuest) {
    print("\n✅ Test guest already exists!");
    print(`Guest: ${existingGuest.firstName} ${existingGuest.lastName}`);
    print(`Email: ${existingGuest.email}`);
    print("\nNo need to insert - guest data is already available.");
} else {
    print("\n⚠️ Test guest does NOT exist. Creating now...");
    
    // Insert test guest record
    const result = db.Guests.insertOne({
        guestId: 1000000000003039,
        firstName: "John",
        lastName: "Smith",
        email: "john.smith@example.com",
        phoneNumber: "+1-555-0123",
        nationality: "United States",
        dateOfBirth: new Date(1985, 5, 15),
        passportNumber: "P12345678",
        address: "123 Main Street, New York, NY 10001",
        createdDate: new Date(),
        lastModified: new Date(),
        isActive: true
    });
    
    if (result.acknowledged) {
        print("✅ SUCCESS! Test guest created:");
        print(`   Guest ID: 1000000000003039`);
        print(`   Name: John Smith`);
        print(`   Email: john.smith@example.com`);
        print(`   Phone: +1-555-0123`);
        print(`   Nationality: United States`);
    }
}

// Verify the guest can be found by the lookup
print("\n========================================");
print("VERIFICATION:");
print("========================================");

const verifyGuest = db.Guests.findOne({ guestId: 1000000000003039 });
if (verifyGuest) {
    print("✅ Guest lookup successful!");
    print(`   Found: ${verifyGuest.firstName} ${verifyGuest.lastName}`);
    print(`   Email: ${verifyGuest.email || 'N/A'}`);
    print(`   Phone: ${verifyGuest.phoneNumber || 'N/A'}`);
    print(`   Nationality: ${verifyGuest.nationality || 'N/A'}`);
} else {
    print("❌ ERROR: Cannot find guest with guestId: 1000000000003039");
}

// Check if bookings can now join with this guest
const bookingsWithGuest = db.Bookings.aggregate([
    { $match: { bookingId: "CURRENT_MONTH_001" } },
    {
        $lookup: {
            from: "Guests",
            localField: "guestId",
            foreignField: "guestId",
            as: "guestInfo"
        }
    },
    {
        $project: {
            bookingId: 1,
            roomNumber: 1,
            guestId: 1,
            guestInfo: 1
        }
    }
]).toArray();

if (bookingsWithGuest.length > 0 && bookingsWithGuest[0].guestInfo.length > 0) {
    print("\n✅ SUCCESS! Booking-Guest join is working!");
    print(`   Booking: ${bookingsWithGuest[0].bookingId}`);
    print(`   Guest: ${bookingsWithGuest[0].guestInfo[0].firstName} ${bookingsWithGuest[0].guestInfo[0].lastName}`);
} else {
    print("\n❌ ERROR: Booking-Guest join is NOT working!");
    print("   Check field name casing (guestId vs GuestId)");
}

print("\n========================================");
print("NEXT STEPS:");
print("========================================");
print("1. Refresh your browser (Ctrl+F5)");
print("2. Click on a booking bar in the Room Planner");
print("3. Guest details should now appear in the dialog!");
print("4. You should see: John Smith, john.smith@example.com, +1-555-0123");
