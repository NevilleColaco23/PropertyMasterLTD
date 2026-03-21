import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule, MatSelectChange } from '@angular/material/select';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatBadgeModule } from '@angular/material/badge';
import { MatDividerModule } from '@angular/material/divider';
import { MatTabsModule, MatTabChangeEvent } from '@angular/material/tabs';
import { Router } from '@angular/router';
import { Gridster, GridsterItem, GridsterConfig } from 'angular-gridster2';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { DashboardService } from '../../services/dashboard.service';
import { 
  DashboardConfiguration, 
  WidgetLibraryItem, 
  SaveDashboardRequest 
} from '../../models/dashboard.models';
import { KpiCardWidgetComponent, KpiCardData } from '../../widgets/kpi-card-widget/kpi-card-widget.component';
import { ListWidgetComponent, ListWidgetData, ListItem } from '../../widgets/list-widget/list-widget.component';
import { ChartWidgetComponent, ChartWidgetData } from '../../widgets/chart-widget/chart-widget.component';
import { CalendarWidgetComponent, CalendarWidgetData, CalendarEvent } from '../../widgets/calendar-widget/calendar-widget.component';
import { ActivityStreamWidgetComponent } from '../../widgets/activity-stream-widget/activity-stream-widget.component';
import { WidgetPickerDialogComponent } from '../../widgets/widget-picker-dialog/widget-picker-dialog.component';
import { SaveDashboardDialogComponent, SaveDashboardDialogData } from '../../dialogs/save-dashboard-dialog/save-dashboard-dialog.component';
import { BookingDetailsDialogComponent, BookingDetailsData } from '../booking-details-dialog/booking-details-dialog.component';
import { BookingContextMenuComponent, BookingContextMenuData, BookingContextMenuResult } from './booking-context-menu/booking-context-menu.component';
import { GridsterConfigService } from '../../services/gridster-config.service';
import { NotificationService } from '../../services/notification.service';

export interface DashboardType {
  value: string;
  label: string;
  icon: string;
  route: string;
}

export interface DashboardGridsterItem {
  x: number;
  y: number;
  cols: number;
  rows: number;
  widgetId: string;
  widgetType: string;
  settings?: any;
  data?: any;
  isRefreshing?: boolean; // Track refresh state for individual widgets
  currentListView?: 'bookings' | 'activity'; // Track current view for list widget
  [key: string]: any; // Allow additional gridster properties
}

// Interface for booking bars in Room Planner
export interface BookingBar {
  bookingId: string;
  guestName: string;
  type: string;
  color: string;
  startDate: Date;
  endDate: Date;
  startCol: number; // Grid column start (1-based)
  span: number; // Number of days to span
  continuesFromPreviousMonth?: boolean; // Indicates if booking starts in previous month
  continuesNextMonth?: boolean; // Indicates if booking extends into next month
  propertyName?: string;
  checkInDate?: Date;
  checkOutDate?: Date;
  guestDetails?: {
    guestId: string;
    firstName: string;
    lastName: string;
    email: string;
    phoneNumber: string;
    nationality: string;
  };
}

@Component({
  selector: 'app-dashboard1',
  standalone: true,
  imports: [
    CommonModule,
    DatePipe,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatSelectModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatDialogModule,
    MatTooltipModule,
    MatBadgeModule,
    MatDividerModule,
    MatTabsModule,
    Gridster,
    GridsterItem,
    KpiCardWidgetComponent,
    ListWidgetComponent,
    ChartWidgetComponent,
    CalendarWidgetComponent,
    ActivityStreamWidgetComponent
  ],
  templateUrl: './dashboard1.component.html',
  styleUrls: ['./dashboard1.component.css', './room-planner-gantt.css'],
  encapsulation: ViewEncapsulation.None
})
export class Dashboard1Component implements OnInit {

  // ===== Dashboard Selection =====
  selectedDashboard: string | null = null;
  userDashboards: DashboardConfiguration[] = [];
  isLoadingDashboards = false;

  // ===== State Management =====
  loading = true;  // Start with loading=true, set to false after data loads
  dashboardConfig: DashboardConfiguration | null = null;
  widgetLibrary: WidgetLibraryItem[] = [];

  // ===== Edit Mode =====
  editMode = false;
  hasUnsavedChanges = false;
  savedDashboardState: DashboardGridsterItem[] = []; // Backup of last saved state

  // ===== Gridster Configuration =====
  options: GridsterConfig;
  dashboardItems: DashboardGridsterItem[] = [];

  // ===== Tab Management =====
  selectedTabIndex = 0;

  // ===== Room Planner Properties =====
  loadingRoomPlanner = false;
  currentPlannerMonth = new Date();
  plannerDays: Array<{ date: Date; dayOfWeek: string }> = [];
  rooms: Array<any> = [];
  roomBookings: Map<string, any> = new Map();
  roomBookingBars: Map<number, BookingBar[]> = new Map(); // NEW: Continuous booking bars per room
  totalRooms = 0;
  occupiedRoomsToday = 0;
  availableRoomsToday = 0;
  occupancyRateToday = 0;
  availablePlannerProperties: Array<{ id: number; name: string }> = []; // Properties available for planner
  selectedPlannerPropertyId: number | null = null; // Currently selected property in planner dropdown
  roomPlannerDataLoaded = false; // Track if data has been loaded at least once

  // ===== Interactive Features State =====
  draggedBooking: BookingBar | null = null;
  draggedFromRoomId: number | null = null;
  isDragging = false;
  dropTargetRoomId: number | null = null;
  dropTargetDate: Date | null = null;

  resizingBooking: BookingBar | null = null;
  resizingRoomId: number | null = null;
  resizeDirection: 'left' | 'right' | null = null;
  isResizing = false;
  resizeStartX: number = 0;
  resizeOriginalSpan: number = 0;
  resizeOriginalStartCol: number = 0;

  constructor(
    private router: Router,
    private dashboardService: DashboardService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef,
    private notificationService: NotificationService
  ) {
    // Initialize gridster configuration
    this.options = GridsterConfigService.getDefaultConfig(false);
  }

  ngOnInit(): void {
    console.log('Dashboard1 component initialized - Phase 5: Gridster + Real Data');
    this.loadUserDashboards();
    this.loadWidgetLibrary();

    // Pre-load Room Planner data in background
    this.initializeRoomPlanner();
  }

  // ========================================
  // DASHBOARD LOADING
  // ========================================

  /**
   * Load all user's dashboards
   */
  loadUserDashboards(): void {
    this.isLoadingDashboards = true;
    const userId = this.getCurrentUserId();

    this.dashboardService.getUserDashboards(userId).subscribe({
      next: (dashboards) => {
        console.log('User dashboards loaded:', dashboards);
        this.userDashboards = dashboards;

        if (dashboards.length > 0) {
          // Find default dashboard or use first one
          const defaultDashboard = dashboards.find(d => d.isDefault) || dashboards[0];
          // Wrap in setTimeout to avoid ExpressionChangedAfterItHasBeenCheckedError
          setTimeout(() => {
            this.selectedDashboard = defaultDashboard.id!;
          });
          this.loadDashboard(defaultDashboard.id!);
        } else {
          // No saved dashboards, load default widgets
          console.log('No saved dashboards found, loading default widgets');
          // Wrap in setTimeout to avoid ExpressionChangedAfterItHasBeenCheckedError
          setTimeout(() => {
            this.selectedDashboard = null;
          });
          this.loadDefaultWidgets();
          this.refreshAllWidgetData();
        }

        this.isLoadingDashboards = false;
      },
      error: (error) => {
        console.error('Error loading user dashboards:', error);
        this.notificationService.error('Failed to load dashboards', 'Close', 3000);
        this.loadDefaultWidgets();
        this.refreshAllWidgetData();
        this.isLoadingDashboards = false;
      }
    });
  }

  /**
   * Load specific dashboard configuration from API
   */
  loadDashboard(dashboardId: string): void {
    this.loading = true;
    const userId = this.getCurrentUserId();

    // Find dashboard in loaded list
    const dashboard = this.userDashboards.find(d => d.id === dashboardId);

    if (dashboard) {
      console.log('Loading dashboard:', dashboard.dashboardName);
      this.dashboardConfig = dashboard;
      this.renderWidgets(dashboard);
      this.refreshAllWidgetData();
      this.loading = false;
    } else {
      // Fallback: load from API
      this.dashboardService.getDashboardByUserId(userId, false).subscribe({
        next: (config) => {
          if (config) {
            this.dashboardConfig = config;
            this.renderWidgets(config);
            this.refreshAllWidgetData();
          } else {
            this.loadDefaultWidgets();
            this.refreshAllWidgetData();
          }
          this.loading = false;
        },
        error: (error) => {
          console.error('Error loading dashboard:', error);
          this.notificationService.error('Failed to load dashboard', 'Close', 3000);
          this.loadDefaultWidgets();
          this.refreshAllWidgetData();
          this.loading = false;
        }
      });
    }
  }

  /**
   * Load available widgets from library
   */
  loadWidgetLibrary(): void {
    this.dashboardService.getWidgetLibrary({ activeOnly: true }).subscribe({
      next: (widgets) => {
        console.log('Widget library loaded:', widgets);
        this.widgetLibrary = widgets;
      },
      error: (error) => {
        console.error('Error loading widget library:', error);
      }
    });
  }

  /**
   * Render widgets from dashboard configuration
   */
  renderWidgets(config: DashboardConfiguration): void {
    this.dashboardItems = [];

    config.layout.widgets.forEach(widget => {
      const item: DashboardGridsterItem = {
        x: widget.position.x,
        y: widget.position.y,
        cols: widget.position.width,
        rows: widget.position.height,
        widgetId: widget.widgetId,
        widgetType: widget.widgetType,
        settings: widget.settings,
        data: null // Will be populated by refreshAllWidgetData()
      };

      this.dashboardItems.push(item);
    });

    // Save the initial state for revert functionality
    this.backupDashboardState();

    console.log('Rendered dashboard items:', this.dashboardItems.length);
  }

