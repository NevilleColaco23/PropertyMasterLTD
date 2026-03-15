// ============================================
// ADD ANALYTICS WIDGET TO WIDGET LIBRARY
// MongoDB Compass MongoSH Script
// ============================================
// HOW TO RUN:
// 1. Open MongoDB Compass
// 2. Connect to your MongoDB instance
// 3. Click on "MongoSH" tab at the bottom
// 4. Type: use ListingDB
// 5. Press Enter
// 6. Paste this entire script and press Enter
// ============================================

// Switch to ListingDB database
db = db.getSiblingDB('ListingDB');

print("\n" + "=".repeat(80));
print("📊 ADDING ANALYTICS WIDGET TO WIDGET LIBRARY");
print("=".repeat(80) + "\n");

// ============================================
// ANALYTICS WIDGET DEFINITION
// ============================================

const analyticsWidget = {
  WidgetId: "analytics-dashboard",
  WidgetType: "analytics",
  Name: "Analytics Dashboard",
  Description: "Comprehensive analytics dashboard with user activity insights, security monitoring, performance metrics, and usage trends",
  Icon: "analytics",
  Category: "Analytics",
  DefaultSettings: {
    title: "Analytics Dashboard",
    showSummary: true,
    showCharts: true,
    showSecurity: true,
    refreshInterval: 300000, // 5 minutes
    topUsersLimit: 10,
    timeRange: "week"
  },
  DefaultSize: { width: 12, height: 8 },
  MinSize: { width: 6, height: 6 },
  MaxSize: { width: 12, height: 12 },
  RequiredPermissions: ["dashboard.view", "analytics.view", "activity.view"],
  IsActive: true,
  CreatedAt: new Date()
};

// ============================================
// INSERT OR UPDATE WIDGET
// ============================================

print("📦 Checking if Analytics Widget exists...\n");

const existingWidget = db.WidgetLibrary.findOne({ WidgetId: "analytics-dashboard" });

if (existingWidget) {
  print("⚠️  Analytics Widget already exists!");
  print(`   Current Name: ${existingWidget.Name}`);
  print(`   Current Category: ${existingWidget.Category}`);
  print(`   Created: ${existingWidget.CreatedAt}\n`);
  
  print("🔄 Updating widget with latest configuration...\n");
  
  const updateResult = db.WidgetLibrary.updateOne(
    { WidgetId: "analytics-dashboard" },
    {
      $set: {
        Name: analyticsWidget.Name,
        Description: analyticsWidget.Description,
        Icon: analyticsWidget.Icon,
        Category: analyticsWidget.Category,
        DefaultSettings: analyticsWidget.DefaultSettings,
        DefaultSize: analyticsWidget.DefaultSize,
        MinSize: analyticsWidget.MinSize,
        MaxSize: analyticsWidget.MaxSize,
        RequiredPermissions: analyticsWidget.RequiredPermissions,
        IsActive: analyticsWidget.IsActive,
        UpdatedAt: new Date()
      }
    }
  );
  
  if (updateResult.modifiedCount > 0) {
    print("✅ Analytics Widget UPDATED successfully!\n");
  } else {
    print("⏭️  Analytics Widget already up-to-date (no changes made)\n");
  }
} else {
  print("➕ Analytics Widget not found. Creating new widget...\n");
  
  const insertResult = db.WidgetLibrary.insertOne(analyticsWidget);
  
  if (insertResult.acknowledged) {
    print("✅ Analytics Widget CREATED successfully!\n");
  } else {
    print("❌ Failed to create Analytics Widget\n");
  }
}

// ============================================
// VERIFY WIDGET
// ============================================

print("=".repeat(80));
print("🔍 VERIFICATION");
print("=".repeat(80) + "\n");

const verifyWidget = db.WidgetLibrary.findOne({ WidgetId: "analytics-dashboard" });

