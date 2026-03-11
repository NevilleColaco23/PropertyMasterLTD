import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface BookingDTO {
  bookingId: string;
  propertyId: number;
  guestId: number;
  roomNumber: string;
  checkInDate: string;
  checkOutDate: string;
  // Add other booking properties as needed
}

export interface BookingsListResponse {
  results: BookingDTO[];
  pageIndex: number;
  pageSize: number;
  totalRowCount: number;
  // Add other response properties as needed
}

@Injectable({
  providedIn: 'root'
})
export class BookingsService {
  private apiUrl = `${environment.apiUrl}/api/v1/Bookings`;

  constructor(private http: HttpClient) { }

  getBookings(
    propertyIds?: number[],
    bookingId?: string,
    searchItem?: string,
    pageIndex: number = 1,
    pageSize: number = 10,
    orderBy: string = 'checkInDate',
    sortDirection: number = -1
  ): Observable<BookingsListResponse> {
    let params = new HttpParams()
      .set('PageIndex', pageIndex.toString())
      .set('PageSize', pageSize.toString())
      .set('OrderBy', orderBy)
      .set('ActiveSortDirection', sortDirection.toString());

    if (propertyIds && propertyIds.length > 0) {
      propertyIds.forEach(id => {
        params = params.append('PropertyIds', id.toString());
      });
    }
    if (bookingId) {
      params = params.set('BookingId', bookingId);
    }
    if (searchItem) {
      params = params.set('SearchItem', searchItem);
    }

    return this.http.get<BookingsListResponse>(`${this.apiUrl}/GetBookings`, { params });
  }

  // Helper method to get bookings for the currently selected properties
  getBookingsForSelectedProperty(
    searchItem?: string,
    pageIndex: number = 1,
    pageSize: number = 10
  ): Observable<BookingsListResponse> {
    const propertyIds = this.getSelectedPropertyIds();

    return this.getBookings(propertyIds, undefined, searchItem, pageIndex, pageSize);
  }

  private getSelectedPropertyIds(): number[] {
    const stored = localStorage.getItem('selectedPropertyIds');
    if (!stored) return [];
    try {
      return JSON.parse(stored) as number[];
    } catch {
      return [];
    }
  }
}
