// MongoDB Script to Insert 5000 Test Bookings
// Split: 2500 bookings for propertyId = -1, 2500 for propertyId = 1
// Run this script in MongoDB shell or MongoDB Compass

// Helper function to generate random date within a range
function randomDate(start, end) {
    return new Date(start.getTime() + Math.random() * (end.getTime() - start.getTime()));
}

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

// Sample special requests
const specialRequests = [
    [],
    ['Early check-in'],
    ['Late check-out'],
    ['Extra pillows', 'Non-smoking room'],
    ['High floor', 'Away from elevator'],
    ['Quiet room', 'Extra towels'],
    ['King bed', 'City view'],
    ['Twin beds', 'Pool access'],
    ['Ground floor', 'Disability access'],
    ['Ocean view', 'Balcony']
];

// Payment status IDs (randomly selected)
const paymentStatusIds = [
    5000000000000010, // Pending
    5000000000000011, // Paid
    5000000000000012, // Partially Paid
    5000000000000013  // Refunded
];

// Booking source IDs
const bookingSourceIds = [
    5000000000000001, // Website
    5000000000000002, // Phone
    5000000000000003, // Walk-in
    5000000000000004, // Travel Agent
    5000000000000005  // Online Travel Agency (OTA)
];

// Generate bookings
const bookings = [];
let bookingIdCounter = 4000000000000001;

// Date ranges for realistic data - Jan 1, 2026 to Apr 30, 2026
const checkInStart = new Date(2026, 0, 1); // Jan 1, 2026
const checkInEnd = new Date(2026, 3, 30); // Apr 30, 2026
const bookingStart = new Date(2025, 11, 1); // Dec 1, 2025 (bookings made before check-in)
const bookingEnd = new Date(2026, 3, 30); // Apr 30, 2026

console.log('Generating 5000 test bookings for period: Jan 1, 2026 - Apr 30, 2026...');

// Generate 2500 bookings for propertyId = -1
for (let i = 0; i < 2500; i++) {
    const checkInDate = randomDate(checkInStart, checkInEnd);
    const bookingDate = randomDate(bookingStart, checkInDate); // Booking made before check-in
    const stayDuration = Math.floor(Math.random() * 14) + 1; // 1-14 days
    const checkOutDate = new Date(checkInDate);
    checkOutDate.setDate(checkOutDate.getDate() + stayDuration);
    
    const numberOfGuests = Math.floor(Math.random() * 4) + 1; // 1-4 guests
    const pricePerNight = (Math.random() * 200 + 50).toFixed(2); // $50-$250 per night
    const totalPrice = (pricePerNight * stayDuration).toFixed(2);
    
    const booking = {
        _id: NumberLong(bookingIdCounter.toString()),
        bookingId: generateBookingId(),
        guestId: NumberLong((1000000000003000 + Math.floor(Math.random() * 500)).toString()), // Random guest IDs
        staffId: NumberLong((2000000000000040 + Math.floor(Math.random() * 20)).toString()), // Random staff IDs
        roomNumber: generateRoomNumber(),
        bookingDate: bookingDate,
        checkInDate: checkInDate,
        checkOutDate: checkOutDate,
        numberOfGuests: numberOfGuests,
        totalPrice: parseFloat(totalPrice),
        paymentStatusId: NumberLong(paymentStatusIds[Math.floor(Math.random() * paymentStatusIds.length)].toString()),
        bookingSourceId: NumberLong(bookingSourceIds[Math.floor(Math.random() * bookingSourceIds.length)].toString()),
        specialRequests: specialRequests[Math.floor(Math.random() * specialRequests.length)],
        isConfirmed: Math.random() > 0.1, // 90% confirmed
        lastModified: randomDate(bookingDate, checkInDate),
        propertyId: -1
    };
    
    bookings.push(booking);
    bookingIdCounter++;
    
    if ((i + 1) % 500 === 0) {
        console.log(`Generated ${i + 1}/2500 bookings for propertyId = -1`);
    }
}

