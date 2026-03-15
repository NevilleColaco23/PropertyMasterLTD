import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FormsModule } from '@angular/forms';
import { MatCheckboxModule } from '@angular/material/checkbox';

export interface SaveDashboardDialogData {
  dashboardName: string;
  isDefault: boolean;
}

@Component({
  selector: 'app-save-dashboard-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    FormsModule
  ],
  template: `
    <h2 mat-dialog-title>
      <mat-icon>save</mat-icon>
      Save Dashboard
    </h2>
    
    <mat-dialog-content>
      <p class="dialog-description">Give your customized dashboard a name</p>
      
      <mat-form-field appearance="outline" class="full-width">
        <mat-label>Dashboard Name</mat-label>
        <input 
          matInput 
          [(ngModel)]="data.dashboardName" 
          placeholder="e.g., My Custom Dashboard"
          (keyup.enter)="onSave()"
          #nameInput>
        <mat-icon matSuffix>edit</mat-icon>
      </mat-form-field>

      <div class="checkbox-container">
        <mat-checkbox [(ngModel)]="data.isDefault">
          Set as default dashboard
        </mat-checkbox>
        <p class="checkbox-hint">This dashboard will load automatically when you log in</p>
      </div>
    </mat-dialog-content>
    
    <mat-dialog-actions align="end">
      <button mat-button (click)="onCancel()">
        <mat-icon>cancel</mat-icon>
        Cancel
      </button>
      <button 
        mat-raised-button 
        color="primary" 
        (click)="onSave()"
        [disabled]="!data.dashboardName || data.dashboardName.trim().length === 0">
        <mat-icon>save</mat-icon>
        Save Dashboard
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .dialog-description {
      margin-bottom: 20px;
      color: #666;
      font-size: 14px;
    }

    .full-width {
      width: 100%;
      margin-bottom: 16px;
    }

    .checkbox-container {
      margin-top: 8px;
      margin-bottom: 16px;
    }

    .checkbox-hint {
      margin: 4px 0 0 32px;
      font-size: 12px;
      color: #999;
    }

    h2 {
      display: flex;
      align-items: center;
      gap: 12px;
      margin: 0;
      padding: 20px 24px;
      background: linear-gradient(135deg, #1976d2 0%, #1565c0 100%);
      color: white;
      margin: -24px -24px 0 -24px;
    }

    h2 mat-icon {
      font-size: 28px;
      width: 28px;
      height: 28px;
    }

    mat-dialog-content {
      padding: 24px;
      min-width: 400px;
    }

    mat-dialog-actions {
      padding: 16px 24px;
      margin-bottom: 0;
    }

    button mat-icon {
      margin-right: 4px;
    }
  `]
})
export class SaveDashboardDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<SaveDashboardDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: SaveDashboardDialogData
  ) {}

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.data.dashboardName && this.data.dashboardName.trim().length > 0) {
      this.dialogRef.close(this.data);
    }
  }
}
