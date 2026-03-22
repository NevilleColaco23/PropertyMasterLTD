import { Injectable } from '@angular/core';
import { HttpClient, HttpParams, HttpErrorResponse } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
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
  CalendarEventResponse,
  RoomData,
  BookingWithGuestData
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
    return this.http.delete(`${this.apiUrl}/${dashboardId}`, {
      params: new HttpParams().set('userId', userId.toString()),
      observe: 'response'
    }).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Delete dashboard error:', error);
        return of({ status: error.status } as any);
      }),
      // Map the response - 204 = success, anything else = failure
      map((response: any) => response.status === 204)
    );
  }

  /**
   * Set a dashboard as default
   */
  setDefaultDashboard(dashboardId: string, userId: number): Observable<boolean> {
    return this.http.post(`${this.apiUrl}/${dashboardId}/set-default`, null, {
      params: new HttpParams().set('userId', userId.toString()),
      observe: 'response'
    }).pipe(
      catchError((error: HttpErrorResponse) => {
        console.error('Set default dashboard error:', error);
        return of({ status: error.status } as any);
      }),
      // Map the response - 200 = success, anything else = failure
      map((response: any) => response.status === 200)
    );
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
  getKpiValue(widgetId: string, userId: number, propertyIds?: number[]): Observable<KpiValueResponse> {
    let params = new HttpParams().set('userId', userId.toString());

    // Add property IDs if provided
    if (propertyIds && propertyIds.length > 0) {
      propertyIds.forEach(id => {
        params = params.append('propertyIds', id.toString());
      });
    }

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
   * Get recent bookings for dashboard
   */
  getRecentBookings(userId: number, limit: number = 10, propertyIds?: number[]): Observable<ActivityItemResponse[]> {
    let params = new HttpParams()
      .set('userId', userId.toString())
      .set('limit', limit.toString());

    // Add property IDs if provided
    if (propertyIds && propertyIds.length > 0) {
      propertyIds.forEach(id => {
        params = params.append('propertyIds', id.toString());
      });
    }

    return this.http.get<ActivityItemResponse[]>(`${this.apiUrl}/activity/recent-bookings`, { params });
  }

  /**
   * Get calendar events for dashboard
   */
  getCalendarEvents(
    userId: number,
    startDate?: Date,
    endDate?: Date,
    propertyIds?: number[]
  ): Observable<CalendarEventResponse[]> {
    let params = new HttpParams().set('userId', userId.toString());

    if (startDate) {
      // Format date as YYYY-MM-DD to avoid timezone conversion
      const year = startDate.getFullYear();
      const month = String(startDate.getMonth() + 1).padStart(2, '0');
      const day = String(startDate.getDate()).padStart(2, '0');
      params = params.set('startDate', `${year}-${month}-${day}`);
    }
    if (endDate) {
      // Format date as YYYY-MM-DD to avoid timezone conversion
      const year = endDate.getFullYear();
      const month = String(endDate.getMonth() + 1).padStart(2, '0');
      const day = String(endDate.getDate()).padStart(2, '0');
      params = params.set('endDate', `${year}-${month}-${day}`);
    }

    // Add property IDs if provided
    if (propertyIds && propertyIds.length > 0) {
      propertyIds.forEach(id => {
        params = params.append('propertyIds', id.toString());
      });
    }

    return this.http.get<CalendarEventResponse[]>(`${this.apiUrl}/activity/calendar-events`, { params });
  }

  /**
   * Get booking trends for chart widget
   */
  getBookingTrends(
    userId: number,
    daysBack: number = 30,
    groupBy: 'day' | 'week' | 'month' = 'day',
    propertyIds?: number[]
  ): Observable<any> {
    let params = new HttpParams()
      .set('userId', userId.toString())
      .set('daysBack', daysBack.toString())
      .set('groupBy', groupBy);

    // Add property IDs if provided
    if (propertyIds && propertyIds.length > 0) {
      propertyIds.forEach(id => {
        params = params.append('propertyIds', id.toString());
      });
    }

    return this.http.get<any>(`${this.apiUrl}/activity/booking-trends`, { params });
  }

  // ==========================================
  // ROOM PLANNER API METHODS
  // ==========================================

  /**
   * Get rooms for room planner (Gantt chart view)
   */
  getRoomsByProperty(userId: number, propertyIds?: number[], activeOnly: boolean = true): Observable<RoomData[]> {
    let params = new HttpParams()
      .set('userId', userId.toString())
      .set('activeOnly', activeOnly.toString());

    // Add property IDs if provided
    if (propertyIds && propertyIds.length > 0) {
      propertyIds.forEach(id => {
        params = params.append('propertyIds', id.toString());
      });
    }

    return this.http.get<RoomData[]>(`${this.apiUrl}/rooms`, { params });
  }

  /**
   * Get bookings with guest details for room planner
   */
  getBookingsWithGuests(
    userId: number,
    startDate?: Date,
    endDate?: Date,
    propertyIds?: number[]
  ): Observable<BookingWithGuestData[]> {
    let params = new HttpParams().set('userId', userId.toString());

    // Add dates if provided
    if (startDate) {
      const year = startDate.getFullYear();
      const month = String(startDate.getMonth() + 1).padStart(2, '0');
      const day = String(startDate.getDate()).padStart(2, '0');
      params = params.set('startDate', `${year}-${month}-${day}`);
    }
    if (endDate) {
      const year = endDate.getFullYear();
      const month = String(endDate.getMonth() + 1).padStart(2, '0');
      const day = String(endDate.getDate()).padStart(2, '0');
      params = params.set('endDate', `${year}-${month}-${day}`);
    }

    // Add property IDs if provided
    if (propertyIds && propertyIds.length > 0) {
      propertyIds.forEach(id => {
        params = params.append('propertyIds', id.toString());
      });
    }

    return this.http.get<BookingWithGuestData[]>(`${this.apiUrl}/bookings-with-guests`, { params });
  }

  // ========================================
  // ROOM PLANNER INTERACTIVE FEATURES
  // ========================================

  /**
   * Move a booking to a different room
   * @param bookingId The booking ID to move
   * @param newRoomNumber The new room number
   * @param userId User ID performing the action
   * @returns Observable with updated booking data
   */
  moveBooking(bookingId: string, newRoomNumber: string, userId: number): Observable<any> {
    const url = `${environment.apiUrl}/bookings/${bookingId}/move-room`;
    const body = {
      newRoomNumber,
      userId
    };

    console.log(`📤 API Call: Move Booking ${bookingId} to Room ${newRoomNumber}`);

    return this.http.put(url, body).pipe(
      map((response: any) => {
        console.log('✅ Move Booking Response:', response);
        return response;
      }),
      catchError((error: HttpErrorResponse) => {
        console.error('❌ Move Booking Error:', error);
        throw error;
      })
    );
  }

  /**
   * Update booking check-in and check-out dates
   * @param bookingId The booking ID to update
   * @param newCheckInDate New check-in date
   * @param newCheckOutDate New check-out date
   * @param userId User ID performing the action
   * @param reason Optional reason for date change
   * @returns Observable with updated booking data
   */
  updateBookingDates(
    bookingId: string,
    newCheckInDate: Date,
    newCheckOutDate: Date,
    userId: number,
    reason?: string
  ): Observable<any> {
    const url = `${environment.apiUrl}/bookings/${bookingId}/update-dates`;
    const body = {
      newCheckInDate: newCheckInDate.toISOString(),
      newCheckOutDate: newCheckOutDate.toISOString(),
      userId,
      reason
    };

    console.log(`📤 API Call: Update Booking ${bookingId} Dates`, {
      checkIn: newCheckInDate.toDateString(),
      checkOut: newCheckOutDate.toDateString()
    });

    return this.http.put(url, body).pipe(
      map((response: any) => {
        console.log('✅ Update Booking Dates Response:', response);
        return response;
      }),
      catchError((error: HttpErrorResponse) => {
        console.error('❌ Update Booking Dates Error:', error);
        throw error;
      })
    );
  }

  /**
   * Cancel a booking
   * @param bookingId The booking ID to cancel
   * @param userId User ID performing the cancellation
   * @param cancellationReason Optional reason for cancellation
   * @returns Observable with cancelled booking data
   */
  cancelBooking(bookingId: string, userId: number, cancellationReason?: string): Observable<any> {
    const url = `${environment.apiUrl}/bookings/${bookingId}/cancel`;
    const body = {
      userId,
      cancellationReason
    };

    console.log(`📤 API Call: Cancel Booking ${bookingId}`);

    return this.http.put(url, body).pipe(
      map((response: any) => {
        console.log('✅ Cancel Booking Response:', response);
        return response;
      }),
      catchError((error: HttpErrorResponse) => {
        console.error('❌ Cancel Booking Error:', error);
        throw error;
      })
    );
  }
}

