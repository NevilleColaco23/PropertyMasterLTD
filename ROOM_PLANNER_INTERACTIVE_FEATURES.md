# Room Planner Interactive Features

## 🎯 Overview

The Room Planner now supports advanced interactive booking manipulation through drag & drop, resize, and context menu features, allowing users to visually manage reservations with immediate feedback.

---

## 🚀 Features Implemented

### 1. **Drag & Drop Booking Movement**

Move bookings between rooms by dragging booking bars to different room rows.

#### **User Experience:**
- **Grab**: Hover over booking bar → cursor changes to `grab`
- **Drag**: Click and drag → cursor changes to `grabbing`, bar becomes semi-transparent
- **Drop Target**: Hovering over target room → blue highlight with "Drop here to move booking" overlay
- **Drop**: Release on target room → confirmation dialog appears
- **Validation**: System checks for availability conflicts before allowing move

#### **Visual Feedback:**
- `.draggable` - Default grab cursor on booking bars
- `.dragging` - Semi-transparent (60% opacity) during drag, scaled up 5%
- `.drop-target` - Blue background on valid drop zones
- Confirmation dialog before finalizing move

#### **Implementation:**
```typescript
// Event handlers in dashboard1.component.ts
onBookingDragStart(bar, roomId, event) // Start drag
onRoomDragOver(roomId, event)          // Highlight drop target
onRoomDrop(roomId, event)              // Execute move with validation
onBookingDragEnd(event)                // Clean up drag state
```

#### **Constraints:**
- Cannot drop on same room (no-op)
- Checks for date conflicts in target room
- Excludes current booking from conflict check
- Shows warning if target room unavailable

---

### 2. **Resize Bookings (Extend/Shorten Stay)**

Adjust booking duration by dragging the left or right edge of booking bars.

#### **User Experience:**
- **Hover**: Move cursor to left/right edge → cursor changes to `ew-resize`
- **Grab**: Click edge handle → booking bar shows resize feedback (80% opacity)
- **Drag**: Move left/right → booking bar expands/contracts in real-time
- **Release**: Drop → system validates new dates and updates booking
- **Validation**: Prevents overlaps with adjacent bookings

#### **Visual Feedback:**
- `.resize-handle` - 12px wide invisible handles on left/right edges
- `.resize-handle::before` - Subtle vertical line indicator on hover
- `.resizing` - Semi-transparent during resize operation
- `body.resizing` - Global cursor override to `ew-resize`

#### **Implementation:**
```typescript
// Event handlers
onResizeStart(bar, roomId, direction, event) // 'left' or 'right'
onResizeMove(event)                          // Calculate new span
onResizeEnd(event)                           // Validate and save
```

#### **Constraints:**
- **Minimum**: 1 day booking duration
- **Maximum**: Cannot extend past end of current month view
- **Continuation Bookings**: Hide resize handle on continuation arrows
  - Left handle hidden if `bar.continuesFromPreviousMonth`
  - Right handle hidden if `bar.continuesNextMonth`
- **Conflict Detection**: Prevents resize if overlaps with existing booking

#### **Resize Logic:**
```typescript
// Right edge (extend/shorten end date)
newSpan = originalSpan + daysDelta

// Left edge (extend/shorten start date)
newSpan = originalSpan - daysDelta
newStartCol = originalStartCol + daysDelta
```

---

### 3. **Right-Click Context Menu**

Quick access to booking actions via right-click menu.

#### **User Experience:**
- **Right-Click**: Click booking bar with right mouse button
- **Menu Appears**: Context menu opens at cursor position
- **Actions Available**:
  1. **View Details** (Primary) - Open full booking details dialog
  2. **Edit Booking** - Navigate to edit form
  3. **Extend Stay** - Hint to use right resize handle
  4. **Shorten Stay** - Hint to use left resize handle
  5. **Move to Another Room** - Hint to use drag & drop
  6. **Upgrade Room** - Coming soon
  7. **Cancel Booking** (Danger) - Delete with confirmation

#### **Visual Feedback:**
- `.context-menu-active` - Blue outline on booking bar while menu open
- Gradient blue header showing guest name and dates
- Material icons for each action
- Keyboard shortcuts displayed (Enter, E, Del)
- Danger styling (red) for cancel action

