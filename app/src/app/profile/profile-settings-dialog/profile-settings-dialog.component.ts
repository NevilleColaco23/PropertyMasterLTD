import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialogModule, MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';

export interface ProfileSettingsData {
  username: string;
  email: string;
}

@Component({
  selector: 'app-profile-settings-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule
  ],
  templateUrl: './profile-settings-dialog.component.html',
  styleUrls: ['./profile-settings-dialog.component.css']
})
export class ProfileSettingsDialogComponent {
  form: FormGroup;

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<ProfileSettingsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: ProfileSettingsData
  ) {
    this.form = this.fb.group({
      username: [data.username, [Validators.required, Validators.minLength(2)]],
      email: [data.email, [Validators.required, Validators.email]]
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSave(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    this.dialogRef.close(this.form.value as ProfileSettingsData);
  }
}
