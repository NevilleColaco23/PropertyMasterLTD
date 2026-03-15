# Quick Testing Guide - Save Dashboard Feature

## 🚀 **Start Testing in 3 Steps**

### **1. Start the Application**
```bash
# Terminal 1 - Start .NET API
cd WebApi
dotnet run

# Terminal 2 - Start Angular
cd app
ng serve
```

### **2. Navigate to Dashboard**
```
http://localhost:4200/propertyLanding/dashboard1
```

### **3. Test Scenarios**

## 📝 **Test Scenario 1: First Time User**

**Expected**: No saved dashboards, shows default widgets

1. ✅ Dashboard loads with default widgets (KPIs, Chart, List, Calendar)
2. ✅ Dropdown shows "Create New Dashboard"
3. ✅ Click "Customize" → Edit mode activates
4. ✅ Move/resize widgets
5. ✅ Click "Save As"
6. ✅ Dialog opens: Enter "My First Dashboard"
7. ✅ Check "Set as default"
8. ✅ Click "Save Dashboard"
9. ✅ Dashboard appears in dropdown with ⭐ icon and "Default" badge

**Screenshot Expected**:
```
┌────────────────────────────────────────────┐
│ Dashboard                                   │
│ ┌──────────────────────────────────────┐   │
│ │ ⭐ My First Dashboard [Default]       │   │
│ │ ➕ Create New Dashboard               │   │
│ └──────────────────────────────────────┘   │
└────────────────────────────────────────────┘
```

---

## 📝 **Test Scenario 2: Create Multiple Dashboards**

**Goal**: Create 3 different dashboards with different layouts

### **Dashboard 1: Executive Dashboard**
1. Click "Create New Dashboard"
2. Add only KPI cards
3. Remove chart and calendar
4. Save As "Executive Dashboard"
5. Don't check "Set as default"

### **Dashboard 2: Analytics Dashboard**
1. Click "Create New Dashboard"
2. Add chart widget full width
3. Add 2 KPI cards
4. Save As "Analytics Dashboard"

### **Dashboard 3: Weekly Report**
1. Click "Create New Dashboard"
2. Add calendar and list widgets only
3. Save As "Weekly Report"
4. Check "Set as default" ✅

**Expected Dropdown**:
```
┌────────────────────────────────────────┐
│ Dashboard                       ▼      │
├────────────────────────────────────────┤
│ ➕ Create New Dashboard                │
│ ────────────────────────────────────── │
│ ⭐ Weekly Report [Default]             │
│ 📊 Analytics Dashboard                 │
│ 📊 Executive Dashboard                 │
└────────────────────────────────────────┘
```

---

## 📝 **Test Scenario 3: Switch Between Dashboards**

**Goal**: Verify switching works and layouts are preserved

1. Select "Executive Dashboard" from dropdown
   - ✅ Should load with only KPI cards
2. Select "Analytics Dashboard"
   - ✅ Should load with chart + 2 KPIs
3. Select "Weekly Report"
   - ✅ Should load with calendar + list
4. Refresh page (F5)
   - ✅ Should load "Weekly Report" (default)

---

## 📝 **Test Scenario 4: Edit and Save**

**Goal**: Modify existing dashboard and save

1. Select "Analytics Dashboard"
2. Click "Customize"
3. Add one more KPI card
4. Click "Save" (not "Save As")
   - ✅ Should save without dialog
   - ✅ Changes persisted
5. Switch to different dashboard and back
   - ✅ Changes should still be there

---

## 📝 **Test Scenario 5: Save As (Create Variant)**

**Goal**: Create a copy of existing dashboard

1. Select "Analytics Dashboard"
2. Click "Save As" (not in edit mode)
3. Enter name: "Analytics Dashboard V2"
4. Click "Save Dashboard"
   - ✅ New dashboard appears in dropdown
   - ✅ Both dashboards exist independently
5. Modify "Analytics Dashboard V2"
6. Save changes
7. Switch to original "Analytics Dashboard"
   - ✅ Original should be unchanged

---

## 📝 **Test Scenario 6: Unsaved Changes Warning**

**Goal**: Verify warning when switching with unsaved changes

1. Select any dashboard
2. Click "Customize"
3. Add a widget
4. Try to switch dashboard (without saving)
   - ✅ Should show confirmation dialog:
   ```
   "You have unsaved changes. Do you want to discard them?"
   [Cancel] [OK]
   ```
5. Click Cancel
   - ✅ Stay on current dashboard
