/**
 * User Activity Models for Activity Stream Widget
 * Maps to backend DTOs from ActivityController API
 */

export interface UserActivityDTO {
  activityId: number;
  userId: number;
  username: string;
  activityType: string;
  entityType?: string;
  entityId?: number;
  action: string;
  description: string;
  displayMessage?: string; // 🆕 Human-readable message for reports
  metadata?: { [key: string]: any };
  timestamp: Date;
  ipAddress?: string;
  isSuccess: boolean;
  errorMessage?: string;
  durationMs?: number;
  module?: string;
  timeAgo?: string; // Computed on backend
}

export interface ActivitySummaryDTO {
  totalToday: number;
  totalThisWeek: number;
  totalThisMonth: number;
  recentActivities: UserActivityDTO[];
  activityTypeCount: { [key: string]: number };
}

export interface PagedActivitiesDTO {
  activities: UserActivityDTO[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface ActivityStatisticsDTO {
  totalActivities: number;
  uniqueUsers: number;
  activityTypeBreakdown: { [key: string]: number };
  topUsers: TopUserActivityDTO[];
  fromDate?: Date;
  toDate?: Date;
  period: string;
}

export interface TopUserActivityDTO {
  username: string;
  activityCount: number;
  mostCommonActivity: string;
}

/**
 * Activity Type Icons Mapping
 */
export const ACTIVITY_TYPE_ICONS: { [key: string]: string } = {
  'PageView': 'visibility',
  'Login': 'login',
  'Logout': 'logout',
  'Create': 'add_circle',
  'Update': 'edit',
  'Delete': 'delete',
  'View': 'remove_red_eye',
  'List': 'list',
  'Search': 'search',
  'Filter': 'filter_list',
  'Sort': 'sort',
  'Export': 'download',
  'Import': 'upload',
  'Print': 'print',
  'Download': 'get_app',
  'Upload': 'cloud_upload',
  'SendEmail': 'email',
  'SendNotification': 'notifications',
  'StatusChange': 'change_circle',
  'Assign': 'person_add',
  'Comment': 'comment',
  'Share': 'share',
  'BulkUpdate': 'playlist_add_check',
  'BulkDelete': 'delete_sweep',
  'Error': 'error',
  'Warning': 'warning',
  'DashboardView': 'dashboard',
  'DashboardCreate': 'add_box',
  'DashboardUpdate': 'dashboard_customize',
  'DashboardDelete': 'delete_outline',
  'WidgetAdd': 'widgets',
  'WidgetRemove': 'remove_circle_outline',
  'WidgetConfigure': 'settings',
  'default': 'circle'
};

/**
 * Activity Type Colors for visual indicators
 */
export const ACTIVITY_TYPE_COLORS: { [key: string]: string } = {
  'Login': '#4caf50',
  'Logout': '#9e9e9e',
  'Create': '#2196f3',
  'Update': '#ff9800',
  'Delete': '#f44336',
  'Export': '#673ab7',
  'Import': '#3f51b5',
  'Error': '#d32f2f',
  'Warning': '#ffa726',
  'DashboardView': '#1976d2',
  'DashboardCreate': '#0288d1',
  'WidgetAdd': '#0097a7',
  'default': '#757575'
};

/**
 * Get icon for activity type
 */
export function getActivityIcon(activityType: string): string {
  return ACTIVITY_TYPE_ICONS[activityType] || ACTIVITY_TYPE_ICONS['default'];
}

/**
 * Get color for activity type
 */
export function getActivityColor(activityType: string): string {
  return ACTIVITY_TYPE_COLORS[activityType] || ACTIVITY_TYPE_COLORS['default'];
}

/**
 * Format timestamp to "time ago" string
 */
export function formatTimeAgo(timestamp: Date): string {
  const now = new Date();
  const then = new Date(timestamp);
  const diffMs = now.getTime() - then.getTime();
  const diffMinutes = Math.floor(diffMs / 60000);
  const diffHours = Math.floor(diffMinutes / 60);
  const diffDays = Math.floor(diffHours / 24);
  
  if (diffMinutes < 1) return 'just now';
  if (diffMinutes < 60) return `${diffMinutes} minute${diffMinutes !== 1 ? 's' : ''} ago`;
  if (diffHours < 24) return `${diffHours} hour${diffHours !== 1 ? 's' : ''} ago`;
  if (diffDays < 30) return `${diffDays} day${diffDays !== 1 ? 's' : ''} ago`;
  
  const diffMonths = Math.floor(diffDays / 30);
  if (diffMonths < 12) return `${diffMonths} month${diffMonths !== 1 ? 's' : ''} ago`;
  
  const diffYears = Math.floor(diffDays / 365);
  return `${diffYears} year${diffYears !== 1 ? 's' : ''} ago`;
}
