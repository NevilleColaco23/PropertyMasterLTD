import { Injectable } from '@angular/core';
import { AppConfig } from '../../Appconfig';
import { HttpClient } from '@angular/common/http';
import { ErrorHandlingCommonServiceService } from '../../Common/Services/error-handling-common-service.service';
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

  constructor(private http: HttpClient, private config: AppConfig,private errorHandling: ErrorHandlingCommonServiceService) {
    this.pathAPI = this.config.setting['PathAPI'];
  }

  getDropdownOptions(): Observable<any[]> {
    console.log('Fetching dropdown options from API:', this.pathAPI + 'v1/property');
    return this.http.get<any>(this.pathAPI + 'v1/property').pipe(
      map(response => {
        // Ensure response.results is an array
        if (Array.isArray(response.results)) {
          return response.results.flatMap((user: { propertyList: { id: any, name: string }[] }) => user.propertyList.map(property => ({
            id: property.id,
            name: property.name
          })));
        } else {
          console.error('Unexpected response format:', response);
          return [];
        }
      }),
      catchError(this.errorHandling.handleError)
    );
  }
}