6. Click dropdown again and select different dashboard
7. Click OK
   - ✅ Switch to new dashboard, changes lost

---

## 📝 **Test Scenario 7: Delete Dashboard**

**Goal**: Remove unwanted dashboards

1. Create 3+ dashboards
2. Select "Analytics Dashboard V2"
3. Click 🗑️ Delete button
   - ✅ Confirmation dialog appears:
   ```
   Delete dashboard "Analytics Dashboard V2"? 
   This action cannot be undone.
   [Cancel] [OK]
   ```
4. Click OK
   - ✅ Dashboard removed from dropdown
   - ✅ Automatically loads first available dashboard
   - ✅ Success message: "Dashboard deleted successfully"

---

## 📝 **Test Scenario 8: Revert Functionality**

**Goal**: Test revert with saved dashboards

1. Select any dashboard
2. Click "Customize"
3. Move several widgets
4. Add new widget
5. Click "Revert" button
   - ✅ Confirmation: "Revert all changes to the last saved state?"
6. Click OK
   - ✅ Dashboard returns to saved state
   - ✅ All changes undone

---

## 📝 **Test Scenario 9: Default Dashboard**

**Goal**: Verify default dashboard behavior

1. Have 3+ dashboards
2. Set "Weekly Report" as default (via Save As dialog)
3. Refresh page (F5)
   - ✅ "Weekly Report" loads automatically
4. Change default to "Executive Dashboard"
5. Refresh page
   - ✅ "Executive Dashboard" loads now
6. Check dropdown
   - ✅ Star icon and "Default" badge on correct dashboard

---

## 📝 **Test Scenario 10: Edge Cases**

### **10.1: Last Dashboard Protection**
1. Delete all dashboards except one
2. Try to delete last dashboard
   - ✅ Delete button should be disabled OR
   - ✅ Warning: "Cannot delete last dashboard"

### **10.2: Empty Name**
1. Click "Save As"
2. Leave name blank
3. Try to save
   - ✅ Save button should be disabled

### **10.3: Very Long Name**
1. Save dashboard with 100+ character name
   - ✅ Should save successfully
   - ✅ Name should truncate in dropdown with "..."

### **10.4: Special Characters**
1. Save dashboard with name: "My Dashboard! @#$%"
   - ✅ Should save successfully
   - ✅ Special chars handled properly

---

## ✅ **Expected Console Logs**

When loading dashboards:
```javascript
✅ User dashboards loaded: (3) [{...}, {...}, {...}]
✅ Loaded 3 menu items:
  1. "Home" -> path: "/propertyLanding/dashboard1"
  2. "Settings" -> path: "/settings"
✅ Loading dashboard: Weekly Report
```

When saving:
```javascript
Saving dashboard... {id: undefined, userId: 1, dashboardName: "My Dashboard", ...}
Dashboard saved with ID: 507f1f77bcf86cd799439011
```

When switching:
```javascript
Loading dashboard: Analytics Dashboard
Dashboard config loaded: {id: "...", dashboardName: "Analytics Dashboard"}
```

---

## 🐛 **Common Issues & Fixes**

| Issue | Fix |
|-------|-----|
| Dropdown empty | Check API `/api/v1/dashboard/user/1/all` returns data |
| Cannot save | Check console for errors, verify MongoDB is running |
| Default not loading | Check `isDefault` flag in MongoDB |
| Delete button missing | Verify you have 2+ dashboards |
| Changes not persisting | Check browser console for save errors |

---

## 📸 **Screenshots to Capture**

1. ✅ Empty state (first time user)
2. ✅ Save As dialog
3. ✅ Dropdown with multiple dashboards
4. ✅ Default dashboard with star + badge
5. ✅ Edit mode with Save/Save As buttons
6. ✅ Unsaved changes warning
7. ✅ Delete confirmation dialog
8. ✅ Success message after save

---

## 🎯 **Success Criteria**

- [ ] Can create multiple dashboards ✅
- [ ] Can switch between dashboards ✅
- [ ] Can save with custom names ✅
- [ ] Can Save As to create copies ✅
- [ ] Can delete dashboards ✅
- [ ] Default dashboard persists after refresh ✅
- [ ] Unsaved changes warning works ✅
- [ ] Revert functionality works ✅
- [ ] All dashboards load correctly ✅
- [ ] UI is responsive and smooth ✅

---

**Testing Duration**: ~30 minutes  
**Status**: Ready for Testing  
**Last Updated**: January 2025
