import { Component, OnInit, ViewEncapsulation, ChangeDetectorRef } from '@angular/core';
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
import { MatDividerModule } from '@angular/material/divider';
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
import { GridsterConfigService } from '../../services/gridster-config.service';

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
  [key: string]: any; // Allow additional gridster properties
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
    MatDividerModule,
    Gridster,
    GridsterItem,
    KpiCardWidgetComponent,
    ListWidgetComponent,
    ChartWidgetComponent,
    CalendarWidgetComponent,
    ActivityStreamWidgetComponent
  ],
  templateUrl: './dashboard1.component.html',
  styleUrls: ['./dashboard1.component.css'],
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

  constructor(
    private router: Router,
    private dashboardService: DashboardService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private cdr: ChangeDetectorRef
  ) {
    // Initialize gridster configuration
    this.options = GridsterConfigService.getDefaultConfig(false);
  }

  ngOnInit(): void {
    console.log('Dashboard1 component initialized - Phase 5: Gridster + Real Data');
    this.loadUserDashboards();
    this.loadWidgetLibrary();
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
          this.selectedDashboard = defaultDashboard.id!;
          this.loadDashboard(defaultDashboard.id!);
        } else {
          // No saved dashboards, load default widgets
          console.log('No saved dashboards found, loading default widgets');
          this.selectedDashboard = null;
          this.loadDefaultWidgets();
          this.refreshAllWidgetData();
        }

        this.isLoadingDashboards = false;
      },
      error: (error) => {
        console.error('Error loading user dashboards:', error);
        this.snackBar.open('Failed to load dashboards', 'Close', { duration: 3000 });
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
          this.snackBar.open('Failed to load dashboard', 'Close', { duration: 3000 });
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

    this.snackBar.open('Widget refreshed', '', { duration: 2000 });
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
    this.dashboardService.getKpiValue(item.widgetId, userId).pipe(
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
   */
  loadListWidgetData(item: DashboardGridsterItem, userId: number): void {
    this.dashboardService.getRecentActivity(userId, 10).pipe(
      catchError(error => {
        console.error('Error loading activity data:', error);
        return of([]);
      })
    ).subscribe(activities => {
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
      } else {
        item.data = this.getDefaultListWidget();
      }
      this.cdr.detectChanges();  // Trigger change detection
    });
  }

  /**
   * Load chart widget data (placeholder for now)
   */
  loadChartWidgetData(item: DashboardGridsterItem, userId: number): void {
    console.log(`📊 Loading chart data for widget: ${item.widgetId}, userId: ${userId}`);

    // Determine days back based on widget settings or default to 30
    const daysBack = item.settings?.daysBack || 30;
    const groupBy = item.settings?.groupBy || 'day';

    this.dashboardService.getBookingTrends(userId, daysBack, groupBy).pipe(
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

    this.dashboardService.getCalendarEvents(userId, startDate, endDate).pipe(
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
      const confirmExit = confirm('You have unsaved changes. Do you want to discard them?');
      if (!confirmExit) return;
    }

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

    this.snackBar.open(`${widget.name} added to dashboard`, 'Close', { duration: 2000 });
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
      this.snackBar.open(`${widgetName} removed`, 'Close', { duration: 2000 });
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
        this.snackBar.open(`Dashboard "${dashboardName}" saved successfully!`, 'Close', { duration: 3000 });
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
      this.snackBar.open('No dashboard selected to delete', 'Close', { duration: 3000 });
      return;
    }

    const confirmDelete = confirm(`Delete dashboard "${this.dashboardConfig.dashboardName}"? This action cannot be undone.`);
    if (!confirmDelete) return;

    this.loading = true;
    const userId = this.getCurrentUserId();

    this.dashboardService.deleteDashboard(this.selectedDashboard, userId).subscribe({
      next: (success) => {
        if (success) {
          this.snackBar.open('Dashboard deleted successfully', 'Close', { duration: 3000 });
          this.selectedDashboard = null;
          this.dashboardConfig = null;
          this.loadUserDashboards(); // Reload dashboard list
        } else {
          this.snackBar.open('Failed to delete dashboard', 'Close', { duration: 3000 });
        }
        this.loading = false;
      },
      error: (error) => {
        console.error('Error deleting dashboard:', error);
        this.snackBar.open('Failed to delete dashboard', 'Close', { duration: 3000 });
        this.loading = false;
      }
    });
  }

  /**
   * Set current dashboard as default
   */
  setAsDefaultDashboard(): void {
    if (!this.selectedDashboard) {
      this.snackBar.open('No dashboard selected', 'Close', { duration: 3000 });
      return;
    }

    this.loading = true;
    const userId = this.getCurrentUserId();

    this.dashboardService.setDefaultDashboard(this.selectedDashboard, userId).subscribe({
      next: (success) => {
        if (success) {
          this.snackBar.open('Default dashboard updated successfully', 'Close', { duration: 3000 });
          // Reload dashboard list to update isDefault flags
          this.loadUserDashboards();
        } else {
          this.snackBar.open('Failed to set default dashboard', 'Close', { duration: 3000 });
          this.loading = false;
        }
      },
      error: (error) => {
        console.error('Error setting default dashboard:', error);
        this.snackBar.open('Failed to set default dashboard', 'Close', { duration: 3000 });
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
    const confirmRevert = confirm('Revert all changes to the last saved state?');
    if (!confirmRevert) return;

    // Restore from backup
    this.dashboardItems = JSON.parse(JSON.stringify(this.savedDashboardState));
    this.hasUnsavedChanges = false;

    // Refresh data for all widgets
    this.refreshAllWidgetData();

    this.snackBar.open('Dashboard reverted to last saved state', 'Close', { duration: 3000 });
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
   * Dashboard selection changed
   */
  onDashboardChange(event: MatSelectChange): void {
    if (this.hasUnsavedChanges) {
      const confirmSwitch = confirm('You have unsaved changes. Do you want to discard them?');
      if (!confirmSwitch) {
        // Revert selection
        event.source.value = this.selectedDashboard;
        return;
      }
    }

    const dashboardId = event.value;
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
}
