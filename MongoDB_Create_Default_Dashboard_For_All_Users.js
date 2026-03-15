// ============================================
// Create Default Dashboard for All Users
// ============================================
// This script creates the default dashboard for all users who don't have one

use('PropertyMaster');

// 1. Get all unique user IDs from the system
const allUsers = db.getCollection('Users').find({ isActive: true }).toArray();

print(`Found ${allUsers.length} active users`);

// 2. Default dashboard template
const defaultDashboardLayout = {
  columns: 12,
  rowHeight: 80,
  widgets: [
    {
      widgetId: "bookings-today",
      widgetType: "kpi-card",
      position: { x: 0, y: 0, width: 3, height: 2 },
      settings: {}
    },
    {
      widgetId: "total-rooms",
      widgetType: "kpi-card",
      position: { x: 3, y: 0, width: 3, height: 2 },
      settings: {
        title: "Total Rooms",
        icon: "meeting_room",
        color: "#1976d2"
      }
    },
    {
      widgetId: "total-properties",
      widgetType: "kpi-card",
      position: { x: 6, y: 0, width: 3, height: 2 },
      settings: {}
    },
    {
      widgetId: "occupancy-rate",
      widgetType: "kpi-card",
      position: { x: 9, y: 0, width: 3, height: 2 },
      settings: {
        title: "Occupancy Rate",
        icon: "people",
        color: "#1976d2"
      }
    },
    {
      widgetId: "revenue-chart",
      widgetType: "chart",
      position: { x: 0, y: 2, width: 12, height: 4 },
      settings: {}
    },
    {
      widgetId: "recent-activity",
      widgetType: "list",
      position: { x: 0, y: 6, width: 6, height: 4 },
      settings: {}
    },
    {
      widgetId: "booking-calendar",
      widgetType: "calendar",
      position: { x: 6, y: 6, width: 6, height: 4 },
      settings: {}
    }
  ]
};

// 3. Process each user
let createdCount = 0;
let skippedCount = 0;
let updatedCount = 0;

allUsers.forEach(user => {
  const userId = user._id;
  
  // Check if user already has a dashboard
  const existingDashboards = db.getCollection('DashboardConfigurations')
    .find({ UserId: userId })
    .toArray();
  
  if (existingDashboards.length === 0) {
    // User has no dashboard - create default one
    const newDashboard = {
      UserId: userId,
      DashboardName: "My Default Dashboard",
      IsDefault: true,
      Layout: defaultDashboardLayout,
      CreatedAt: new Date(),
      UpdatedAt: new Date()
    };
    
    db.getCollection('DashboardConfigurations').insertOne(newDashboard);
    print(`✅ Created default dashboard for User ${userId}`);
    createdCount++;
    
  } else {
    const hasDefault = existingDashboards.some(d => d.IsDefault === true);
    
    if (!hasDefault) {
      // User has dashboards but none are default - update first one
      db.getCollection('DashboardConfigurations').updateOne(
        { _id: existingDashboards[0]._id },
        { $set: { IsDefault: true, UpdatedAt: new Date() } }
      );
      print(`⚡ Set existing dashboard as default for User ${userId}`);
      updatedCount++;
    } else {
      print(`⏭️  User ${userId} already has a default dashboard`);
      skippedCount++;
    }
  }
});

print('\n=====================================');
print('📊 SUMMARY:');
print(`✅ Created: ${createdCount} new default dashboards`);
print(`⚡ Updated: ${updatedCount} existing dashboards to default`);
print(`⏭️  Skipped: ${skippedCount} users (already have default)`);
print(`📈 Total Users Processed: ${allUsers.length}`);
print('=====================================');

// 4. Verify the results
const totalDefaultDashboards = db.getCollection('DashboardConfigurations')
  .countDocuments({ IsDefault: true });

print(`\n✅ Total default dashboards in database: ${totalDefaultDashboards}`);

// 5. Show sample dashboard
const sampleDashboard = db.getCollection('DashboardConfigurations')
  .findOne({ IsDefault: true });

print('\n📋 Sample Default Dashboard:');
printjson({
  UserId: sampleDashboard.UserId,
  DashboardName: sampleDashboard.DashboardName,
  IsDefault: sampleDashboard.IsDefault,
  WidgetCount: sampleDashboard.Layout.widgets.length,
  Widgets: sampleDashboard.Layout.widgets.map(w => w.widgetId)
});
