// ============================================
// ADD USER ACTIVITY STREAM WIDGET
// Run this in MongoDB Compass MongoSH tab
// ============================================
// HOW TO RUN:
// 1. In MongoSH tab, first type: use ListingDB
// 2. Press Enter
// 3. Then paste this script below
// ============================================

// Switch to ListingDB database
db = db.getSiblingDB('ListingDB');

print("\n" + "=".repeat(80));
print("📊 ADDING USER ACTIVITY STREAM WIDGET");
print("=".repeat(80) + "\n");

// Define the Activity Stream Widget
const activityWidget = {
  WidgetId: "user-activity-stream",
  WidgetType: "activity-stream",
  Name: "User Activity Stream",
  Description: "Real-time user activity tracking with statistics and timeline view",
  Icon: "timeline",
  Category: "Activity",
  DefaultSettings: {
    title: "User Activity",
    showStats: true,
    maxActivities: 10,
    refreshInterval: 60000 // 60 seconds
  },
  DefaultSize: { width: 6, height: 6 },
  MinSize: { width: 4, height: 4 },
  MaxSize: { width: 12, height: 8 },
  RequiredPermissions: ["dashboard.view", "activity.view"],
  IsActive: true,
  CreatedAt: new Date()
};

// Check if widget already exists
const existing = db.WidgetLibrary.findOne({ WidgetId: "user-activity-stream" });

if (existing) {
  print("⚠️  Widget 'user-activity-stream' already exists!");
  print("   Updating existing widget...\n");
  
  const updateResult = db.WidgetLibrary.updateOne(
    { WidgetId: "user-activity-stream" },
    { 
      $set: {
        ...activityWidget,
        UpdatedAt: new Date()
      }
    }
  );
  
  print(`✅ Widget updated successfully!`);
  print(`   Matched: ${updateResult.matchedCount}`);
  print(`   Modified: ${updateResult.modifiedCount}\n`);
} else {
  print("📦 Adding new widget...\n");
  
  const insertResult = db.WidgetLibrary.insertOne(activityWidget);
  
  print(`✅ Widget added successfully!`);
  print(`   Inserted ID: ${insertResult.insertedId}\n`);
}

// Verify insertion
const widget = db.WidgetLibrary.findOne({ WidgetId: "user-activity-stream" });

print("=".repeat(80));
print("📋 WIDGET DETAILS");
print("=".repeat(80));
print(`Widget ID:          ${widget.WidgetId}`);
print(`Name:               ${widget.Name}`);
print(`Type:               ${widget.WidgetType}`);
print(`Category:           ${widget.Category}`);
print(`Icon:               ${widget.Icon}`);
print(`Description:        ${widget.Description}`);
print(`Default Size:       ${widget.DefaultSize.width}x${widget.DefaultSize.height}`);
print(`Min Size:           ${widget.MinSize.width}x${widget.MinSize.height}`);
print(`Max Size:           ${widget.MaxSize.width}x${widget.MaxSize.height}`);
print(`Active:             ${widget.IsActive}`);
print(`Created:            ${widget.CreatedAt}`);

print("\n📐 Default Settings:");
print(`   Title:           ${widget.DefaultSettings.title}`);
print(`   Show Stats:      ${widget.DefaultSettings.showStats}`);
print(`   Max Activities:  ${widget.DefaultSettings.maxActivities}`);
print(`   Refresh (ms):    ${widget.DefaultSettings.refreshInterval}`);

print("\n🔒 Required Permissions:");
widget.RequiredPermissions.forEach(perm => {
  print(`   - ${perm}`);
});

// Count total widgets
const totalWidgets = db.WidgetLibrary.countDocuments({ IsActive: true });
print(`\n📊 Total Active Widgets: ${totalWidgets}`);

print("\n" + "=".repeat(80));
print("✅ ACTIVITY STREAM WIDGET READY!");
print("=".repeat(80));
print("\n💡 Next Steps:");
print("   1. Restart your Angular frontend");
print("   2. Login and navigate to Dashboard");
print("   3. Click 'Add Widget' button");
print("   4. Look for 'User Activity Stream' in Activity category");
print("   5. Add it to your dashboard and watch real-time activities!\n");
print("🎉 Your Activity Stream Widget is ready to use!\n");
