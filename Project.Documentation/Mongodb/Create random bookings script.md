// ================================================================
//  DEMO BOOKINGS GENERATOR  v2  —  mongosh script
//  ✅ _id sourced from KeyCounter collection (atomic increment)
//  ✅ Auto-initialises Bookings counter if it doesn't exist yet
//  ✅ Duplicate _id = skip silently, never throws
//  ✅ Run as many times as you like
// ================================================================

// ================================================================
//  ⚙️  CONFIG — only edit this block before each run
// ================================================================
const PROPERTY_ID  = -1;                               // 👈 target property _id
const COUNT        = 100;                              // 👈 bookings to insert this run
const PERIOD_START = new Date("2025-01-01T00:00:00Z"); // 👈 earliest possible check-in
const PERIOD_END   = new Date("2026-12-31T23:59:59Z"); // 👈 latest possible check-in
// ================================================================

// ── Guard: period must be valid ──────────────────────────────────
if (PERIOD_START >= PERIOD_END) {
  throw new Error("❌ PERIOD_START must be before PERIOD_END");
}

// ── Guard: property must exist ───────────────────────────────────
const propertyDoc = db.Property.findOne({ _id: PROPERTY_ID });
if (!propertyDoc) {
  throw new Error(`❌ No property found with _id: ${PROPERTY_ID}. Insert the property first.`);
}

const rooms = (propertyDoc.Rooms ?? [])
  .filter(r => r.Active !== false)
  .map(r => r.RoomCode);

if (rooms.length === 0) {
  throw new Error(`❌ Property ${PROPERTY_ID} has no active rooms.`);
}

// ── KeyCounter: initialise Bookings counter if missing ───────────
// Looks at the current max booking _id so the counter is never
// behind what already exists in the collection.
const existingCounter = db.KeyCounter.findOne({ EntityName: "Bookings" });

if (!existingCounter) {
  const lastBooking = db.Bookings
    .find({}, { _id: 1 })
    .sort({ _id: -1 })
    .limit(1)
    .toArray();

  const initSeq = lastBooking.length > 0
    ? parseInt(lastBooking[0]._id.toString(), 10)   // start from current max
    : 4000000000000000;                              // cold start value

  db.KeyCounter.insertOne({
    EntityName: "Bookings",
    Sequence:   NumberLong(initSeq)
  });

  print(`📝 Created KeyCounter "Bookings" initialised at ${initSeq}`);
}

// ── KeyCounter: atomically reserve COUNT ids ─────────────────────
// $inc advances the counter by COUNT in one atomic operation.
// returnDocument:"before" gives us the value BEFORE the increment,
// so our first usable id is oldSequence + 1.
const counterBefore = db.KeyCounter.findOneAndUpdate(
  { EntityName: "Bookings" },
  { $inc: { Sequence: NumberLong(COUNT) } },
  { returnDocument: "before" }
);

// Safe Int64 → JS number conversion (values ~4e15 are within JS safe integer range)
const counterVal = parseInt(counterBefore.Sequence.toString(), 10);
const startId    = counterVal + 1;
const endId      = counterVal + COUNT;

print(`\n🏨  Property   : "${propertyDoc.Name}" (id: ${PROPERTY_ID})`);
print(`🚪  Rooms      : ${rooms.join(", ")}`);
print(`📅  Period     : ${PERIOD_START.toDateString()} → ${PERIOD_END.toDateString()}`);
print(`🔢  Count      : ${COUNT}   |   🆔 Reserved: ${startId} → ${endId}\n`);

// ── Helpers ──────────────────────────────────────────────────────
function randInt(min, max) {
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

function randDate(from, to) {
  return new Date(from.getTime() + Math.random() * (to.getTime() - from.getTime()));
}

function randCode(len) {
  const chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
  let s = "";
  for (let i = 0; i < len; i++) s += chars[randInt(0, chars.length - 1)];
  return s;
}

function pick(arr) { return arr[randInt(0, arr.length - 1)]; }

// ── Reference data ───────────────────────────────────────────────
const paymentStatuses = [
  NumberLong("5000000000000011"),  // Paid
  NumberLong("5000000000000012"),  // Pending
  NumberLong("5000000000000013"),  // Refunded
];

const bookingSources = [
  NumberLong("5000000000000001"),  // Direct
  NumberLong("5000000000000002"),  // Booking.com
  NumberLong("5000000000000003"),  // Airbnb
];

const specialRequestPool = [
  [], [], [],                                          // ~25% no requests
  ["Late check-in"],
  ["Early check-in"],
  ["Extra pillows"],
  ["Non-smoking room"],
  ["Sea view room"],
  ["Airport transfer"],
  ["Cot required"],
  ["Vegetarian meals"],
  ["Late check-in", "Extra pillows"],
  ["Early check-in", "Non-smoking room"],
  ["Airport transfer", "Late check-in"],
];

// Weighted stay length: 1–10 nights, shorter stays more likely
const stayWeights = [0, 28, 24, 18, 12, 7, 4, 3, 2, 1, 1]; // index = nights

function randStayNights() {
  const roll = randInt(1, 100);
  let cumulative = 0;
  for (let n = 1; n <= 10; n++) {
    cumulative += stayWeights[n];
    if (roll <= cumulative) return n;
  }
  return 1;
}

// ── Build and insert bookings one-by-one ─────────────────────────
// Individual insertion lets us silently skip any duplicate _id
// without losing the rest of the batch.
let inserted = 0;
let skipped  = 0;

for (let i = 0; i < COUNT; i++) {
  const id = startId + i;

  const checkIn     = randDate(PERIOD_START, PERIOD_END);
  const stayNights  = randStayNights();
  const checkOut    = new Date(checkIn.getTime() + stayNights * 86400000);
  const leadDays    = randInt(1, 120);
  const bookingDate = new Date(checkIn.getTime() - leadDays * 86400000);
  const baseRate    = randInt(80, 350);
  const totalPrice  = Math.round((baseRate * stayNights + Math.random() * 50) * 100) / 100;

  const doc = {
    _id:             NumberLong(id),
    bookingId:       randCode(10),
    guestId:         NumberLong(randInt(10000000000001, 10000000000999)),
    staffId:         NumberLong(randInt(20000000000001, 20000000000020)),
    roomNumber:      pick(rooms),
    bookingDate:     bookingDate,
    checkInDate:     checkIn,
    checkOutDate:    checkOut,
    numberOfGuests:  randInt(1, 4),
    totalPrice:      totalPrice,
    paymentStatusId: pick(paymentStatuses),
    bookingSourceId: pick(bookingSources),
    specialRequests: pick(specialRequestPool),
    isConfirmed:     Math.random() > 0.12,   // ~88% confirmed
    lastModified:    new Date(),
    propertyId:      PROPERTY_ID
  };

  try {
    db.Bookings.insertOne(doc);
    inserted++;
  } catch (e) {
    if (e.code === 11000) {
      // Duplicate key — this id is already taken, skip silently
      skipped++;
    } else {
      // Unexpected error — re-throw so we don't silently swallow real problems
      throw e;
    }
  }
}

// ── Summary ──────────────────────────────────────────────────────
const totalNow    = db.Bookings.countDocuments({ propertyId: PROPERTY_ID });
const counterNow  = db.KeyCounter.findOne({ EntityName: "Bookings" }).Sequence;

print(`✅  Inserted        : ${inserted}`);
print(`⏭️   Skipped (dup)   : ${skipped}`);
print(`📊  Total (property): ${totalNow}`);
print(`🔖  KeyCounter now  : ${counterNow}`);