import { Injectable, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ActivityAnalyticsSummary,
  UserActivityStats,
  ActivityTypeDistribution,
  EntityAccessStats,
  PeakUsageTime,
  DailyActivityTrend,
  FailedLoginAttempt,
  SecurityAlertSummary,
  PerformanceMetrics,
  ActivityExport
} from '../models/analytics.models';
import { APP_CONFIG, IAppConfig } from '../configuration/app.config.token';

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private readonly apiUrl: string;

  constructor(
    private http: HttpClient,
    @Inject(APP_CONFIG) private config: IAppConfig
  ) {
    this.apiUrl = `${this.config.apiEndpoint}/api/v1/activity/analytics`;
  }

  /**
   * Get overall activity analytics summary
   * @param startDate Optional start date filter
   * @param endDate Optional end date filter
   * @returns Observable of ActivityAnalyticsSummary
   */
  getAnalyticsSummary(
    startDate?: Date,
    endDate?: Date
  ): Observable<ActivityAnalyticsSummary> {
    let params = new HttpParams();
    
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }

    return this.http.get<ActivityAnalyticsSummary>(`${this.apiUrl}/summary`, { params });
  }

  /**
   * Get top active users
   * @param limit Number of users to return (default: 10)
   * @param startDate Optional start date filter
   * @param endDate Optional end date filter
   * @returns Observable array of UserActivityStats
   */
  getTopActiveUsers(
    limit: number = 10,
    startDate?: Date,
    endDate?: Date
  ): Observable<UserActivityStats[]> {
    let params = new HttpParams().set('limit', limit.toString());
    
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }

    return this.http.get<UserActivityStats[]>(`${this.apiUrl}/top-users`, { params });
  }

  /**
   * Get activity type distribution
   * @param startDate Optional start date filter
   * @param endDate Optional end date filter
   * @returns Observable array of ActivityTypeDistribution
   */
  getActivityDistribution(
    startDate?: Date,
    endDate?: Date
  ): Observable<ActivityTypeDistribution[]> {
    let params = new HttpParams();
    
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }

    return this.http.get<ActivityTypeDistribution[]>(`${this.apiUrl}/distribution`, { params });
  }

  /**
   * Get most accessed entities
   * @param entityType Optional entity type filter (e.g., 'Property', 'User')
   * @param limit Number of entities to return (default: 10)
   * @param startDate Optional start date filter
   * @param endDate Optional end date filter
   * @returns Observable array of EntityAccessStats
   */
  getMostAccessedEntities(
    entityType?: string,
    limit: number = 10,
    startDate?: Date,
    endDate?: Date
  ): Observable<EntityAccessStats[]> {
    let params = new HttpParams().set('limit', limit.toString());
    
    if (entityType) {
      params = params.set('entityType', entityType);
    }
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }

    return this.http.get<EntityAccessStats[]>(`${this.apiUrl}/top-entities`, { params });
  }

  /**
   * Get peak usage times (hourly activity distribution)
   * @param startDate Optional start date filter
   * @param endDate Optional end date filter
   * @returns Observable array of PeakUsageTime
   */
  getPeakUsageTimes(
    startDate?: Date,
    endDate?: Date
  ): Observable<PeakUsageTime[]> {
    let params = new HttpParams();
    
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }

    return this.http.get<PeakUsageTime[]>(`${this.apiUrl}/peak-times`, { params });
  }

  /**
   * Get daily activity trends
   * @param days Number of days to analyze (default: 30)
   * @returns Observable array of DailyActivityTrend
   */
  getDailyTrends(days: number = 30): Observable<DailyActivityTrend[]> {
    const params = new HttpParams().set('days', days.toString());
    return this.http.get<DailyActivityTrend[]>(`${this.apiUrl}/trends`, { params });
  }

  /**
   * Get failed login attempts
   * @param limit Number of attempts to return (default: 50)
   * @param startDate Optional start date filter
   * @param endDate Optional end date filter
   * @returns Observable array of FailedLoginAttempt
   */
  getFailedLoginAttempts(
    limit: number = 50,
    startDate?: Date,
    endDate?: Date
  ): Observable<FailedLoginAttempt[]> {
    let params = new HttpParams().set('limit', limit.toString());
    
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }

    return this.http.get<FailedLoginAttempt[]>(`${this.apiUrl}/security/failed-logins`, { params });
  }

  /**
   * Get security alerts summary
   * @param hours Number of hours to analyze (default: 24)
   * @returns Observable of SecurityAlertSummary
   */
  getSecurityAlerts(hours: number = 24): Observable<SecurityAlertSummary> {
    const params = new HttpParams().set('hours', hours.toString());
    return this.http.get<SecurityAlertSummary>(`${this.apiUrl}/security/alerts`, { params });
  }

  /**
   * Get performance metrics by activity type
   * @param startDate Optional start date filter
   * @param endDate Optional end date filter
   * @returns Observable array of PerformanceMetrics
   */
  getPerformanceMetrics(
    startDate?: Date,
    endDate?: Date
  ): Observable<PerformanceMetrics[]> {
    let params = new HttpParams();
    
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }

    return this.http.get<PerformanceMetrics[]>(`${this.apiUrl}/performance`, { params });
  }

  /**
   * Export activities with filters
   * @param userId Optional user filter
   * @param activityType Optional activity type filter
   * @param entityType Optional entity type filter
   * @param startDate Optional start date filter
   * @param endDate Optional end date filter
   * @param includeSuccessful Include successful activities (default: true)
   * @param includeFailed Include failed activities (default: true)
   * @returns Observable array of ActivityExport
   */
  exportActivities(
    userId?: number,
    activityType?: string,
    entityType?: string,
    startDate?: Date,
    endDate?: Date,
    includeSuccessful: boolean = true,
    includeFailed: boolean = true
  ): Observable<ActivityExport[]> {
    let params = new HttpParams()
      .set('includeSuccessful', includeSuccessful.toString())
      .set('includeFailed', includeFailed.toString());
    
    if (userId !== undefined) {
      params = params.set('userId', userId.toString());
    }
    if (activityType) {
      params = params.set('activityType', activityType);
    }
    if (entityType) {
      params = params.set('entityType', entityType);
    }
    if (startDate) {
      params = params.set('startDate', startDate.toISOString());
    }
    if (endDate) {
      params = params.set('endDate', endDate.toISOString());
    }

    return this.http.get<ActivityExport[]>(`${this.apiUrl}/export`, { params });
  }

  /**
   * Helper method to get analytics for a specific time range
   * @param rangeType 'today' | 'week' | 'month' | 'custom'
   * @param customStartDate For custom range
   * @param customEndDate For custom range
   * @returns Object with startDate and endDate
   */
  getDateRange(
    rangeType: 'today' | 'week' | 'month' | 'custom',
    customStartDate?: Date,
    customEndDate?: Date
  ): { startDate: Date; endDate: Date } {
    const endDate = new Date();
    let startDate = new Date();

    switch (rangeType) {
      case 'today':
        startDate.setHours(0, 0, 0, 0);
        break;
      case 'week':
        startDate.setDate(endDate.getDate() - 7);
        break;
      case 'month':
        startDate.setMonth(endDate.getMonth() - 1);
        break;
      case 'custom':
        if (customStartDate && customEndDate) {
          return { startDate: customStartDate, endDate: customEndDate };
        }
        break;
    }

    return { startDate, endDate };
  }

  /**
   * Helper method to download export data as CSV
   * @param data ActivityExport array
   * @param filename Filename for download
   */
  downloadAsCSV(data: ActivityExport[], filename: string = 'activity-export.csv'): void {
    if (!data || data.length === 0) {
      console.warn('No data to export');
      return;
    }

    // Create CSV header
    const headers = Object.keys(data[0]).join(',');
    
    // Create CSV rows
    const rows = data.map(item => 
      Object.values(item).map(val => 
        typeof val === 'string' && val.includes(',') ? `"${val}"` : val
      ).join(',')
    );

    // Combine header and rows
    const csv = [headers, ...rows].join('\n');

    // Create blob and download
    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    const url = URL.createObjectURL(blob);
    
    link.setAttribute('href', url);
    link.setAttribute('download', filename);
    link.style.visibility = 'hidden';
    
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }
}