  /**
   * Load default widgets when no dashboard configuration exists
   */
  loadDefaultWidgets(): void {
    this.dashboardItems = [
      // Row 1: KPI Cards
      {
        x: 0, y: 0, cols: 3, rows: 2,
        widgetId: 'total-properties',
        widgetType: 'kpi-card',
        data: null
      },
      {
        x: 3, y: 0, cols: 3, rows: 2,
        widgetId: 'total-rooms',
        widgetType: 'kpi-card',
        data: null
      },
      {
        x: 6, y: 0, cols: 3, rows: 2,
        widgetId: 'bookings-today',
        widgetType: 'kpi-card',
        data: null
      },
      {
        x: 9, y: 0, cols: 3, rows: 2,
        widgetId: 'occupancy-rate',
        widgetType: 'kpi-card',
        data: null
      },
      // Row 2: Chart Widget
      {
        x: 0, y: 2, cols: 12, rows: 4,
        widgetId: 'revenue-chart',
        widgetType: 'chart',
        data: null
      },
      // Row 3: List and Calendar
      {
        x: 0, y: 6, cols: 6, rows: 4,
        widgetId: 'recent-activity',
        widgetType: 'list',
        data: null
      },
      {
        x: 6, y: 6, cols: 6, rows: 4,
        widgetId: 'booking-calendar',
        widgetType: 'calendar',
        data: null
      }
    ];

    console.log('Loaded default widgets:', this.dashboardItems.length);
  }

  // ========================================
  // REAL DATA LOADING - PHASE 4 INTEGRATION
  // ========================================

  /**
   * Refresh data for all widgets on dashboard
   */
  refreshAllWidgetData(): void {
    console.log('🔄 Refreshing data for all widgets. Total widgets:', this.dashboardItems.length);
    this.dashboardItems.forEach((item, index) => {
      console.log(`📊 Loading data for widget ${index + 1}/${this.dashboardItems.length}:`, item.widgetType, item.widgetId);
      this.loadWidgetRealData(item);
    });
  }

  /**
   * Refresh data for a single widget
   */
  refreshWidget(item: DashboardGridsterItem): void {
    console.log('🔄 Refreshing widget:', item.widgetType, item.widgetId);

    // Set loading state
    item.isRefreshing = true;
    this.cdr.detectChanges();

    // Load fresh data
    this.loadWidgetRealData(item);

    // Clear loading state after a short delay
    setTimeout(() => {
      item.isRefreshing = false;
      this.cdr.detectChanges();
    }, 500);

    this.notificationService.info('Widget refreshed', '', 2000);
  }

  /**
   * Handle chart filter changes (time period selection)
   */
  onChartFilterChange(filter: { daysBack: number; groupBy: 'day' | 'week' | 'month' }, item: DashboardGridsterItem): void {
    console.log(`📊 Chart filter changed:`, filter);

    // Update widget settings
    item.settings = {
      ...item.settings,
      daysBack: filter.daysBack,
      groupBy: filter.groupBy
    };

    // Mark as changed
    this.hasUnsavedChanges = true;

    // Reload chart data with new filters
    const userId = this.getCurrentUserId();
    this.loadChartWidgetData(item, userId);
  }

  /**
   * Load real data for a specific widget
   */
  loadWidgetRealData(item: DashboardGridsterItem): void {
    const userId = this.getCurrentUserId();

    switch (item.widgetType) {
      case 'kpi-card':
        this.loadKpiWidgetData(item, userId);
        break;
      case 'list':
        this.loadListWidgetData(item, userId);
        break;
      case 'chart':
        this.loadChartWidgetData(item, userId);
        break;
      case 'calendar':
        this.loadCalendarWidgetData(item, userId);
        break;
      case 'activity-stream':
        // Activity stream widget loads its own data directly from ActivityService
        item.data = true; // Mark as loaded so it doesn't show loading spinner
        break;
      default:
        console.warn('Unknown widget type:', item.widgetType);
    }
  }

  /**
   * Load KPI widget real data from API
   */
  loadKpiWidgetData(item: DashboardGridsterItem, userId: number): void {
    console.log(`🔢 Loading KPI data for widget: ${item.widgetId}, userId: ${userId}`);

    // Get selected property IDs from localStorage
    const propertyIds = this.getSelectedPropertyIds();
    console.log(`📍 Selected property IDs: ${propertyIds.length > 0 ? propertyIds.join(', ') : 'none (all properties)'}`);

    this.dashboardService.getKpiValue(item.widgetId, userId, propertyIds).pipe(
      catchError(error => {
        console.error(`Error loading KPI ${item.widgetId}:`, error);
        return of({ 
          widgetId: item.widgetId, 
          value: 0, 
          showTrend: false, 
          trendValue: undefined, 
          trendDirection: undefined, 
          calculatedAt: new Date() 
        });
      })
    ).subscribe(response => {
      console.log(`✅ KPI data loaded for ${item.widgetId}:`, response);
      item.data = {
        title: this.getKpiTitle(item.widgetId),
        value: response.value,
        icon: this.getKpiIcon(item.widgetId),
        color: '#1976d2',
        showTrend: response.showTrend,
        trendValue: response.trendValue,
        trendDirection: response.trendDirection
      } as KpiCardData;
      console.log(`📦 Assigned data to item.data:`, item.data);
      console.log(`📦 Full item object:`, item);
      this.cdr.detectChanges();  // Trigger change detection
      console.log(`🔄 Change detection triggered for ${item.widgetId}`);
    });
  }

  /**
   * Load list widget real data from API
   * Defaults to Recent Bookings view
   */
  loadListWidgetData(item: DashboardGridsterItem, userId: number): void {
    // Default to bookings view and track it
    item.currentListView = 'bookings';
    this.loadListWidgetBookings(item, userId);
  }

  /**
   * Load Recent Bookings view for list widget
   */
  private loadListWidgetBookings(item: DashboardGridsterItem, userId: number): void {
    console.log('📦 loadListWidgetBookings called for userId:', userId);
    const requestedView = 'bookings'; // Track what we're requesting

    // Get selected property IDs from localStorage
    const propertyIds = this.getSelectedPropertyIds();
    console.log(`📍 Loading bookings for properties: ${propertyIds.length > 0 ? propertyIds.join(', ') : 'all'}`);

    this.dashboardService.getRecentBookings(userId, 10, propertyIds).pipe(
      catchError(error => {
        console.error('❌ Error loading bookings data:', error);
        return of([]);
      })
    ).subscribe(bookings => {
      console.log('✅ Bookings received:', bookings?.length || 0, 'bookings');

      // Only update if still on bookings view (prevent race condition)
      if (item.currentListView !== requestedView) {
        console.log('⚠️ View changed during API call, ignoring bookings response');
        return;
      }

      if (bookings && bookings.length > 0) {
        item.data = {
          title: 'Recent Bookings',
          items: bookings.map(booking => ({
            id: booking.id,
            title: booking.title,
            subtitle: booking.subtitle,
            timestamp: new Date(booking.timestamp),
            icon: booking.icon,
            iconColor: booking.iconColor,
            metadata: booking.metadata
          }))
        } as ListWidgetData;
        console.log('✅ Set widget data to Recent Bookings with', bookings.length, 'items');
      } else {
        console.log('⚠️ No bookings found, using fallback data');
        item.data = this.getDefaultListWidget();
      }
      this.cdr.detectChanges();
    });
  }

  /**
   * Load Recent Activity view for list widget
   */
  private loadListWidgetActivity(item: DashboardGridsterItem, userId: number): void {
    console.log('📋 loadListWidgetActivity called for userId:', userId);
    const requestedView = 'activity'; // Track what we're requesting

    this.dashboardService.getRecentActivity(userId, 10).pipe(
      catchError(error => {
        console.error('❌ Error loading activity data:', error);
        return of([]);
      })
    ).subscribe(activities => {
      console.log('✅ Activities received:', activities?.length || 0, 'activities');

      // Only update if still on activity view (prevent race condition)
      if (item.currentListView !== requestedView) {
        console.log('⚠️ View changed during API call, ignoring activity response');
        return;
      }

      if (activities && activities.length > 0) {
        item.data = {
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
        } as ListWidgetData;
        console.log('✅ Set widget data to Recent Activity with', activities.length, 'items');
      } else {
        console.log('⚠️ No activities found, using fallback data');
        item.data = this.getDefaultListWidget();
      }
      this.cdr.detectChanges();
    });
  }

  /**
   * Handle list widget view toggle (bookings ↔ activity)
   */
  onListViewChange(view: 'bookings' | 'activity', item: DashboardGridsterItem): void {
    console.log('📋 List View Change Event Received! View:', view, 'Widget:', item.widgetId);

    // Update the current view immediately (before API call)
    item.currentListView = view;

    const userId = 1; // TODO: Get from auth service
    if (view === 'bookings') {
      console.log('→ Loading Recent Bookings...');
      this.loadListWidgetBookings(item, userId);
    } else {
      console.log('→ Loading Recent Activity...');
      this.loadListWidgetActivity(item, userId);
    }
  }

  /**
   * Load chart widget data (placeholder for now)
   */
  loadChartWidgetData(item: DashboardGridsterItem, userId: number): void {
    console.log(`📊 Loading chart data for widget: ${item.widgetId}, userId: ${userId}`);

    // Determine days back based on widget settings or default to 30
    const daysBack = item.settings?.daysBack || 30;
    const groupBy = item.settings?.groupBy || 'day';

    // Get selected property IDs from localStorage
    const propertyIds = this.getSelectedPropertyIds();
    console.log(`📍 Loading chart for properties: ${propertyIds.length > 0 ? propertyIds.join(', ') : 'all'}`);

    this.dashboardService.getBookingTrends(userId, daysBack, groupBy, propertyIds).pipe(
      catchError(error => {
        console.error('Error loading chart data:', error);
        // Return default data on error
        return of(this.getDefaultChartWidget());
      })
    ).subscribe(trendsData => {
      console.log(`✅ Chart data loaded:`, trendsData);
      item.data = trendsData;
      this.cdr.detectChanges();  // Trigger change detection
    });
  }

