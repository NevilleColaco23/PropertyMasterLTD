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
export class GetAllPropertiesServiceService {
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
    console.log('Fetching properties from API:', `${this.pathAPI}/property`, params);
  return this.http
    .get<any>(`${this.pathAPI}/property`, { params })
    .pipe(
      map(response => {
        if (Array.isArray(response.results)) {
          return response.results.map((property: any) => ({
            id: property.Id || property.id || property._id,  // Handle Id, id, and _id
            name: property.Name || property.name,
            rooms: property.Rooms || property.rooms || []
          }));
        } else {
          console.error('Unexpected response format: results is not an array');
          return [];
        }
      }),
      catchError(this.errorHandling.handleError)
    );
}
}
