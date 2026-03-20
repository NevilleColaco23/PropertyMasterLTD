// ========================================
// DEBUG: Test Bookings Query
// ========================================
// Run this in MongoDB Compass or mongosh to test the bookings query

use PropertyMasterDB;

// Step 1: Check if Bookings collection exists and has data
print("========================================");
print("STEP 1: Checking Bookings Collection");
print("========================================");

const totalBookings = db.Bookings.countDocuments({});
print(`Total bookings in collection: ${totalBookings}`);

if (totalBookings === 0) {
    print("❌ ERROR: Bookings collection is empty!");
    print("Run the QuickInsert_RoomPlanner.js script to create sample bookings.");
} else {
    print("✅ Bookings collection has data");
    
    // Show sample booking
    const sampleBooking = db.Bookings.findOne({});
    print("\nSample booking document:");
    printjson(sampleBooking);
}

// Step 2: Check bookings for current date range
print("\n========================================");
print("STEP 2: Checking Bookings for Current Month");
print("========================================");

const now = new Date();
const startDate = new Date(now.getFullYear(), now.getMonth(), 1);
const endDate = new Date(now.getFullYear(), now.getMonth() + 1, 0);

print(`Date range: ${startDate.toISOString()} to ${endDate.toISOString()}`);

// Check for bookings in this date range (both camelCase and PascalCase)
const bookingsInRange = db.Bookings.find({
    $or: [
        {
            $and: [
                { checkInDate: { $lte: endDate } },
                { checkOutDate: { $gte: startDate } }
            ]
        },
        {
            $and: [
                { CheckInDate: { $lte: endDate } },
                { CheckOutDate: { $gte: startDate } }
            ]
        }
    ]
}).toArray();

print(`Found ${bookingsInRange.length} bookings in current month`);

if (bookingsInRange.length === 0) {
    print("❌ No bookings found in current month!");
    print("This is likely why the grid is empty.");
    print("\nOptions:");
    print("1. Create bookings for current month using the script below");
    print("2. Or navigate to a different month that has bookings");
} else {
    print("✅ Bookings found in date range:");
    bookingsInRange.forEach((booking, index) => {
        print(`\n${index + 1}. Booking ID: ${booking.bookingId || booking._id}`);
        print(`   Room: ${booking.roomNumber || booking.RoomNumber}`);
        print(`   Check-in: ${booking.checkInDate || booking.CheckInDate}`);
        print(`   Check-out: ${booking.checkOutDate || booking.CheckOutDate}`);
        print(`   Property ID: ${booking.propertyId || booking.PropertyId}`);
        print(`   Guest ID: ${booking.guestId || booking.GuestId || 'MISSING'}`);
    });
}

// Step 3: Check field name consistency
print("\n========================================");
print("STEP 3: Checking Field Name Casing");
print("========================================");

const camelCaseBookings = db.Bookings.countDocuments({ checkInDate: { $exists: true } });
const pascalCaseBookings = db.Bookings.countDocuments({ CheckInDate: { $exists: true } });

print(`Bookings with camelCase (checkInDate): ${camelCaseBookings}`);
print(`Bookings with PascalCase (CheckInDate): ${pascalCaseBookings}`);

if (camelCaseBookings > 0 && pascalCaseBookings > 0) {
    print("⚠️ WARNING: Mixed casing detected! This might cause query issues.");
}

// Step 4: Test the full aggregation pipeline
print("\n========================================");
print("STEP 4: Testing Full Aggregation Pipeline");
print("========================================");

