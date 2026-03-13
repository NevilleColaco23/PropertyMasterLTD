import { Component, Inject, OnInit, ChangeDetectorRef } from '@angular/core';
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
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { CloudinaryUploadService } from '../../services/cloudinary-upload.service';

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
    MatTooltipModule,
    MatProgressSpinnerModule,
    MatProgressBarModule
  ],
  templateUrl: './property-dialog.component.html',
  styleUrl: './property-dialog.component.css'
})
export class PropertyDialogComponent implements OnInit {
  propertyForm: FormGroup;
  selectedLogoFile: File | null = null;
  logoPreviewUrl: string | null = null;
  rooms: RoomData[] = [];
  isUploadingLogo: boolean = false;
  uploadProgress: number = 0; // Track upload progress (0-100)
  uploadError: string | null = null;
  isSubmitting: boolean = false; // Track form submission state
  imageLoadError: boolean = false; // Track if image failed to load

  constructor(
    private fb: FormBuilder,
    public dialogRef: MatDialogRef<PropertyDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: PropertyDialogData,
    private cloudinaryService: CloudinaryUploadService,
    private cdr: ChangeDetectorRef
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

    // Debug: Log existing property data
    if (this.data.mode === 'edit' && this.data.existingProperty) {
      console.log('📝 Edit mode - Existing property data:', this.data.existingProperty);
      console.log('🖼️ Existing logo URL:', this.data.existingProperty.companyLogoURL);
      console.log('🏠 Existing rooms:', this.data.existingProperty.rooms);

      // Verify logoPreviewUrl was set
      console.log('🎨 logoPreviewUrl set to:', this.logoPreviewUrl);
    }
  }

  onLogoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files[0]) {
      const file = input.files[0];

      // Validate file type
      if (!file.type.startsWith('image/')) {
        this.uploadError = 'Please select a valid image file';
        return;
      }

      // Validate file size (max 2MB)
      if (file.size > 2 * 1024 * 1024) {
        this.uploadError = 'File size must be less than 2MB';
        return;
      }

      this.uploadError = null;
      this.selectedLogoFile = file; // Store file for later upload

      // Create local preview (base64) for UI feedback
      const reader = new FileReader();
      reader.onload = (e) => {
        this.logoPreviewUrl = e.target?.result as string;
      };
      reader.readAsDataURL(file);

      console.log('📁 Image selected:', file.name, 'Size:', (file.size / 1024).toFixed(2), 'KB');
      console.log('⏳ Image will be uploaded to Cloudinary when you click "Create Property"');
    }
  }

  triggerLogoUpload(): void {
    const fileInput = document.getElementById('logoFileInput') as HTMLInputElement;
    fileInput?.click();
  }

  removeLogo(): void {
    this.selectedLogoFile = null;
    this.logoPreviewUrl = null;
    this.uploadError = null;
    this.imageLoadError = false;
    this.propertyForm.patchValue({ companyLogoURL: '' });
  }

  onImageError(event: Event): void {
    console.error('❌ Failed to load image:', this.logoPreviewUrl);
    this.imageLoadError = true;
    this.uploadError = 'Failed to load existing image. You may need to upload a new one.';
  }

  onImageLoad(event: Event): void {
    console.log('✅ Image loaded successfully:', this.logoPreviewUrl);
    this.imageLoadError = false;
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

      // Use setTimeout to avoid change detection error
      setTimeout(() => {
        this.isSubmitting = true;
        this.cdr.detectChanges();

        // If user selected a logo, upload it to Cloudinary first
        if (this.selectedLogoFile) {
          console.log('📤 Uploading image to Cloudinary...');
          this.uploadToCloudinary();
        } else {
          // No new image selected, proceed with form submission
          this.submitForm();
        }
      }, 0);
    }
  }

  private uploadToCloudinary(): void {
    if (!this.selectedLogoFile) {
      this.submitForm();
      return;
    }

    // Set uploading state with setTimeout to avoid change detection error
    setTimeout(() => {
      this.isUploadingLogo = true;
      this.uploadProgress = 0;
      this.uploadError = null;
      this.cdr.detectChanges();
    }, 0);

    this.cloudinaryService.uploadImageWithProgress(this.selectedLogoFile, 'property-logos').subscribe({
      next: (progressData) => {
        this.uploadProgress = progressData.progress;
        console.log(`📊 Upload progress: ${progressData.progress}%`);
        this.cdr.detectChanges();

        // When upload is completed
        if (progressData.status === 'completed' && progressData.response) {
          console.log('✅ Cloudinary upload successful:', progressData.response);
          console.log('🖼️ Image URL:', progressData.response.secure_url);
          console.log('🆔 Public ID:', progressData.response.public_id);

          // Store the Cloudinary URL in the form
          this.propertyForm.patchValue({ companyLogoURL: progressData.response.secure_url });
          this.isUploadingLogo = false;
          this.cdr.detectChanges();

          // Now submit the form with the Cloudinary URL
          this.submitForm();
        }
      },
      error: (err) => {
        console.error('❌ Cloudinary upload failed:', err);
        this.uploadError = 'Failed to upload image. Please try again.';
        this.isUploadingLogo = false;
        this.uploadProgress = 0;
        this.isSubmitting = false;
        this.cdr.detectChanges();

        // Ask user if they want to proceed without image
        if (confirm('Image upload failed. Do you want to create the property without a logo?')) {
          this.propertyForm.patchValue({ companyLogoURL: '' });
          this.submitForm();
        }
      }
    });
  }

  private submitForm(): void {
    const formValue = this.propertyForm.value;

    // Use the Cloudinary URL from the form (or empty string if no image)
    const logoURL = formValue.companyLogoURL || '';

    console.log('💾 Submitting property with logo URL:', logoURL);
    console.log('🏠 Property ID:', this.data.existingProperty?.id);
    console.log('📝 Mode:', this.data.mode);
    console.log('📦 Full result object:', {
      name: formValue.name.trim(),
      isActive: formValue.isActive,
      companyLogoURL: logoURL,
      rooms: this.rooms,
      propertyId: this.data.existingProperty?.id
    });

    this.dialogRef.close({
      name: formValue.name.trim(),
      isActive: formValue.isActive,
      companyLogoURL: logoURL,
      rooms: this.rooms,
      propertyId: this.data.existingProperty?.id
    });
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
