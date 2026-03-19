// ============================================
// QUICK FIX: Force Property 1 Selection
// Run this in MongoDB Compass to check property exists
// ============================================

print("🔧 QUICK FIX: Checking Property 1");
print("=" .repeat(60));

// Check if Property 1 exists
const prop1 = db.Property.findOne({ PropertyId: 1 });

if (prop1) {
  print(`✅ Property 1 exists: "${prop1.PropertyName || prop1.Name || 'Unknown'}"`);
  print(`   ID: ${prop1.PropertyId || prop1._id}`);
} else {
  print("❌ Property 1 does NOT exist!");
  print("\n⚠️  CREATING Property 1...\n");
  
  // Create Property 1 if it doesn't exist
  const result = db.Property.insertOne({
    PropertyId: 1,
    PropertyName: "Test Property 1",
    Name: "Test Property 1",
    PropertyCode: "PROP001",
    Address: "123 Test Street",
    City: "Test City",
    State: "Test State",
    Country: "Test Country",
    ZipCode: "12345",
    Phone: "+1234567890",
    Email: "test@property1.com",
    Status: "Active",
    IsActive: true,
    Active: true,
    CreatedAt: new Date(),
    CreatedBy: 1,
    IsDeleted: false
  });
  
  if (result.acknowledged) {
    print("✅ Property 1 created successfully!");
  } else {
    print("❌ Failed to create Property 1");
  }
}

print("\n" + "=".repeat(60));
print("📋 NEXT STEPS:");
print("1. In your browser, press F12 (open Developer Tools)");
print("2. Go to Console tab");
print("3. Run: localStorage.setItem('selectedPropertyIds', JSON.stringify([1]))");
print("4. Refresh the page or click Room Planner tab");
print("=" .repeat(60));
