import { Injectable, Inject } from '@angular/core';
import { APP_CONFIG, AppConfig } from '../../app.config.token';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlingService } from '../../core/system-messages-snackbar/service/error-handling-service.service';
import { Observable } from 'rxjs';
import { map,catchError } from 'rxjs/operators';

export interface PropertyModel {
  name: string;
}

@Injectable({
  providedIn: 'root'
})
export class GetAllPropertiesServiceService {
  public baseUrLocal!: string;
  public propertyNames: PropertyModel[] = [];
  public event: any;
  private pathAPI : string;

  constructor(private http: HttpClient, private errorHandling : ErrorHandlingService, @Inject(APP_CONFIG) private appConfig: AppConfig
  ) { this.pathAPI = this.appConfig.apiUrl; }


  getDropdownOptions(): Observable<PropertyModel[]> {

  return this.http.get<any>(this.pathAPI + 'v1/property').pipe(
    map(response => {
      if (Array.isArray(response.results)) {
        return response.results.map((property: any) => ({
          id: property.id,
          name: property.name,
          rooms: property.rooms ?? []
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