  /**
   * Load calendar widget real data from API
   */
  loadCalendarWidgetData(item: DashboardGridsterItem, userId: number): void {
    const now = new Date();
    const startDate = new Date(now.getFullYear(), now.getMonth(), 1);
    const endDate = new Date(now.getFullYear(), now.getMonth() + 1, 0);

    // Get selected property IDs from localStorage
    const propertyIds = this.getSelectedPropertyIds();
    console.log(`📍 Loading calendar for properties: ${propertyIds.length > 0 ? propertyIds.join(', ') : 'all'}`);

    this.dashboardService.getCalendarEvents(userId, startDate, endDate, propertyIds).pipe(
      catchError(error => {
        console.error('Error loading calendar data:', error);
        return of([]);
      })
    ).subscribe(events => {
      if (events && events.length > 0) {
        item.data = {
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
        } as CalendarWidgetData;
      } else {
        item.data = this.getDefaultCalendarWidget();
      }
      this.cdr.detectChanges();  // Trigger change detection
    });
  }

  // ========================================
  // EDIT MODE METHODS - PHASE 3 INTEGRATION
  // ========================================

  /**
   * Toggle edit mode on/off
   */
  toggleEditMode(): void {
    if (this.editMode && this.hasUnsavedChanges) {
      this.notificationService.confirmUnsavedChanges().subscribe(confirmed => {
        if (!confirmed) return;
        this.performToggleEditMode();
      });
      return;
    }
    this.performToggleEditMode();
  }

  private performToggleEditMode(): void {

    this.editMode = !this.editMode;
    this.options = GridsterConfigService.getDefaultConfig(this.editMode);

    if (this.options['api'] && this.options['api'].optionsChanged) {
      this.options['api'].optionsChanged();
    }

    if (!this.editMode) {
      this.hasUnsavedChanges = false;
    }

    // Backup current state when entering edit mode
    if (this.editMode) {
      this.backupDashboardState();
    }

    console.log('Edit mode:', this.editMode);
  }

  /**
   * Open widget picker dialog
   */
  openWidgetPicker(): void {
    const addedWidgetIds = this.dashboardItems.map(item => item.widgetId);

    const dialogRef = this.dialog.open(WidgetPickerDialogComponent, {
      width: '700px',
      data: {
        availableWidgets: this.widgetLibrary,
        addedWidgetIds: addedWidgetIds
      }
    });

    dialogRef.afterClosed().subscribe((selectedWidget: WidgetLibraryItem | undefined) => {
      if (selectedWidget) {
        this.addWidget(selectedWidget);
      }
    });
  }

  /**
   * Add widget to dashboard
   */
  addWidget(widget: WidgetLibraryItem): void {
    // Find next available position based on widget size
    const position = this.findNextAvailablePosition(
      widget.defaultSize.width, 
      widget.defaultSize.height
    );

    const newItem: DashboardGridsterItem = {
      x: position.x,
      y: position.y,
      cols: widget.defaultSize.width,
      rows: widget.defaultSize.height,
      widgetId: widget.widgetId,
      widgetType: widget.widgetType,
      settings: { ...widget.defaultSettings },
      data: null
    };

    this.dashboardItems.push(newItem);
    this.hasUnsavedChanges = true;

    // Load real data for the new widget
    this.loadWidgetRealData(newItem);

    this.notificationService.success(`${widget.name} added to dashboard`);
    console.log('Widget added:', widget.widgetId, 'at position:', position);
  }

  /**
   * Remove widget from dashboard
   */
  removeWidget(item: DashboardGridsterItem): void {
    const widgetName = this.widgetLibrary.find(w => w.widgetId === item.widgetId)?.name || 'Widget';

    const index = this.dashboardItems.indexOf(item);
    if (index > -1) {
      this.dashboardItems.splice(index, 1);
      this.hasUnsavedChanges = true;
      this.notificationService.info(`${widgetName} removed`);
      console.log('Widget removed:', item.widgetId);
    }
  }

  /**
   * Save dashboard configuration
   */
  saveDashboard(): void {
    // If this is a new dashboard (no ID), show Save As dialog
    if (!this.dashboardConfig?.id) {
      this.saveDashboardAs();
      return;
    }

    // Update existing dashboard
    this.performSave(
      this.dashboardConfig.id,
      this.dashboardConfig.dashboardName,
      this.dashboardConfig.isDefault
    );
  }

  /**
   * Save dashboard with a new name
   */
  saveDashboardAs(): void {
    const dialogData: SaveDashboardDialogData = {
      dashboardName: this.dashboardConfig?.dashboardName || 'My Dashboard',
      isDefault: this.userDashboards.length === 0 // First dashboard is default
    };

    const dialogRef = this.dialog.open(SaveDashboardDialogComponent, {
      width: '500px',
      data: dialogData,
      disableClose: false
    });

    dialogRef.afterClosed().subscribe((result: SaveDashboardDialogData | undefined) => {
      if (result) {
        // Create new dashboard with the given name
        this.performSave(
          undefined, // No ID = create new
          result.dashboardName,
          result.isDefault
        );
      }
    });
  }

  /**
   * Perform the actual save operation
   */
  private performSave(dashboardId: string | undefined, dashboardName: string, isDefault: boolean): void {
    this.loading = true;

    const widgets = this.dashboardItems.map(item => GridsterConfigService.toWidgetConfig(item));

    const request: SaveDashboardRequest = {
      id: dashboardId,
      userId: this.getCurrentUserId(),
      dashboardName: dashboardName,
      isDefault: isDefault,
      layout: {
        columns: 12,
        rowHeight: 80,
        widgets: widgets
      }
    };

    console.log('Saving dashboard...', request);

    this.dashboardService.saveDashboard(request).subscribe({
      next: (id) => {
        console.log('Dashboard saved with ID:', id);
        this.notificationService.success(`Dashboard "${dashboardName}" saved successfully!`);
        this.hasUnsavedChanges = false;
        this.editMode = false;
        this.loading = false;

        // Backup the new saved state
        this.backupDashboardState();

        // Reload all dashboards to update the list
        this.loadUserDashboards();
      },
      error: (error) => {
        console.error('Error saving dashboard:', error);
        this.notificationService.error('Failed to save dashboard', 'Close');
        this.loading = false;
      }
    });
  }

  /**
   * Cancel edit mode
   */
  cancelEdit(): void {
    if (this.hasUnsavedChanges) {
      this.notificationService.confirmDiscard().subscribe(confirmed => {
        if (confirmed) {
          this.performCancelEdit();
        }
      });
      return;
    }
    this.performCancelEdit();
  }

  private performCancelEdit(): void {

    this.editMode = false;
    this.hasUnsavedChanges = false;

    // Reload current dashboard
    if (this.selectedDashboard) {
      this.loadDashboard(this.selectedDashboard);
    } else {
      this.loadDefaultWidgets();
      this.refreshAllWidgetData();
    }
  }

  /**
   * Delete current dashboard
   */
  deleteDashboard(): void {
    if (!this.selectedDashboard || !this.dashboardConfig) {
      this.notificationService.warning('No dashboard selected to delete');
      return;
    }

    this.notificationService.confirmDelete(this.dashboardConfig.dashboardName).subscribe(confirmed => {
      if (!confirmed) return;
      this.performDeleteDashboard();
    });
  }

