import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule, MatSelectChange } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { Router } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { DashboardService } from '../../services/dashboard.service';
import { KpiCardWidgetComponent, KpiCardData } from '../../widgets/kpi-card-widget/kpi-card-widget.component';
import { ListWidgetComponent, ListWidgetData, ListItem } from '../../widgets/list-widget/list-widget.component';
import { ChartWidgetComponent, ChartWidgetData } from '../../widgets/chart-widget/chart-widget.component';
import { CalendarWidgetComponent, CalendarWidgetData, CalendarEvent } from '../../widgets/calendar-widget/calendar-widget.component';

export interface DashboardType {
  value: string;
  label: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-dashboard1',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    KpiCardWidgetComponent,
    ListWidgetComponent,
    ChartWidgetComponent,
    CalendarWidgetComponent
  ],
  templateUrl: './dashboard1.component.html',
  styleUrls: ['./dashboard1.component.css']
})
export class Dashboard1Component implements OnInit {

  selectedDashboard: string = 'dashboard1';
  loading = false;

  // Widget data arrays
  kpiCards: KpiCardData[] = [];
  listWidgets: ListWidgetData[] = [];
  chartWidgets: ChartWidgetData[] = [];
  calendarWidgets: CalendarWidgetData[] = [];

  dashboardTypes: DashboardType[] = [
    {
      value: 'dashboard1',
      label: 'Overview Dashboard',
      icon: 'dashboard',
      route: '/propertyLanding/dashboard1'
    },
    {
      value: 'dashboard2',
      label: 'Analytics Dashboard',
      icon: 'analytics',
      route: '/propertyLanding/dashboard2'
    },
    {
      value: 'dashboard3',
      label: 'Reports Dashboard',
      icon: 'assessment',
      route: '/propertyLanding/dashboard3'
    }
  ];

  constructor(
    private router: Router,
    private dashboardService: DashboardService,
    private snackBar: MatSnackBar
  ) {}

  ngOnInit(): void {
    console.log('Dashboard1 component initialized - Phase 4 with real data');
    this.loadAllRealData();
  }

  /**
   * PHASE 4: Load all real data from backend APIs
   */
  loadAllRealData(): void {
    this.loading = true;
    console.log('Loading real data from APIs...');

    // Load KPI data in parallel
    this.loadKpiData();

    // Load activity data
    this.loadActivityData();

    // Load calendar data
    this.loadCalendarData();

    this.loading = false;
  }

  /**
   * Load real KPI values from backend
   */
  loadKpiData(): void {
    const userId = this.getCurrentUserId();

    // Call all KPI endpoints in parallel using forkJoin
    forkJoin({
      totalProperties: this.dashboardService.getKpiValue('total-properties', userId).pipe(
        catchError(error => {
          console.error('Error loading total properties:', error);
          return of({ widgetId: 'total-properties', value: 0, showTrend: false, trendValue: undefined, trendDirection: undefined, calculatedAt: new Date() });
        })
      ),
      totalRooms: this.dashboardService.getKpiValue('total-rooms', userId).pipe(
        catchError(error => {
          console.error('Error loading total rooms:', error);
          return of({ widgetId: 'total-rooms', value: 0, showTrend: false, trendValue: undefined, trendDirection: undefined, calculatedAt: new Date() });
        })
      ),
      bookingsToday: this.dashboardService.getKpiValue('bookings-today', userId).pipe(
        catchError(error => {
          console.error('Error loading bookings today:', error);
          return of({ widgetId: 'bookings-today', value: 0, showTrend: false, trendValue: undefined, trendDirection: undefined, calculatedAt: new Date() });
        })
      ),
      occupancyRate: this.dashboardService.getKpiValue('occupancy-rate', userId).pipe(
        catchError(error => {
          console.error('Error loading occupancy rate:', error);
          return of({ widgetId: 'occupancy-rate', value: '0%', showTrend: false, trendValue: undefined, trendDirection: undefined, calculatedAt: new Date() });
        })
      )
    }).subscribe({
      next: (results) => {
        console.log('KPI data loaded:', results);

        this.kpiCards = [
          {
            title: 'Total Properties',
            value: results.totalProperties.value,
            icon: 'hotel',
            color: '#1976d2',
            showTrend: results.totalProperties.showTrend,
            trendValue: results.totalProperties.trendValue,
            trendDirection: results.totalProperties.trendDirection
          },
          {
            title: 'Total Rooms',
            value: results.totalRooms.value,
            icon: 'meeting_room',
            color: '#1976d2',
            showTrend: results.totalRooms.showTrend,
            trendValue: results.totalRooms.trendValue,
            trendDirection: results.totalRooms.trendDirection
          },
          {
            title: 'Bookings Today',
            value: results.bookingsToday.value,
            icon: 'event_available',
            color: '#1976d2',
            showTrend: results.bookingsToday.showTrend,
            trendValue: results.bookingsToday.trendValue,
            trendDirection: results.bookingsToday.trendDirection
          },
          {
            title: 'Occupancy Rate',
            value: results.occupancyRate.value,
            icon: 'people',
            color: '#1976d2',
            showTrend: results.occupancyRate.showTrend,
            trendValue: results.occupancyRate.trendValue,
            trendDirection: results.occupancyRate.trendDirection
          }
        ];
      },
      error: (error) => {
        console.error('Error loading KPI data:', error);
        this.snackBar.open('Failed to load KPI data', 'Close', { duration: 3000 });
        // Show placeholder data on error
        this.loadDefaultKpiCards();
      }
    });
  }

