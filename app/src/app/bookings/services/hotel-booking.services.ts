import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { delay, map } from 'rxjs/operators';
import { HotelBooking } from '../Models/HotelBooking-Model';

@Injectable({
  providedIn: 'root'
})
export class HotelBookingService {

  private mockBookings: HotelBooking[] = [
    {
      id: 'b1',
      roomNumber: '101',
      guestName: 'Alice Smith',
      checkInDate: new Date('2025-07-21T15:00:00'),
      checkOutDate: new Date('2025-07-23T11:00:00'),
      status: 'confirmed',
      allDay: false
    },
    {
      id: 'b2',
      roomNumber: '205',
      guestName: 'Bob Johnson',
      checkInDate: new Date('2025-07-22T15:00:00'),
      checkOutDate: new Date('2025-07-25T11:00:00'),
      status: 'pending',
      allDay: false
    },
    {
      id: 'b3',
      roomNumber: '302',
      guestName: 'Charlie Brown',
      checkInDate: new Date('2025-07-26T15:00:00'),
      checkOutDate: new Date('2025-07-28T11:00:00'),
      status: 'confirmed',
      allDay: false
    },
    {
      id: 'b4',
      roomNumber: '101',
      guestName: 'Diana Prince',
      checkInDate: new Date('2025-07-29T15:00:00'),
      checkOutDate: new Date('2025-08-01T11:00:00'),
      status: 'confirmed',
      allDay: false
    },
    {
      id: 'b5',
      roomNumber: 'VIP Suite',
      guestName: 'Bruce Wayne',
      checkInDate: new Date('2025-08-05T00:00:00'),
      checkOutDate: new Date('2025-08-08T00:00:00'),
      status: 'confirmed',
      allDay: true // An all-day booking example
    }
  ];

  constructor() { }

  /**
   * Fetches hotel bookings for a given date range.
   * @param startDate The start of the visible calendar range.
   * @param endDate The end of the visible calendar range.
   */
  getHotelBookings(startDate: Date, endDate: Date): Observable<HotelBooking[]> {
    console.log(`Fetching bookings from ${startDate.toISOString()} to ${endDate.toISOString()}`);
    return of(this.mockBookings.filter(booking =>
      // Filter bookings that overlap with the requested date range
      booking.checkInDate < endDate && booking.checkOutDate > startDate
    )).pipe(delay(500)); // Simulate network delay
  }

  // Simulate updating a booking (e.g., after drag-and-drop)
  updateHotelBooking(updatedBooking: HotelBooking): Observable<HotelBooking> {
    const index = this.mockBookings.findIndex(b => b.id === updatedBooking.id);
    if (index > -1) {
      this.mockBookings[index] = { ...this.mockBookings[index], ...updatedBooking };
      console.log('Booking updated:', this.mockBookings[index]);
      return of(this.mockBookings[index]).pipe(delay(200));
    }
    return of(updatedBooking).pipe(delay(200));
  }

  // Simulate creating a booking
  createHotelBooking(newBooking: Partial<HotelBooking>): Observable<HotelBooking> {
    const id = `b${this.mockBookings.length + 1}`;
    const booking = { id, ...newBooking } as HotelBooking;
    this.mockBookings.push(booking);
    console.log('Booking created:', booking);
    return of(booking).pipe(delay(200));
  }

  // Simulate deleting a booking
  deleteHotelBooking(id: string): Observable<void> {
    this.mockBookings = this.mockBookings.filter(b => b.id !== id);
    console.log('Booking deleted:', id);
    return of(undefined).pipe(delay(200));
  }
}