  private performDeleteDashboard(): void {

    if (!this.selectedDashboard) return;

    this.loading = true;
    const userId = this.getCurrentUserId();

    this.dashboardService.deleteDashboard(this.selectedDashboard, userId).subscribe({
      next: (success) => {
        if (success) {
          this.notificationService.success('Dashboard deleted successfully');
          this.selectedDashboard = null;
          this.dashboardConfig = null;
          this.loadUserDashboards(); // Reload dashboard list
        } else {
          this.notificationService.error('Failed to delete dashboard');
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error deleting dashboard:', error);
        this.notificationService.error('Failed to delete dashboard');
        this.loading = false;
      }
    });
  }

  /**
   * Set current dashboard as default
   */
  setAsDefaultDashboard(): void {
    if (!this.selectedDashboard) {
      this.notificationService.warning('No dashboard selected', 'Close');
      return;
    }

    this.loading = true;
    const userId = this.getCurrentUserId();

    this.dashboardService.setDefaultDashboard(this.selectedDashboard, userId).subscribe({
      next: (success) => {
        if (success) {
          this.notificationService.success('Default dashboard updated successfully');
          // Reload dashboard list to update isDefault flags
          this.loadUserDashboards();
        } else {
          this.notificationService.error('Failed to set default dashboard', 'Close');
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('Error setting default dashboard:', error);
        this.notificationService.error('Failed to set default dashboard', 'Close');
        this.loading = false;
      }
    });
  }

  /**
   * Check if current dashboard is the default
   */
  isCurrentDashboardDefault(): boolean {
    if (!this.selectedDashboard) return false;
    const currentDashboard = this.userDashboards.find(d => d.id === this.selectedDashboard);
    return currentDashboard?.isDefault || false;
  }

  /**
   * Revert dashboard to last saved state
   */
  revertToLastSaved(): void {
    this.notificationService.confirm(
      'Revert Changes?',
      'Revert all changes to the last saved state? Current changes will be lost.',
      'Revert',
      'Cancel',
      'restore',
      '#ff9800',
      'warn'
    ).subscribe(confirmed => {
      if (!confirmed) return;
      this.performRevertToLastSaved();
    });
  }

  private performRevertToLastSaved(): void {

    // Restore from backup
    this.dashboardItems = JSON.parse(JSON.stringify(this.savedDashboardState));
    this.hasUnsavedChanges = false;

    // Refresh data for all widgets
    this.refreshAllWidgetData();

    this.notificationService.success('Dashboard reverted to last saved state');
    console.log('Dashboard reverted to last saved state');
  }

  /**
   * Backup current dashboard state for revert functionality
   */
  private backupDashboardState(): void {
    this.savedDashboardState = JSON.parse(JSON.stringify(this.dashboardItems));
    console.log('Dashboard state backed up:', this.savedDashboardState.length, 'items');
  }

  /**
   * Item changed (drag/resize)
   */
  itemChange(item: any, itemComponent: any): void {
    if (this.editMode) {
      this.hasUnsavedChanges = true;
      console.log('Item changed:', item);
    }
  }

  /**
   * Find next available position in grid
   */
  private findNextAvailablePosition(cols: number = 3, rows: number = 2): { x: number; y: number } {
    if (this.dashboardItems.length === 0) {
      return { x: 0, y: 0 };
    }

    // Try to find first available space starting from top-left
    const maxCols = 12;
    const maxRows = 100;

    // Create a grid map to track occupied cells
    const occupied: boolean[][] = [];
    for (let y = 0; y < maxRows; y++) {
      occupied[y] = new Array(maxCols).fill(false);
    }

    // Mark occupied cells
    this.dashboardItems.forEach(item => {
      const startX = item.x || 0;
      const startY = item.y || 0;
      const endX = startX + (item.cols || 1);
      const endY = startY + (item.rows || 1);

      for (let y = startY; y < endY && y < maxRows; y++) {
        for (let x = startX; x < endX && x < maxCols; x++) {
          occupied[y][x] = true;
        }
      }
    });

    // Find first available position that fits the widget
    for (let y = 0; y < maxRows; y++) {
      for (let x = 0; x <= maxCols - cols; x++) {
        // Check if this position fits
        let fits = true;
        for (let dy = 0; dy < rows && y + dy < maxRows; dy++) {
          for (let dx = 0; dx < cols && x + dx < maxCols; dx++) {
            if (occupied[y + dy][x + dx]) {
              fits = false;
              break;
            }
          }
          if (!fits) break;
        }

        if (fits) {
          return { x, y };
        }
      }
    }

    // Fallback: add to bottom if no space found
    let maxY = 0;
    this.dashboardItems.forEach(item => {
      const bottom = (item.y || 0) + (item.rows || 2);
      if (bottom > maxY) {
        maxY = bottom;
      }
    });

    return { x: 0, y: maxY };
  }

  // ========================================
  // HELPER METHODS
  // ========================================

  /**
   * Get KPI title by widget ID
   */
  getKpiTitle(widgetId: string): string {
    const titles: { [key: string]: string } = {
      'total-properties': 'Total Properties',
      'total-rooms': 'Total Rooms',
      'bookings-today': 'Bookings Today',
      'occupancy-rate': 'Occupancy Rate'
    };
    return titles[widgetId] || widgetId.replace(/-/g, ' ').toUpperCase();
  }

  /**
   * Get KPI icon by widget ID
   */
  getKpiIcon(widgetId: string): string {
    const icons: { [key: string]: string } = {
      'total-properties': 'hotel',
      'total-rooms': 'meeting_room',
      'bookings-today': 'event_available',
      'occupancy-rate': 'people'
    };
    return icons[widgetId] || 'analytics';
  }

  /**
   * Get default list widget data
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
   * Get default chart widget data
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
   * Get default calendar widget data
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

  /**
   * Get current user ID
   */
  private getCurrentUserId(): number {
    // TODO: Get from AuthService or JWT token
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const payload = JSON.parse(atob(token.split('.')[1]));
        return parseInt(payload.UserId || payload.sub || '1');
      } catch (e) {
        console.error('Error parsing token:', e);
      }
    }
    return 1; // Default test user
  }

  /**
   * Get selected property IDs from localStorage
   */
  private getSelectedPropertyIds(): number[] {
    const stored = localStorage.getItem('selectedPropertyIds');
    if (!stored) return [];
    try {
      return JSON.parse(stored) as number[];
    } catch {
      return [];
    }
  }

  // ========================================
  // TAB MANAGEMENT
  // ========================================

  /**
   * Handle tab change event
   */
  onTabChange(event: MatTabChangeEvent): void {
    console.log(`Tab changed to index: ${event.index}, label: ${event.tab.textLabel}`);
    this.selectedTabIndex = event.index;

    // Room Planner tab selected - data is pre-loaded in background, no auto-refresh
    // User can explicitly click refresh button if needed
  }

  // ========================================
  // ROOM PLANNER METHODS
  // ========================================

  /**
   * Initialize Room Planner - Load available properties and pre-load data in background
   */
  private initializeRoomPlanner(): void {
    console.log('🏨 Initializing Room Planner in background...');

    // Pre-load room planner data in background (non-blocking)
    setTimeout(() => {
      if (!this.roomPlannerDataLoaded) {
        console.log('🏨 Pre-loading Room Planner data in background...');
        this.loadRoomPlannerData();
      }
    }, 1000); // Small delay to not interfere with dashboard loading
  }

  /**
   * Extract available properties from loaded rooms
   */
  private extractAvailablePropertiesFromRooms(): void {
    const propertyMap = new Map<number, string>();

    this.rooms.forEach(room => {
      if (room.propertyId && room.propertyName && !propertyMap.has(room.propertyId)) {
        propertyMap.set(room.propertyId, room.propertyName);
      }
    });

    this.availablePlannerProperties = Array.from(propertyMap.entries())
      .map(([id, name]) => ({ id, name }))
      .sort((a, b) => a.name.localeCompare(b.name));

    console.log('✅ Available planner properties extracted from rooms:', this.availablePlannerProperties);

    // Set first property as default if not already set
    if (this.availablePlannerProperties.length > 0 && !this.selectedPlannerPropertyId) {
      this.selectedPlannerPropertyId = this.availablePlannerProperties[0].id;
      console.log('📍 Default planner property selected:', this.selectedPlannerPropertyId);
    }
  }

  /**
   * Handle planner property selection change
   */
  onPlannerPropertyChange(propertyId: number): void {
    console.log('🏨 Planner property changed to:', propertyId);
    this.selectedPlannerPropertyId = propertyId;
    this.loadRoomPlannerData();
  }

  /**
   * Load room planner data
   */
  loadRoomPlannerData(): void {
    this.loadingRoomPlanner = true;
    this.generatePlannerDays();

    const userId = this.getCurrentUserId();
    let propertyIds: number[];

    // Use selected property from dropdown if available
    if (this.selectedPlannerPropertyId) {
      propertyIds = [this.selectedPlannerPropertyId];
      console.log(`🏨 Loading Room Planner for selected property: ${this.selectedPlannerPropertyId}`);
    } else {
      // Fallback to all selected properties or default to Property 1
      propertyIds = this.getSelectedPropertyIds();
      if (propertyIds.length === 0) {
        console.warn('⚠️ No properties selected, defaulting to Property ID 1 for Room Planner');
        propertyIds = [1];
      }
      console.log(`🏨 Loading Room Planner for properties: ${JSON.stringify(propertyIds)}`);
    }

    // Load rooms and bookings
    this.loadRoomsForPlanner(propertyIds, userId);
  }

  /**
   * Generate days for the current planner month
   */
  private generatePlannerDays(): void {
    const year = this.currentPlannerMonth.getFullYear();
    const month = this.currentPlannerMonth.getMonth();
    const daysInMonth = new Date(year, month + 1, 0).getDate();

    this.plannerDays = [];
    for (let day = 1; day <= daysInMonth; day++) {
      const date = new Date(year, month, day);
      this.plannerDays.push({
        date: date,
        dayOfWeek: date.toLocaleDateString('en-US', { weekday: 'short' })
      });
    }
  }

  /**
   * Load rooms for the selected properties
   */
  private loadRoomsForPlanner(propertyIds: number[], userId: number): void {
    console.log('🏨 Loading rooms from backend API...');

    // Call the real API
    this.dashboardService.getRoomsByProperty(userId, propertyIds, true).pipe(
      catchError(error => {
        console.error('❌ Error loading rooms:', error);
        this.notificationService.error('Failed to load rooms', 'Close');
        return of([]);
      })
    ).subscribe(rooms => {
      console.log(`✅ Rooms loaded from API: ${rooms.length} rooms`);

      // Map API response to component format
      this.rooms = rooms.map(room => ({
        id: room.roomId,
        roomNumber: room.roomNumber,
        roomName: room.roomName || room.roomNumber,
        roomType: room.roomType,
        propertyId: room.propertyId,
        propertyName: room.propertyName,
        floor: room.floor,
        capacity: room.capacity,
        status: room.status,
        amenities: room.amenities,
        pricePerNight: room.pricePerNight
      }));

      this.totalRooms = this.rooms.length;

      // Extract available properties for dropdown (first time only)
      if (this.availablePlannerProperties.length === 0) {
        this.extractAvailablePropertiesFromRooms();
      }

      // Load bookings with guest details for the month
      const startDate = new Date(this.currentPlannerMonth.getFullYear(), this.currentPlannerMonth.getMonth(), 1);
      const endDate = new Date(this.currentPlannerMonth.getFullYear(), this.currentPlannerMonth.getMonth() + 1, 0);

      console.log('🔍 Loading bookings with guest details...');
      this.dashboardService.getBookingsWithGuests(userId, startDate, endDate, propertyIds).pipe(
        catchError(error => {
          console.error('❌ Error loading bookings with guests:', error);
          this.notificationService.error('Failed to load bookings', 'Close');
          return of([]);
        })
      ).subscribe(bookings => {
        console.log(`✅ Bookings with guests loaded: ${bookings.length} bookings`);
        this.processBookingsForPlanner(bookings);
        this.calculateOccupancyStats();
        this.loadingRoomPlanner = false;
        this.roomPlannerDataLoaded = true; // Mark as loaded
        this.cdr.detectChanges();
      });
    });
  }

