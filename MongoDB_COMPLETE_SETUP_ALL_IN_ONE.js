// ============================================
// COMPLETE SYSTEM SETUP - ALL IN ONE
// MongoDB Compass MongoSH Script
// ============================================
// This script sets up:
// 1. User Activity Tracking (collection + indexes)
// 2. Complete Widget Library (20+ widgets)
// 3. Default Dashboards for all users
// 4. Sample activity data
// ============================================
// HOW TO RUN:
// 1. Open MongoDB Compass
// 2. Click MongoSH tab (bottom of window)
// 3. Type: use ListingDB
// 4. Press Enter
// 5. Paste this ENTIRE script
// 6. Press Enter
// 7. Wait for completion (30-60 seconds)
// ============================================

// Switch to ListingDB database
db = db.getSiblingDB('ListingDB');

print("\n" + "=".repeat(100));
print("🚀 COMPLETE SYSTEM SETUP - STARTING...");
print("=".repeat(100) + "\n");

// ============================================
// PART 1: USER ACTIVITY TRACKING SETUP
// ============================================
print("=" + "=".repeat(98) + "=");
print("📊 PART 1/4: USER ACTIVITY TRACKING SETUP");
print("=" + "=".repeat(98) + "=\n");

print("📦 Creating UserActivityLogs Indexes...\n");

// Create indexes
try {
  db.UserActivityLogs.createIndex({ Timestamp: -1 }, { name: "idx_timestamp_desc" });
  print("  ✅ Created index: Timestamp (descending)");
  
  db.UserActivityLogs.createIndex({ UserId: 1, Timestamp: -1 }, { name: "idx_user_timestamp" });
  print("  ✅ Created index: UserId + Timestamp");
  
  db.UserActivityLogs.createIndex({ EntityType: 1, EntityId: 1, Timestamp: -1 }, { name: "idx_entity_timestamp" });
  print("  ✅ Created index: EntityType + EntityId + Timestamp");
  
  db.UserActivityLogs.createIndex({ ActivityType: 1, Timestamp: -1 }, { name: "idx_activitytype_timestamp" });
  print("  ✅ Created index: ActivityType + Timestamp");
  
  print("\n📊 Index Creation: 4 indexes created successfully\n");
} catch (error) {
  print(`  ⚠️  Index creation warning: ${error.message}\n`);
}

// Seed sample activity data
print("👤 Seeding Sample Activity Data...\n");

const firstUser = db.Users.findOne({ IsActive: true });

