import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ActivitySummaryDTO, PagedActivitiesDTO, UserActivityDTO } from '../models/activity.models';
import { APP_CONFIG, IAppConfig } from '../configuration/app.config.token';

@Injectable({
  providedIn: 'root'
})
export class ActivityService {
  private readonly apiUrl: string;

  constructor(
    private http: HttpClient,
    @Inject(APP_CONFIG) private config: IAppConfig
  ) {
    this.apiUrl = `${this.config.apiEndpoint}/api/v1/activity`;
  }

  /**
   * Get activity summary for dashboard widget
   * Returns today/week/month counts and recent activities
   */
  getActivitySummary(recentCount: number = 10): Observable<ActivitySummaryDTO> {
    const params = new HttpParams().set('recentCount', recentCount.toString());
    return this.http.get<ActivitySummaryDTO>(`${this.apiUrl}/summary`, { params });
  }

  /**
   * Get recent activities
   */
  getRecentActivities(count: number = 20): Observable<UserActivityDTO[]> {
    const params = new HttpParams().set('count', count.toString());
    return this.http.get<UserActivityDTO[]>(`${this.apiUrl}/recent`, { params });
  }

  /**
   * Get activities for a specific user
   */
  getUserActivities(
    userId: number,
    from?: Date,
    to?: Date,
    limit?: number
  ): Observable<UserActivityDTO[]> {
    let params = new HttpParams();
    
    if (from) {
      params = params.set('from', from.toISOString());
    }
    if (to) {
      params = params.set('to', to.toISOString());
    }
    if (limit) {
      params = params.set('limit', limit.toString());
    }

    return this.http.get<UserActivityDTO[]>(`${this.apiUrl}/user/${userId}`, { params });
  }

  /**
   * Get activities by type
   */
  getActivitiesByType(
    activityType: string,
    from?: Date,
    to?: Date,
    limit?: number
  ): Observable<UserActivityDTO[]> {
    let params = new HttpParams();
    
    if (from) {
      params = params.set('from', from.toISOString());
    }
    if (to) {
      params = params.set('to', to.toISOString());
    }
    if (limit) {
      params = params.set('limit', limit.toString());
    }

    return this.http.get<UserActivityDTO[]>(`${this.apiUrl}/type/${activityType}`, { params });
  }

  /**
   * Get activities for a specific entity
   */
  getEntityActivities(
    entityType: string,
    entityId: number,
    from?: Date,
    to?: Date
  ): Observable<UserActivityDTO[]> {
    let params = new HttpParams();
    
    if (from) {
      params = params.set('from', from.toISOString());
    }
    if (to) {
      params = params.set('to', to.toISOString());
    }

    return this.http.get<UserActivityDTO[]>(
      `${this.apiUrl}/entity/${entityType}/${entityId}`,
      { params }
    );
  }

  /**
   * Get paginated and filtered activities
   */
  getActivitiesPaged(options: {
    userId?: number;
    activityType?: string;
    entityType?: string;
    from?: Date;
    to?: Date;
    page?: number;
    pageSize?: number;
    sortBy?: string;
    sortDescending?: boolean;
  }): Observable<PagedActivitiesDTO> {
    let params = new HttpParams();
    
    if (options.userId) {
      params = params.set('userId', options.userId.toString());
    }
    if (options.activityType) {
      params = params.set('activityType', options.activityType);
    }
    if (options.entityType) {
      params = params.set('entityType', options.entityType);
    }
    if (options.from) {
      params = params.set('from', options.from.toISOString());
    }
    if (options.to) {
      params = params.set('to', options.to.toISOString());
    }
    if (options.page !== undefined) {
      params = params.set('page', options.page.toString());
    }
    if (options.pageSize !== undefined) {
      params = params.set('pageSize', options.pageSize.toString());
    }
    if (options.sortBy) {
      params = params.set('sortBy', options.sortBy);
    }
    if (options.sortDescending !== undefined) {
      params = params.set('sortDescending', options.sortDescending.toString());
    }

    return this.http.get<PagedActivitiesDTO>(`${this.apiUrl}/paged`, { params });
  }
}
