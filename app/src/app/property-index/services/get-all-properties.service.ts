
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map,catchError } from 'rxjs/operators';
import { Observable } from 'rxjs';
import { AppConfig } from '../../Appconfig';
import { ErrorHandlingCommonServiceService } from '../../Common/error-handling-common.service';

export interface PropertyModel {
  name: string;
}

@Injectable({
  providedIn: 'root'
})

export class GetAllPropertiesService {
  public baseUrLocal: string;
  public propertyNames: PropertyModel[] = [];
  public event: any;
  private pathAPI : string;

  constructor(private http: HttpClient, private config: AppConfig,private errorHandling: ErrorHandlingCommonServiceService) {
    this.pathAPI = this.config.setting['PathAPI'];
  }

  getDropdownOptions(): Observable<any[]> {
    return this.http.get<any>(this.pathAPI + 'v1/property').pipe(
      map(response => {
        // Ensure response.results is an array
        if (Array.isArray(response.results)) {
          return response.results.flatMap(user => user.propertyList.map(property => ({
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