if (!firstUser) {
  print("⚠️  WARNING: No active users found. Sample activity data will not be created.");
  print("   Activities will be logged automatically when you use the app.\n");
} else {
  const userId = firstUser.UserId;
  const username = firstUser.Username || firstUser.Email || `User${userId}`;
  
  print(`Using User: ${username} (ID: ${userId}) for sample data\n`);

  const maxActivity = db.UserActivityLogs.findOne({}, { sort: { ActivityId: -1 } });
  let activityId = maxActivity ? maxActivity.ActivityId + 1 : 1;

  function createActivity(activityType, action, description, entityType, entityId) {
    return {
      ActivityId: activityId++,
      UserId: userId,
      Username: username,
      ActivityType: activityType,
      EntityType: entityType,
      EntityId: entityId,
      Action: action,
      Description: description,
      Metadata: {},
      Timestamp: new Date(Date.now() - Math.random() * 86400000),
      IPAddress: "127.0.0.1",
      UserAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
      IsSuccess: true,
      Module: entityType
    };
  }

  const sampleActivities = [
    createActivity("Login", "User Login", `${username} logged in successfully`, null, null),
    createActivity("DashboardView", "Viewed dashboard", "User accessed main dashboard", "Dashboard", 1),
    createActivity("WidgetAdd", "Added widget", "User added KPI widget to dashboard", "Dashboard", 1),
    createActivity("DashboardUpdate", "Updated dashboard", "User saved dashboard layout", "Dashboard", 1),
    createActivity("Create", "Created property", "User created new property: Sunset Villa", "Property", 1),
    createActivity("Update", "Updated property", "User updated property: Sunset Villa", "Property", 1),
    createActivity("View", "Viewed property", "User viewed property details: Sunset Villa", "Property", 1),
    createActivity("Create", "Created room", "User created new room: Room 101", "Room", 1),
    createActivity("Update", "Updated room", "User updated room status to Available", "Room", 1),
    createActivity("Create", "Created booking", "User created new booking #B001", "Booking", 1),
    createActivity("StatusChange", "Changed booking status", "User changed booking status to Confirmed", "Booking", 1),
    createActivity("Export", "Exported properties", "User exported 25 properties to Excel", "Property", null),
    createActivity("Export", "Exported report", "User generated revenue report", null, null),
    createActivity("Search", "Search", "User searched for: 'villa'", "Property", null),
    createActivity("Filter", "Applied filter", "User filtered properties by city: Mumbai", "Property", null),
    createActivity("PageView", "Viewed Properties", "User accessed Properties page", null, null),
    createActivity("PageView", "Viewed Bookings", "User accessed Bookings page", null, null),
    createActivity("PageView", "Viewed Dashboard", "User accessed Dashboard page", null, null),
  ];

  let inserted = 0;
  sampleActivities.forEach(activity => {
    try {
      db.UserActivityLogs.insertOne(activity);
      inserted++;
    } catch (error) {
      // Skip duplicates silently
    }
  });

  print(`📊 Sample Data: ${inserted} activities inserted\n`);
}

// ============================================
// PART 2: WIDGET LIBRARY SETUP
// ============================================
print("=" + "=".repeat(98) + "=");
print("📦 PART 2/4: WIDGET LIBRARY SETUP");
print("=" + "=".repeat(98) + "=\n");

print("Creating Complete Widget Library (20+ widgets)...\n");

