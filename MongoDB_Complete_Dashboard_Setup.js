// ============================================
// COMPLETE DASHBOARD SETUP SCRIPT
// Run this in MongoDB Compass MongoSH tab
// ============================================
// HOW TO RUN:
// 1. In MongoSH tab, first type: use ListingDB
// 2. Press Enter
// 3. Then paste this ENTIRE script below
// ============================================
// This script will:
// 1. Seed all widget types (18 widgets)
// 2. Create default dashboards for all users
// 3. Verify and display results
// ============================================

// Switch to ListingDB database (safe for copy-paste)
db = db.getSiblingDB('ListingDB');

print("\n" + "=".repeat(80));
print("🚀 COMPLETE DASHBOARD SETUP - STARTING...");
print("=".repeat(80) + "\n");

// ============================================
// STEP 1: SEED WIDGET LIBRARY
// ============================================
print("📦 STEP 1: Seeding Widget Library...\n");

const allWidgets = [
  // ===== KPI WIDGETS =====
  {
    WidgetId: "total-properties",
    WidgetType: "kpi-card",
    Name: "Total Properties",
    Description: "Total number of active properties",
    Icon: "hotel",
    Category: "KPI",
    DefaultSettings: {
      title: "Total Properties",
      icon: "hotel",
      color: "#1976d2"
    },
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
    DefaultSettings: {
      title: "Total Rooms",
      icon: "meeting_room",
      color: "#1976d2"
    },
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
    DefaultSettings: {
      title: "Bookings Today",
      icon: "event_available",
      color: "#1976d2"
    },
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
    DefaultSettings: {
      title: "Occupancy Rate",
      icon: "people",
      color: "#1976d2"
    },
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
    DefaultSettings: {
      title: "Revenue vs Target",
      min: 0,
      max: 100000,
      target: 80000,
      unit: "currency"
    },
    DefaultSize: { width: 3, height: 3 },
    MinSize: { width: 3, height: 3 },
    MaxSize: { width: 4, height: 4 },
    RequiredPermissions: ["dashboard.view", "analytics.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // ===== CHART WIDGETS =====
  {
    WidgetId: "revenue-chart",
    WidgetType: "chart",
    Name: "Revenue Chart",
    Description: "Monthly revenue visualization",
    Icon: "trending_up",
    Category: "Analytics",
    DefaultSettings: {
      title: "Revenue Trend",
      chartType: "line"
    },
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
    DefaultSettings: {
      title: "Bookings Trend",
      chartType: "bar"
    },
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
    DefaultSettings: {
      title: "Room Types",
      chartType: "pie",
      showLegend: true
    },
    DefaultSize: { width: 4, height: 4 },
    MinSize: { width: 3, height: 3 },
    MaxSize: { width: 6, height: 6 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // ===== LIST WIDGETS =====
  {
    WidgetId: "recent-activity",
    WidgetType: "list",
    Name: "Recent Activity",
    Description: "Latest system activity and events",
    Icon: "list",
    Category: "Activity",
    DefaultSettings: {
      title: "Recent Activity",
      itemsToShow: 10
    },
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
    DefaultSettings: {
      title: "Recent Bookings",
      itemsToShow: 5
    },
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
    DefaultSettings: {
      title: "Notifications",
      itemsToShow: 8
    },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 8, height: 6 },
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // ===== CALENDAR WIDGET =====
  {
    WidgetId: "booking-calendar",
    WidgetType: "calendar",
    Name: "Booking Calendar",
    Description: "Visual calendar of bookings and events",
    Icon: "calendar_month",
    Category: "Bookings",
    DefaultSettings: {
      title: "Bookings Calendar",
      view: "month"
    },
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
    DefaultSettings: {
      title: "Booking Timeline",
      timeRange: "week",
      groupBy: "property"
    },
    DefaultSize: { width: 12, height: 5 },
    MinSize: { width: 8, height: 4 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "bookings.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // ===== TABLE WIDGET =====
  {
    WidgetId: "properties-table",
    WidgetType: "table",
    Name: "Properties Table",
    Description: "Sortable and filterable table of all properties",
    Icon: "table_chart",
    Category: "Properties",
    DefaultSettings: {
      title: "Properties",
      pageSize: 10,
      sortBy: "PropertyName"
    },
    DefaultSize: { width: 12, height: 6 },
    MinSize: { width: 6, height: 4 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "properties.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // ===== MAP WIDGET =====
  {
    WidgetId: "property-map",
    WidgetType: "map",
    Name: "Property Map",
    Description: "Interactive map showing property locations",
    Icon: "map",
    Category: "Location",
    DefaultSettings: {
      title: "Property Locations",
      zoom: 12,
      mapType: "roadmap"
    },
    DefaultSize: { width: 6, height: 6 },
    MinSize: { width: 4, height: 4 },
    MaxSize: { width: 12, height: 8 },
    RequiredPermissions: ["dashboard.view", "properties.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // ===== STATS WIDGET =====
  {
    WidgetId: "financial-summary",
    WidgetType: "stats",
    Name: "Financial Overview",
    Description: "Multi-metric financial summary",
    Icon: "account_balance",
    Category: "Finance",
    DefaultSettings: {
      title: "Financial Overview",
      metrics: ["revenue", "expenses", "profit", "growth"],
      timeframe: "month"
    },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 12, height: 6 },
    RequiredPermissions: ["dashboard.view", "finance.view"],
    IsActive: true,
    CreatedAt: new Date()
  },

  // ===== TASKS WIDGET =====
  {
    WidgetId: "maintenance-tasks",
    WidgetType: "tasks",
    Name: "Maintenance Tasks",
    Description: "Pending maintenance tasks",
    Icon: "task_alt",
    Category: "Operations",
    DefaultSettings: {
      title: "Pending Tasks",
      showCompleted: false,
      sortBy: "priority"
    },
    DefaultSize: { width: 6, height: 4 },
    MinSize: { width: 4, height: 3 },
    MaxSize: { width: 8, height: 6 },
    RequiredPermissions: ["dashboard.view", "maintenance.view"],
    IsActive: true,
    CreatedAt: new Date()
  }
];

// Insert widgets
let widgetInserted = 0;
let widgetSkipped = 0;
let widgetErrors = [];

allWidgets.forEach(widget => {
  try {
    const existing = db.WidgetLibrary.findOne({ WidgetId: widget.WidgetId });
    if (!existing) {
      db.WidgetLibrary.insertOne(widget);
      widgetInserted++;
      print(`  ✅ Added: ${widget.Name}`);
    } else {
      widgetSkipped++;
      print(`  ⏭️  Exists: ${widget.Name}`);
    }
  } catch (error) {
    widgetErrors.push({ widget: widget.Name, error: error.message });
    print(`  ❌ Error: ${widget.Name} - ${error.message}`);
  }
});

print(`\n📊 Widget Library Summary:`);
print(`   ✅ Inserted: ${widgetInserted}`);
print(`   ⏭️  Skipped: ${widgetSkipped}`);
print(`   ❌ Errors: ${widgetErrors.length}`);
print(`   📦 Total: ${allWidgets.length}\n`);

// ============================================
// STEP 2: CREATE DEFAULT DASHBOARDS
// ============================================
print("=".repeat(80));
print("👤 STEP 2: Creating Default Dashboards for Users...\n");

const defaultDashboardLayout = {
  columns: 12,
  rowHeight: 80,
  widgets: [
    // Row 1: KPI Cards
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
      settings: {}
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
      settings: {}
    },
    // Row 2: Revenue Chart
    {
      widgetId: "revenue-chart",
      widgetType: "chart",
      position: { x: 0, y: 2, width: 12, height: 4 },
      settings: {}
    },
    // Row 3: Recent Activity & Calendar
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

// Get all users
const users = db.Users.find({ IsActive: true }).toArray();
print(`Found ${users.length} active users\n`);

let dashboardsCreated = 0;
let dashboardsUpdated = 0;
let dashboardsSkipped = 0;

users.forEach(user => {
  const userId = user.UserId;
  const existingDashboards = db.DashboardConfigurations.find({ UserId: userId }).toArray();
  
  if (existingDashboards.length === 0) {
    // User has no dashboards - create default
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
    // User has dashboards - check if default exists
    const hasDefault = existingDashboards.some(d => d.IsDefault);
    
    if (!hasDefault) {
      // Set first dashboard as default
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

print(`\n📊 Dashboard Creation Summary:`);
print(`   ✅ Created: ${dashboardsCreated}`);
print(`   ⚡ Updated: ${dashboardsUpdated}`);
print(`   ⏭️  Skipped: ${dashboardsSkipped}`);
print(`   👥 Total Users: ${users.length}\n`);

// ============================================
// STEP 3: VERIFICATION & STATISTICS
// ============================================
print("=".repeat(80));
print("🔍 STEP 3: Verification & Statistics\n");

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

// Widgets by Type
print("🎨 Widgets by Type:");
const types = db.WidgetLibrary.aggregate([
  { $match: { IsActive: true } },
  { $group: { _id: "$WidgetType", count: { $sum: 1 } } },
  { $sort: { count: -1 } }
]).toArray();
types.forEach(type => {
  print(`   ${type._id.padEnd(20)} : ${type.count} widget(s)`);
});
print("");

// Dashboard Stats
const totalDashboards = db.DashboardConfigurations.countDocuments({});
const usersWithDashboards = db.DashboardConfigurations.distinct("UserId").length;
const defaultDashboards = db.DashboardConfigurations.countDocuments({ IsDefault: true });

print("📊 Dashboard Statistics:");
print(`   Total Dashboards: ${totalDashboards}`);
print(`   Users with Dashboards: ${usersWithDashboards}`);
print(`   Default Dashboards: ${defaultDashboards}\n`);

// Sample Dashboard
print("📋 Sample Dashboard Configuration:");
const sampleDashboard = db.DashboardConfigurations.findOne({});
if (sampleDashboard) {
  print(`   User ID: ${sampleDashboard.UserId}`);
  print(`   Name: ${sampleDashboard.DashboardName}`);
  print(`   Is Default: ${sampleDashboard.IsDefault}`);
  print(`   Widget Count: ${sampleDashboard.Layout.widgets.length}`);
  print(`   Widgets:`);
  sampleDashboard.Layout.widgets.forEach(w => {
    const widgetInfo = db.WidgetLibrary.findOne({ WidgetId: w.widgetId });
    const name = widgetInfo ? widgetInfo.Name : w.widgetId;
    print(`      - ${name} (${w.widgetType}) at [${w.position.x},${w.position.y}] size ${w.position.width}x${w.position.height}`);
  });
}
print("");

// ============================================
// STEP 4: QUICK REFERENCE
// ============================================
print("=".repeat(80));
print("📖 QUICK REFERENCE\n");
print("Available Widget IDs:");
db.WidgetLibrary.find({ IsActive: true }).sort({ Category: 1, Name: 1 }).forEach(w => {
  print(`   ${w.WidgetId.padEnd(30)} | ${w.Category.padEnd(15)} | ${w.Name}`);
});

print("\n" + "=".repeat(80));
print("✅ SETUP COMPLETE!");
print("=".repeat(80));
print("\n💡 Next Steps:");
print("   1. Restart your .NET backend API");
print("   2. Restart your Angular frontend");
print("   3. Login and navigate to Dashboard");
print("   4. Click 'Add Widget' to see all available widgets");
print("   5. Customize your dashboard and save it\n");
print("🎉 Your dashboard system is ready to use!\n");
