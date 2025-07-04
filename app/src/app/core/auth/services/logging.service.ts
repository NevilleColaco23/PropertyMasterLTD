import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router, NavigationEnd } from '@angular/router';
import { filter } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class LoggingService {

  constructor(private http: HttpClient, private router: Router) {
    this.router.events.pipe(filter(event => event instanceof NavigationEnd)).subscribe((event: NavigationEnd) => {
      this.logPageNavigation(event.urlAfterRedirects);
    });
  }

  logPageNavigation(url: string): void {
    const log = { AccessLog: url, timestamp: new Date() };
    this.http.post(`${environment.apiUrl}/accessLog`, log).subscribe();
  }
}