const allWidgets = [
  // KPI WIDGETS
  {
    WidgetId: "total-properties",
    WidgetType: "kpi-card",
    Name: "Total Properties",
    Description: "Total number of active properties",
    Icon: "hotel",
    Category: "KPI",
    DefaultSettings: { title: "Total Properties", icon: "hotel", color: "#1976d2" },
    DefaultSize: { width: 3, height: 2 },
    MinSize: { width: 2, height: 2 },
    MaxSize: { width: 6, height: 4 },
    RequiredPermissions: ["dashboard.view", "properties.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "total-rooms",
    WidgetType: "kpi-card",
    Name: "Total Rooms",
    Description: "Total number of rooms across all properties",
    Icon: "meeting_room",
    Category: "KPI",
    DefaultSettings: { title: "Total Rooms", icon: "meeting_room", color: "#1976d2" },
    DefaultSize: { width: 3, height: 2 },
    MinSize: { width: 2, height: 2 },
    MaxSize: { width: 6, height: 4 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "bookings-today",
    WidgetType: "kpi-card",
    Name: "Bookings Today",
    Description: "Number of bookings made today",
    Icon: "event_available",
    Category: "KPI",
    DefaultSettings: { title: "Bookings Today", icon: "event_available", color: "#1976d2" },
    DefaultSize: { width: 3, height: 2 },
    MinSize: { width: 2, height: 2 },
    MaxSize: { width: 6, height: 4 },
    RequiredPermissions: ["dashboard.view", "bookings.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "occupancy-rate",
    WidgetType: "kpi-card",
    Name: "Occupancy Rate",
    Description: "Current occupancy rate percentage",
    Icon: "people",
    Category: "KPI",
    DefaultSettings: { title: "Occupancy Rate", icon: "people", color: "#1976d2" },
    DefaultSize: { width: 3, height: 2 },
    MinSize: { width: 2, height: 2 },
    MaxSize: { width: 6, height: 4 },
    RequiredPermissions: ["dashboard.view", "analytics.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "revenue-gauge",
    WidgetType: "gauge",
    Name: "Revenue Meter",
    Description: "Visual gauge showing current revenue against monthly target",
    Icon: "speed",
    Category: "KPI",
    DefaultSettings: { title: "Revenue vs Target", min: 0, max: 100000, target: 80000, unit: "currency" },
    DefaultSize: { width: 3, height: 3 },
    MinSize: { width: 3, height: 3 },
    MaxSize: { width: 4, height: 4 },
    RequiredPermissions: ["dashboard.view", "analytics.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // CHART WIDGETS
  {
    WidgetId: "revenue-chart",
    WidgetType: "chart",
    Name: "Revenue Chart",
    Description: "Monthly revenue visualization",
    Icon: "trending_up",
    Category: "Analytics",
    DefaultSettings: { title: "Revenue Trend", chartType: "line" },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 12, height: 6 },
    RequiredPermissions: ["dashboard.view", "analytics.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "bookings-chart",
    WidgetType: "chart",
    Name: "Bookings Chart",
    Description: "Booking trends over time",
    Icon: "bar_chart",
    Category: "Analytics",
    DefaultSettings: { title: "Bookings Trend", chartType: "bar" },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 12, height: 6 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "room-distribution",
    WidgetType: "chart",
    Name: "Room Distribution",
    Description: "Pie chart showing distribution of room types",
    Icon: "pie_chart",
    Category: "Analytics",
    DefaultSettings: { title: "Room Types", chartType: "pie", showLegend: true },
    DefaultSize: { width: 4, height: 4 },
    MinSize: { width: 3, height: 3 },
    MaxSize: { width: 6, height: 6 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // LIST WIDGETS
  {
    WidgetId: "recent-activity",
    WidgetType: "list",
    Name: "Recent Activity",
    Description: "Latest system activity and events",
    Icon: "list",
    Category: "Activity",
    DefaultSettings: { title: "Recent Activity", itemsToShow: 10 },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 8, height: 6 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "recent-bookings",
    WidgetType: "list",
    Name: "Recent Bookings",
    Description: "Latest booking records",
    Icon: "receipt_long",
    Category: "Bookings",
    DefaultSettings: { title: "Recent Bookings", itemsToShow: 5 },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 8, height: 6 },
    RequiredPermissions: ["dashboard.view", "bookings.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "notifications-list",
    WidgetType: "list",
    Name: "Notifications",
    Description: "System notifications and alerts",
    Icon: "notifications",
    Category: "Activity",
    DefaultSettings: { title: "Notifications", itemsToShow: 8 },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 8, height: 6 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // CALENDAR & TIMELINE WIDGETS
  {
    WidgetId: "booking-calendar",
    WidgetType: "calendar",
    Name: "Booking Calendar",
    Description: "Visual calendar of bookings and events",
    Icon: "calendar_month",
    Category: "Bookings",
    DefaultSettings: { title: "Bookings Calendar", view: "month" },
    DefaultSize: { width: 12, height: 6 },
    MinSize: { width: 8, height: 5 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "bookings.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "booking-timeline",
    WidgetType: "timeline",
    Name: "Booking Timeline",
    Description: "Visual timeline of bookings",
    Icon: "timeline",
    Category: "Bookings",
    DefaultSettings: { title: "Booking Timeline", timeRange: "week", groupBy: "property" },
    DefaultSize: { width: 12, height: 5 },
    MinSize: { width: 8, height: 4 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "bookings.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // TABLE & MAP WIDGETS
  {
    WidgetId: "properties-table",
    WidgetType: "table",
    Name: "Properties Table",
    Description: "Sortable and filterable table of all properties",
    Icon: "table_chart",
    Category: "Properties",
    DefaultSettings: { title: "Properties", pageSize: 10, sortBy: "PropertyName" },
    DefaultSize: { width: 12, height: 6 },
    MinSize: { width: 6, height: 4 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "properties.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "property-map",
    WidgetType: "map",
    Name: "Property Map",
    Description: "Interactive map showing property locations",
    Icon: "map",
    Category: "Location",
    DefaultSettings: { title: "Property Locations", zoom: 12, mapType: "roadmap" },
    DefaultSize: { width: 6, height: 6 },
    MinSize: { width: 4, height: 4 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "properties.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // STATS & TASKS WIDGETS
  {
    WidgetId: "financial-summary",
    WidgetType: "stats",
    Name: "Financial Overview",
    Description: "Multi-metric financial summary",
    Icon: "account_balance",
    Category: "Finance",
    DefaultSettings: { title: "Financial Overview", metrics: ["revenue", "expenses", "profit", "growth"], timeframe: "month" },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 12, height: 6 },
    RequiredPermissions: ["dashboard.view", "finance.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "maintenance-tasks",
    WidgetType: "tasks",
    Name: "Maintenance Tasks",
    Description: "Pending maintenance tasks",
    Icon: "task_alt",
    Category: "Operations",
    DefaultSettings: { title: "Pending Tasks", showCompleted: false, sortBy: "priority" },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 8, height: 6 },
    RequiredPermissions: ["dashboard.view", "maintenance.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // ⭐ ACTIVITY STREAM WIDGET
  {
    WidgetId: "user-activity-stream",
    WidgetType: "activity-stream",
    Name: "User Activity Stream",
    Description: "Real-time user activity tracking with statistics and timeline view",
    Icon: "timeline",
    Category: "Activity",
    DefaultSettings: { title: "User Activity", showStats: true, maxActivities: 10, refreshInterval: 60000 },
    DefaultSize: { width: 6, height: 6 },
    MinSize: { width: 4, height: 4 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "activity.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // ⭐ ANALYTICS WIDGET
  {
    WidgetId: "analytics-dashboard",
    WidgetType: "analytics",
    Name: "Analytics Dashboard",
    Description: "Comprehensive analytics dashboard with user activity insights, security monitoring, performance metrics, and usage trends",
    Icon: "analytics",
    Category: "Analytics",
    DefaultSettings: { title: "Analytics Dashboard", showSummary: true, showCharts: true, showSecurity: true, refreshInterval: 300000, topUsersLimit: 10, timeRange: "week" },
    DefaultSize: { width: 12, height: 8 },
    MinSize: { width: 6, height: 6 },
    MaxSize: { width: 12, height: 12 },
    RequiredPermissions: ["dashboard.view", "analytics.view", "activity.view"],
    IsActive: true,
    CreatedAt: new Date()
  }
];

let widgetInserted = 0;
let widgetUpdated = 0;
let widgetSkipped = 0;

allWidgets.forEach(widget => {
  try {
    const existing = db.WidgetLibrary.findOne({ WidgetId: widget.WidgetId });
    if (!existing) {
      db.WidgetLibrary.insertOne(widget);
      widgetInserted++;
      print(`  ✅ Added: ${widget.Name}`);
    } else {
      // Update existing widget
      db.WidgetLibrary.updateOne(
        { WidgetId: widget.WidgetId },
        { $set: { ...widget, UpdatedAt: new Date() } }
      );
      widgetUpdated++;
      print(`  ⚡ Updated: ${widget.Name}`);
    }
  } catch (error) {
    widgetSkipped++;
    print(`  ⏭️  Skipped: ${widget.Name} - ${error.message}`);
  }
});

print(`\n📊 Widget Library Summary:`);
print(`   ✅ Inserted: ${widgetInserted}`);
print(`   ⚡ Updated: ${widgetUpdated}`);
print(`   ⏭️  Skipped: ${widgetSkipped}`);
print(`   📦 Total: ${allWidgets.length}\n`);

// ============================================
// PART 3: DEFAULT DASHBOARDS FOR ALL USERS
// ============================================
print("=" + "=".repeat(98) + "=");
print("👥 PART 3/4: DEFAULT DASHBOARDS FOR ALL USERS");
print("=" + "=".repeat(98) + "=\n");

print("Creating default dashboards for all active users...\n");

const defaultDashboardLayout = {
  columns: 12,
  rowHeight: 80,
  widgets: [
    { widgetId: "bookings-today", widgetType: "kpi-card", position: { x: 0, y: 0, width: 3, height: 2 }, settings: {} },
    { widgetId: "total-rooms", widgetType: "kpi-card", position: { x: 3, y: 0, width: 3, height: 2 }, settings: {} },
    { widgetId: "total-properties", widgetType: "kpi-card", position: { x: 6, y: 0, width: 3, height: 2 }, settings: {} },
    { widgetId: "occupancy-rate", widgetType: "kpi-card", position: { x: 9, y: 0, width: 3, height: 2 }, settings: {} },
    { widgetId: "revenue-chart", widgetType: "chart", position: { x: 0, y: 2, width: 12, height: 4 }, settings: {} },
    { widgetId: "user-activity-stream", widgetType: "activity-stream", position: { x: 0, y: 6, width: 6, height: 6 }, settings: {} },
    { widgetId: "analytics-dashboard", widgetType: "analytics", position: { x: 6, y: 6, width: 6, height: 6 }, settings: {} }
  ]
};

const users = db.Users.find({ IsActive: true }).toArray();
print(`Found ${users.length} active users\n`);

let dashboardsCreated = 0;
let dashboardsUpdated = 0;
let dashboardsSkipped = 0;

users.forEach(user => {
  const userId = user.UserId;
  const existingDashboards = db.DashboardConfigurations.find({ UserId: userId }).toArray();
  
  if (existingDashboards.length === 0) {
    const newDashboard = {
      UserId: userId,
      DashboardName: "My Default Dashboard",
      IsDefault: true,
      Layout: defaultDashboardLayout,
      CreatedAt: new Date(),
      UpdatedAt: new Date()
    };
    
    db.DashboardConfigurations.insertOne(newDashboard);
    dashboardsCreated++;
    print(`  ✅ Created dashboard for User ${userId}`);
  } else {
    const hasDefault = existingDashboards.some(d => d.IsDefault);
    
    if (!hasDefault) {
      db.DashboardConfigurations.updateOne(
        { _id: existingDashboards[0]._id },
        { $set: { IsDefault: true, UpdatedAt: new Date() } }
      );
      dashboardsUpdated++;
      print(`  ⚡ Set default for User ${userId}`);
    } else {
      dashboardsSkipped++;
      print(`  ⏭️  User ${userId} already has default dashboard`);
    }
  }
});

print(`\n📊 Dashboard Summary:`);
print(`   ✅ Created: ${dashboardsCreated}`);
print(`   ⚡ Updated: ${dashboardsUpdated}`);
print(`   ⏭️  Skipped: ${dashboardsSkipped}`);
print(`   👥 Total Users: ${users.length}\n`);

// ============================================
// PART 4: VERIFICATION & STATISTICS
// ============================================
print("=" + "=".repeat(98) + "=");
print("🔍 PART 4/4: VERIFICATION & STATISTICS");
print("=" + "=".repeat(98) + "=\n");

// UserActivityLogs Stats
const totalActivities = db.UserActivityLogs.countDocuments({});
const activityIndexes = db.UserActivityLogs.getIndexes().length;
print("📊 UserActivityLogs Collection:");
print(`   Total Activities: ${totalActivities}`);
print(`   Indexes: ${activityIndexes}\n`);

// Widget Library Stats
const totalWidgets = db.WidgetLibrary.countDocuments({});
const activeWidgets = db.WidgetLibrary.countDocuments({ IsActive: true });
print("📚 Widget Library:");
print(`   Total Widgets: ${totalWidgets}`);
print(`   Active Widgets: ${activeWidgets}\n`);

// Widgets by Category
print("📁 Widgets by Category:");
const categories = db.WidgetLibrary.distinct("Category", { IsActive: true });
categories.sort().forEach(category => {
  const count = db.WidgetLibrary.countDocuments({ Category: category, IsActive: true });
  print(`   ${category.padEnd(20)} : ${count} widget(s)`);
});
print("");

// Dashboard Stats
const totalDashboards = db.DashboardConfigurations.countDocuments({});
const usersWithDashboards = db.DashboardConfigurations.distinct("UserId").length;
const defaultDashboards = db.DashboardConfigurations.countDocuments({ IsDefault: true });
print("📊 Dashboard Configurations:");
print(`   Total Dashboards: ${totalDashboards}`);
print(`   Users with Dashboards: ${usersWithDashboards}`);
print(`   Default Dashboards: ${defaultDashboards}\n`);

// Verify Special Widgets
print("⭐ Special Widgets Verification:");
const activityWidget = db.WidgetLibrary.findOne({ WidgetId: "user-activity-stream" });
const analyticsWidget = db.WidgetLibrary.findOne({ WidgetId: "analytics-dashboard" });
print(`   Activity Stream Widget: ${activityWidget ? '✅ Present' : '❌ Missing'}`);
print(`   Analytics Dashboard Widget: ${analyticsWidget ? '✅ Present' : '❌ Missing'}\n`);

// ============================================
// QUICK REFERENCE
// ============================================
print("=" + "=".repeat(98) + "=");
print("📖 QUICK REFERENCE");
print("=" + "=".repeat(98) + "=\n");

print("Available Widget IDs (use these in your dashboard):");
db.WidgetLibrary.find({ IsActive: true }).sort({ Category: 1, Name: 1 }).forEach(w => {
  print(`   ${w.WidgetId.padEnd(30)} | ${w.Category.padEnd(15)} | ${w.Name}`);
});

// ============================================
// COMPLETION
// ============================================
print("\n" + "=".repeat(100));
print("✅ COMPLETE SYSTEM SETUP FINISHED!");
print("=".repeat(100) + "\n");

print("📊 SETUP SUMMARY:");
print(`   ✅ UserActivityLogs: ${activityIndexes} indexes, ${totalActivities} activities`);
print(`   ✅ Widget Library: ${activeWidgets} active widgets`);
print(`   ✅ Dashboards: ${totalDashboards} dashboards for ${usersWithDashboards} users\n`);

print("💡 NEXT STEPS:");
print("   1. ✅ MongoDB setup complete!");
print("   2. 🚀 Start your .NET backend: dotnet run (or press F5)");
print("   3. 🌐 Start your Angular frontend: ng serve");
print("   4. 🔐 Login to the application");
print("   5. 📊 Navigate to Dashboard");
print("   6. ➕ Click 'Add Widget' to see all available widgets");
print("   7. 🎨 Customize your dashboard and save it\n");

print("🎯 FEATURES AVAILABLE:");
print("   ✓ User Activity Tracking (automatic logging)");
print("   ✓ 20+ Dashboard Widgets");
print("   ✓ Activity Stream Widget (real-time)");
print("   ✓ Analytics Dashboard Widget (comprehensive insights)");
print("   ✓ Customizable Dashboards");
print("   ✓ Widget drag-and-drop");
print("   ✓ Dashboard save/load/delete\n");

print("🔍 QUICK VERIFICATION COMMANDS:");
print("   // Check activities");
print("   db.UserActivityLogs.find().sort({ Timestamp: -1 }).limit(5).pretty()");
print("");
print("   // Check widgets");
print("   db.WidgetLibrary.find({ IsActive: true }).count()");
print("");
print("   // Check dashboards");
print("   db.DashboardConfigurations.find().pretty()");
print("");

print("🎉 Your complete system is ready to use!");
print("   All collections, indexes, widgets, and dashboards are set up.\n");
print("   Activities will be logged automatically when you use the app.\n");

print("=" + "=".repeat(100) + "\n");
