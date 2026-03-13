# 🎯 Phase 3 Implementation Plan: Drag-and-Drop Dashboard

## 📋 Overview

Phase 3 adds **interactive customization** to your dashboard with drag-and-drop functionality, allowing users to:
- Drag widgets to reposition them
- Resize widgets
- Add new widgets from a picker
- Remove widgets
- Save custom layouts
- Toggle edit mode

---

## 🛠️ Implementation Steps

### ✅ Step 1: Install Dependencies (COMPLETE)
```bash
npm install angular-gridster2 --save
```

### ✅ Step 2: Create Supporting Services (COMPLETE)
- [x] `gridster-config.service.ts` - Gridster configuration helper
- [x] `widget-picker-dialog.component.ts` - Widget selection modal

### 🔄 Step 3: Update Dashboard Component (IN PROGRESS)
**File**: `app/src/app/dashboard/dashboard1/dashboard1.component.ts`

**Changes Needed**:
1. Add Gridster imports
2. Add edit mode state
3. Add gridster configuration
4. Add widget items array (GridsterItem[])
5. Add methods:
   - `toggleEditMode()`
   - `addWidget()`
   - `removeWidget(id)`
   - `saveDashboard()`
   - `openWidgetPicker()`
   - `itemChange(item, itemComponent)`
   - `initializeGridsterItems()`

###Step 4: Update Dashboard Template
**File**: `app/src/app/dashboard/dashboard1/dashboard1.component.html`

**Changes Needed**:
1. Add edit mode toolbar
2. Wrap widgets in `<gridster>` container
3. Add `<gridster-item>` for each widget
4. Add drag handles
5. Add remove buttons (edit mode only)
6. Add floating action button (FAB) for widget picker

### ⏳ Step 5: Update Dashboard Styles
**File**: `app/src/app/dashboard/dashboard1/dashboard1.component.css`

**Changes Needed**:
1. Add gridster grid styles
2. Add edit mode styles
3. Add drag handle styles
4. Add FAB styles
5. Add toolbar styles

### ⏳ Step 6: Update Models (Optional)
**File**: `app/src/app/models/dashboard.models.ts`

**Add**:
- `GridsterItem` interface extensions if needed

---

## 📊 Feature Breakdown

### Feature 1: Edit Mode Toggle

**UI**:
```
┌─────────────────────────────────────┐
│ [Edit Mode] 🔧                      │
│ [Save Changes] [Cancel]             │
└─────────────────────────────────────┘
```

**State**:
- `editMode: boolean = false`
- When `true`: Enable drag, resize, show remove buttons
- When `false`: Static display

**Methods**:
```typescript
toggleEditMode(): void {
  this.editMode = !this.editMode;
  this.options = GridsterConfigService.getDefaultConfig(this.editMode);
  if (this.options.api) {
    this.options.api.optionsChanged();
  }
}
```

---

### Feature 2: Drag-and-Drop

**Gridster Configuration**:
```typescript
draggable: {
  enabled: editMode,
  dragHandleClass: 'drag-handler'
}
```

**UI**:
```
┌─────────────────────┐
│ ⋮⋮⋮ Widget Title    │ ← Drag Handle
│                     │
│   Widget Content    │
└─────────────────────┘
```

**Behavior**:
- User clicks drag handle (⋮⋮⋮)
- Widget follows cursor
- Grid shows drop zones
- Other widgets push away
- Drop to place

---

### Feature 3: Resize Widgets

**Gridster Configuration**:
```typescript
resizable: {
  enabled: editMode,
  handles: {
    s: true,  // south
    e: true,  // east
    se: true  // south-east
  }
}
```

**UI**:
```
┌─────────────────────┐
│   Widget Content    │
│                     │
│                   ◢ │ ← Resize Handle
└─────────────────────┘
```

**Constraints**:
- Min size: 2 cols × 2 rows
- Max size: 12 cols × 10 rows
- Snap to grid

---

### Feature 4: Add Widget

**Flow**:
1. User clicks **Add Widget** FAB
2. Dialog opens with widget library
3. User selects a widget
4. Widget added to dashboard at next available position
5. User can drag to desired location

**UI - FAB Button**:
```
                     ┌───┐
                     │ + │ ← Floating Action Button
                     └───┘
```

**UI - Widget Picker Dialog**:
```
┌──────────────────────────────────────┐
│  Add Widget to Dashboard             │
├──────────────────────────────────────┤
│  [Search...] [KPI] [Charts] [Lists]  │
│                                       │
│  ┌──────┐  ┌──────┐  ┌──────┐       │
│  │  🏨  │  │  📊  │  │  📋  │       │
│  │ KPI  │  │Chart │  │ List │       │
│  └──────┘  └──────┘  └──────┘       │
└──────────────────────────────────────┘
```

---

### Feature 5: Remove Widget

**UI** (Edit Mode Only):
```
┌─────────────────────┐
│ ⋮⋮⋮ Widget Title  ❌ │ ← Remove Button
│                     │
│   Widget Content    │
└─────────────────────┘
```

**Behavior**:
- Click ❌ button
- Confirmation (optional)
- Widget removed from grid
- Other widgets re-flow
- Changes not saved until user clicks "Save"

---

### Feature 6: Save Dashboard

**Flow**:
1. User makes changes (drag/resize/add/remove)
2. "Save Changes" button becomes enabled
3. User clicks "Save Changes"
4. Gather gridster items
5. Convert to dashboard configuration
6. Call API: `POST /api/v1/dashboard`
7. Show success message

