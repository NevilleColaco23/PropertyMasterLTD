# Quick Start: Client-Provided Activity Messages

## 🚀 Example: Room Service with Activity Messages

### Before (Server Guessing)
```typescript
// room.service.ts - OLD WAY
createRoom(room: Room) {
  return this.http.post('/api/rooms', room);
  // Server logs: "john created Room" ❌ Generic!
}
```

### After (Client Provides Context)
```typescript
// room.service.ts - NEW WAY
import { Injectable, Inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ActivityMessageService } from './activity-message.service';
import { APP_CONFIG, AppConfig } from '../configuration/app.config.token';

@Injectable({ providedIn: 'root' })
export class RoomService {
  constructor(
    private http: HttpClient,
    private activityMessage: ActivityMessageService,
    @Inject(APP_CONFIG) private config: AppConfig
  ) {}

  // CREATE with meaningful message
  createRoom(room: Room, floor: string) {
    const headers = this.activityMessage.createMessageForCreate(
      room.name,
      `to ${floor}`
    );

    return this.http.post(`${this.config.apiUrl}/api/rooms`, room, { headers });
    // MongoDB logs: "Added Ocean View Suite to 3rd floor" ✅ Perfect!
  }

  // UPDATE with what changed
  updateRoom(id: number, room: Room, changes: string) {
    const headers = this.activityMessage.createMessageForUpdate(
      room.name,
      changes
    );

    return this.http.put(`${this.config.apiUrl}/api/rooms/${id}`, room, { headers });
    // MongoDB logs: "Updated Ocean View Suite - room rate" ✅
  }

  // DELETE with room name
  deleteRoom(id: number, roomName: string) {
    const headers = this.activityMessage.createMessageForDelete(roomName);

    return this.http.delete(`${this.config.apiUrl}/api/rooms/${id}`, { headers });
    // MongoDB logs: "Removed Ocean View Suite" ✅
  }

  // CUSTOM message for complex operation
  assignRoomToBooking(roomId: number, bookingId: number, roomName: string, guestName: string) {
    const message = `Assigned ${roomName} to ${guestName}'s booking`;
    const headers = this.activityMessage.createHeaders(message);

    return this.http.post(
      `${this.config.apiUrl}/api/bookings/${bookingId}/assign-room`,
      { roomId },
      { headers }
    );
    // MongoDB logs: "Assigned Ocean View Suite to John Smith's booking" ✅
  }
}
```

---

## 📝 Component Usage Example

```typescript
// room-form.component.ts
import { Component } from '@angular/core';
import { RoomService } from '../services/room.service';

@Component({
  selector: 'app-room-form',
  template: `
    <h2>{{ isEdit ? 'Edit' : 'Add' }} Room</h2>
    
    <form (ngSubmit)="saveRoom()">
      <input [(ngModel)]="room.name" placeholder="Room Name" required />
      
      <select [(ngModel)]="room.floor">
        <option value="1">1st Floor</option>
        <option value="2">2nd Floor</option>
        <option value="3">3rd Floor</option>
      </select>

      <input [(ngModel)]="room.rate" type="number" placeholder="Rate" />

      <button type="submit">
        {{ isEdit ? 'Update' : 'Add' }} Room
      </button>
    </form>
  `
})
export class RoomFormComponent {
  room: Room = { name: '', floor: '1', rate: 0 };
  isEdit = false;
  originalRoom?: Room;

  constructor(private roomService: RoomService) {}

  saveRoom() {
    if (this.isEdit) {
      // Build changes description
      const changes = this.getChanges();
      
      this.roomService.updateRoom(this.room.id!, this.room, changes)
        .subscribe(
          () => console.log('✅ Room updated'),
          error => console.error('❌ Update failed', error)
        );
    } else {
      // Include floor context in create message
      const floorName = this.getFloorName(this.room.floor);
      
      this.roomService.createRoom(this.room, floorName)
        .subscribe(
          () => console.log('✅ Room created'),
          error => console.error('❌ Create failed', error)
        );
    }
  }

  private getFloorName(floor: string): string {
    return `${floor}${this.getOrdinalSuffix(parseInt(floor))} floor`;
  }

  private getOrdinalSuffix(n: number): string {
    if (n >= 11 && n <= 13) return 'th';
    switch (n % 10) {
      case 1: return 'st';
      case 2: return 'nd';
      case 3: return 'rd';
      default: return 'th';
    }
  }

  private getChanges(): string {
    if (!this.originalRoom) return 'details';

    const changes: string[] = [];
    if (this.room.rate !== this.originalRoom.rate) changes.push('rate');
    if (this.room.floor !== this.originalRoom.floor) changes.push('floor');
    
    return changes.length > 0 ? changes.join(' and ') : 'details';
  }
}
```

---

## 🎯 Real-World Example: Booking Management

```typescript
// booking.service.ts
@Injectable({ providedIn: 'root' })
export class BookingService {
  constructor(
    private http: HttpClient,
    private activityMessage: ActivityMessageService,
    @Inject(APP_CONFIG) private config: AppConfig
  ) {}