  /**
   * Process bookings and map them to rooms and dates
   */
  private processBookingsForPlanner(bookings: any[]): void {
    this.roomBookings.clear();
    this.roomBookingBars.clear();

    console.log(`📊 ===== PROCESSING BOOKINGS FOR PLANNER =====`);
    console.log(`📊 Received ${bookings.length} bookings from API`);
    console.log(`📊 Current rooms loaded: ${this.rooms.length}`);
    console.log(`📊 Current planner month: ${this.currentPlannerMonth.toLocaleDateString()}`);

    if (bookings.length === 0) {
      console.warn(`⚠️ No bookings received from API!`);
      console.warn(`⚠️ Check if:`);
      console.warn(`⚠️   1. Bookings collection has data for this date range`);
      console.warn(`⚠️   2. Property ID filter is correct`);
      console.warn(`⚠️   3. Backend query is returning results`);
      return;
    }

    // Log first booking structure for debugging
    console.log(`📊 First booking structure:`, bookings[0]);

    // Log available room numbers
    console.log(`📊 Available room numbers:`, this.rooms.map(r => r.roomNumber));

    let processedCount = 0;
    let skippedCount = 0;

    bookings.forEach((booking, index) => {
      // Find the room by room number
      const room = this.rooms.find(r => r.roomNumber === booking.roomNumber);

      if (room) {
        const startDate = new Date(booking.checkInDate);
        const endDate = new Date(booking.checkOutDate);

        console.log(`📌 [${index + 1}/${bookings.length}] Processing Booking:`);
        console.log(`   - Booking ID: ${booking.bookingId}`);
        console.log(`   - Room: ${booking.roomNumber} (matched room ID: ${room.id})`);
        console.log(`   - Dates: ${startDate.toDateString()} - ${endDate.toDateString()}`);
        console.log(`   - Status: ${booking.status}`);
        console.log(`   - Guest: ${booking.guestFirstName} ${booking.guestLastName}`);

        // Determine booking type/status
        const bookingType = booking.status || 'confirmed';
        const color = this.getBookingColor(bookingType);

        // Construct guest name
        const guestName = (booking.guestFirstName !== 'Unavailable' && booking.guestLastName !== 'Unavailable')
          ? `${booking.guestFirstName} ${booking.guestLastName}`
          : 'Guest';

        // Add booking for each day in the range (for backward compatibility with cell-based view)
        let currentDate = new Date(startDate);
        while (currentDate <= endDate) {
          const key = `${room.id}-${currentDate.toISOString().split('T')[0]}`;
          this.roomBookings.set(key, {
            type: bookingType,
            guestName: guestName,
            bookingId: booking.bookingId,
            color: color
          });
          currentDate.setDate(currentDate.getDate() + 1);
        }

        // Create continuous booking bar with guest details
        this.addBookingBar(room.id, {
          bookingId: booking.bookingId,
          guestName: guestName,
          type: bookingType,
          color: color,
          startDate: startDate,
          endDate: endDate,
          propertyName: booking.propertyName,
          checkInDate: startDate,
          checkOutDate: endDate,
          guestDetails: (booking.guestFirstName !== 'Unavailable') ? {
            guestId: booking.guestId,
            firstName: booking.guestFirstName,
            lastName: booking.guestLastName,
            email: booking.guestEmail,
            phoneNumber: booking.guestPhoneNumber,
            nationality: booking.guestNationality
          } : undefined
        });

        processedCount++;
      } else {
        console.warn(`⚠️ [${index + 1}/${bookings.length}] Room NOT FOUND for booking:`);
        console.warn(`   - Booking ID: ${booking.bookingId}`);
        console.warn(`   - Room Number: ${booking.roomNumber}`);
        console.warn(`   - This booking will be skipped`);
        skippedCount++;
      }
    });

    console.log(`✅ ===== BOOKING PROCESSING COMPLETE =====`);
    console.log(`✅ Successfully processed: ${processedCount} bookings`);
    console.log(`⚠️ Skipped (room not found): ${skippedCount} bookings`);
    console.log(`✅ Created ${this.roomBookingBars.size} room booking bars`);
    console.log(`✅ Total booking entries in map: ${this.roomBookings.size}`);

    // Log booking bars for each room
    this.roomBookingBars.forEach((bars, roomId) => {
      const room = this.rooms.find(r => r.id === roomId);
      console.log(`   Room ${room?.roomNumber}: ${bars.length} booking bar(s)`);
    });
  }

  /**
   * Get color for booking type
   */
  private getBookingColor(status: string): string {
    switch (status?.toLowerCase()) {
      case 'confirmed': return '#4caf50';
      case 'checkin': return '#2196f3';
      case 'checkout': return '#ff9800';
      case 'cancelled': return '#f44336';
      case 'pending': return '#ffc107';
      default: return '#4caf50';
    }
  }

  /**
   * Add a booking bar for a room (NEW METHOD)
   */
  private addBookingBar(roomId: number, booking: {
    bookingId: string;
    guestName: string;
    type: string;
    color: string;
    startDate: Date;
    endDate: Date;
    propertyName?: string;
    checkInDate?: Date;
    checkOutDate?: Date;
    guestDetails?: {
      guestId: string;
      firstName: string;
      lastName: string;
      email: string;
      phoneNumber: string;
      nationality: string;
    };
  }): void {
    const monthStart = new Date(this.currentPlannerMonth.getFullYear(), this.currentPlannerMonth.getMonth(), 1);
    const monthEnd = new Date(this.currentPlannerMonth.getFullYear(), this.currentPlannerMonth.getMonth() + 1, 0);

    // Set to end of day (23:59:59.999) to include the full last day
    monthEnd.setHours(23, 59, 59, 999);

    // Calculate start column (1-based)
    const bookingStart = new Date(Math.max(booking.startDate.getTime(), monthStart.getTime()));
    const bookingEnd = new Date(Math.min(booking.endDate.getTime(), monthEnd.getTime()));

    // Detect if booking continues from previous month or to next month
    const continuesFromPreviousMonth = booking.startDate.getTime() < monthStart.getTime();
    const continuesNextMonth = booking.endDate.getTime() > monthEnd.getTime();

    // Calculate day difference from month start (1-based, so add 1)
    const dayDiff = Math.floor((bookingStart.getTime() - monthStart.getTime()) / (1000 * 60 * 60 * 24));
    const startCol = dayDiff + 1; // 1-based column index

    // Calculate span (number of days) - adding 1 to include both start and end dates
    // For bookings extending past month end, ensure we include the last day
    const daysInMonth = monthEnd.getDate();
    const span = Math.min(
      Math.floor((bookingEnd.getTime() - bookingStart.getTime()) / (1000 * 60 * 60 * 24)) + 1,
      daysInMonth - dayDiff // Maximum span to last day of month
    );

    console.log(`📍 Booking Bar Position Calculation:`);
    console.log(`   - Month Start: ${monthStart.toDateString()}`);
    console.log(`   - Month End: ${monthEnd.toDateString()}`);
    console.log(`   - Days in Month: ${daysInMonth}`);
    console.log(`   - Booking Start: ${bookingStart.toDateString()}`);
    console.log(`   - Booking End: ${bookingEnd.toDateString()}`);
    console.log(`   - Original Booking Start: ${booking.startDate.toDateString()}`);
    console.log(`   - Original Booking End: ${booking.endDate.toDateString()}`);
    console.log(`   - Continues From Previous Month: ${continuesFromPreviousMonth ? 'YES ← ✅' : 'NO'}`);
    console.log(`   - Continues Next Month: ${continuesNextMonth ? 'YES ✅ →' : 'NO'}`);
    console.log(`   - Day Diff: ${dayDiff}`);
    console.log(`   - Start Column: ${startCol} (1-based)`);
    console.log(`   - Span: ${span} days`);
    console.log(`   - Grid Column CSS: ${startCol} / span ${span}`);

    if (span > 0) {
      const bar: BookingBar = {
        ...booking,
        startCol,
        span,
        continuesFromPreviousMonth, // NEW: Flag for previous month continuation
        continuesNextMonth // Flag for next month continuation
      };

      if (!this.roomBookingBars.has(roomId)) {
        this.roomBookingBars.set(roomId, []);
      }
      this.roomBookingBars.get(roomId)!.push(bar);

      const continuationFlags = [];
      if (continuesFromPreviousMonth) continuationFlags.push('← Continues from previous month');
      if (continuesNextMonth) continuationFlags.push('Continues to next month →');
      const continuationMessage = continuationFlags.length > 0 ? ` (${continuationFlags.join(' | ')})` : '';

      console.log(`✅ Booking bar created: Column ${startCol}, spanning ${span} day(s)${continuationMessage}`);
    } else {
      console.warn(`⚠️ Invalid span (${span}), booking bar not created`);
    }
  }

  /**
   * Get booking bars for a specific room (NEW METHOD)
   */
  getBookingBars(roomId: number): BookingBar[] {
    return this.roomBookingBars.get(roomId) || [];
  }

  /**
   * Check if a date is today
   */
  isToday(date: Date): boolean {
    const today = new Date();
    return date.toDateString() === today.toDateString();
  }

  /**
   * Check if a date is weekend
   */
  isWeekend(date: Date): boolean {
    const day = date.getDay();
    return day === 0 || day === 6; // Sunday or Saturday
  }

