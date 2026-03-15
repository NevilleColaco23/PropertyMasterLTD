# Dashboard Header Button Layout - Visual Guide

## Edit Mode Header Layout

```
┌─────────────────────────────────────────────────────────────────────────────────────────┐
│  🔷 Dashboard                                                                            │
│  Welcome to your property management dashboard                                          │
│                                                                                          │
│                    [💾 Save]  [❌ Cancel]  [🔄 Revert]  [➕ Add Widget]  [❌ Exit Edit]  │
└─────────────────────────────────────────────────────────────────────────────────────────┘
```

## Button Details

### 1. **Save Button** 
```typescript
Color: primary (blue)
Icon: save 💾
Text: "Save"
Tooltip: "Save Dashboard"
State: Disabled when no unsaved changes
Badge: Orange dot when unsaved changes exist
```

### 2. **Cancel Button**
```typescript
Color: default (white/gray)
Icon: cancel ❌
Text: "Cancel"
Tooltip: "Cancel Changes"
Action: Exit edit mode and reload from database
```

### 3. **Revert Button** ⭐ NEW
```typescript
Color: warn (red/orange #ff6b6b)
Icon: restore 🔄
Text: "Revert"
Tooltip: "Revert to Last Saved State"
State: Disabled when no unsaved changes
Action: Restore to backup state (stays in edit mode)
```

### 4. **Add Widget Button**
```typescript
Color: accent (teal/green)
Icon: add ➕
Text: "Add Widget"
Tooltip: "Add Widget"
Action: Open widget picker dialog
```

### 5. **Exit Edit Button**
```typescript
Color: warn (red) when in edit mode
Icon: close ❌
Text: "Exit Edit"
Tooltip: "Exit Edit Mode"
Action: Exit edit mode (confirms if unsaved changes)
```

## Button States Comparison

### When No Changes Made
```
[💾 Save] ─────────── DISABLED (gray)
[❌ Cancel] ────────── ENABLED
[🔄 Revert] ────────── DISABLED (gray)  ⭐
[➕ Add Widget] ────── ENABLED
[❌ Exit Edit] ────── ENABLED
```

### When Changes Made (Unsaved)
```
[💾 Save 🔴] ──────── ENABLED (blue with orange dot)
[❌ Cancel] ────────── ENABLED
[🔄 Revert] ────────── ENABLED (red/orange)  ⭐
[➕ Add Widget] ────── ENABLED
[❌ Exit Edit] ────── ENABLED
```

### During Save/Load
```
[💾 Save] ─────────── DISABLED (loading)
[❌ Cancel] ────────── DISABLED
[🔄 Revert] ────────── DISABLED  ⭐
[➕ Add Widget] ────── ENABLED
[❌ Exit Edit] ────── ENABLED
```

## User Scenarios

### Scenario 1: Quick Undo After Mistake
```
1. User drags widget to wrong position by accident
2. User clicks [🔄 Revert] button
3. Confirms dialog: "Revert all changes to the last saved state?"
4. Dashboard instantly restores to previous state
5. User continues editing
```

### Scenario 2: Compare Options
```
1. User has Layout A (saved)
2. User experiments with Layout B
3. User clicks [🔄 Revert] to see Layout A again
4. User decides they prefer Layout B
5. User recreates changes and clicks [💾 Save]
```

### Scenario 3: Cancel vs Revert
```
User has unsaved changes and wants to discard them:

Option A - Cancel:
├─ Click [❌ Cancel]
├─ Exits edit mode
└─ Reloads dashboard from database

Option B - Revert:  ⭐
├─ Click [🔄 Revert]
├─ Stays in edit mode
└─ Restores to last saved state
```

## Visual Design

### Color Scheme
```css
Save Button:     #1976d2 (Blue)
Cancel Button:   #424242 (Dark Gray)
Revert Button:   #ff6b6b (Red/Orange)  ⭐ NEW
Add Button:      #00897b (Teal)
Exit Button:     #d32f2f (Red)
```

### Hover Effects
```css
Save:     Blue → Darker Blue + Shadow
Cancel:   Gray → Darker Gray
Revert:   #ff6b6b → #ee5253 + Red Shadow  ⭐
Add:      Teal → Darker Teal + Shadow
Exit:     Red → Darker Red
```

## Mobile/Responsive Layout

### Desktop (Wide Screen)
```
[💾 Save]  [❌ Cancel]  [🔄 Revert]  [➕ Add Widget]  [❌ Exit Edit]
```

### Tablet/Mobile (Narrow Screen)
```
[💾 Save]  [❌ Cancel]
[🔄 Revert]  [➕ Add Widget]
[❌ Exit Edit]
```

## Confirmation Dialogs

### Revert Confirmation
```
┌────────────────────────────────────┐
│  Confirm Revert                    │
├────────────────────────────────────┤
│  Revert all changes to the last    │
│  saved state?                      │
│                                    │
│  [Cancel]          [OK]            │
└────────────────────────────────────┘
```

### Cancel Confirmation (with unsaved changes)
```
┌────────────────────────────────────┐
│  Discard Changes                   │
├────────────────────────────────────┤
│  You have unsaved changes. Do you  │
│  want to discard them?             │
│                                    │
│  [No]              [Yes]           │
└────────────────────────────────────┘
```

## Success Messages

### After Revert
```
┌─────────────────────────────────────────────────┐
│  ✓ Dashboard reverted to last saved state       │
└─────────────────────────────────────────────────┘
```

### After Save
```
┌─────────────────────────────────────────────────┐
│  ✓ Dashboard saved successfully!                │
└─────────────────────────────────────────────────┘
```

## Keyboard Shortcuts (Future Enhancement)

```
Ctrl + S      → Save Dashboard
Ctrl + Z      → Revert (Undo)  ⭐ Future
Escape        → Exit Edit Mode
Ctrl + Shift + A → Add Widget
```

## Accessibility

- All buttons have proper `aria-label` and `matTooltip`
- Disabled states are clearly indicated
- Color contrast meets WCAG AA standards
- Focus indicators visible for keyboard navigation
- Confirmation dialogs are keyboard accessible

## Testing Commands

```bash
# Run Angular development server
cd app
ng serve

# Navigate to dashboard
http://localhost:4200/propertyLanding/dashboard1

# Test Revert feature:
1. Click "Customize" to enter edit mode
2. Move/resize widgets or add new widgets
3. Click "Revert" button
4. Confirm the action
5. Verify widgets return to previous state
6. Verify button is disabled (no unsaved changes)
```

---

**Feature Status**: ✅ Implemented  
**Visual Design**: ✅ Complete  
**User Testing**: ⏳ Pending
