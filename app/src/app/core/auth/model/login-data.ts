import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { Observable, map, catchError } from "rxjs";
import { environment } from '../../../../environments/environment';
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
  public event: any;
  private pathAPI: string = environment.apiHost;

  constructor(private http: HttpClient, private errorHandling: ErrorHandlingCommonServiceService) {
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
  