import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, map, catchError } from "rxjs";
import { AppConfig } from "../../../Appconfig";
import { ErrorHandlingCommonServiceService } from '../../../Common/Services/error-handling-common-service.service';

export interface AuthenticationSuccessData {
    accessToken: string;
    tokenType: string;
    expiresIn: number;
    username: string;
    email: string;
    isExternalLogin: string;
    externalAuthenticationProvider: string;
  }
@Injectable({
  providedIn: 'root'
})
export class GetAllPropertiesService {
  public baseUrLocal: string;
  public event: any;
  private pathAPI: string;

  constructor(private http: HttpClient, private config: AppConfig, private errorHandling: ErrorHandlingCommonServiceService) {
    this.pathAPI = this.config.setting['PathAPI'];
    this.baseUrLocal = this.pathAPI;
  }

  getDropdownOptions(): Observable<any[]> {
    return this.http.get<any[]>(this.pathAPI + 'v1/property').pipe(
      map(response => {
        return (response as any).results.map((item: { id: any; name: any; }) => ({
          id: item.id,
          name: item.name
        }));
      }),
      catchError(this.errorHandling.handleError)
    );
  }

}
  