  /**
   * Load recent activity from backend
   */
  loadActivityData(): void {
    const userId = this.getCurrentUserId();

    this.dashboardService.getRecentActivity(userId, 10).subscribe({
      next: (activities) => {
        console.log('Activity data loaded:', activities);

        if (activities && activities.length > 0) {
          this.listWidgets = [{
            title: 'Recent Activity',
            items: activities.map(act => ({
              id: act.id,
              title: act.title,
              subtitle: act.subtitle,
              timestamp: new Date(act.timestamp),
              icon: act.icon,
              iconColor: act.iconColor,
              metadata: act.metadata
            }))
          }];
        } else {
          // Use default if no data
          this.listWidgets = [this.getDefaultListWidget()];
        }
      },
      error: (error) => {
        console.error('Error loading activity data:', error);
        this.listWidgets = [this.getDefaultListWidget()];
      }
    });
  }

  /**
   * Load calendar events from backend
   */
  loadCalendarData(): void {
    const userId = this.getCurrentUserId();
    const now = new Date();
    const startDate = new Date(now.getFullYear(), now.getMonth(), 1); // First day of current month
    const endDate = new Date(now.getFullYear(), now.getMonth() + 1, 0); // Last day of current month

    this.dashboardService.getCalendarEvents(userId, startDate, endDate).subscribe({
      next: (events) => {
        console.log('Calendar data loaded:', events);

        if (events && events.length > 0) {
          this.calendarWidgets = [{
            title: 'Upcoming Bookings',
            events: events.map(evt => ({
              id: evt.id,
              title: evt.title,
              start: new Date(evt.start),
              end: evt.end ? new Date(evt.end) : undefined,
              color: evt.color,
              type: evt.type,
              description: evt.description
            }))
          }];
        } else {
          // Use default if no data
          this.calendarWidgets = [this.getDefaultCalendarWidget()];
        }
      },
      error: (error) => {
        console.error('Error loading calendar data:', error);
        this.calendarWidgets = [this.getDefaultCalendarWidget()];
      }
    });
  }

  /**
   * Map activity type to icon
   */
  getActivityIcon(type: string): string {
    const iconMap: { [key: string]: string } = {
      'booking_created': 'event_available',
      'booking_modified': 'edit_calendar',
      'booking_cancelled': 'event_busy',
      'property_added': 'add_business',
      'property_modified': 'business',
      'room_added': 'meeting_room',
      'payment_received': 'payment'
    };
    return iconMap[type] || 'info';
  }

  /**
   * Load default KPI cards with placeholder data
   */
  loadDefaultKpiCards(): void {
    this.kpiCards = [
      {
        title: 'Total Properties',
        value: 0,
        icon: 'hotel',
        color: '#1976d2'
      },
      {
        title: 'Total Rooms',
        value: 0,
        icon: 'meeting_room',
        color: '#1976d2'
      },
      {
        title: 'Bookings Today',
        value: 0,
        icon: 'event_available',
        color: '#1976d2'
      },
      {
        title: 'Occupancy Rate',
        value: '0%',
        icon: 'people',
        color: '#1976d2'
      }
    ];
  }

  /**
   * Handle dashboard type selection change
   */
  onDashboardChange(event: MatSelectChange): void {
    const selected = this.dashboardTypes.find(d => d.value === event.value);
    if (selected) {
      console.log('Navigating to:', selected.route);
      this.router.navigate([selected.route]);
    }
  }

  /**
   * Get current user ID (mock - replace with real auth)
   */
  private getCurrentUserId(): number {
    // TODO: Replace with actual user authentication service
    return 1;
  }

  /**
   * Get default list widget with placeholder data
   */
  private getDefaultListWidget(): ListWidgetData {
    return {
      title: 'Recent Activity',
      items: [
        {
          id: 1,
          title: 'New booking created',
          subtitle: 'Property: Sunset Villa',
          timestamp: new Date(),
          icon: 'event_available',
          iconColor: '#4caf50'
        },
        {
          id: 2,
          title: 'Property updated',
          subtitle: 'Ocean View Apartment',
          timestamp: new Date(Date.now() - 3600000),
          icon: 'business',
          iconColor: '#2196f3'
        },
        {
          id: 3,
          title: 'Payment received',
          subtitle: '$1,200 for booking #12345',
          timestamp: new Date(Date.now() - 7200000),
          icon: 'payment',
          iconColor: '#9c27b0'
        }
      ]
    };
  }

  /**
   * Get default chart widget with placeholder data
   */
  private getDefaultChartWidget(): ChartWidgetData {
    return {
      title: 'Booking Trends',
      chartType: 'line',
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      datasets: [{
        label: 'Bookings',
        data: [12, 19, 15, 25, 22, 30],
        backgroundColor: '#1976d2',
        borderColor: '#1976d2'
      }]
    };
  }

  /**
   * Get default calendar widget with placeholder data
   */
  private getDefaultCalendarWidget(): CalendarWidgetData {
    const today = new Date();
    return {
      title: 'Upcoming Bookings',
      events: [
        {
          id: 1,
          title: 'Check-in: Smith Family',
          start: new Date(today.getTime() + 86400000),
          color: '#4caf50',
          type: 'check-in',
          description: 'Guest check-in at 14:00'
        },
        {
          id: 2,
          title: 'Check-out: Johnson',
          start: new Date(today.getTime() + 172800000),
          color: '#f44336',
          type: 'check-out',
          description: 'Guest check-out at 11:00'
        },
        {
          id: 3,
          title: 'Maintenance: Room 301',
          start: new Date(today.getTime() + 259200000),
          color: '#ff9800',
          type: 'maintenance',
          description: 'Scheduled maintenance at 09:00'
        }
      ]
    };
  }
}
