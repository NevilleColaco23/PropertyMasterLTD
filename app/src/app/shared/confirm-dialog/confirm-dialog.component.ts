import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

export interface ConfirmDialogData {
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  icon?: string;
  iconColor?: string;
  confirmColor?: 'primary' | 'accent' | 'warn';
}

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule
  ],
  template: `
    <div class="confirm-dialog">
      <div class="dialog-header" [style.background]="getGradient()">
        <div class="icon-container">
          <mat-icon class="dialog-icon pulse-animation">{{ data.icon || 'warning' }}</mat-icon>
        </div>
      </div>

      <div class="dialog-content">
        <h2 class="dialog-title">{{ data.title }}</h2>
        <p class="dialog-message">{{ data.message }}</p>
      </div>

      <div class="dialog-actions">
        <button mat-stroked-button (click)="onCancel()" class="cancel-button">
          <mat-icon>close</mat-icon>
          {{ data.cancelText || 'Cancel' }}
        </button>
        <button mat-raised-button 
                [color]="data.confirmColor || 'warn'" 
                (click)="onConfirm()"
                class="confirm-button">
          <mat-icon>check</mat-icon>
          {{ data.confirmText || 'Confirm' }}
        </button>
      </div>
    </div>
  `,
  styles: [`
    .confirm-dialog {
      width: 100%;
      max-width: 440px;
      border-radius: 16px;
      overflow: hidden;
      box-shadow: 0 24px 48px rgba(0, 0, 0, 0.2), 0 8px 16px rgba(0, 0, 0, 0.1);
      background: white;
    }

    .dialog-header {
      padding: 32px 24px;
      text-align: center;
      position: relative;
      overflow: hidden;
    }

    .dialog-header::before {
      content: '';
      position: absolute;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: radial-gradient(circle at 50% 0%, rgba(255, 255, 255, 0.2) 0%, transparent 70%);
      pointer-events: none;
    }

    .icon-container {
      display: inline-block;
      background: rgba(255, 255, 255, 0.2);
      border-radius: 50%;
      padding: 16px;
      backdrop-filter: blur(10px);
    }

    .dialog-icon {
      font-size: 56px;
      width: 56px;
      height: 56px;
      color: white;
      display: inline-block;
      filter: drop-shadow(0 4px 8px rgba(0, 0, 0, 0.2));
    }

    .pulse-animation {
      animation: pulse 2s ease-in-out infinite;
    }

    @keyframes pulse {
      0%, 100% {
        transform: scale(1);
      }
      50% {
        transform: scale(1.08);
      }
    }

    .dialog-content {
      padding: 28px 32px;
      text-align: center;
      background: linear-gradient(to bottom, transparent, rgba(0, 0, 0, 0.01));
    }

    .dialog-title {
      margin: 0 0 16px 0;
      font-size: 22px;
      font-weight: 600;
      color: #1a1a1a;
      letter-spacing: -0.5px;
    }

    .dialog-message {
      margin: 0;
      font-size: 15px;
      color: #555;
      line-height: 1.7;
    }

    .dialog-actions {
      padding: 0 32px 28px 32px;
      display: flex;
      justify-content: flex-end;
      gap: 12px;
      background: rgba(0, 0, 0, 0.01);
    }

    .cancel-button {
      color: #666;
      border-color: #ddd;
      min-width: 110px;
      font-weight: 500;
      transition: all 0.3s ease;
    }

    .cancel-button:hover {
      background: rgba(0, 0, 0, 0.04);
      border-color: #999;
    }

    .cancel-button mat-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
      margin-right: 4px;
    }

    .confirm-button {
      min-width: 120px;
      font-weight: 600;
      box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
      transition: all 0.3s ease;
    }

    .confirm-button:hover {
      transform: translateY(-2px);
      box-shadow: 0 6px 16px rgba(0, 0, 0, 0.2);
    }

    .confirm-button mat-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
      margin-right: 4px;
    }

    /* Smooth entrance animation */
    :host {
      display: block;
      animation: slideInScale 0.4s cubic-bezier(0.34, 1.56, 0.64, 1);
    }

    @keyframes slideInScale {
      from {
        opacity: 0;
        transform: translateY(40px) scale(0.95);
      }
      to {
        opacity: 1;
        transform: translateY(0) scale(1);
      }
    }
  `]
})
export class ConfirmDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ConfirmDialogData
  ) {}

  onConfirm(): void {
    this.dialogRef.close(true);
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }

  getGradient(): string {
    const color = this.data.iconColor || '#ff9800';
    // Create gradient from base color
    const gradients: { [key: string]: string } = {
      '#4caf50': 'linear-gradient(135deg, #4caf50 0%, #388e3c 100%)', // Green
      '#f44336': 'linear-gradient(135deg, #f44336 0%, #d32f2f 100%)', // Red
      '#ff9800': 'linear-gradient(135deg, #ff9800 0%, #f57c00 100%)', // Orange
      '#2196f3': 'linear-gradient(135deg, #2196f3 0%, #1976d2 100%)', // Blue
      '#9c27b0': 'linear-gradient(135deg, #9c27b0 0%, #7b1fa2 100%)'  // Purple
    };
    return gradients[color] || `linear-gradient(135deg, ${color} 0%, ${this.darkenColor(color, 20)} 100%)`;
  }

  private darkenColor(color: string, percent: number): string {
    // Simple color darkening (works for hex colors)
    const num = parseInt(color.replace('#', ''), 16);
    const amt = Math.round(2.55 * percent);
    const R = (num >> 16) - amt;
    const G = (num >> 8 & 0x00FF) - amt;
    const B = (num & 0x0000FF) - amt;
    return '#' + (0x1000000 + (R < 255 ? R < 1 ? 0 : R : 255) * 0x10000
      + (G < 255 ? G < 1 ? 0 : G : 255) * 0x100
      + (B < 255 ? B < 1 ? 0 : B : 255))
      .toString(16).slice(1);
  }
}
