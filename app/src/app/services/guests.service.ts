import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

export interface GuestSummary {
  guestId: number;
  firstName: string;
  lastName: string;
  email?: string;
  phoneNumber?: string;
}

@Injectable({ providedIn: 'root' })
export class GuestsService {
  // environment.apiUrl already contains the api base and version (e.g. /api/v1)
  private apiUrl = `${environment.apiUrl}/Guests`;

  constructor(private http: HttpClient) {}

  // Simple search endpoint - backend should support a 'q' query param
  searchGuests(query: string, limit: number = 20): Observable<GuestSummary[]> {
    let params = new HttpParams().set('q', query).set('limit', limit.toString());
    return this.http.get<GuestSummary[]>(`${this.apiUrl}/search`, { params });
  }

  // Get single guest by id
  getGuestById(guestId: number): Observable<GuestSummary> {
    return this.http.get<GuestSummary>(`${this.apiUrl}/${guestId}`);
  }
}
