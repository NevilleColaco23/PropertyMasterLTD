import { Component, inject, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule, AbstractControl, FormControl } from '@angular/forms';
import { BookingsService } from '../services/bookings.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { Subject, Subscription } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { PropertySelectionComponent } from '../../property/property-selection/property-selection.component';
import { GuestsService, GuestSummary } from '../../services/guests.service';
import { RoomsService, RoomSummary } from '../../services/rooms.service';

function dateRangeValidator(group: AbstractControl) {
  const start = group.get('checkInDate')?.value;
  const end = group.get('checkOutDate')?.value;
  if (!start || !end) return null;
  const s = new Date(start);
  const e = new Date(end);
  return e > s ? null : { dateRange: true };
}

@Component({
  selector: 'app-booking-entry',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatSelectModule, MatButtonModule, MatIconModule, MatDatepickerModule, MatNativeDateModule, MatCheckboxModule, MatAutocompleteModule, MatProgressSpinnerModule],
  templateUrl: './booking-entry.component.html',
  styleUrls: ['./booking-entry.component.css']
})
export class BookingEntryComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  bookingsService = inject(BookingsService);
  router = inject(Router);
  private guestsService = inject(GuestsService);
  private roomsService = inject(RoomsService);

  // UI helpers
  selectedPropertyId: number | null = null;
  guestSearch = new FormControl('');
  guestOptions: GuestSummary[] = [];
  guestLoading = false;

  roomSearch = new FormControl('');
  allRoomOptions: RoomSummary[] = [];   // keep master copy for filtering
  roomOptions: RoomSummary[] = [];

  private subs = new Subscription();

  bookingForm: FormGroup;

  paymentStatuses = [
    { id: 50000000000000013, label: 'Paid' },
    { id: 50000000000000012, label: 'Pending' },
    { id: 50000000000000011, label: 'Cancelled' },
  ];

  bookingSources = [
    { id: 50000000000000002, label: 'Walk-in' },
    { id: 50000000000000003, label: 'Phone' },
    { id: 50000000000000004, label: 'OTA' },
  ];

  constructor() {
    this.bookingForm = this.fb.group({
      bookingId: [''],
      // propertyId will be set automatically from selected property
      propertyId: [null],
      guestId: [null, [Validators.required]],
      roomNumber: ['', [Validators.required, Validators.maxLength(10)]],
      checkInDate: [null, [Validators.required]],
      checkOutDate: [null, [Validators.required]],
      numberOfGuests: [1, [Validators.required, Validators.min(1), Validators.max(20)]],
      totalPrice: [0, [Validators.required, Validators.min(0)]],
      paymentStatusId: [this.paymentStatuses[1].id, [Validators.required]],
      bookingSourceId: [this.bookingSources[0].id, [Validators.required]],
      specialRequests: [''],
      isConfirmed: [true]
    }, { validators: dateRangeValidator });
  }

  ngOnInit(): void {
    const ids = PropertySelectionComponent.getSelectedPropertyIds();
    if (!ids || ids.length === 0) {
      console.warn('No property selected - redirecting to property selector');
      this.router.navigate(['/propertySelector']);
      return;
    }

    this.selectedPropertyId = ids[0];
    this.bookingForm.patchValue({ propertyId: this.selectedPropertyId });
    this.loadRoomsForProperty(this.selectedPropertyId);

    // Guest search: debounce 300ms, cancel in-flight requests on new input
    const guestSub = this.guestSearch.valueChanges.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap((v: string | null) => {
        const term = (v || '').trim();
        if (term.length < 2) {
          this.guestOptions = [];
          this.guestLoading = false;
          return of([]);
        }
        this.guestLoading = true;
        return this.guestsService.searchGuests(term).pipe(
          catchError(err => {
            console.error('Guest search error', err);
            return of([]);
          })
        );
      })
    ).subscribe((results: GuestSummary[]) => {
      this.guestOptions = results || [];
      this.guestLoading = false;
    });

    // Room filter: filter from master list so filtering is non-destructive
    const roomSub = this.roomSearch.valueChanges.pipe(debounceTime(150)).subscribe((v: string | null) => {
      const term = (v || '').trim().toLowerCase();
      this.roomOptions = term
        ? this.allRoomOptions.filter(r => `${r.roomCode} ${r.roomName}`.toLowerCase().includes(term))
        : [...this.allRoomOptions];
    });

    this.subs.add(guestSub);
    this.subs.add(roomSub);
  }

  ngOnDestroy(): void {
    this.subs.unsubscribe();
  }

  private loadRoomsForProperty(propertyId: number) {
    this.roomsService.getRoomsByProperty(propertyId).subscribe(list => {
      this.allRoomOptions = (list || []).filter(r => r.active !== false);
      this.roomOptions = [...this.allRoomOptions];
    });
  }

  onGuestSelected(guest: GuestSummary) {
    if (!guest) return;
    this.bookingForm.patchValue({ guestId: guest.guestId });
    // Use emitEvent: false so valueChanges doesn't fire another search
    this.guestSearch.setValue(`${guest.firstName} ${guest.lastName}`, { emitEvent: false });
    this.guestOptions = [];
  }

  onRoomSelected(room: RoomSummary) {
    if (!room) return;
    this.bookingForm.patchValue({ roomNumber: room.roomCode });
    // Use emitEvent: false so valueChanges doesn't re-filter
    this.roomSearch.setValue(`${room.roomCode} - ${room.roomName}`, { emitEvent: false });
  }

  get f() { return this.bookingForm.controls; }

  submit(): void {
    if (this.bookingForm.invalid) {
      this.bookingForm.markAllAsTouched();
      return;
    }

    const payload = this.bookingForm.value;

    // Provide some lightweight UX feedback via console and then call service.
    console.log('Submitting booking', payload);

    this.bookingsService.createBooking(payload).subscribe({
      next: (res: any) => {
        console.log('Booking created', res);
        // Navigate to bookings report after successful create
        this.router.navigate(['/bookings']);
      },
      error: (err: any) => {
        console.error('Failed to create booking', err);
        // keep form so user can correct
      }
    });
  }
}
