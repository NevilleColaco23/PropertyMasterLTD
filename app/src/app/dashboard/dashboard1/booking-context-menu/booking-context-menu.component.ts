import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatMenuModule } from '@angular/material/menu';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';

export interface BookingContextMenuData {
  bookingId: string;
  guestName: string;
  roomNumber: string;
  checkInDate: Date;
  checkOutDate: Date;
  status: string;
  position: { x: number; y: number };
}

export interface BookingContextMenuResult {
  action: 'edit' | 'cancel' | 'upgrade' | 'extend' | 'shorten' | 'move' | 'view-details';
  bookingId: string;
}

@Component({
  selector: 'app-booking-context-menu',
  standalone: true,
  imports: [
    CommonModule,
    MatMenuModule,
    MatIconModule,
    MatDividerModule,
    MatDialogModule
  ],
  template: `
    <div class="context-menu-container">
      <div class="context-menu-header">
        <mat-icon class="header-icon">event</mat-icon>
        <div class="header-info">
          <h3>{{ data.guestName }}</h3>
          <p>{{ data.roomNumber }} • {{ formatDateRange() }}</p>
        </div>
      </div>

      <mat-divider></mat-divider>

      <div class="context-menu-actions">
        <!-- Primary Actions -->
        <button class="menu-item primary" (click)="onAction('view-details')">
          <mat-icon>visibility</mat-icon>
          <span>View Details</span>
          <span class="shortcut">Enter</span>
        </button>

        <button class="menu-item" (click)="onAction('edit')">
          <mat-icon>edit</mat-icon>
          <span>Edit Booking</span>
          <span class="shortcut">E</span>
        </button>

        <mat-divider></mat-divider>

        <!-- Modification Actions -->
        <button class="menu-item" (click)="onAction('extend')">
          <mat-icon>add_circle_outline</mat-icon>
          <span>Extend Stay</span>
        </button>

        <button class="menu-item" (click)="onAction('shorten')">
          <mat-icon>remove_circle_outline</mat-icon>
          <span>Shorten Stay</span>
        </button>

        <button class="menu-item" (click)="onAction('move')">
          <mat-icon>swap_horiz</mat-icon>
          <span>Move to Another Room</span>
        </button>

        <button class="menu-item" (click)="onAction('upgrade')">
          <mat-icon>upgrade</mat-icon>
          <span>Upgrade Room</span>
        </button>

        <mat-divider></mat-divider>

        <!-- Danger Actions -->
        <button class="menu-item danger" (click)="onAction('cancel')">
          <mat-icon>cancel</mat-icon>
          <span>Cancel Booking</span>
          <span class="shortcut">Del</span>
        </button>
      </div>
    </div>
  `,
  styles: [`
    .context-menu-container {
      min-width: 280px;
      max-width: 320px;
      background: white;
      border-radius: 8px;
      overflow: hidden;
      box-shadow: 0 8px 24px rgba(0, 0, 0, 0.15), 0 4px 12px rgba(0, 0, 0, 0.1);
    }

    .context-menu-header {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 16px;
      background: linear-gradient(135deg, #2196f3 0%, #1976d2 100%);
      color: white;
    }

    .header-icon {
      font-size: 32px;
      width: 32px;
      height: 32px;
    }

    .header-info {
      flex: 1;
    }

    .header-info h3 {
      margin: 0 0 4px 0;
      font-size: 16px;
      font-weight: 600;
    }

    .header-info p {
      margin: 0;
      font-size: 12px;
      opacity: 0.9;
    }

    .context-menu-actions {
      padding: 8px 0;
    }

    .menu-item {
      width: 100%;
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 12px 16px;
      border: none;
      background: transparent;
      cursor: pointer;
      font-size: 14px;
      color: #333;
      transition: all 0.2s ease;
      text-align: left;
    }

    .menu-item mat-icon {
      font-size: 20px;
      width: 20px;
      height: 20px;
      color: #666;
    }

    .menu-item span:first-of-type {
      flex: 1;
    }

    .shortcut {
      font-size: 11px;
      color: #999;
      background: #f5f5f5;
      padding: 2px 6px;
      border-radius: 4px;
      font-family: monospace;
    }

    .menu-item:hover {
      background: #f5f5f5;
    }

    .menu-item.primary {
      background: linear-gradient(to right, rgba(33, 150, 243, 0.1), transparent);
      font-weight: 500;
    }

    .menu-item.primary:hover {
      background: linear-gradient(to right, rgba(33, 150, 243, 0.15), transparent);
    }

    .menu-item.primary mat-icon {
      color: #2196f3;
    }

    .menu-item.danger {
      color: #f44336;
    }

    .menu-item.danger mat-icon {
      color: #f44336;
    }

    .menu-item.danger:hover {
      background: #ffebee;
    }

    mat-divider {
      margin: 8px 0;
    }
  `]
})
export class BookingContextMenuComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA) public data: BookingContextMenuData,
    private dialogRef: MatDialogRef<BookingContextMenuComponent>
  ) {}

  formatDateRange(): string {
    const checkIn = new Date(this.data.checkInDate);
    const checkOut = new Date(this.data.checkOutDate);
    
    const options: Intl.DateTimeFormatOptions = { month: 'short', day: 'numeric' };
    return `${checkIn.toLocaleDateString('en-US', options)} - ${checkOut.toLocaleDateString('en-US', options)}`;
  }

  onAction(action: BookingContextMenuResult['action']): void {
    this.dialogRef.close({
      action,
      bookingId: this.data.bookingId
    });
  }
}
