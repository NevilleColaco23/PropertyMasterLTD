import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

export interface PermissionDialogData {
  mode: 'add' | 'edit';
  userId: number;
  menuId?: number;
  menuLabel?: string;
  allMenus: any[];
  existingPermission?: {
    id: number;
    menuId: number;
    accessLevel: string;
    isActive: boolean;
    from: Date;
    to: Date;
  };
}

@Component({
  selector: 'app-permission-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule
  ],
  templateUrl: './permission-dialog.component.html',
  styleUrl: './permission-dialog.component.css'
})
export class PermissionDialogComponent implements OnInit {
  permissionForm: FormGroup;
  availableMenus: any[] = [];

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<PermissionDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: PermissionDialogData
  ) {
    this.permissionForm = this.fb.group({
      menuId: [data.menuId || null, Validators.required],
      accessLevel: [data.existingPermission?.accessLevel || 'read', Validators.required],
      isActive: [data.existingPermission?.isActive ?? true],
      from: [data.existingPermission?.from || new Date(), Validators.required],
      to: [data.existingPermission?.to || this.getDefaultEndDate(), Validators.required]
    });

    if (data.mode === 'edit' && data.menuId) {
      this.permissionForm.get('menuId')?.disable();
    }
  }

  ngOnInit(): void {
    console.log('Dialog data received:', this.data);
    console.log('All menus:', this.data.allMenus);

    // Show ALL menus
    this.availableMenus = this.data.allMenus;

    console.log('Available menus after filter:', this.availableMenus);

    // Log the first menu item to see its structure
    if (this.availableMenus.length > 0) {
      console.log('First menu item structure:', this.availableMenus[0]);
      console.log('First menu item keys:', Object.keys(this.availableMenus[0]));
    }
  }

  getDefaultEndDate(): Date {
    const date = new Date();
    date.setFullYear(date.getFullYear() + 1);
    return date;
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (this.permissionForm.valid) {
      const formValue = this.permissionForm.getRawValue();
      this.dialogRef.close({
        ...formValue,
        userId: this.data.userId,
        permissionId: this.data.existingPermission?.id
      });
    }
  }

  get dialogTitle(): string {
    return this.data.mode === 'add' ? 'Add Menu Permission' : 'Edit Menu Permission';
  }

  get submitButtonText(): string {
    return this.data.mode === 'add' ? 'Grant Access' : 'Update Permission';
  }

  getSelectedMenuLabel(): string {
    const selectedMenuId = this.permissionForm.get('menuId')?.value;
    const selectedMenu = this.availableMenus.find(m => m.id === selectedMenuId);
    return selectedMenu ? selectedMenu.label : '';
  }
}
