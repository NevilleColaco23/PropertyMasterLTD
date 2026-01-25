import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { LOG_PAGE_NAVIGATION} from '../../../Common/Constants/Constants';

@Injectable({
  providedIn: 'root'
})
export class LoggingService {

  constructor(private http: HttpClient, private router: Router) {

    this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe((event: NavigationEnd) => {
      this.logPageNavigation(event.urlAfterRedirects, LOG_PAGE_NAVIGATION, LOG_PAGE_NAVIGATION);
    });
  }

  logPageNavigation(url: string, action : string, detail: string): void {
    const log = { AccessLog: url, timestamp: new Date(), Action: action, Detail: detail };
    this.http.post(`${environment.apiUrl}/accessLog/accessLog`, log).subscribe();
  }
}