#### **Implementation:**
```typescript
// Component: BookingContextMenuComponent
onBookingRightClick(bar, roomId, event) {
  dialog.open(BookingContextMenuComponent, {
    position: { left: event.clientX, top: event.clientY },
    data: { bookingId, guestName, roomNumber, dates }
  })
}

handleContextMenuAction(result) {
  switch (result.action) {
    case 'view-details': openBookingDetails()
    case 'cancel': confirmCancelBooking()
    // ... other actions
  }
}
```

#### **Menu Structure:**
```
┌─────────────────────────────────┐
│ 🗓️  John Doe                    │
│    Room 101 • Jan 15 - Jan 20   │ ← Header
├─────────────────────────────────┤
│ 👁️  View Details         [Enter]│ ← Primary
│ ✏️  Edit Booking            [E] │
├─────────────────────────────────┤
│ ➕ Extend Stay                  │
│ ➖ Shorten Stay                 │ ← Modifications
│ ↔️  Move to Another Room        │
│ ⬆️  Upgrade Room                │
├─────────────────────────────────┤
│ ❌ Cancel Booking         [Del] │ ← Danger
└─────────────────────────────────┘
```

---

## 🎨 CSS Classes Reference

### Drag & Drop States

| Class | Applied To | Purpose |
|-------|-----------|---------|
| `.draggable` | `.booking-bar` | Default - shows grab cursor |
| `.dragging` | `.booking-bar` | During drag - semi-transparent |
| `.drop-target` | `.gantt-timeline` | Room row accepting drop |
| `.selected` | `.booking-bar` | Highlight dragged booking |

### Resize States

| Class | Applied To | Purpose |
|-------|-----------|---------|
| `.resizable` | `.booking-bar` | Enables resize handles |
| `.resize-handle` | `<div>` inside bar | Left/right edge handles |
| `.resizing` | `.booking-bar` | During resize operation |
| `body.resizing` | `<body>` | Global cursor override |

### Context Menu States

| Class | Applied To | Purpose |
|-------|-----------|---------|
| `.context-menu-active` | `.booking-bar` | Blue outline while menu open |

---

## 🔧 Component State Management

### Dashboard Component State

```typescript
// Drag & Drop
draggedBooking: BookingBar | null = null
draggedFromRoomId: number | null = null
isDragging: boolean = false
dropTargetRoomId: number | null = null
dropTargetDate: Date | null = null

// Resize
resizingBooking: BookingBar | null = null
resizingRoomId: number | null = null
resizeDirection: 'left' | 'right' | null = null
isResizing: boolean = false
resizeStartX: number = 0
resizeOriginalSpan: number = 0
resizeOriginalStartCol: number = 0
```

---

## 📋 HTML Template Bindings

### Booking Bar Attributes

```html
<div class="booking-bar draggable resizable"
     [attr.draggable]="true"
     (dragstart)="onBookingDragStart(bar, room.id, $event)"
     (dragend)="onBookingDragEnd($event)"
     (contextmenu)="onBookingRightClick(bar, room.id, $event)">
  
  <!-- Left Resize Handle -->
  <div class="resize-handle resize-handle-left"
       (mousedown)="onResizeStart(bar, room.id, 'left', $event)"
       *ngIf="!bar.continuesFromPreviousMonth"></div>
  
  <!-- Booking Label -->
  <span class="bar-label">{{ bar.guestName }}</span>
  
  <!-- Action Hint -->
  <span class="action-hint">Drag to move • Right-click for options</span>
  
  <!-- Right Resize Handle -->
  <div class="resize-handle resize-handle-right"
       (mousedown)="onResizeStart(bar, room.id, 'right', $event)"
       *ngIf="!bar.continuesNextMonth"></div>
</div>
```

### Room Row Drop Zone

```html
<div class="gantt-timeline"
     [class.drop-target]="dropTargetRoomId === room.id"
     (dragover)="onRoomDragOver(room.id, $event)"
     (dragleave)="onRoomDragLeave(room.id, $event)"
     (drop)="onRoomDrop(room.id, $event)">
  <!-- day columns and booking bars -->
</div>
```

---

## 🔄 Workflow Examples

### Move Booking Between Rooms

1. **User Action**: Drag "John Doe" booking from Room 101 to Room 102
2. **System**: 
   - Sets `draggedBooking` = John Doe bar
   - Sets `isDragging` = true
   - Adds `.dragging` class
3. **Hover Room 102**: 
   - Sets `dropTargetRoomId` = 102
   - Adds `.drop-target` class to Room 102 timeline
4. **Drop**: 
   - Calls `checkRoomAvailability(102, Jan 15, Jan 20)`
   - Shows confirmation dialog