// Generate 2500 bookings for propertyId = 1
for (let i = 0; i < 2500; i++) {
    const checkInDate = randomDate(checkInStart, checkInEnd);
    const bookingDate = randomDate(bookingStart, checkInDate); // Booking made before check-in
    const stayDuration = Math.floor(Math.random() * 14) + 1; // 1-14 days
    const checkOutDate = new Date(checkInDate);
    checkOutDate.setDate(checkOutDate.getDate() + stayDuration);

    const numberOfGuests = Math.floor(Math.random() * 4) + 1; // 1-4 guests
    const pricePerNight = (Math.random() * 200 + 50).toFixed(2); // $50-$250 per night
    const totalPrice = (pricePerNight * stayDuration).toFixed(2);

    const booking = {
        _id: NumberLong(bookingIdCounter.toString()),
        bookingId: generateBookingId(),
        guestId: NumberLong((1000000000003000 + Math.floor(Math.random() * 500)).toString()), // Random guest IDs
        staffId: NumberLong((2000000000000040 + Math.floor(Math.random() * 20)).toString()), // Random staff IDs
        roomNumber: generateRoomNumber(),
        bookingDate: bookingDate,
        checkInDate: checkInDate,
        checkOutDate: checkOutDate,
        numberOfGuests: numberOfGuests,
        totalPrice: parseFloat(totalPrice),
        paymentStatusId: NumberLong(paymentStatusIds[Math.floor(Math.random() * paymentStatusIds.length)].toString()),
        bookingSourceId: NumberLong(bookingSourceIds[Math.floor(Math.random() * bookingSourceIds.length)].toString()),
        specialRequests: specialRequests[Math.floor(Math.random() * specialRequests.length)],
        isConfirmed: Math.random() > 0.1, // 90% confirmed
        lastModified: randomDate(bookingDate, checkInDate),
        propertyId: 1
    };
    
    bookings.push(booking);
    bookingIdCounter++;
    
    if ((i + 1) % 500 === 0) {
        console.log(`Generated ${i + 1}/2500 bookings for propertyId = 1`);
    }
}

console.log(`Total bookings generated: ${bookings.length}`);
console.log('Inserting bookings into database...');

// Insert into MongoDB collection
// Replace 'your_database_name' and 'bookings' with your actual database and collection names
db.bookings.insertMany(bookings);

console.log('✅ Successfully inserted 5000 bookings!');
console.log('   - 2500 bookings for propertyId = -1');
console.log('   - 2500 bookings for propertyId = 1');

// Verify insertion
const count = db.bookings.countDocuments();
console.log(`Total bookings in collection: ${count}`);

// Show sample statistics
console.log('\n📊 Sample Statistics:');
console.log('Property -1 bookings:', db.bookings.countDocuments({ propertyId: -1 }));
console.log('Property 1 bookings:', db.bookings.countDocuments({ propertyId: 1 }));

// Show bookings in January 2026
console.log('\nBookings in January 2026:');
const jan2026Start = new Date(2026, 0, 1);
const jan2026End = new Date(2026, 0, 31);
console.log('Property -1:', db.bookings.countDocuments({ 
    propertyId: -1, 
    checkInDate: { $gte: jan2026Start, $lte: jan2026End } 
}));
console.log('Property 1:', db.bookings.countDocuments({ 
    propertyId: 1, 
    checkInDate: { $gte: jan2026Start, $lte: jan2026End } 
}));

// Show bookings in February 2026
console.log('\nBookings in February 2026:');
const feb2026Start = new Date(2026, 1, 1);
const feb2026End = new Date(2026, 1, 28);
console.log('Property -1:', db.bookings.countDocuments({ 
    propertyId: -1, 
    checkInDate: { $gte: feb2026Start, $lte: feb2026End } 
}));
console.log('Property 1:', db.bookings.countDocuments({ 
    propertyId: 1, 
    checkInDate: { $gte: feb2026Start, $lte: feb2026End } 
}));

// Show bookings in March 2026
console.log('\nBookings in March 2026:');
const mar2026Start = new Date(2026, 2, 1);
const mar2026End = new Date(2026, 2, 31);
console.log('Property -1:', db.bookings.countDocuments({ 
    propertyId: -1, 
    checkInDate: { $gte: mar2026Start, $lte: mar2026End } 
}));
console.log('Property 1:', db.bookings.countDocuments({ 
    propertyId: 1, 
    checkInDate: { $gte: mar2026Start, $lte: mar2026End } 
}));

// Show bookings in April 2026
console.log('\nBookings in April 2026:');
const apr2026Start = new Date(2026, 3, 1);
const apr2026End = new Date(2026, 3, 30);
console.log('Property -1:', db.bookings.countDocuments({ 
    propertyId: -1, 
    checkInDate: { $gte: apr2026Start, $lte: apr2026End } 
}));
console.log('Property 1:', db.bookings.countDocuments({ 
    propertyId: 1, 
    checkInDate: { $gte: apr2026Start, $lte: apr2026End } 
}));
