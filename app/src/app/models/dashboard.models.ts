// Dashboard Models - matching backend DTOs

export interface WidgetPosition {
  x: number;
  y: number;
  width: number;
  height: number;
}

export interface WidgetConfiguration {
  widgetId: string;
  widgetType: string;
  position: WidgetPosition;
  settings: { [key: string]: any };
}

export interface DashboardLayout {
  columns: number;
  rowHeight: number;
  widgets: WidgetConfiguration[];
}

export interface DashboardConfiguration {
  id?: string;
  userId: number;
  dashboardName: string;
  isDefault: boolean;
  layout: DashboardLayout;
  createdAt?: Date;
  updatedAt?: Date;
}

export interface DashboardListItem {
  id: string;
  dashboardName: string;
  isDefault: boolean;
  widgetCount: number;
  createdAt: Date;
  updatedAt: Date;
}

export interface WidgetSize {
  width: number;
  height: number;
}

export interface WidgetLibraryItem {
  id?: string;
  widgetId: string;
  widgetType: string;
  name: string;
  description: string;
  icon: string;
  category: string;
  defaultSettings: { [key: string]: any };
  defaultSize: WidgetSize;
  minSize: WidgetSize;
  maxSize: WidgetSize;
  requiredPermissions: string[];
  isActive: boolean;
}

export interface DashboardTemplate {
  id?: string;
  templateName: string;
  description: string;
  roleId: number;
  layout: DashboardLayout;
  isPublic: boolean;
  previewImage?: string;
  createdAt?: Date;
}

// API Request/Response models
export interface SaveDashboardRequest {
  id?: string;
  userId: number;
  dashboardName: string;
  isDefault: boolean;
  layout: DashboardLayout;
}

export interface GetWidgetLibraryParams {
  category?: string;
  activeOnly?: boolean;
}

export interface GetDashboardTemplatesParams {
  roleId?: number;
  publicOnly?: boolean;
}

// ==========================================
// PHASE 4: REAL DATA MODELS
// ==========================================

/**
 * KPI Value Response from backend
 */
export interface KpiValueResponse {
  widgetId: string;
  value: number | string;
  showTrend: boolean;
  trendValue?: number;
  trendDirection?: 'up' | 'down';
  calculatedAt: Date;
}

/**
 * Activity Item Response
 */
export interface ActivityItemResponse {
  id: string;
  icon: string;
  iconColor: string;
  title: string;
  subtitle: string;
  timestamp: Date;
  metadata: string;
  activityType: string;
}

/**
 * Calendar Event Response
 */
export interface CalendarEventResponse {
  id: string;
  title: string;
  start: Date;
  end?: Date;
  color: string;
  type: string;
  description: string;
}

/**
 * Room Data for Room Planner
 */
export interface RoomData {
  id: string;
  roomId: number;
  roomNumber: string;
  roomName?: string;
  roomType: string;
  propertyId: number;
  propertyName: string;
  floor?: number;
  capacity?: number;
  status: string;
  amenities: string[];
  pricePerNight?: number;
  isActive: boolean;
}

/**
 * Booking with guest details response from backend
 */
export interface BookingWithGuestData {
  id: string;
  bookingId: string;
  roomNumber: string;
  propertyId: number;
  propertyName: string;
  checkInDate: Date | string;
  checkOutDate: Date | string;
  status: string;
  guestId: string;
  guestFirstName: string;
  guestLastName: string;
  guestEmail: string;
  guestPhoneNumber: string;
  guestNationality: string;
}