5. **Confirm**: 
   - Removes bar from Room 101 `roomBookingBars`
   - Adds bar to Room 102 `roomBookingBars`
   - Updates `roomBookings` map
   - Shows success notification
6. **Cleanup**: 
   - Clears all drag state
   - Removes CSS classes
   - Triggers `cdr.detectChanges()`

### Extend Booking by 2 Days

1. **User Action**: Drag right edge of booking bar 2 days to the right
2. **System**: 
   - Sets `resizingBooking` = booking bar
   - Sets `resizeDirection` = 'right'
   - Stores `resizeOriginalSpan` = 5
   - Adds `.resizing` class
   - Adds `body.resizing` class
3. **Mouse Move**: 
   - Calculates `deltaX` = 120px (2 days @ 60px/day)
   - Calculates `daysDelta` = 2
   - Updates `bar.span` = 7 (5 + 2)
   - Triggers `cdr.detectChanges()` (visual feedback)
4. **Mouse Up**: 
   - Calculates new end date = original + 2 days
   - Calls `checkRoomAvailabilityForResize()`
   - Updates booking dates in state
   - Shows success notification "Booking extended successfully!"
5. **Cleanup**: 
   - Removes `body.resizing` class
   - Clears resize state
   - Triggers change detection

### Cancel Booking via Context Menu

1. **User Action**: Right-click booking bar → select "Cancel Booking"
2. **System**: 
   - Opens `BookingContextMenuComponent` at cursor position
   - Adds `.context-menu-active` class to booking bar
3. **User Clicks Cancel**: 
   - Dialog returns `{ action: 'cancel', bookingId: '12345' }`
   - Removes `.context-menu-active` class
4. **Confirmation**: 
   - Shows danger confirmation dialog
   - "Are you sure you want to cancel...?"
5. **Confirm**: 
   - Removes booking from `roomBookingBars.get(roomId)`
   - Removes entries from `roomBookings` map
   - Shows success notification
   - Triggers change detection

---

## ⚠️ Known Constraints & Limitations

### Current Implementation

✅ **Implemented:**
- Visual drag & drop with feedback
- Real-time resize with grid snapping
- Context menu with 8 actions
- Availability conflict detection
- Optimistic UI updates
- Confirmation dialogs
- Beautiful notifications

⚠️ **Stubbed (TODO):**
- Backend API integration for:
  - `dashboardService.moveBooking(bookingId, newRoomId)`
  - `dashboardService.resizeBooking(bookingId, newStart, newEnd)`
  - `dashboardService.cancelBooking(bookingId)`
  - `dashboardService.upgradeRoom(bookingId, newRoomId)`
- Rollback logic on API failure
- Real-time updates (SignalR/WebSocket)

### Edge Cases Handled

✅ Minimum 1-day booking duration
✅ Cannot extend past month boundary
✅ Hide resize handles on continuation bookings
✅ Prevent drop on same room
✅ Date overlap validation
✅ Exclude self from conflict check

### Edge Cases NOT Handled

❌ Cross-month drag & drop (requires multi-month view)
❌ Multi-select bookings for batch operations
❌ Undo/redo functionality
❌ Keyboard shortcuts (Escape to cancel, etc.)
❌ Touch/mobile drag support

---

## 🧪 Testing Checklist

### Drag & Drop
- [ ] Drag booking to different room → drop zone highlights
- [ ] Drag booking to same room → no action
- [ ] Drag booking to occupied room → warning notification
- [ ] Cancel drag (Escape key) → booking returns to original position
- [ ] Drag booking outside planner → booking returns to original

### Resize
- [ ] Drag right edge → booking extends
- [ ] Drag left edge → booking start date changes
- [ ] Resize to 1 day → allowed
- [ ] Resize to 0 days → prevented (minimum 1)
- [ ] Resize past month end → clamped to last day
- [ ] Resize into occupied dates → warning notification
- [ ] Resize continuation booking → handles hidden

### Context Menu
- [ ] Right-click booking → menu opens at cursor
- [ ] Click "View Details" → booking details dialog opens
- [ ] Click "Cancel Booking" → confirmation dialog appears
- [ ] Click "Extend Stay" → hint notification shows
- [ ] Click outside menu → menu closes
- [ ] Press Escape → menu closes
- [ ] Keyboard shortcuts work (Enter, E, Del)

---

## 🔮 Future Enhancements

