// ============================================
// USER ACTIVITY TRACKING - MONGODB SETUP
// Run this in MongoDB Compass MongoSH tab
// ============================================
// HOW TO RUN:
// 1. In MongoSH tab, type: use ListingDB
// 2. Press Enter
// 3. Then paste this ENTIRE script below
// ============================================

// Switch to ListingDB database
db = db.getSiblingDB('ListingDB');

print("\n" + "=".repeat(80));
print("🚀 USER ACTIVITY TRACKING SETUP - STARTING...");
print("=".repeat(80) + "\n");

// ============================================
// STEP 1: CREATE INDEXES FOR PERFORMANCE
// ============================================
print("📦 STEP 1: Creating Indexes...\n");

// Index for recent activities query (most common)
db.UserActivityLogs.createIndex({ Timestamp: -1 }, { name: "idx_timestamp_desc" });
print("  ✅ Created index: Timestamp (descending)");

// Index for user activities
db.UserActivityLogs.createIndex({ UserId: 1, Timestamp: -1 }, { name: "idx_user_timestamp" });
print("  ✅ Created index: UserId + Timestamp");

// Index for entity activities
db.UserActivityLogs.createIndex({ EntityType: 1, EntityId: 1, Timestamp: -1 }, { name: "idx_entity_timestamp" });
print("  ✅ Created index: EntityType + EntityId + Timestamp");

// Index for activity type filtering
db.UserActivityLogs.createIndex({ ActivityType: 1, Timestamp: -1 }, { name: "idx_activitytype_timestamp" });
print("  ✅ Created index: ActivityType + Timestamp");

print("\n📊 Index Creation Summary: 4 indexes created\n");

// ============================================
// STEP 2: SEED SAMPLE ACTIVITY DATA
// ============================================
print("=".repeat(80));
print("👤 STEP 2: Seeding Sample Activity Data...\n");

// Get first user for sample data
const firstUser = db.Users.findOne({ IsActive: true });

if (!firstUser) {
  print("⚠️  WARNING: No active users found. Sample data will not be created.");
  print("   Please create a user first, then run this script again.\n");
} else {
  const userId = firstUser.UserId;
  const username = firstUser.Username || firstUser.Email || `User${userId}`;
  
  print(`Using User: ${username} (ID: ${userId}) for sample data\n`);

  // Get current max activity ID
  const maxActivity = db.UserActivityLogs.findOne({}, { sort: { ActivityId: -1 } });
  let activityId = maxActivity ? maxActivity.ActivityId + 1 : 1;

  // Helper function to create activity
  function createActivity(activityType, action, description, entityType, entityId) {
    const activity = {
      ActivityId: activityId++,
      UserId: userId,
      Username: username,
      ActivityType: activityType,
      EntityType: entityType,
      EntityId: entityId,
      Action: action,
      Description: description,
      Metadata: {},
      Timestamp: new Date(Date.now() - Math.random() * 86400000), // Random time within last 24 hours
      IPAddress: "127.0.0.1",
      UserAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
      IsSuccess: true,
      Module: entityType
    };
    return activity;
  }

  // Sample activities
  const sampleActivities = [
    // Recent login
    createActivity("Login", "User Login", `${username} logged in successfully`, null, null),
    
    // Dashboard operations
    createActivity("DashboardView", "Viewed dashboard", "User accessed main dashboard", "Dashboard", 1),
    createActivity("WidgetAdd", "Added widget", "User added KPI widget to dashboard", "Dashboard", 1),
    createActivity("DashboardUpdate", "Updated dashboard", "User saved dashboard layout", "Dashboard", 1),
    
    // Property operations
    createActivity("Create", "Created property", "User created new property: Sunset Villa", "Property", 1),
    createActivity("Update", "Updated property", "User updated property: Sunset Villa", "Property", 1),
    createActivity("View", "Viewed property", "User viewed property details: Sunset Villa", "Property", 1),
    
    // Room operations
    createActivity("Create", "Created room", "User created new room: Room 101", "Room", 1),
    createActivity("Update", "Updated room", "User updated room status to Available", "Room", 1),
    
    // Booking operations
    createActivity("Create", "Created booking", "User created new booking #B001", "Booking", 1),
    createActivity("StatusChange", "Changed booking status", "User changed booking status to Confirmed", "Booking", 1),
    
    // Export operations
    createActivity("Export", "Exported properties", "User exported 25 properties to Excel", "Property", null),
    createActivity("Export", "Exported report", "User generated revenue report", null, null),
    
    // Search operations
    createActivity("Search", "Search", "User searched for: 'villa'", "Property", null),
    createActivity("Filter", "Applied filter", "User filtered properties by city: Mumbai", "Property", null),
    
    // Page views
    createActivity("PageView", "Viewed Properties", "User accessed Properties page", null, null),
    createActivity("PageView", "Viewed Bookings", "User accessed Bookings page", null, null),
    createActivity("PageView", "Viewed Dashboard", "User accessed Dashboard page", null, null),
  ];

  // Insert sample activities
  let inserted = 0;
  sampleActivities.forEach(activity => {
    try {
      db.UserActivityLogs.insertOne(activity);
      inserted++;
      print(`  ✅ Added: ${activity.Action}`);
    } catch (error) {
      print(`  ❌ Error adding ${activity.Action}: ${error.message}`);
    }
  });

  print(`\n📊 Sample Data Summary:`);
  print(`   ✅ Inserted: ${inserted} activities`);
  print(`   📦 Total: ${sampleActivities.length}\n`);
}

// ============================================
// STEP 3: VERIFICATION
// ============================================
print("=".repeat(80));
print("🔍 STEP 3: Verification & Statistics\n");

const totalActivities = db.UserActivityLogs.countDocuments({});
print(`📚 Total User Activities: ${totalActivities}\n`);

if (totalActivities > 0) {
  // Activities by Type
  print("🎨 Activities by Type:");
  const activityTypes = db.UserActivityLogs.aggregate([
    { $group: { _id: "$ActivityType", count: { $sum: 1 } } },
    { $sort: { count: -1 } }
  ]).toArray();
  
  activityTypes.forEach(type => {
    print(`   ${type._id.padEnd(20)} : ${type.count} activity(ies)`);
  });
  print("");

  // Recent activities
  print("📋 Most Recent Activities (Last 5):");
  const recentActivities = db.UserActivityLogs.find({})
    .sort({ Timestamp: -1 })
    .limit(5)
    .toArray();
  
  recentActivities.forEach(activity => {
    const timeAgo = Math.floor((new Date() - activity.Timestamp) / 60000); // minutes ago
    print(`   ${activity.Username.padEnd(20)} | ${activity.Action.padEnd(30)} | ${timeAgo} min ago`);
  });
  print("");
}

// Verify indexes
print("🔍 Indexes on UserActivityLogs:");
const indexes = db.UserActivityLogs.getIndexes();
indexes.forEach(idx => {
  const keyNames = Object.keys(idx.key).join(", ");
  print(`   ✅ ${idx.name}: ${keyNames}`);
});
print("");

print("=".repeat(80));
print("✅ USER ACTIVITY TRACKING SETUP COMPLETE!");
print("=".repeat(80));
print("\n💡 Next Steps:");
print("   1. Restart your .NET backend API");
print("   2. User activities will be automatically tracked");
print("   3. Access activities via API: GET /api/v1/activity/recent");
print("   4. View activities in the User Activity Widget (coming soon)\n");
print("🎉 Activity tracking system is ready!\n");