  // Create booking with guest name
  createBooking(booking: Booking) {
    const message = `Created booking for ${booking.guestName} (${booking.checkIn} - ${booking.checkOut})`;
    const headers = this.activityMessage.createHeaders(message);

    return this.http.post(`${this.config.apiUrl}/api/bookings`, booking, { headers });
    // MongoDB: "Created booking for John Smith (2024-03-15 - 2024-03-18)"
  }

  // Update status with context
  updateStatus(id: number, guestName: string, oldStatus: string, newStatus: string) {
    const message = `Changed ${guestName}'s booking from ${oldStatus} to ${newStatus}`;
    const headers = this.activityMessage.createHeaders(message);

    return this.http.put(
      `${this.config.apiUrl}/api/bookings/${id}/status`,
      { status: newStatus },
      { headers }
    );
    // MongoDB: "Changed John Smith's booking from Pending to Confirmed"
  }

  // Cancel with reason
  cancelBooking(id: number, guestName: string, reason: string) {
    const message = `Cancelled ${guestName}'s booking - ${reason}`;
    const headers = this.activityMessage.createHeaders(message);

    return this.http.put(
      `${this.config.apiUrl}/api/bookings/${id}/cancel`,
      { reason },
      { headers }
    );
    // MongoDB: "Cancelled John Smith's booking - Guest requested"
  }
}
```

---

## 🎨 Button Click Examples

### Delete Button with Confirmation
```typescript
@Component({
  template: `
    <button (click)="deleteRoom(room)" mat-icon-button color="warn">
      <mat-icon>delete</mat-icon>
    </button>
  `
})
export class RoomListComponent {
  constructor(
    private roomService: RoomService,
    private dialog: MatDialog
  ) {}

  deleteRoom(room: Room) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      data: {
        title: 'Delete Room',
        message: `Are you sure you want to delete ${room.name}?`
      }
    });

    dialogRef.afterClosed().subscribe(confirmed => {
      if (confirmed) {
        this.roomService.deleteRoom(room.id, room.name)
          .subscribe(() => {
            console.log('✅ Room deleted with activity message');
            this.loadRooms(); // Refresh list
          });
      }
    });
  }
}
```

### Status Change Dropdown
```typescript
@Component({
  template: `
    <mat-select 
      [(ngModel)]="booking.status" 
      (selectionChange)="onStatusChange($event)">
      <mat-option value="Pending">Pending</mat-option>
      <mat-option value="Confirmed">Confirmed</mat-option>
      <mat-option value="CheckedIn">Checked In</mat-option>
      <mat-option value="CheckedOut">Checked Out</mat-option>
    </mat-select>
  `
})
export class BookingStatusComponent {
  @Input() booking!: Booking;
  originalStatus: string;

  constructor(private bookingService: BookingService) {
    this.originalStatus = this.booking.status;
  }

  onStatusChange(event: any) {
    this.bookingService.updateStatus(
      this.booking.id,
      this.booking.guestName,
      this.originalStatus,
      event.value
    ).subscribe(() => {
      console.log('✅ Status changed');
      this.originalStatus = event.value;
    });
  }
}
```

---

## 📊 Results in MongoDB

### Before (Server Auto-Generated)
```javascript
{
  "DisplayMessage": "john created Room",
  "ActivityType": "Create",
  "EntityType": "Room"
}
```
❌ Generic, not helpful for reporting

### After (Client-Provided)
```javascript
{
  "DisplayMessage": "Added Ocean View Suite to 3rd floor",
  "ActivityType": "Create",
  "EntityType": "Room",
  "Metadata": {
    "response_Name": "Ocean View Suite",
    "selected_PropertyId": "5",
    "selected_PropertyName": "Sunset Villa"
  }
}
```
✅ **Perfect for business reporting!**

---

## ✅ Quick Implementation Checklist

1. **Install New Files** (Already Done)
   - [x] `activity-message.interceptor.ts`
   - [x] `activity-message.service.ts`
   - [x] Updated `app.config.ts`
   - [x] Updated backend filter

2. **Update Your Service** (Do This Now)
   ```typescript
   // Add to constructor
   constructor(
     private http: HttpClient,
     private activityMessage: ActivityMessageService  // ← Add this
   ) {}

   // Update methods
   createRoom(room: Room, floor: string) {
     const headers = this.activityMessage.createMessageForCreate(
       room.name,
       `to ${floor}`
     );
     return this.http.post(url, room, { headers });  // ← Add headers
   }
   ```

3. **Test It**
   - Create/update/delete an entity
   - Check MongoDB: `db.UserActivityLogs.find().sort({_id: -1}).limit(1)`
   - Should see your custom message in `DisplayMessage` field

4. **Celebrate** 🎉
   - Your activity logs now tell the real story!

---

## 🎊 Summary

**You're absolutely right!** The client knows what the user did, so let it tell the server:

✅ **Angular sends:** `X-Activity-Message: "Added Ocean View Suite to 3rd floor"`  
✅ **Backend logs:** Exact message from client  
✅ **Fallback:** Server still generates message if client doesn't provide one  
✅ **Result:** Meaningful activity logs that match real user actions!

**Start using it now with your most important operations!** 🚀