  /**
   * Handle booking bar click
   */
  onBookingBarClick(room: any, bar: BookingBar, event: Event): void {
    event.stopPropagation(); // Prevent day cell click
    console.log('Booking bar clicked:', room.roomNumber, bar);

    // Open dialog with comprehensive booking and guest details
    const dialogData: BookingDetailsData = {
      room: {
        roomNumber: room.roomNumber,
        roomName: room.roomName,
        roomType: room.roomType,
        floor: room.floor,
        capacity: room.capacity,
        status: room.status
      },
      date: bar.startDate,
      propertyName: bar.propertyName || room.propertyName || 'Unknown Property',
      booking: {
        type: bar.type,
        guestName: bar.guestName,
        bookingId: bar.bookingId,
        color: bar.color,
        checkInDate: bar.checkInDate,
        checkOutDate: bar.checkOutDate,
        guestDetails: bar.guestDetails
      }
    };

    const dialogRef = this.dialog.open(BookingDetailsDialogComponent, {
      width: '600px',
      data: dialogData,
      panelClass: 'booking-details-dialog'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        console.log('Dialog result:', result);

        if (result.action === 'create') {
          this.notificationService.info('Booking creation coming soon!');
        } else if (result.action === 'view') {
          this.notificationService.info('View booking details coming soon!');
        }
      }
    });
  }

  /**
   * Calculate occupancy statistics
   */
  private calculateOccupancyStats(): void {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    this.occupiedRoomsToday = 0;
    this.rooms.forEach(room => {
      const status = this.getRoomStatus(room.id, today);
      if (status === 'occupied' || status === 'checkin') {
        this.occupiedRoomsToday++;
      }
    });

    this.availableRoomsToday = this.totalRooms - this.occupiedRoomsToday;
    this.occupancyRateToday = this.totalRooms > 0 
      ? Math.round((this.occupiedRoomsToday / this.totalRooms) * 100) 
      : 0;
  }

  /**
   * Get room status for a specific date
   */
  getRoomStatus(roomId: number, date: Date): string {
    const key = `${roomId}-${date.toISOString().split('T')[0]}`;
    const booking = this.roomBookings.get(key);

    if (booking) {
      return booking.type || 'occupied';
    }

    return 'available';
  }

  /**
   * Get booking info for tooltip
   */
  getRoomTooltip(roomId: number, date: Date): string {
    const status = this.getRoomStatus(roomId, date);
    const key = `${roomId}-${date.toISOString().split('T')[0]}`;
    const booking = this.roomBookings.get(key);

    switch (status) {
      case 'occupied':
        return `Occupied by ${booking?.guestName || 'Guest'}`;
      case 'checkin':
        return `Check-in: ${booking?.guestName || 'Guest'}`;
      case 'checkout':
        return `Check-out: ${booking?.guestName || 'Guest'}`;
      case 'maintenance':
        return 'Under maintenance';
      default:
        return 'Available';
    }
  }

  /**
   * Get booking info for display
   */
  getBookingInfo(roomId: number, date: Date): any {
    const key = `${roomId}-${date.toISOString().split('T')[0]}`;
    return this.roomBookings.get(key);
  }

  /**
   * Navigate to previous/next month
   */
  navigateMonth(direction: number): void {
    this.currentPlannerMonth = new Date(
      this.currentPlannerMonth.getFullYear(),
      this.currentPlannerMonth.getMonth() + direction,
      1
    );
    this.loadRoomPlannerData();
  }

  /**
   * Go to today's date
   */
  goToToday(): void {
    this.currentPlannerMonth = new Date();
    this.loadRoomPlannerData();
  }

  /**
   * Refresh room planner data
   */
  refreshRoomPlanner(): void {
    this.loadRoomPlannerData();
    this.notificationService.success('Room planner refreshed');
  }

  /**
   * Handle room day click
   */
  onRoomDayClick(room: any, date: Date): void {
    console.log('Room day clicked:', room.roomNumber, date);

    // Get booking info for this room and date
    const bookingInfo = this.getBookingInfo(room.id, date);

    // Prepare dialog data
    const dialogData: BookingDetailsData = {
      room: {
        roomNumber: room.roomNumber,
        roomName: room.roomName,
        roomType: room.roomType,
        floor: room.floor,
        capacity: room.capacity,
        status: room.status
      },
      date: new Date(date), // Create new Date object to avoid reference issues
      booking: bookingInfo
    };

    // Open dialog
    const dialogRef = this.dialog.open(BookingDetailsDialogComponent, {
      width: '600px',
      data: dialogData,
      panelClass: 'booking-details-dialog'
    });

    // Handle dialog result
    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        console.log('Dialog result:', result);

        if (result.action === 'create') {
          // TODO: Navigate to booking creation or open booking form
          console.log('Create new booking for room:', result.room.roomNumber, 'on', result.date);
          this.notificationService.info('Booking creation coming soon!');
        } else if (result.action === 'view') {
          // TODO: Navigate to booking details page
          console.log('View booking:', result.bookingId);
          this.notificationService.info('View booking details coming soon!');
        }
      }
    });
  }

  /**
   * Dashboard selection changed
   */
  onDashboardChange(event: MatSelectChange): void {
    if (this.hasUnsavedChanges) {
      this.notificationService.confirmUnsavedChanges().subscribe(confirmed => {
        if (!confirmed) {
          // Revert selection
          event.source.value = this.selectedDashboard;
          return;
        }
        this.performDashboardChange(event.value);
      });
      return;
    }
    this.performDashboardChange(event.value);
  }

  private performDashboardChange(dashboardId: string): void {

    this.selectedDashboard = dashboardId;

    if (dashboardId) {
      this.loadDashboard(dashboardId);
    } else {
      // "Create New" option
      this.dashboardConfig = null;
      this.loadDefaultWidgets();
      this.refreshAllWidgetData();
    }
  }

  /**
   * Handle "More Details" button click from widgets
   */
  onWidgetMoreDetails(item: DashboardGridsterItem): void {
    console.log('📋 More Details clicked for widget:', item.widgetId, item.widgetType);

    // Handle different widget types
    switch (item.widgetType) {
      case 'kpi-card':
        this.handleKpiMoreDetails(item);
        break;
      case 'list':
        this.handleListMoreDetails(item);
        break;
      case 'chart':
        this.handleChartMoreDetails(item);
        break;
      case 'calendar':
        this.handleCalendarMoreDetails(item);
        break;
      default:
        console.warn('More Details not implemented for widget type:', item.widgetType);
    }
  }

  /**
   * Handle More Details for KPI widgets
   */
  private handleKpiMoreDetails(item: DashboardGridsterItem): void {
    switch (item.widgetId) {
      case 'bookings-today':
        // Navigate to bookings report filtered for today's bookings
        this.navigateToBookingsReport('today');
        break;
      case 'total-properties':
        // Navigate to properties list
        console.log('Navigate to properties list');
        // TODO: Implement when properties report is ready
        break;
      case 'total-rooms':
        // Navigate to rooms list
        console.log('Navigate to rooms list');
        // TODO: Implement when rooms report is ready
        break;
      case 'occupancy-rate':
        // Navigate to occupancy report
        console.log('Navigate to occupancy report');
        // TODO: Implement when occupancy report is ready
        break;
      default:
        console.warn('More Details not configured for KPI:', item.widgetId);
    }
  }

  /**
   * Handle More Details for List widgets
   */
  private handleListMoreDetails(item: DashboardGridsterItem): void {
    if (item.currentListView === 'bookings') {
      // Navigate to recent bookings
      this.navigateToBookingsReport('recent');
    } else {
      // Navigate to activity log
      console.log('Navigate to activity log');
      // TODO: Implement when activity log page is ready
    }
  }

  /**
   * Handle More Details for Chart widgets
   */
  private handleChartMoreDetails(item: DashboardGridsterItem): void {
    // Navigate to full analytics/reports page with current filter settings
    const settings = item.settings || {};
    const daysBack = settings.daysBack || 30;

    // Calculate date range based on current filter
    const today = new Date();
    const startDate = new Date(today.getTime() - daysBack * 24 * 60 * 60 * 1000);

    const queryParams: any = {
      startDate: startDate.toISOString().split('T')[0],
      endDate: today.toISOString().split('T')[0],
      filter: 'trends',
      daysBack: daysBack,
      groupBy: settings.groupBy || 'day'
    };

    console.log('📊 Navigating to bookings report with chart filters:', queryParams);
    this.router.navigate(['/bookings'], { queryParams });
  }

  /**
   * Handle More Details for Calendar widgets
   */
  private handleCalendarMoreDetails(item: DashboardGridsterItem): void {
    // Navigate to calendar view with current month
    this.navigateToBookingsReport('calendar');
  }

  /**
   * Handle calendar day click event
   */
  onCalendarDayClick(event: { date: Date; events: any[] }): void {
    console.log('📅 Calendar day clicked:', event.date, 'Events:', event.events.length);

    // Navigate to bookings report filtered for the specific day
    const selectedDate = event.date.toISOString().split('T')[0]; // YYYY-MM-DD format
    const queryParams = {
      bookingDate: selectedDate,
      filter: 'calendar-day'
    };

    console.log('🔗 Navigating to bookings report for date:', selectedDate);
    this.router.navigate(['/bookings'], { queryParams });
  }

  /**
   * Navigate to bookings report with specific filter
   */
  private navigateToBookingsReport(filterType: 'today' | 'recent' | 'trends' | 'calendar'): void {
    const today = new Date();
    const queryParams: any = {};

    switch (filterType) {
      case 'today':
        // Filter for today's bookings
        queryParams.bookingDate = today.toISOString().split('T')[0]; // YYYY-MM-DD format
        queryParams.filter = 'today';
        break;
      case 'recent':
        // Show recent bookings (last 7 days)
        const weekAgo = new Date(today.getTime() - 7 * 24 * 60 * 60 * 1000);
        queryParams.startDate = weekAgo.toISOString().split('T')[0];
        queryParams.endDate = today.toISOString().split('T')[0];
        queryParams.filter = 'recent';
        break;
      case 'trends':
        // Show last 30 days for trends
        const monthAgo = new Date(today.getTime() - 30 * 24 * 60 * 60 * 1000);
        queryParams.startDate = monthAgo.toISOString().split('T')[0];
        queryParams.endDate = today.toISOString().split('T')[0];
        queryParams.filter = 'trends';
        break;
      case 'calendar':
        // Show current month
        queryParams.view = 'calendar';
        queryParams.month = today.getMonth() + 1;
        queryParams.year = today.getFullYear();
        break;
    }

    console.log('🔗 Navigating to bookings report with params:', queryParams);
    this.router.navigate(['/bookings'], { queryParams });
  }

  // ========================================
  // INTERACTIVE FEATURES: DRAG & DROP
  // ========================================

  /**
   * Handle booking drag start
   */
  onBookingDragStart(bar: BookingBar, roomId: number, event: DragEvent): void {
    console.log('🎯 Drag started:', bar.guestName, 'from room', roomId);

    this.draggedBooking = bar;
    this.draggedFromRoomId = roomId;
    this.isDragging = true;

    // Set drag data
    if (event.dataTransfer) {
      event.dataTransfer.effectAllowed = 'move';
      event.dataTransfer.setData('text/plain', bar.bookingId);
    }

    // Add dragging class to element after short delay (to avoid flickering)
    setTimeout(() => {
      const element = event.target as HTMLElement;
      element.classList.add('dragging');
    }, 0);
  }

  /**
   * Handle booking drag over room row
   */
  onRoomDragOver(roomId: number, event: DragEvent): void {
    if (!this.isDragging || !this.draggedBooking) return;

    event.preventDefault(); // Required to allow drop
    event.stopPropagation();

    if (event.dataTransfer) {
      event.dataTransfer.dropEffect = 'move';
    }

    // Update drop target
    if (this.dropTargetRoomId !== roomId) {
      this.dropTargetRoomId = roomId;
      console.log('📍 Drop target updated:', roomId);
    }
  }

  /**
   * Handle drag leave from room row
   */
  onRoomDragLeave(roomId: number, event: DragEvent): void {
    if (this.dropTargetRoomId === roomId) {
      this.dropTargetRoomId = null;
    }
  }

  /**
   * Handle drop on room row
   */
  onRoomDrop(roomId: number, event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();

    if (!this.draggedBooking || !this.draggedFromRoomId) {
      console.warn('⚠️ No dragged booking data');
      return;
    }

    console.log('📦 Drop event:', {
      booking: this.draggedBooking.bookingId,
      fromRoom: this.draggedFromRoomId,
      toRoom: roomId
    });

    // Check if dropped on same room
    if (roomId === this.draggedFromRoomId) {
      console.log('ℹ️ Dropped on same room, no action needed');
      this.clearDragState();
      return;
    }

    // Get room information
    const targetRoom = this.rooms.find(r => r.id === roomId);
    const sourceRoom = this.rooms.find(r => r.id === this.draggedFromRoomId);

    if (!targetRoom || !sourceRoom) {
      this.notificationService.error('Room not found');
      this.clearDragState();
      return;
    }

    // Check if target room is available for the booking dates
    const isAvailable = this.checkRoomAvailability(
      roomId, 
      this.draggedBooking.startDate, 
      this.draggedBooking.endDate
    );

    if (!isAvailable) {
      this.notificationService.warning(
        `Room ${targetRoom.roomNumber} is not available for these dates`
      );
      this.clearDragState();
      return;
    }

    // Confirm move
    this.notificationService.confirm(
      'Move Booking?',
      `Move ${this.draggedBooking.guestName}'s booking from Room ${sourceRoom.roomNumber} to Room ${targetRoom.roomNumber}?`,
      'Move',
      'Cancel',
      'swap_horiz',
      '#2196f3',
      'primary'
    ).subscribe(confirmed => {
      if (confirmed) {
        this.performBookingMove(this.draggedBooking!, roomId);
      }
      this.clearDragState();
    });
  }

  /**
   * Handle drag end
   */
  onBookingDragEnd(event: DragEvent): void {
    console.log('🏁 Drag ended');

    // Remove dragging class
    const element = event.target as HTMLElement;
    element.classList.remove('dragging');

    this.clearDragState();
  }

  /**
   * Clear drag state
   */
  private clearDragState(): void {
    this.draggedBooking = null;
    this.draggedFromRoomId = null;
    this.isDragging = false;
    this.dropTargetRoomId = null;
    this.dropTargetDate = null;
  }

  /**
   * Check if room is available for given dates
   */
  private checkRoomAvailability(roomId: number, startDate: Date, endDate: Date): boolean {
    // Get all booking bars for this room
    const bars = this.roomBookingBars.get(roomId) || [];

    // Check for overlaps
    for (const bar of bars) {
      // Skip if it's the same booking being moved
      if (this.draggedBooking && bar.bookingId === this.draggedBooking.bookingId) {
        continue;
      }

      // Check date overlap
      const barStart = new Date(bar.startDate);
      const barEnd = new Date(bar.endDate);
      const checkStart = new Date(startDate);
      const checkEnd = new Date(endDate);

      if (checkStart <= barEnd && checkEnd >= barStart) {
        return false; // Overlap detected
      }
    }

    return true; // No conflicts
  }

  /**
   * Perform booking move to another room
   */
  private performBookingMove(booking: BookingBar, newRoomId: number): void {
    console.log('🚀 Moving booking:', booking.bookingId, 'to room:', newRoomId);

    // TODO: Call backend API to move booking
    // For now, just update the UI optimistically
    const sourceRoom = this.rooms.find(r => r.id === this.draggedFromRoomId);
    const targetRoom = this.rooms.find(r => r.id === newRoomId);

    if (!sourceRoom || !targetRoom) return;

    // Remove from source room
    const sourceBars = this.roomBookingBars.get(this.draggedFromRoomId!) || [];
    const updatedSourceBars = sourceBars.filter(b => b.bookingId !== booking.bookingId);
    this.roomBookingBars.set(this.draggedFromRoomId!, updatedSourceBars);

    // Add to target room
    if (!this.roomBookingBars.has(newRoomId)) {
      this.roomBookingBars.set(newRoomId, []);
    }
    this.roomBookingBars.get(newRoomId)!.push({ ...booking });

    // Update room bookings map
    let currentDate = new Date(booking.startDate);
    const endDate = new Date(booking.endDate);
    while (currentDate <= endDate) {
      const sourceKey = `${this.draggedFromRoomId}-${currentDate.toISOString().split('T')[0]}`;
      const targetKey = `${newRoomId}-${currentDate.toISOString().split('T')[0]}`;

      const bookingData = this.roomBookings.get(sourceKey);
      if (bookingData) {
        this.roomBookings.delete(sourceKey);
        this.roomBookings.set(targetKey, bookingData);
      }

      currentDate.setDate(currentDate.getDate() + 1);
    }

    this.cdr.detectChanges();
    this.notificationService.success(
      `Booking moved from Room ${sourceRoom.roomNumber} to Room ${targetRoom.roomNumber}!`
    );

    // TODO: Call actual backend API
    // this.dashboardService.moveBooking(booking.bookingId, newRoomId).subscribe(...)
  }

  // ========================================
  // INTERACTIVE FEATURES: RESIZE
  // ========================================

  /**
   * Handle resize start
   */
  onResizeStart(bar: BookingBar, roomId: number, direction: 'left' | 'right', event: MouseEvent): void {
    event.stopPropagation(); // Prevent drag start
    event.preventDefault();

    console.log('📏 Resize started:', direction, bar.guestName);

    this.resizingBooking = bar;
    this.resizingRoomId = roomId;
    this.resizeDirection = direction;
    this.resizeStartX = event.clientX;
    this.resizeOriginalSpan = bar.span;
    this.resizeOriginalStartCol = bar.startCol;
    this.isResizing = true;

    // Add resizing class
    const barElement = (event.target as HTMLElement).closest('.booking-bar');
    if (barElement) {
      barElement.classList.add('resizing');
    }

    // Add global cursor override
    document.body.classList.add('resizing');

    // Add global mouse listeners
    document.addEventListener('mousemove', this.onResizeMove);
    document.addEventListener('mouseup', this.onResizeEnd);
  }

  /**
   * Handle resize move
   */
  onResizeMove = (event: MouseEvent): void => {
    if (!this.isResizing || !this.resizingBooking || !this.resizingRoomId) return;

    const deltaX = event.clientX - this.resizeStartX;

    // Calculate day width from grid (approximately 60px per day)
    const dayWidth = 60; // This should match CSS grid column width
    const daysDelta = Math.round(deltaX / dayWidth);

    console.log('📏 Resize delta:', daysDelta, 'days');

    let newSpan = this.resizeOriginalSpan;
    let newStartCol = this.resizeOriginalStartCol;

    if (this.resizeDirection === 'right') {
      // Extending/shortening from end date
      newSpan = this.resizeOriginalSpan + daysDelta;
    } else {
      // Extending/shortening from start date
      newSpan = this.resizeOriginalSpan - daysDelta;
      newStartCol = this.resizeOriginalStartCol + daysDelta;
    }

    // Minimum 1 day booking
    if (newSpan < 1) {
      newSpan = 1;
      if (this.resizeDirection === 'left') {
        newStartCol = this.resizeOriginalStartCol + this.resizeOriginalSpan - 1;
      }
    }

    // Maximum span to end of month
    const daysInMonth = this.plannerDays.length;
    if (newStartCol + newSpan > daysInMonth + 1) {
      newSpan = daysInMonth - newStartCol + 1;
    }

    // Update booking bar visually
    this.resizingBooking.span = newSpan;
    this.resizingBooking.startCol = newStartCol;
    this.cdr.detectChanges();
  };

  /**
   * Handle resize end
   */
  onResizeEnd = (event: MouseEvent): void => {
    if (!this.isResizing || !this.resizingBooking || !this.resizingRoomId) return;

    console.log('🏁 Resize ended');

    // Remove global listeners
    document.removeEventListener('mousemove', this.onResizeMove);
    document.removeEventListener('mouseup', this.onResizeEnd);

    // Remove global cursor override
    document.body.classList.remove('resizing');

    // Remove resizing class
    const barElements = document.querySelectorAll('.booking-bar.resizing');
    barElements.forEach(el => el.classList.remove('resizing'));

    // Check if size actually changed
    const spanChanged = this.resizingBooking.span !== this.resizeOriginalSpan;
    const startChanged = this.resizingBooking.startCol !== this.resizeOriginalStartCol;

    if (spanChanged || startChanged) {
      // Calculate new dates
      const monthStart = new Date(
        this.currentPlannerMonth.getFullYear(), 
        this.currentPlannerMonth.getMonth(), 
        1
      );

      const newStartDate = new Date(monthStart);
      newStartDate.setDate(this.resizingBooking.startCol);

      const newEndDate = new Date(newStartDate);
      newEndDate.setDate(newStartDate.getDate() + this.resizingBooking.span - 1);

      console.log('📅 New dates:', {
        start: newStartDate.toDateString(),
        end: newEndDate.toDateString(),
        span: this.resizingBooking.span
      });

      // Check availability for new dates
      const isAvailable = this.checkRoomAvailabilityForResize(
        this.resizingRoomId,
        newStartDate,
        newEndDate,
        this.resizingBooking.bookingId
      );

      if (!isAvailable) {
        this.notificationService.warning('Cannot resize: dates conflict with another booking');
        // Revert to original size
        this.resizingBooking.span = this.resizeOriginalSpan;
        this.resizingBooking.startCol = this.resizeOriginalStartCol;
        this.cdr.detectChanges();
      } else {
        // Confirm resize
        const action = this.resizingBooking.span > this.resizeOriginalSpan ? 'extend' : 'shorten';
        this.performBookingResize(
          this.resizingBooking,
          this.resizingRoomId,
          newStartDate,
          newEndDate,
          action
        );
      }
    }

    // Clear resize state
    this.clearResizeState();
  };

  /**
   * Check room availability for resize (excluding current booking)
   */
  private checkRoomAvailabilityForResize(
    roomId: number, 
    startDate: Date, 
    endDate: Date, 
    excludeBookingId: string
  ): boolean {
    const bars = this.roomBookingBars.get(roomId) || [];

    for (const bar of bars) {
      if (bar.bookingId === excludeBookingId) continue;

      const barStart = new Date(bar.startDate);
      const barEnd = new Date(bar.endDate);
      const checkStart = new Date(startDate);
      const checkEnd = new Date(endDate);

      if (checkStart <= barEnd && checkEnd >= barStart) {
        return false; // Overlap detected
      }
    }

    return true;
  }

  /**
   * Perform booking resize
   */
  private performBookingResize(
    booking: BookingBar, 
    roomId: number, 
    newStartDate: Date, 
    newEndDate: Date,
    action: 'extend' | 'shorten'
  ): void {
    console.log('🚀 Resizing booking:', booking.bookingId, action);

    // Update booking dates
    booking.startDate = newStartDate;
    booking.endDate = newEndDate;

    // Recalculate room bookings map
    // First, remove old entries
    let currentDate = new Date(booking.startDate);
    const endDate = new Date(booking.endDate);

    // Clear old entries for this booking
    this.roomBookings.forEach((value, key) => {
      if (value.bookingId === booking.bookingId) {
        this.roomBookings.delete(key);
      }
    });

    // Add new entries
    currentDate = new Date(newStartDate);
    while (currentDate <= newEndDate) {
      const key = `${roomId}-${currentDate.toISOString().split('T')[0]}`;
      this.roomBookings.set(key, {
        type: booking.type,
        guestName: booking.guestName,
        bookingId: booking.bookingId,
        color: booking.color
      });
      currentDate.setDate(currentDate.getDate() + 1);
    }

    this.cdr.detectChanges();

    const actionText = action === 'extend' ? 'extended' : 'shortened';
    this.notificationService.success(`Booking ${actionText} successfully!`);

    // TODO: Call actual backend API
    // this.dashboardService.resizeBooking(booking.bookingId, newStartDate, newEndDate).subscribe(...)
  }

  /**
   * Clear resize state
   */
  private clearResizeState(): void {
    this.resizingBooking = null;
    this.resizingRoomId = null;
    this.resizeDirection = null;
    this.isResizing = false;
    this.resizeStartX = 0;
    this.resizeOriginalSpan = 0;
    this.resizeOriginalStartCol = 0;
  }

  // ========================================
  // INTERACTIVE FEATURES: CONTEXT MENU
  // ========================================

  /**
   * Handle right-click on booking bar
   */
  onBookingRightClick(bar: BookingBar, roomId: number, event: MouseEvent): void {
    event.preventDefault();
    event.stopPropagation();

    console.log('🖱️ Right-click on booking:', bar.guestName);

    const room = this.rooms.find(r => r.id === roomId);
    if (!room) return;

    const data: BookingContextMenuData = {
      bookingId: bar.bookingId,
      guestName: bar.guestName,
      roomNumber: room.roomNumber,
      checkInDate: bar.checkInDate || bar.startDate,
      checkOutDate: bar.checkOutDate || bar.endDate,
      status: bar.type,
      position: { x: event.clientX, y: event.clientY }
    };

    const dialogRef = this.dialog.open(BookingContextMenuComponent, {
      position: { 
        left: `${event.clientX}px`, 
        top: `${event.clientY}px` 
      },
      backdropClass: 'context-menu-backdrop',
      panelClass: 'context-menu-panel',
      data: data
    });

    // Add active state to booking bar
    const barElement = (event.target as HTMLElement).closest('.booking-bar');
    if (barElement) {
      barElement.classList.add('context-menu-active');
    }

    dialogRef.afterClosed().subscribe((result: BookingContextMenuResult | undefined) => {
      // Remove active state
      if (barElement) {
        barElement.classList.remove('context-menu-active');
      }

      if (result) {
        this.handleContextMenuAction(result, bar, roomId);
      }
    });
  }

  /**
   * Handle context menu action selection
   */
  private handleContextMenuAction(result: BookingContextMenuResult, bar: BookingBar, roomId: number): void {
    console.log('⚡ Context menu action:', result.action, 'for booking:', result.bookingId);

    switch (result.action) {
      case 'view-details':
        // Open booking details dialog
        this.openBookingDetailsDialog(bar, roomId);
        break;

      case 'edit':
        this.notificationService.info('Edit booking feature coming soon!');
        // TODO: Navigate to booking edit page or open edit dialog
        break;

      case 'extend':
        this.notificationService.info('Tip: Drag the right edge of the booking bar to extend the stay!', '', 4000);
        break;

      case 'shorten':
        this.notificationService.info('Tip: Drag the left edge of the booking bar to shorten the stay!', '', 4000);
        break;

      case 'move':
        this.notificationService.info('Tip: Drag the booking bar to a different room to move it!', '', 4000);
        break;

      case 'upgrade':
        this.notificationService.info('Room upgrade feature coming soon!');
        // TODO: Show room upgrade options
        break;

      case 'cancel':
        this.confirmCancelBooking(bar, roomId);
        break;

      default:
        console.warn('Unknown action:', result.action);
    }
  }

  /**
   * Open booking details dialog from context menu
   */
  private openBookingDetailsDialog(bar: BookingBar, roomId: number): void {
    const room = this.rooms.find(r => r.id === roomId);
    if (!room) return;

    const dialogData: BookingDetailsData = {
      room: {
        roomNumber: room.roomNumber,
        roomName: room.roomName,
        roomType: room.roomType,
        floor: room.floor,
        capacity: room.capacity,
        status: room.status
      },
      date: bar.startDate,
      propertyName: bar.propertyName || room.propertyName || 'Unknown Property',
      booking: {
        type: bar.type,
        guestName: bar.guestName,
        bookingId: bar.bookingId,
        color: bar.color,
        checkInDate: bar.checkInDate,
        checkOutDate: bar.checkOutDate,
        guestDetails: bar.guestDetails
      }
    };

    this.dialog.open(BookingDetailsDialogComponent, {
      width: '600px',
      data: dialogData,
      panelClass: 'booking-details-dialog'
    });
  }

  /**
   * Confirm and cancel booking
   */
  private confirmCancelBooking(bar: BookingBar, roomId: number): void {
    const room = this.rooms.find(r => r.id === roomId);
    if (!room) return;

    this.notificationService.confirm(
      'Cancel Booking?',
      `Are you sure you want to cancel the booking for ${bar.guestName} in Room ${room.roomNumber}? This action cannot be undone.`,
      'Cancel Booking',
      'Keep Booking',
      'cancel',
      '#f44336',
      'warn'
    ).subscribe(confirmed => {
      if (confirmed) {
        this.performBookingCancellation(bar, roomId);
      }
    });
  }

  /**
   * Perform booking cancellation
   */
  private performBookingCancellation(bar: BookingBar, roomId: number): void {
    console.log('🚀 Cancelling booking:', bar.bookingId);

    // Remove from room booking bars
    const bars = this.roomBookingBars.get(roomId) || [];
    const updatedBars = bars.filter(b => b.bookingId !== bar.bookingId);
    this.roomBookingBars.set(roomId, updatedBars);

    // Remove from room bookings map
    let currentDate = new Date(bar.startDate);
    const endDate = new Date(bar.endDate);
    while (currentDate <= endDate) {
      const key = `${roomId}-${currentDate.toISOString().split('T')[0]}`;
      this.roomBookings.delete(key);
      currentDate.setDate(currentDate.getDate() + 1);
    }

    this.cdr.detectChanges();
    this.notificationService.success(`Booking for ${bar.guestName} cancelled successfully!`);

    // TODO: Call actual backend API
    // this.dashboardService.cancelBooking(bar.bookingId).subscribe(...)
  }
}