### Phase 2 (Backend Integration)
- [ ] Real backend API endpoints
- [ ] Error handling and rollback
- [ ] Optimistic updates with undo
- [ ] Real-time sync across users

### Phase 3 (Advanced Features)
- [ ] Multi-select bookings (Ctrl+Click)
- [ ] Batch operations (move/cancel multiple)
- [ ] Keyboard shortcuts (Escape, Delete, Arrow keys)
- [ ] Undo/redo stack (Ctrl+Z, Ctrl+Y)
- [ ] Copy/paste bookings (Ctrl+C, Ctrl+V)
- [ ] Drag to create new bookings
- [ ] Quick duplicate booking

### Phase 4 (Polish)
- [ ] Touch/mobile drag support
- [ ] Smooth animations for state transitions
- [ ] Loading states during API calls
- [ ] Ghost preview during drag
- [ ] Snap-to-grid feedback
- [ ] Visual hints for keyboard users
- [ ] Accessibility (ARIA labels, keyboard nav)

---

## 📝 API Integration TODO

### Required Backend Endpoints

```typescript
// Move booking to different room
moveBooking(bookingId: string, newRoomId: number): Observable<boolean>

// Resize booking dates
resizeBooking(bookingId: string, newStartDate: Date, newEndDate: Date): Observable<boolean>

// Update booking details
updateBooking(bookingId: string, updates: Partial<Booking>): Observable<Booking>

// Cancel booking
cancelBooking(bookingId: string): Observable<boolean>

// Upgrade room (move to higher tier)
upgradeRoom(bookingId: string, newRoomId: number): Observable<boolean>
```

### Integration Points

```typescript
// dashboard1.component.ts

private performBookingMove(booking: BookingBar, newRoomId: number): void {
  // TODO: Replace with actual API call
  this.dashboardService.moveBooking(booking.bookingId, newRoomId).subscribe({
    next: (success) => {
      if (success) {
        // Update UI (already done optimistically)
        this.notificationService.success('Booking moved successfully!');
      } else {
        // Rollback UI changes
        this.revertBookingMove();
        this.notificationService.error('Failed to move booking');
      }
    },
    error: (error) => {
      this.revertBookingMove();
      this.notificationService.error('Server error: ' + error.message);
    }
  });
}
```

---

## 🎓 Developer Notes

### Key Architectural Decisions

1. **Optimistic UI Updates**: UI updates immediately before API confirmation for better UX
2. **State Management**: Component-level state (no NgRx) for simplicity
3. **Event Delegation**: Global mouse listeners for resize (better performance)
4. **CSS-First Approach**: Visual feedback via CSS classes, not inline styles
5. **Confirmation Dialogs**: All destructive actions require confirmation

### Performance Considerations

- **Change Detection**: Manual `cdr.detectChanges()` after state updates
- **Event Listeners**: Removed properly to prevent memory leaks
- **Grid Calculations**: Cached day width (60px) to avoid layout thrashing
- **Conflict Detection**: O(n) iteration through booking bars (acceptable for typical room count)

### Maintenance Tips

- **CSS Classes**: All interactive states in `room-planner-gantt.css`
- **State Properties**: Grouped by feature (drag, resize, context menu)
- **Event Handlers**: Named consistently (on[Action][Event])
- **Cleanup Methods**: Always clear state and remove event listeners

---

## 📚 Related Documentation

- [ROOM_PLANNER_GANTT_REDESIGN.md](./ROOM_PLANNER_GANTT_REDESIGN.md) - Gantt chart architecture
- [ROOM_PLANNER_CONTINUATION_INDICATORS.md](./ROOM_PLANNER_CONTINUATION_INDICATORS.md) - Multi-month bookings
- [ROOM_PLANNER_GUEST_DETAILS_FEATURE_COMPLETE.md](./ROOM_PLANNER_GUEST_DETAILS_FEATURE_COMPLETE.md) - Guest information
- [BEAUTIFUL_NOTIFICATIONS_IMPLEMENTATION.md](./BEAUTIFUL_NOTIFICATIONS_IMPLEMENTATION.md) - Notification system
- [PREMIUM_NOTIFICATIONS_BEAUTIFICATION.md](./PREMIUM_NOTIFICATIONS_BEAUTIFICATION.md) - Enhanced notifications

---

**Last Updated**: January 2025  
**Version**: 1.0.0  
**Status**: ✅ Phase 1 Complete (UI + Interactions), ⚠️ Phase 2 Pending (Backend Integration)
