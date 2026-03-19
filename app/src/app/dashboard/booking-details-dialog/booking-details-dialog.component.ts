import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatChipsModule } from '@angular/material/chips';

export interface BookingDetailsData {
  room: {
    roomNumber: string;
    roomName: string;
    roomType: string;
    floor?: number;
    capacity?: number;
    status: string;
  };
  date: Date;
  booking?: {
    type: string;
    guestName: string;
    bookingId: string;
    color: string;
  };
}

@Component({
  selector: 'app-booking-details-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatChipsModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>info</mat-icon>
      {{ getDialogTitle() }}
    </h2>

    <mat-dialog-content>
      <!-- Room Information -->
      <div class="info-section">
        <h3>
          <mat-icon>hotel</mat-icon>
          Room Information
        </h3>
        <div class="info-grid">
          <div class="info-item">
            <span class="label">Room Number:</span>
            <span class="value">{{ data.room.roomNumber }}</span>
          </div>
          <div class="info-item">
            <span class="label">Room Name:</span>
            <span class="value">{{ data.room.roomName }}</span>
          </div>
          <div class="info-item">
            <span class="label">Room Type:</span>
            <span class="value">{{ data.room.roomType }}</span>
          </div>
          <div class="info-item" *ngIf="data.room.floor">
            <span class="label">Floor:</span>
            <span class="value">{{ data.room.floor }}</span>
          </div>
          <div class="info-item" *ngIf="data.room.capacity">
            <span class="label">Capacity:</span>
            <span class="value">{{ data.room.capacity }} guests</span>
          </div>
          <div class="info-item">
            <span class="label">Date:</span>
            <span class="value">{{ data.date | date:'fullDate' }}</span>
          </div>
        </div>
      </div>

      <mat-divider></mat-divider>

      <!-- Booking Information -->
      <div class="info-section">
        <h3>
          <mat-icon>{{ getBookingIcon() }}</mat-icon>
          Status
        </h3>
        
        <div class="status-container">
          <mat-chip 
            [ngClass]="getStatusClass()"
            class="status-chip">
            {{ getStatusLabel() }}
          </mat-chip>
        </div>

        <div class="info-grid" *ngIf="data.booking">
          <div class="info-item">
            <span class="label">Guest:</span>
            <span class="value">{{ data.booking.guestName }}</span>
          </div>
          <div class="info-item">
            <span class="label">Booking ID:</span>
            <span class="value">{{ data.booking.bookingId }}</span>
          </div>
        </div>

        <div class="empty-state" *ngIf="!data.booking">
          <mat-icon>event_available</mat-icon>
          <p>This room is available for booking on the selected date.</p>
        </div>
      </div>
    </mat-dialog-content>

    <mat-dialog-actions align="end">
      <button mat-button (click)="close()">
        <mat-icon>close</mat-icon>
        Close
      </button>
      <button 
        mat-raised-button 
        color="primary" 
        *ngIf="!data.booking"
        (click)="createBooking()">
        <mat-icon>add</mat-icon>
        Create Booking
      </button>
      <button 
        mat-raised-button 
        color="primary" 
        *ngIf="data.booking"
        (click)="viewFullBooking()">
        <mat-icon>visibility</mat-icon>
        View Full Booking
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    :host {
      display: block;
    }

    h2[mat-dialog-title] {
      display: flex;
      align-items: center;
      gap: 12px;
      margin: 0;
      padding: 20px 24px;
      color: #1976d2;
      font-size: 24px;
      font-weight: 500;
    }

    mat-dialog-content {
      padding: 24px;
      min-width: 500px;
    }

    .info-section {
      margin-bottom: 24px;
    }

    .info-section:last-child {
      margin-bottom: 0;
    }

    .info-section h3 {
      display: flex;
      align-items: center;
      gap: 8px;
      font-size: 18px;
      font-weight: 500;
      color: #424242;
      margin: 0 0 16px 0;
    }

    .info-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 16px;
    }

    .info-item {
      display: flex;
      flex-direction: column;
      gap: 4px;
    }

    .info-item .label {
      font-size: 12px;
      font-weight: 500;
      color: #757575;
      text-transform: uppercase;
      letter-spacing: 0.5px;
    }

    .info-item .value {
      font-size: 16px;
      color: #212121;
      font-weight: 400;
    }

    mat-divider {
      margin: 24px 0;
    }

    .status-container {
      margin-bottom: 16px;
    }

    .status-chip {
      font-size: 14px;
      font-weight: 500;
      padding: 8px 16px;
      height: auto;
    }

    .status-chip.available {
      background-color: #4caf50;
      color: white;
    }

    .status-chip.occupied {
      background-color: #f44336;
      color: white;
    }

    .status-chip.checkin {
      background-color: #2196f3;
      color: white;
    }

    .status-chip.checkout {
      background-color: #ff9800;
      color: white;
    }

    .status-chip.maintenance {
      background-color: #9e9e9e;
      color: white;
    }

    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      padding: 32px;
      text-align: center;
      color: #757575;
    }

    .empty-state mat-icon {
      font-size: 64px;
      width: 64px;
      height: 64px;
      margin-bottom: 16px;
      color: #4caf50;
    }

    .empty-state p {
      margin: 0;
      font-size: 16px;
    }

    mat-dialog-actions {
      padding: 16px 24px;
      gap: 8px;
    }
  `]
})
export class BookingDetailsDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<BookingDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: BookingDetailsData
  ) {}

  getDialogTitle(): string {
    return `Room ${this.data.room.roomNumber} - ${this.data.date.toLocaleDateString()}`;
  }

  getBookingIcon(): string {
    if (!this.data.booking) return 'event_available';
    
    switch (this.data.booking.type) {
      case 'checkin': return 'login';
      case 'checkout': return 'logout';
      case 'maintenance': return 'build';
      default: return 'person';
    }
  }

  getStatusClass(): string {
    if (!this.data.booking) return 'available';
    return this.data.booking.type || 'occupied';
  }

  getStatusLabel(): string {
    if (!this.data.booking) return 'Available';
    
    switch (this.data.booking.type) {
      case 'checkin': return 'Check-in Today';
      case 'checkout': return 'Check-out Today';
      case 'maintenance': return 'Under Maintenance';
      case 'occupied': return 'Occupied';
      default: return 'Occupied';
    }
  }

  close(): void {
    this.dialogRef.close();
  }

  createBooking(): void {
    // TODO: Navigate to booking creation page or open booking form dialog
    console.log('Create booking for room:', this.data.room.roomNumber, 'on', this.data.date);
    this.dialogRef.close({ action: 'create', room: this.data.room, date: this.data.date });
  }

  viewFullBooking(): void {
    // TODO: Navigate to booking details page
    console.log('View booking:', this.data.booking?.bookingId);
    this.dialogRef.close({ action: 'view', bookingId: this.data.booking?.bookingId });
  }
}