if (verifyWidget) {
  print("✅ Analytics Widget is now in the Widget Library!\n");
  print("📋 Widget Details:");
  print(`   Widget ID: ${verifyWidget.WidgetId}`);
  print(`   Name: ${verifyWidget.Name}`);
  print(`   Type: ${verifyWidget.WidgetType}`);
  print(`   Category: ${verifyWidget.Category}`);
  print(`   Icon: ${verifyWidget.Icon}`);
  print(`   Default Size: ${verifyWidget.DefaultSize.width}x${verifyWidget.DefaultSize.height}`);
  print(`   Min Size: ${verifyWidget.MinSize.width}x${verifyWidget.MinSize.height}`);
  print(`   Max Size: ${verifyWidget.MaxSize.width}x${verifyWidget.MaxSize.height}`);
  print(`   Is Active: ${verifyWidget.IsActive}`);
  print(`   Required Permissions: ${verifyWidget.RequiredPermissions.join(', ')}\n`);
  
  print("📊 Default Settings:");
  print(`   Title: ${verifyWidget.DefaultSettings.title}`);
  print(`   Show Summary: ${verifyWidget.DefaultSettings.showSummary}`);
  print(`   Show Charts: ${verifyWidget.DefaultSettings.showCharts}`);
  print(`   Show Security: ${verifyWidget.DefaultSettings.showSecurity}`);
  print(`   Refresh Interval: ${verifyWidget.DefaultSettings.refreshInterval}ms (${verifyWidget.DefaultSettings.refreshInterval / 60000} min)`);
  print(`   Top Users Limit: ${verifyWidget.DefaultSettings.topUsersLimit}`);
  print(`   Time Range: ${verifyWidget.DefaultSettings.timeRange}\n`);
  
  print("📝 Description:");
  print(`   ${verifyWidget.Description}\n`);
} else {
  print("❌ ERROR: Analytics Widget not found in Widget Library!\n");
}

// ============================================
// WIDGET LIBRARY STATISTICS
// ============================================

print("=".repeat(80));
print("📊 WIDGET LIBRARY STATISTICS");
print("=".repeat(80) + "\n");

const totalWidgets = db.WidgetLibrary.countDocuments({});
const activeWidgets = db.WidgetLibrary.countDocuments({ IsActive: true });
const analyticsWidgets = db.WidgetLibrary.countDocuments({ Category: "Analytics", IsActive: true });

print(`Total Widgets in Library: ${totalWidgets}`);
print(`Active Widgets: ${activeWidgets}`);
print(`Analytics Category Widgets: ${analyticsWidgets}\n`);

// Show all Analytics category widgets
print("📈 Analytics Widgets:");
db.WidgetLibrary.find({ Category: "Analytics", IsActive: true }).forEach(w => {
  print(`   • ${w.Name} (${w.WidgetId})`);
});
print("");

// ============================================
// NEXT STEPS
// ============================================

print("=".repeat(80));
print("✅ SETUP COMPLETE!");
print("=".repeat(80) + "\n");

print("💡 Next Steps:\n");
print("1. Restart your Angular frontend (ng serve)");
print("2. Login to the application");
print("3. Navigate to Dashboard");
print("4. Click 'Add Widget' button");
print("5. Find 'Analytics Dashboard' widget in the Analytics category");
print("6. Click 'Add to Dashboard'");
print("7. The widget will load with:");
print("   • Activity summary KPI cards");
print("   • Top active users leaderboard");
print("   • Activity distribution breakdown");
print("   • Peak usage times chart");
print("   • Daily trends visualization");
print("   • Security alerts panel");
print("   • Performance metrics table\n");

print("⚙️  Widget Features:");
print("   ✓ Time range filter (Today/Week/Month)");
print("   ✓ Auto-refresh every 5 minutes");
print("   ✓ Export data to CSV");
print("   ✓ Multiple tabs: Overview, Usage Patterns, Security, Performance");
print("   ✓ Beautiful Material Design UI with gradients");
print("   ✓ Responsive grid layout");
print("   ✓ Interactive charts and visualizations\n");

print("🔒 Required Permissions:");
print("   • dashboard.view");
print("   • analytics.view");
print("   • activity.view\n");

print("🎨 Customization Options:");
print("   • title: Widget title");
print("   • showSummary: Show/hide summary KPI cards");
print("   • showCharts: Show/hide charts");
print("   • showSecurity: Show/hide security tab");
print("   • refreshInterval: Auto-refresh interval in milliseconds");
print("   • topUsersLimit: Number of top users to display");
print("   • timeRange: Default time range (today/week/month)\n");

print("🎉 Your Analytics Dashboard Widget is ready to use!\n");
