import { Injectable } from '@angular/core';
import { MatSnackBar, MatSnackBarConfig, MatSnackBarHorizontalPosition, MatSnackBarVerticalPosition } from '@angular/material/snack-bar';
import { MatDialog, MatDialogConfig } from '@angular/material/dialog';
import { ConfirmDialogComponent, ConfirmDialogData } from '../shared/confirm-dialog/confirm-dialog.component';
import { CustomSnackbarComponent, SnackbarData } from '../shared/custom-snackbar/custom-snackbar.component';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  
  private defaultConfig: MatSnackBarConfig = {
    duration: 3000,
    horizontalPosition: 'right' as MatSnackBarHorizontalPosition,
    verticalPosition: 'bottom' as MatSnackBarVerticalPosition,
    panelClass: ['custom-snackbar']
  };

  constructor(
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {}

  /**
   * Show success notification (green) - bottom right
   */
  success(message: string, action: string = '', duration: number = 3000): void {
    const data: SnackbarData = {
      message,
      icon: 'check_circle',
      type: 'success',
      action
    };

    this.snackBar.openFromComponent(CustomSnackbarComponent, {
      ...this.defaultConfig,
      duration,
      data,
      panelClass: ['custom-snackbar', 'success-snackbar']
    });
  }

  /**
   * Show error notification (red) - bottom right
   */
  error(message: string, action: string = 'Close', duration: number = 5000): void {
    const data: SnackbarData = {
      message,
      icon: 'error',
      type: 'error',
      action
    };

    this.snackBar.openFromComponent(CustomSnackbarComponent, {
      ...this.defaultConfig,
      duration,
      data,
      panelClass: ['custom-snackbar', 'error-snackbar']
    });
  }

  /**
   * Show warning notification (orange) - bottom right
   */
  warning(message: string, action: string = 'Close', duration: number = 4000): void {
    const data: SnackbarData = {
      message,
      icon: 'warning',
      type: 'warning',
      action
    };

    this.snackBar.openFromComponent(CustomSnackbarComponent, {
      ...this.defaultConfig,
      duration,
      data,
      panelClass: ['custom-snackbar', 'warning-snackbar']
    });
  }

  /**
   * Show info notification (blue) - bottom right
   */
  info(message: string, action: string = '', duration: number = 3000): void {
    const data: SnackbarData = {
      message,
      icon: 'info',
      type: 'info',
      action
    };

    this.snackBar.openFromComponent(CustomSnackbarComponent, {
      ...this.defaultConfig,
      duration,
      data,
      panelClass: ['custom-snackbar', 'info-snackbar']
    });
  }

  /**
   * Show custom notification - bottom right
   */
  show(message: string, action: string = '', config?: MatSnackBarConfig): void {
    this.snackBar.open(message, action, {
      ...this.defaultConfig,
      ...config
    });
  }

  /**
   * Show beautiful confirmation dialog - bottom right
   */
  confirm(
    title: string,
    message: string,
    confirmText: string = 'Confirm',
    cancelText: string = 'Cancel',
    icon: string = 'warning',
    iconColor: string = '#ff9800',
    confirmColor: 'primary' | 'accent' | 'warn' = 'warn'
  ): Observable<boolean> {
    const dialogConfig: MatDialogConfig<ConfirmDialogData> = {
      width: '420px',
      position: {
        bottom: '20px',
        right: '20px'
      },
      panelClass: 'confirm-dialog-container',
      data: {
        title,
        message,
        confirmText,
        cancelText,
        icon,
        iconColor,
        confirmColor
      },
      hasBackdrop: true,
      backdropClass: 'confirm-dialog-backdrop'
    };

    const dialogRef = this.dialog.open(ConfirmDialogComponent, dialogConfig);
    return dialogRef.afterClosed();
  }

  /**
   * Quick confirmation for unsaved changes
   */
  confirmUnsavedChanges(): Observable<boolean> {
    return this.confirm(
      'Unsaved Changes',
      'You have unsaved changes. Do you want to discard them?',
      'Discard',
      'Keep Editing',
      'warning',
      '#ff9800',
      'warn'
    );
  }

  /**
   * Quick confirmation for delete actions
   */
  confirmDelete(itemName: string): Observable<boolean> {
    return this.confirm(
      'Delete Confirmation',
      `Delete "${itemName}"? This action cannot be undone.`,
      'Delete',
      'Cancel',
      'delete_forever',
      '#f44336',
      'warn'
    );
  }

  /**
   * Quick confirmation for discard actions
   */
  confirmDiscard(): Observable<boolean> {
    return this.confirm(
      'Discard Changes?',
      'All unsaved changes will be lost. This action cannot be undone.',
      'Discard',
      'Cancel',
      'cancel',
      '#ff9800',
      'warn'
    );
  }
}