const propertyId = 1; // Change this to match your property
const result = db.Bookings.aggregate([
    // Stage 1: $match
    {
        $match: {
            $and: [
                {
                    $or: [
                        {
                            $and: [
                                { checkInDate: { $lte: endDate } },
                                { checkOutDate: { $gte: startDate } }
                            ]
                        },
                        {
                            $and: [
                                { CheckInDate: { $lte: endDate } },
                                { CheckOutDate: { $gte: startDate } }
                            ]
                        }
                    ]
                },
                {
                    $or: [
                        { propertyId: { $in: [propertyId] } },
                        { PropertyId: { $in: [propertyId] } }
                    ]
                }
            ]
        }
    },
    // Stage 2: $lookup with Guests
    {
        $lookup: {
            from: "Guests",
            localField: "guestId",
            foreignField: "guestId",
            as: "guestInfo"
        }
    },
    // Stage 3: $lookup with Property
    {
        $lookup: {
            from: "Property",
            let: { propId: "$propertyId" },
            pipeline: [
                {
                    $match: {
                        $expr: { $eq: ["$_id", "$$propId"] }
                    }
                }
            ],
            as: "propertyInfo"
        }
    },
    // Stage 4: $project
    {
        $project: {
            _id: 1,
            bookingId: { $ifNull: ["$bookingId", "$BookingId", ""] },
            roomNumber: { $ifNull: ["$roomNumber", "$RoomNumber", ""] },
            propertyId: { $ifNull: ["$propertyId", "$PropertyId", 0] },
            propertyName: {
                $ifNull: [
                    { $arrayElemAt: ["$propertyInfo.PropertyName", 0] },
                    { $arrayElemAt: ["$propertyInfo.propertyName", 0] },
                    "Unknown Property"
                ]
            },
            checkInDate: { $ifNull: ["$checkInDate", "$CheckInDate", null] },
            checkOutDate: { $ifNull: ["$checkOutDate", "$CheckOutDate", null] },
            status: { $ifNull: ["$status", "$Status", "confirmed"] },
            guestId: { $ifNull: ["$guestId", "$GuestId", ""] },
            guestFirstName: {
                $ifNull: [
                    { $arrayElemAt: ["$guestInfo.firstName", 0] },
                    { $arrayElemAt: ["$guestInfo.FirstName", 0] },
                    "Unavailable"
                ]
            },
            guestLastName: {
                $ifNull: [
                    { $arrayElemAt: ["$guestInfo.lastName", 0] },
                    { $arrayElemAt: ["$guestInfo.LastName", 0] },
                    "Unavailable"
                ]
            },
            guestEmail: {
                $ifNull: [
                    { $arrayElemAt: ["$guestInfo.email", 0] },
                    { $arrayElemAt: ["$guestInfo.Email", 0] },
                    "Unavailable"
                ]
            }
        }
    }
]).toArray();

print(`Aggregation returned ${result.length} bookings`);

if (result.length === 0) {
    print("❌ Aggregation returned 0 results");
    print("\nPossible causes:");
    print("1. No bookings in the specified date range");
    print("2. Property ID filter is excluding all bookings");
    print("3. Field name casing mismatch");
} else {
    print("✅ Aggregation successful!");
    print("\nFirst result:");
    printjson(result[0]);
}

// ========================================
// QUICK FIX: Create bookings for current month
// ========================================
print("\n========================================");
print("QUICK FIX: Create Sample Bookings for Current Month");
print("========================================");
print("Run the following to create bookings for the current month:\n");

print(`
// Create bookings for ${startDate.toLocaleDateString()} to ${endDate.toLocaleDateString()}
db.Bookings.insertMany([
    {
        bookingId: "CURRENT_MONTH_001",
        roomNumber: "101",
        propertyId: 1,
        checkInDate: new Date("${new Date(now.getFullYear(), now.getMonth(), 5).toISOString()}"),
        checkOutDate: new Date("${new Date(now.getFullYear(), now.getMonth(), 8).toISOString()}"),
        status: "confirmed",
        guestId: "G001"
    },
    {
        bookingId: "CURRENT_MONTH_002",
        roomNumber: "102",
        propertyId: 1,
        checkInDate: new Date("${new Date(now.getFullYear(), now.getMonth(), 10).toISOString()}"),
        checkOutDate: new Date("${new Date(now.getFullYear(), now.getMonth(), 15).toISOString()}"),
        status: "confirmed",
        guestId: "G002"
    },
    {
        bookingId: "CURRENT_MONTH_003",
        roomNumber: "103",
        propertyId: 1,
        checkInDate: new Date("${new Date(now.getFullYear(), now.getMonth(), 12).toISOString()}"),
        checkOutDate: new Date("${new Date(now.getFullYear(), now.getMonth(), 18).toISOString()}"),
        status: "confirmed",
        guestId: "G003"
    },
    {
        bookingId: "CURRENT_MONTH_004",
        roomNumber: "201",
        propertyId: 1,
        checkInDate: new Date("${new Date(now.getFullYear(), now.getMonth(), 15).toISOString()}"),
        checkOutDate: new Date("${new Date(now.getFullYear(), now.getMonth(), 20).toISOString()}"),
        status: "confirmed",
        guestId: "G001"
    }
]);

print("✅ Created 4 sample bookings for current month");
`);

print("\n========================================");
print("Summary");
print("========================================");
print("Next steps:");
print("1. If no bookings exist, run the insert script above");
print("2. Check backend console logs for query debugging");
print("3. Check browser console for frontend processing logs");
print("4. Verify property ID matches between frontend and backend");
