import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, FormArray } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatListModule } from '@angular/material/list';
import { MatChipsModule } from '@angular/material/chips';
import { MatTooltipModule } from '@angular/material/tooltip';

export interface PropertyDialogData {
  mode: 'add' | 'edit';
  existingProperty?: {
    id: number;
    name: string;
    isActive: boolean;
    companyLogoURL?: string;
    rooms?: RoomData[];
  };
}

export interface RoomData {
  id: string | number;
  roomName: string;
  roomCode: string;
  active: boolean;
  companyLogoURL?: string;
}

@Component({
  selector: 'app-property-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSlideToggleModule,
    MatListModule,
    MatChipsModule,
    MatTooltipModule
  ],
  templateUrl: './property-dialog.component.html',
  styleUrl: './property-dialog.component.css'
})
export class PropertyDialogComponent implements OnInit {
  propertyForm: FormGroup;
  selectedLogoFile: File | null = null;
  logoPreviewUrl: string | null = null;
  rooms: RoomData[] = [];

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<PropertyDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: PropertyDialogData
  ) {
    this.propertyForm = this.fb.group({
      name: [data.existingProperty?.name || '', [Validators.required, Validators.minLength(2)]],
      isActive: [data.existingProperty?.isActive ?? true],
      companyLogoURL: [data.existingProperty?.companyLogoURL || ''],
      newRoomName: [''],
      newRoomCode: ['']
    });

    // Initialize rooms from existing property
    this.rooms = data.existingProperty?.rooms || [];
    this.logoPreviewUrl = data.existingProperty?.companyLogoURL || null;
  }

  ngOnInit(): void {
    console.log('Property Dialog opened in mode:', this.data.mode);
  }

  onLogoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files[0]) {
      const file = input.files[0];

      // Validate file type
      if (!file.type.startsWith('image/')) {
        alert('Please select a valid image file');
        return;
      }

      // Validate file size (max 2MB)
      if (file.size > 2 * 1024 * 1024) {
        alert('File size must be less than 2MB');
        return;
      }

      this.selectedLogoFile = file;

      // Create preview
      const reader = new FileReader();
      reader.onload = (e) => {
        this.logoPreviewUrl = e.target?.result as string;
      };
      reader.readAsDataURL(file);
    }
  }

  triggerLogoUpload(): void {
    const fileInput = document.getElementById('logoFileInput') as HTMLInputElement;
    fileInput?.click();
  }

  removeLogo(): void {
    this.selectedLogoFile = null;
    this.logoPreviewUrl = null;
    this.propertyForm.patchValue({ companyLogoURL: '' });
  }

  addRoom(): void {
    const roomName = this.propertyForm.get('newRoomName')?.value?.trim();
    const roomCode = this.propertyForm.get('newRoomCode')?.value?.trim();

    if (!roomName || !roomCode) {
      alert('Please enter both room name and room code');
      return;
    }

    // Generate a simple integer ID based on current timestamp (last 9 digits)
    const roomId = Date.now() % 1000000000; // This gives us a 9-digit integer

    const newRoom: RoomData = {
      id: roomId,  // Now using integer instead of string
      roomName: roomName,
      roomCode: roomCode,
      active: true,
      companyLogoURL: ''
    };

    this.rooms.push(newRoom);

    // Clear the input fields
    this.propertyForm.patchValue({
      newRoomName: '',
      newRoomCode: ''
    });
  }

  removeRoom(index: number): void {
    if (confirm('Are you sure you want to remove this room?')) {
      this.rooms.splice(index, 1);
    }
  }

  toggleRoomActive(room: RoomData): void {
    room.active = !room.active;
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (this.propertyForm.valid) {
      // Validation: At least one room is required
      if (this.rooms.length === 0) {
        alert('Please add at least one room to the property.');
        return;
      }

      const formValue = this.propertyForm.value;

      // Convert logo to base64 or URL (simplified for now)
      const logoURL = this.logoPreviewUrl || formValue.companyLogoURL || '';

      this.dialogRef.close({
        name: formValue.name.trim(),
        isActive: formValue.isActive,
        companyLogoURL: logoURL,
        rooms: this.rooms,
        propertyId: this.data.existingProperty?.id
      });
    }
  }

  get dialogTitle(): string {
    return this.data.mode === 'add' ? 'Add New Property' : 'Edit Property';
  }

  get submitButtonText(): string {
    return this.data.mode === 'add' ? 'Create Property' : 'Update Property';
  }

  get nameError(): string {
    const nameControl = this.propertyForm.get('name');
    if (nameControl?.hasError('required')) {
      return 'Property name is required';
    }
    if (nameControl?.hasError('minlength')) {
      return 'Property name must be at least 2 characters';
    }
    return '';
  }
}
