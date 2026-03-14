import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { environment } from '../environments/environment';
import {
  DashboardConfiguration,
  WidgetLibraryItem,
  DashboardTemplate,
  SaveDashboardRequest,
  GetWidgetLibraryParams,
  GetDashboardTemplatesParams,
  KpiValueResponse,
  ActivityItemResponse,
  CalendarEventResponse
} from '../models/dashboard.models';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  private readonly apiUrl = `${environment.apiUrl}/dashboard`;

  constructor(private http: HttpClient) {}

  /**
   * Get user's dashboard (default or specific)
   * Returns null if dashboard doesn't exist (404)
   */
  getDashboardByUserId(userId: number, defaultOnly: boolean = true): Observable<DashboardConfiguration | null> {
    let params = new HttpParams();
    if (defaultOnly) {
      params = params.set('defaultOnly', 'true');
    }
    return this.http.get<DashboardConfiguration | null>(`${this.apiUrl}/user/${userId}`, { params })
      .pipe(
        catchError((error: HttpErrorResponse) => {
          // If 404, dashboard doesn't exist yet - return null instead of error
          if (error.status === 404) {
            console.log('No dashboard found for user, will use defaults');
            return of(null);
          }
          // For other errors, rethrow
          throw error;
        })
      );
  }

  /**
   * Get all dashboards for a user
   */
  getUserDashboards(userId: number): Observable<DashboardConfiguration[]> {
    return this.http.get<DashboardConfiguration[]>(`${this.apiUrl}/user/${userId}/all`);
  }

  /**
   * Save or update dashboard configuration
   */
  saveDashboard(request: SaveDashboardRequest): Observable<string> {
    return this.http.post<string>(this.apiUrl, request);
  }

  /**
   * Delete a dashboard
   */
  deleteDashboard(dashboardId: string, userId: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/${dashboardId}`, {
      params: new HttpParams().set('userId', userId.toString())
    });
  }

  /**
   * Set a dashboard as default
   */
  setDefaultDashboard(dashboardId: string, userId: number): Observable<boolean> {
    return this.http.put<boolean>(`${this.apiUrl}/${dashboardId}/default`, { userId });
  }

  /**
   * Get widget library (available widgets)
   */
  getWidgetLibrary(params?: GetWidgetLibraryParams): Observable<WidgetLibraryItem[]> {
    let httpParams = new HttpParams();
    if (params?.category) {
      httpParams = httpParams.set('category', params.category);
    }
    if (params?.activeOnly !== undefined) {
      httpParams = httpParams.set('activeOnly', params.activeOnly.toString());
    }
    return this.http.get<WidgetLibraryItem[]>(`${this.apiUrl}/widgets`, { params: httpParams });
  }

  /**
   * Get dashboard templates
   */
  getDashboardTemplates(params?: GetDashboardTemplatesParams): Observable<DashboardTemplate[]> {
    let httpParams = new HttpParams();
    if (params?.roleId !== undefined) {
      httpParams = httpParams.set('roleId', params.roleId.toString());
    }
    if (params?.publicOnly !== undefined) {
      httpParams = httpParams.set('publicOnly', params.publicOnly.toString());
    }
    return this.http.get<DashboardTemplate[]>(`${this.apiUrl}/templates`, { params: httpParams });
  }

  /**
   * Reset dashboard to a template
   */
  resetDashboardToTemplate(templateId: string, userId: number): Observable<string> {
    return this.http.post<string>(`${this.apiUrl}/reset-to-template`, {
      templateId,
      userId
    });
  }

  // ==========================================
  // PHASE 4: REAL DATA API METHODS
  // ==========================================

  /**
   * Get KPI value for a specific widget
   */
  getKpiValue(widgetId: string, userId: number): Observable<KpiValueResponse> {
    const params = new HttpParams().set('userId', userId.toString());
    return this.http.get<KpiValueResponse>(`${this.apiUrl}/kpi/${widgetId}`, { params });
  }

  /**
   * Get recent activity for dashboard
   */
  getRecentActivity(userId: number, limit: number = 10): Observable<ActivityItemResponse[]> {
    const params = new HttpParams()
      .set('userId', userId.toString())
      .set('limit', limit.toString());
    return this.http.get<ActivityItemResponse[]>(`${this.apiUrl}/activity/recent`, { params });
  }

  /**
   * Get calendar events for dashboard
   */
  getCalendarEvents(
    userId: number,
    startDate?: Date,
    endDate?: Date
  ): Observable<CalendarEventResponse[]> {
    let params = new HttpParams().set('userId', userId.toString());

    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }

    return this.http.get<CalendarEventResponse[]>(`${this.apiUrl}/activity/calendar-events`, { params });
  }
}
