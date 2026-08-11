import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../environments/environment';
import { PropertySelectionComponent } from '../property/property-selection/property-selection.component';

export interface MentionableUser {
  id: number;
  username: string;
  email: string;
  emailConfirmed: boolean;
  lockoutEnabled: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class MentionsService {
  private apiUrl = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) { }

  getUsersForCurrentProperty(): Observable<MentionableUser[]> {
    const propertyIds = PropertySelectionComponent.getSelectedPropertyIds();
    const propertyId = propertyIds?.[0];

    if (!propertyId) {
      return of([]);
    }

    return this.http.get<MentionableUser[]>(`${this.apiUrl}/by-property/${propertyId}`)
      .pipe(
        catchError(() => of([] as MentionableUser[]))
      );
  }
}
