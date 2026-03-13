// ============================================
// MongoDB Compass Executable Script
// Phase 2: Add Additional Widget Types
// ============================================
// HOW TO RUN:
// 1. Open MongoDB Compass
// 2. Connect to your database
// 3. Select your database (e.g., "PropertyMaster")
// 4. Click "_MONGOSH" tab at the bottom
// 5. Copy and paste this ENTIRE file
// 6. Press Enter
// ============================================

print("\n🎨 Phase 2: Adding Additional Widget Types...\n");

// ============================================
// Insert Additional Widgets
// ============================================
print("📦 Inserting new widgets...");

const newWidgets = [
  // Chart Widgets
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
  
  // Calendar Widget
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
  
  // List Widgets
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
  }
];

// Insert new widgets (skip if already exists)
let insertedCount = 0;
newWidgets.forEach(widget => {
  const existing = db.WidgetLibrary.findOne({ WidgetId: widget.WidgetId });
  if (!existing) {
    db.WidgetLibrary.insertOne(widget);
    insertedCount++;
    print(`  ✅ Added: ${widget.Name}`);
  } else {
    print(`  ⏭️  Skipped (exists): ${widget.Name}`);
  }
});

print(`\n📊 Inserted ${insertedCount} new widgets\n`);

// ============================================
// Verification
// ============================================
print("🔍 Verifying widget library...\n");

const totalWidgets = db.WidgetLibrary.countDocuments();
print(`  📊 Total Widgets: ${totalWidgets}`);

// Count by category
const kpiCount = db.WidgetLibrary.countDocuments({ Category: "KPI" });
const analyticsCount = db.WidgetLibrary.countDocuments({ Category: "Analytics" });
const bookingsCount = db.WidgetLibrary.countDocuments({ Category: "Bookings" });
const activityCount = db.WidgetLibrary.countDocuments({ Category: "Activity" });

print(`  📊 KPI Widgets: ${kpiCount}`);
print(`  📊 Analytics Widgets: ${analyticsCount}`);
print(`  📊 Bookings Widgets: ${bookingsCount}`);
print(`  📊 Activity Widgets: ${activityCount}`);

// Count by type
const kpiCardCount = db.WidgetLibrary.countDocuments({ WidgetType: "kpi-card" });
const chartCount = db.WidgetLibrary.countDocuments({ WidgetType: "chart" });
const calendarCount = db.WidgetLibrary.countDocuments({ WidgetType: "calendar" });
const listCount = db.WidgetLibrary.countDocuments({ WidgetType: "list" });

print(`\n  📊 By Type:`);
print(`    • KPI Cards: ${kpiCardCount}`);
print(`    • Charts: ${chartCount}`);
print(`    • Calendars: ${calendarCount}`);
print(`    • Lists: ${listCount}`);

// ============================================
// Display All Widgets
// ============================================
print("\n📋 Complete Widget Library:");
print("─────────────────────────────────────────");
db.WidgetLibrary.find().forEach(widget => {
  print(`\n  ${widget.Icon} ${widget.Name}`);
  print(`    Type: ${widget.WidgetType} | Category: ${widget.Category}`);
  print(`    ID: ${widget.WidgetId}`);
});
print("─────────────────────────────────────────\n");

// ============================================
// Success
// ============================================
print("✅ Phase 2 widget library update complete!\n");
print("📝 Summary:");
print(`  • Total widgets now: ${totalWidgets}`);
print(`  • New widgets added: ${insertedCount}`);
print(`  • Widget types: kpi-card, chart, calendar, list\n`);

print("🚀 Ready to test! Refresh your Angular dashboard to see new widgets.");
