export interface HotelBooking {
  id: string;
  roomNumber: string; // Or roomId if you have a separate Room entity
  guestName: string;
  checkInDate: Date; // Use Date objects for easier manipulation
  checkOutDate: Date;
  status: 'confirmed' | 'pending' | 'cancelled';
  notes?: string;
  allDay?: boolean; // FullCalendar uses this for multi-day events
}