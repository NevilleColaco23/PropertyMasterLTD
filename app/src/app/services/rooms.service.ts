import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../environments/environment';

export interface RoomSummary {
  id: number;
  propertyId: number;
  roomCode: string;
  roomName: string;
  active: boolean;
}

@Injectable({ providedIn: 'root' })
export class RoomsService {
  // environment.apiUrl already contains the api base and version (e.g. /api/v1)
  private apiUrl = `${environment.apiUrl}/Rooms`;

  constructor(private http: HttpClient) {}

  // Get rooms filtered by property id
  getRoomsByProperty(propertyId: number): Observable<RoomSummary[]> {
    let params = new HttpParams().set('propertyId', propertyId.toString());
    return this.http.get<RoomSummary[]>(this.apiUrl, { params });
  }
}
