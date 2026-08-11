import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../environments/environment';

export interface FeedSettings {
  maxPostsToShow: number;
}

@Injectable({
  providedIn: 'root'
})
export class FeedSettingsService {
  private apiUrl = `${environment.apiUrl}/feed-settings`;

  constructor(private http: HttpClient) { }

  get(): Observable<FeedSettings> {
    return this.http.get<FeedSettings>(this.apiUrl)
      .pipe(
        catchError(() => of({ maxPostsToShow: 50 } as FeedSettings))
      );
  }

  save(maxPostsToShow: number): Observable<void> {
    return this.http.put<void>(this.apiUrl, { maxPostsToShow })
      .pipe(
        catchError(() => of(void 0))
      );
  }
}
