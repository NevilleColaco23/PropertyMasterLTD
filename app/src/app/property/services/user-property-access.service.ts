import { Injectable, Inject, inject } from '@angular/core';
import { APP_CONFIG, AppConfig } from '../../configuration/app.config.token';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlingService } from '../../core/system/service/error-handling-service.service';
import { Observable } from 'rxjs';
import { map,catchError } from 'rxjs/operators';

export interface PropertyModel {
  id: number;
  name: string;
  rooms?: any[];
}

@Injectable({
  providedIn: 'root'
})
export class UserPropertyAccessService {
  public baseUrLocal!: string;
  public propertyNames: PropertyModel[] = [];
  public event: any;
  private pathAPI : string;
    private config = inject(APP_CONFIG);


  constructor(private http: HttpClient, private errorHandling : ErrorHandlingService
  ) { this.pathAPI = this.config.apiUrl; }


  getDropdownOptions(): Observable<PropertyModel[]> {
    const params:any = {
      PageIndex: 1,
      PageSize: 100,
      OrderBy: 'name',
      SearchItem: ''
    };

    // 🔐 SECURITY FIX: Call /accessible endpoint to get only user's accessible properties
    console.log('Fetching user-accessible properties from API:', `${this.pathAPI}/property/accessible`, params);

    return this.http
      .get<any>(`${this.pathAPI}/property/accessible`, { params })
      .pipe(
        map(response => {
          console.log('Property Selector API response:', response);

          if (Array.isArray(response.results)) {
            const mapped = response.results.map((property: any) => ({
              id: property.Id || property.id || property._id,
              name: property.Name || property.name,
              rooms: property.Rooms || property.rooms || []
            }));

            console.log('Properties accessible to user:', mapped);
            return mapped;
          } else {
            console.error('Unexpected response format: results is not an array');
            return [];
          }
        }),
        catchError(this.errorHandling.handleError)
      );
  }
}
