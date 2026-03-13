import { Component, OnInit, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
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
import { Router } from '@angular/router';
import { GridsterModule, GridsterConfig, GridsterItem } from 'angular-gridster2';
import { DashboardService } from '../../services/dashboard.service';
import { 
  DashboardConfiguration, 
  WidgetLibraryItem, 
  SaveDashboardRequest 
} from '../../models/dashboard.models';
import { KpiCardWidgetComponent, KpiCardData } from '../../widgets/kpi-card-widget/kpi-card-widget.component';
import { ListWidgetComponent, ListWidgetData } from '../../widgets/list-widget/list-widget.component';
import { ChartWidgetComponent, ChartWidgetData } from '../../widgets/chart-widget/chart-widget.component';
import { CalendarWidgetComponent, CalendarWidgetData } from '../../widgets/calendar-widget/calendar-widget.component';
import { WidgetPickerDialogComponent } from '../../widgets/widget-picker-dialog/widget-picker-dialog.component';
import { GridsterConfigService } from '../../services/gridster-config.service';

export interface DashboardType {
  value: string;
  label: string;
  icon: string;
  route: string;
}

export interface DashboardGridsterItem extends GridsterItem {
  widgetId: string;
  widgetType: string;
  settings?: any;
  data?: any;
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
    MatDialogModule,
    MatTooltipModule,
    MatBadgeModule,
    GridsterModule,
    KpiCardWidgetComponent,
    ListWidgetComponent,
    ChartWidgetComponent,
    CalendarWidgetComponent
  ],
  templateUrl: './dashboard1.component.html',
  styleUrls: ['./dashboard1.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class Dashboard1Component implements OnInit {

  // ===== Dashboard Selection =====
  selectedDashboard: string = 'dashboard1';
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

  // ===== State Management =====
  loading = false;
  dashboardConfig: DashboardConfiguration | null = null;
  widgetLibrary: WidgetLibraryItem[] = [];

  // ===== Phase 3: Edit Mode =====
  editMode = false;
  hasUnsavedChanges = false;

  // ===== Phase 3: Gridster =====
  options: GridsterConfig;
  dashboardItems: DashboardGridsterItem[] = [];

  constructor(
    private router: Router,
    private dashboardService: DashboardService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog
  ) {
    // Initialize gridster configuration
    this.options = GridsterConfigService.getDefaultConfig(false);
  }

  ngOnInit(): void {
    console.log('Dashboard1 component initialized');
    this.loadDashboard();
    this.loadWidgetLibrary();
  }

  // ========================================
  // PHASE 3: EDIT MODE METHODS
  // ========================================

  /**
   * Toggle edit mode on/off
   */
  toggleEditMode(): void {
    if (this.editMode && this.hasUnsavedChanges) {
      const confirmExit = confirm('You have unsaved changes. Do you want to discard them?');
      if (!confirmExit) return;
    }

    this.editMode = !this.editMode;
    this.options = GridsterConfigService.getDefaultConfig(this.editMode);

    if (this.options.api && this.options.api.optionsChanged) {
      this.options.api.optionsChanged();
    }

    if (!this.editMode) {
      this.hasUnsavedChanges = false;
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
    // Find next available position
    const position = this.findNextAvailablePosition();

    const newItem: DashboardGridsterItem = {
      x: position.x,
      y: position.y,
      cols: widget.defaultSize.width,
      rows: widget.defaultSize.height,
      widgetId: widget.widgetId,
      widgetType: widget.widgetType,
      settings: { ...widget.defaultSettings },
      data: this.getWidgetData(widget.widgetId, widget.widgetType)
    };

    this.dashboardItems.push(newItem);
    this.hasUnsavedChanges = true;

    this.snackBar.open(`${widget.name} added to dashboard`, 'Close', { duration: 2000 });
    console.log('Widget added:', widget.widgetId);
  }

  /**
   * Remove widget from dashboard
   */
  removeWidget(item: DashboardGridsterItem): void {
    const widgetName = this.widgetLibrary.find(w => w.widgetId === item.widgetId)?.name || 'Widget';
    const confirmRemove = confirm(`Remove ${widgetName} from dashboard?`);

    if (confirmRemove) {
      const index = this.dashboardItems.indexOf(item);
      if (index > -1) {
        this.dashboardItems.splice(index, 1);
        this.hasUnsavedChanges = true;
        this.snackBar.open(`${widgetName} removed`, 'Close', { duration: 2000 });
        console.log('Widget removed:', item.widgetId);
      }
    }
  }

  /**
   * Save dashboard configuration
   */
  saveDashboard(): void {
    this.loading = true;

    const widgets = this.dashboardItems.map(item => GridsterConfigService.toWidgetConfig(item));

    const request: SaveDashboardRequest = {
      id: this.dashboardConfig?.id,
      userId: this.getCurrentUserId(),
      dashboardName: this.dashboardConfig?.dashboardName || 'My Dashboard',
      isDefault: true,
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
        this.snackBar.open('Dashboard saved successfully!', 'Close', { duration: 3000 });
        this.hasUnsavedChanges = false;
        this.editMode = false;
        this.loading = false;

        // Reload dashboard
        this.loadDashboard();
      },
      error: (error) => {
        console.error('Error saving dashboard:', error);
        this.snackBar.open('Failed to save dashboard', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  /**
   * Cancel edit mode
   */
  cancelEdit(): void {
    if (this.hasUnsavedChanges) {
      const confirmCancel = confirm('Discard unsaved changes?');
      if (!confirmCancel) return;
    }

    this.editMode = false;
    this.hasUnsavedChanges = false;

    // Reload dashboard to restore original state
    this.loadDashboard();
  }

  /**
   * Item changed (drag/resize)
   */
  itemChange(item: GridsterItem, itemComponent: any): void {
    if (this.editMode) {
      this.hasUnsavedChanges = true;
      console.log('Item changed:', item);
    }
  }

  /**
   * Find next available position in grid
   */
  private findNextAvailablePosition(): { x: number; y: number } {
    if (this.dashboardItems.length === 0) {
      return { x: 0, y: 0 };
    }

    // Find the lowest Y position with available space
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
  // DATA LOADING METHODS
  // ========================================

  /**
   * Load user's dashboard configuration from API
   */
  loadDashboard(): void {
    this.loading = true;
    const userId = this.getCurrentUserId();

    this.dashboardService.getDashboardByUserId(userId, true).subscribe({
      next: (config) => {
        console.log('Dashboard config loaded:', config);
        this.dashboardConfig = config;

        if (config) {
          this.renderWidgets(config);
        } else {
          // No saved dashboard, load default widgets
          this.loadDefaultWidgets();
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading dashboard:', error);
        this.snackBar.open('Failed to load dashboard', 'Close', { duration: 3000 });
        // Fallback to default widgets
        this.loadDefaultWidgets();
        this.loading = false;
      }
    });
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
        data: this.getWidgetData(widget.widgetId, widget.widgetType)
      };

      this.dashboardItems.push(item);
    });

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
        data: {
          title: 'Total Properties',
          value: 24,
          icon: 'hotel',
          color: '#1976d2',
          showTrend: true,
          trendValue: 12,
          trendDirection: 'up'
        }
      },
      {
        x: 3, y: 0, cols: 3, rows: 2,
        widgetId: 'total-rooms',
        widgetType: 'kpi-card',
        data: {
          title: 'Total Rooms',
          value: 156,
          icon: 'meeting_room',
          color: '#1976d2'
        }
      },
      {
        x: 6, y: 0, cols: 3, rows: 2,
        widgetId: 'bookings-today',
        widgetType: 'kpi-card',
        data: {
          title: 'Bookings Today',
          value: 12,
          icon: 'event_available',
          color: '#1976d2'
        }
      },
      {
        x: 9, y: 0, cols: 3, rows: 2,
        widgetId: 'occupancy-rate',
        widgetType: 'kpi-card',
        data: {
          title: 'Occupancy Rate',
          value: '78%',
          icon: 'people',
          color: '#1976d2',
          showTrend: true,
          trendValue: 5,
          trendDirection: 'up'
        }
      },
      // Row 2: Chart Widget
      {
        x: 0, y: 2, cols: 12, rows: 4,
        widgetId: 'revenue-chart',
        widgetType: 'chart',
        data: this.getDefaultChartWidget()
      },
      // Row 3: List and Calendar
      {
        x: 0, y: 6, cols: 6, rows: 4,
        widgetId: 'recent-activity',
        widgetType: 'list',
        data: this.getDefaultListWidget()
      },
      {
        x: 6, y: 6, cols: 6, rows: 4,
        widgetId: 'booking-calendar',
        widgetType: 'calendar',
        data: this.getDefaultCalendarWidget()
      }
    ];

    console.log('Loaded default widgets:', this.dashboardItems.length);
  }

  // ========================================
  // WIDGET DATA HELPERS
  // ========================================

  /**
   * Get widget data by type
   */
  getWidgetData(widgetId: string, widgetType: string): any {
    switch (widgetType) {
      case 'kpi-card':
        return this.getKpiCardData(widgetId);
      case 'list':
        return this.getDefaultListWidget();
      case 'chart':
        return this.getDefaultChartWidget();
      case 'calendar':
        return this.getDefaultCalendarWidget();
      default:
        return {};
    }
  }

  /**
   * Get KPI card data
   */
  getKpiCardData(widgetId: string): KpiCardData {
    const mockData: { [key: string]: KpiCardData } = {
      'total-properties': {
        title: 'Total Properties',
        value: 24,
        icon: 'hotel',
        color: '#1976d2',
        showTrend: true,
        trendValue: 12,
        trendDirection: 'up'
      },
      'total-rooms': {
        title: 'Total Rooms',
        value: 156,
        icon: 'meeting_room',
        color: '#1976d2'
      },
      'bookings-today': {
        title: 'Bookings Today',
        value: 12,
        icon: 'event_available',
        color: '#1976d2'
      },
      'occupancy-rate': {
        title: 'Occupancy Rate',
        value: '78%',
        icon: 'people',
        color: '#1976d2',
        showTrend: true,
        trendValue: 5,
        trendDirection: 'up'
      }
    };

    return mockData[widgetId] || {
      title: widgetId.replace(/-/g, ' ').toUpperCase(),
      value: 0,
      icon: 'analytics',
      color: '#1976d2'
    };
  }

  /**
   * Get default list widget data
   */
  getDefaultListWidget(): ListWidgetData {
    return {
      title: 'Recent Activity',
      items: this.getListItems('recent-activity'),
      emptyMessage: 'No recent activity'
    };
  }

  /**
   * Get default chart widget data
   */
  getDefaultChartWidget(): ChartWidgetData {
    return {
      title: 'Booking Trends',
      labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
      datasets: [
        {
          label: 'Bookings',
          data: [12, 19, 15, 25, 22, 30],
          backgroundColor: '#1976d2',
          borderColor: '#1976d2'
        }
      ],
      chartType: 'line'
    };
  }

  /**
   * Get default calendar widget data
   */
  getDefaultCalendarWidget(): CalendarWidgetData {
    return {
      title: 'Booking Calendar',
      events: this.getCalendarEvents(),
      currentDate: new Date()
    };
  }

  /**
   * Get list items (mock data)
   */
  getListItems(widgetId: string): any[] {
    return [
      {
        id: 1,
        icon: 'check_circle',
        iconColor: '#4caf50',
        title: 'New booking confirmed',
        subtitle: 'Room 101 - John Doe',
        timestamp: new Date(Date.now() - 3600000), // 1 hour ago
        metadata: 'Booking'
      },
      {
        id: 2,
        icon: 'person_add',
        iconColor: '#2196f3',
        title: 'New guest checked in',
        subtitle: 'Room 205 - Jane Smith',
        timestamp: new Date(Date.now() - 7200000), // 2 hours ago
        metadata: 'Check-in'
      },
      {
        id: 3,
        icon: 'edit',
        iconColor: '#ff9800',
        title: 'Property updated',
        subtitle: 'Grand Hotel - Details modified',
        timestamp: new Date(Date.now() - 10800000), // 3 hours ago
        metadata: 'Update'
      },
      {
        id: 4,
        icon: 'payment',
        iconColor: '#9c27b0',
        title: 'Payment received',
        subtitle: '$250.00 - Room 303',
        timestamp: new Date(Date.now() - 14400000), // 4 hours ago
        metadata: 'Payment'
      },
      {
        id: 5,
        icon: 'event_busy',
        iconColor: '#f44336',
        title: 'Booking cancelled',
        subtitle: 'Room 102 - Refund processed',
        timestamp: new Date(Date.now() - 18000000), // 5 hours ago
        metadata: 'Cancellation'
      }
    ];
  }

  /**
   * Get calendar events (mock data)
   */
  getCalendarEvents(): any[] {
    const today = new Date();
    return [
      {
        id: 1,
        title: 'Check-in: John Doe',
        start: today,
        color: '#4caf50',
        type: 'check-in'
      },
      {
        id: 2,
        title: 'Check-out: Jane Smith',
        start: new Date(today.getTime() + 86400000), // Tomorrow
        color: '#f44336',
        type: 'check-out'
      },
      {
        id: 3,
        title: 'Booking: Mike Johnson',
        start: new Date(today.getTime() + 172800000), // 2 days
        color: '#2196f3',
        type: 'booking'
      }
    ];
  }

  // ========================================
  // UTILITY METHODS
  // ========================================

  /**
   * Get current user ID from JWT token
   */
  getCurrentUserId(): number {
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
   * Dashboard type changed
   */
  onDashboardChange(event: MatSelectChange): void {
    const dashboard = this.dashboardTypes.find(d => d.value === event.value);
    if (dashboard) {
      console.log('Switching to dashboard:', dashboard.label);
      this.snackBar.open(`Switching to ${dashboard.label}`, 'Close', { duration: 2000 });
      // TODO: Navigate to different dashboard or load different configuration
      // this.router.navigate([dashboard.route]);
    }
  }
}