**API Call**:
```typescript
saveDashboard(): void {
  const widgets = this.dashboardItems.map(item => 
    GridsterConfigService.toWidgetConfig(item)
  );
  
  const request: SaveDashboardRequest = {
    id: this.dashboardConfig?.id,
    userId: this.getCurrentUserId(),
    dashboardName: 'My Dashboard',
    isDefault: true,
    layout: {
      columns: 12,
      rowHeight: 80,
      widgets: widgets
    }
  };
  
  this.dashboardService.saveDashboard(request).subscribe({
    next: (id) => {
      this.snackBar.open('Dashboard saved!', 'Close', { duration: 3000 });
      this.editMode = false;
      this.hasUnsavedChanges = false;
    },
    error: (err) => {
      this.snackBar.open('Failed to save dashboard', 'Close', { duration: 3000 });
    }
  });
}
```

---

## 🎨 Visual Design

### Normal Mode (View Only)
```
┌────────────────────────────────────────────┐
│  Dashboard                    [Overview ▼]  │
│  ┌────────┐ ┌────────┐ ┌────────┐         │
│  │ KPI 1  │ │ KPI 2  │ │ KPI 3  │         │
│  └────────┘ └────────┘ └────────┘         │
│  ┌──────────────────────────────┐         │
│  │      Chart Widget            │         │
│  └──────────────────────────────┘         │
└────────────────────────────────────────────┘
```

### Edit Mode (Customizable)
```
┌────────────────────────────────────────────┐
│  🔧 Edit Mode  [Save] [Cancel]             │
├────────────────────────────────────────────┤
│  ┌────────────────┐ ┌────────────────┐    │
│  │⋮⋮⋮ KPI 1    ❌ │ │⋮⋮⋮ KPI 2    ❌ │    │
│  │                │ │                │    │
│  │    Value: 24   │ │   Value: 156   │    │
│  └────────────────┘ └────────────────┘    │
│                                            │
│  ┌──────────────────────────────────────┐ │
│  │⋮⋮⋮ Chart Widget                   ❌ │ │
│  │                                      │ │
│  │   [Chart Content]                  ◢ │ │
│  └──────────────────────────────────────┘ │
│                                       ┌──┐ │
│                                       │+│ │ ← Add Widget FAB
│                                       └──┘ │
└────────────────────────────────────────────┘
```

---

## 🔧 Key Classes and Interfaces

### GridsterItem (Extended)
```typescript
interface DashboardGridsterItem extends GridsterItem {
  x: number;
  y: number;
  cols: number;
  rows: number;
  widgetId: string;
  widgetType: string;
  settings: any;
  data?: any; // KpiCardData | ListWidgetData | etc.
}
```

### Component State
```typescript
export class Dashboard1Component {
  // Edit mode
  editMode = false;
  hasUnsavedChanges = false;
  
  // Gridster
  options: GridsterConfig;
  dashboardItems: DashboardGridsterItem[] = [];
  
  // Widgets
  widgetLibrary: WidgetLibraryItem[] = [];
  
  // State
  loading = false;
  dashboardConfig: DashboardConfiguration | null = null;
}
```

---

## 🎯 User Workflows

### Workflow 1: Customize Existing Dashboard
1. User loads dashboard
2. Clicks "Edit Mode" button
3. Dashboard enters edit mode:
   - Grid becomes visible
   - Drag handles appear
   - Resize handles appear
   - Remove buttons appear
4. User drags widget to new position
5. User resizes another widget
6. User clicks "Save Changes"
7. Dashboard saved to backend
8. Success message shown
9. Edit mode exits

### Workflow 2: Add New Widget
1. User in edit mode
2. Clicks **Add Widget** FAB
3. Widget picker dialog opens
4. User filters by category
5. User clicks desired widget
6. Dialog closes
7. Widget appears on dashboard
8. User drags to desired position
9. User clicks "Save Changes"

### Workflow 3: Remove Widget
1. User in edit mode
2. Hovers over widget
3. Clicks ❌ remove button
4. Widget removed (with animation)
5. Other widgets re-flow
6. User clicks "Save Changes"

---

## ⚠️ Important Considerations

### Performance
- Limit max widgets: 20 per dashboard
- Lazy load widget data
- Debounce resize/drag events

### UX
- Confirm before removing widgets
- Warn about unsaved changes on navigation
- Auto-save draft every 30 seconds (optional)
- Undo/redo (future enhancement)

### Validation
- Prevent overlapping widgets
- Enforce min/max sizes
- Validate widget configuration
- Check permissions before adding widgets

### Responsive Design
- Mobile: Disable drag-and-drop, show list view
- Tablet: Reduce columns (8 instead of 12)
- Desktop: Full 12-column grid

---

## 📦 Files to Modify

| File | Status | Changes |
|------|--------|---------|
| `dashboard1.component.ts` | 🔄 | Add gridster logic |
| `dashboard1.component.html` | 🔄 | Add gridster template |
| `dashboard1.component.css` | 🔄 | Add gridster styles |
| `gridster-config.service.ts` | ✅ | Created |
| `widget-picker-dialog.component.ts` | ✅ | Created |

---

## 🚀 Next Actions

**Option A: Incremental Implementation**
1. Add edit mode toggle (30 min)
2. Add gridster to template (1 hour)
3. Add drag-and-drop (30 min)
4. Add resize (15 min)
5. Add widget picker (1 hour)
6. Add save functionality (30 min)
7. Add remove functionality (15 min)
8. Test and polish (1 hour)

**Option B: Complete Implementation**
- Provide complete updated files
- Test all together
- Fix issues

**Which approach do you prefer?**

---

## 📚 Documentation References

- **Angular Gridster2**: https://tiberiuzuld.github.io/angular-gridster2/
- **Material Dialog**: https://material.angular.io/components/dialog
- **Our API**: See `DashboardController.cs`

---

**Ready to proceed?** Let me know which option you prefer, and I'll implement Phase 3! 🚀
