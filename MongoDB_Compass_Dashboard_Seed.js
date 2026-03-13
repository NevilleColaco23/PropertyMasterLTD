// ============================================
// MongoDB Compass Executable Script
// Dashboard Customization - Seed Data
// ============================================
// HOW TO RUN:
// 1. Open MongoDB Compass
// 2. Connect to your database
// 3. Select your database (e.g., "PropertyMaster")
// 4. Click "_MONGOSH" tab at the bottom
// 5. Copy and paste this ENTIRE file
// 6. Press Enter
// ============================================

print("\n🚀 Starting Dashboard Customization Setup...\n");

// ============================================
// STEP 1: Create Indexes
// ============================================
print("📊 Creating indexes...");

// DashboardConfigurations indexes
db.DashboardConfigurations.createIndex({ "UserId": 1, "IsDefault": 1 });
db.DashboardConfigurations.createIndex({ "UserId": 1, "DashboardName": 1 });
print("  ✅ DashboardConfigurations indexes created");

// WidgetLibrary indexes
db.WidgetLibrary.createIndex({ "WidgetId": 1 }, { unique: true });
db.WidgetLibrary.createIndex({ "Category": 1, "IsActive": 1 });
print("  ✅ WidgetLibrary indexes created");

// DashboardTemplates indexes
db.DashboardTemplates.createIndex({ "RoleId": 1 });
db.DashboardTemplates.createIndex({ "IsPublic": 1 });
print("  ✅ DashboardTemplates indexes created\n");

// ============================================
// STEP 2: Clear Existing Widget Data (Optional)
// ============================================
print("🗑️  Clearing existing widget library (if any)...");
const deleteResult = db.WidgetLibrary.deleteMany({});
print(`  ✅ Removed ${deleteResult.deletedCount} existing widgets\n`);

// ============================================
// STEP 3: Insert Widget Library
// ============================================
print("📦 Inserting widget library...");

const widgets = [
  {
    WidgetId: "total-properties",
    WidgetType: "kpi-card",
    Name: "Total Properties",
    Description: "Displays total active properties",
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
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "total-rooms",
    WidgetType: "kpi-card",
    Name: "Total Rooms",
    Description: "Displays total available rooms",
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
    Description: "Check-ins and check-outs for today",
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
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  },
  {
    WidgetId: "occupancy-rate",
    WidgetType: "kpi-card",
    Name: "Occupancy Rate",
    Description: "Current occupancy percentage",
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
    RequiredPermissions: ["dashboard.view"],
    IsActive: true,
    CreatedAt: new Date()
  }
];

const insertResult = db.WidgetLibrary.insertMany(widgets);
print(`  ✅ Inserted ${Object.keys(insertResult.insertedIds).length} widgets\n`);

// ============================================
// STEP 4: Verification
// ============================================
print("🔍 Verifying setup...\n");

const widgetCount = db.WidgetLibrary.countDocuments();
print(`  📊 Widget Library: ${widgetCount} widgets`);

const dashboardCount = db.DashboardConfigurations.countDocuments();
print(`  📊 User Dashboards: ${dashboardCount} dashboards`);

const templateCount = db.DashboardTemplates.countDocuments();
print(`  📊 Templates: ${templateCount} templates\n`);

// ============================================
// STEP 5: Display Sample Widget
// ============================================
print("📋 Sample Widget:");
print("─────────────────────────────────────────");
const sampleWidget = db.WidgetLibrary.findOne({ WidgetId: "total-properties" });
printjson(sampleWidget);
print("─────────────────────────────────────────\n");

// ============================================
// STEP 6: Next Steps
// ============================================
print("✅ Dashboard customization setup complete!\n");
print("📝 Next Steps:");
print("  1. Test API: GET /api/v1/dashboard/widgets");
print("  2. Create dashboard via API or use Angular UI");
print("  3. Check QUICK_START_DASHBOARD_API.md for testing guide\n");

print("🎉 You're ready to customize dashboards!");
