import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MAT_SNACK_BAR_DATA, MatSnackBarRef } from '@angular/material/snack-bar';

export interface SnackbarData {
  message: string;
  icon: string;
  type: 'success' | 'error' | 'warning' | 'info';
  action?: string;
}

@Component({
  selector: 'app-custom-snackbar',
  standalone: true,
  imports: [CommonModule, MatIconModule, MatButtonModule],
  template: `
    <div class="custom-snackbar-container">
      <div class="snackbar-icon-wrapper">
        <mat-icon class="snackbar-icon">{{ data.icon }}</mat-icon>
      </div>
      <div class="snackbar-message">{{ data.message }}</div>
      <button *ngIf="data.action" 
              mat-button 
              class="snackbar-action" 
              (click)="snackBarRef.dismissWithAction()">
        {{ data.action }}
      </button>
      <button mat-icon-button 
              class="snackbar-close" 
              (click)="snackBarRef.dismiss()">
        <mat-icon>close</mat-icon>
      </button>
    </div>
  `,
  styles: [`
    .custom-snackbar-container {
      display: flex;
      align-items: center;
      gap: 12px;
      padding: 4px 8px 4px 4px;
      min-height: 48px;
    }

    .snackbar-icon-wrapper {
      display: flex;
      align-items: center;
      justify-content: center;
      width: 40px;
      height: 40px;
      border-radius: 50%;
      background: rgba(255, 255, 255, 0.2);
      flex-shrink: 0;
    }

    .snackbar-icon {
      color: white;
      font-size: 22px;
      width: 22px;
      height: 22px;
      animation: iconPop 0.4s cubic-bezier(0.34, 1.56, 0.64, 1);
    }

    @keyframes iconPop {
      0% {
        transform: scale(0);
        opacity: 0;
      }
      50% {
        transform: scale(1.2);
      }
      100% {
        transform: scale(1);
        opacity: 1;
      }
    }

    .snackbar-message {
      flex: 1;
      color: white;
      font-size: 14px;
      font-weight: 500;
      line-height: 1.4;
    }

    .snackbar-action {
      color: white;
      font-weight: 600;
      text-transform: uppercase;
      font-size: 13px;
      letter-spacing: 0.5px;
    }

    .snackbar-close {
      color: rgba(255, 255, 255, 0.8);
      width: 32px;
      height: 32px;
    }

    .snackbar-close mat-icon {
      font-size: 18px;
      width: 18px;
      height: 18px;
    }

    .snackbar-close:hover {
      background: rgba(255, 255, 255, 0.1);
      color: white;
    }
  `]
})
export class CustomSnackbarComponent {
  constructor(
    @Inject(MAT_SNACK_BAR_DATA) public data: SnackbarData,
    public snackBarRef: MatSnackBarRef<CustomSnackbarComponent>
  ) {}
}
