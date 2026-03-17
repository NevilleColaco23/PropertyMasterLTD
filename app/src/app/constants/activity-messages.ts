/**
 * Centralized Activity Message Templates
 * 
 * This is the single source of truth for all client-provided activity messages.
 * MODIFY THIS FILE to change how activity messages appear in logs.
 * 
 * New developers should refer to this file to understand message formats.
 * 
 * Usage Example:
 *   this.activityMessage.setMessage(ActivityMessages.PROPERTY_SELECTED(propertyName));
 */

export class ActivityMessages {
  // ==========================================
  // PROPERTY MESSAGES
  // ==========================================
  
  /**
   * When user selects a single property
   * @param name - Property name
   */
  static readonly PROPERTY_SELECTED = (name: string) => 
    `Selected '${name}' property`;
  
  /**
   * When user selects multiple properties
   * @param count - Number of properties
   * @param names - Comma-separated property names
   */
  static readonly PROPERTIES_BULK_SELECTED = (count: number, names: string) => 
    `Selected ${count} properties: ${names}`;
  
  /**
   * When user creates a new property
   * @param name - Property name
   */
  static readonly PROPERTY_CREATED = (name: string) => 
    `Created new property '${name}'`;
  
  /**
   * When user updates a property
   * @param name - Property name
   */
  static readonly PROPERTY_UPDATED = (name: string) => 
    `Updated property '${name}'`;
  
  /**
   * When user deletes a property
   * @param name - Property name
   */
  static readonly PROPERTY_DELETED = (name: string) => 
    `Deleted property '${name}'`;

  // ==========================================
  // ROOM MESSAGES
  // ==========================================
  
  /**
   * When user creates a new room
   * @param name - Room name
   * @param floor - Floor number
   */
  static readonly ROOM_CREATED = (name: string, floor: number) => 
    `Added '${name}' room to ${floor}th floor`;
  
  /**
   * When user creates a room without floor info
   * @param name - Room name
   */
  static readonly ROOM_CREATED_SIMPLE = (name: string) => 
    `Added '${name}' room`;
  
  /**
   * When user updates a room
   * @param name - Room name
   * @param changes - Description of changes
   */
  static readonly ROOM_UPDATED = (name: string, changes: string) => 
    `Updated '${name}' room - ${changes}`;
  
  /**
   * When user updates a room (simple)
   * @param name - Room name
   */
  static readonly ROOM_UPDATED_SIMPLE = (name: string) => 
    `Updated room '${name}'`;
  
  /**
   * When user deletes a room
   * @param name - Room name
   */
  static readonly ROOM_DELETED = (name: string) => 
    `Deleted room '${name}'`;

  // ==========================================
  // BOOKING MESSAGES
  // ==========================================
  
  /**
   * When user creates a new booking
   * @param guestName - Guest name
   * @param dates - Date range (e.g., "Jan 15 - Jan 20")
   */
  static readonly BOOKING_CREATED = (guestName: string, dates: string) => 
    `Created booking for ${guestName} (${dates})`;
  
  /**
   * When user creates a booking (simple)
   * @param guestName - Guest name
   */
  static readonly BOOKING_CREATED_SIMPLE = (guestName: string) => 
    `Created booking for ${guestName}`;
  
  /**
   * When user updates a booking
   * @param bookingId - Booking ID
   */
  static readonly BOOKING_UPDATED = (bookingId: string | number) => 
    `Updated booking #${bookingId}`;
  
  /**
   * When user cancels a booking
   * @param bookingId - Booking ID
   * @param reason - Cancellation reason
   */
  static readonly BOOKING_CANCELLED = (bookingId: string | number, reason: string) => 
    `Cancelled booking #${bookingId}: ${reason}`;
  
  /**
   * When user cancels a booking (simple)
   * @param bookingId - Booking ID
   */
  static readonly BOOKING_CANCELLED_SIMPLE = (bookingId: string | number) => 
    `Cancelled booking #${bookingId}`;
  
  /**
   * When user deletes a booking
   * @param bookingId - Booking ID
   */
  static readonly BOOKING_DELETED = (bookingId: string | number) => 
    `Deleted booking #${bookingId}`;

  // ==========================================
  // DASHBOARD MESSAGES
  // ==========================================
  
  /**
   * When user opens a specific dashboard
   * @param dashboardName - Dashboard name
   */
  static readonly DASHBOARD_VIEWED = (dashboardName: string) => 
    `Opened ${dashboardName} dashboard`;
  
  /**
   * When user views dashboard list
   */
  static readonly DASHBOARDS_VIEWED = () => 
    `Viewed all dashboards`;

  // ==========================================
  // WIDGET MESSAGES
  // ==========================================
  
  /**
   * When user adds a widget to dashboard
   * @param widgetName - Widget name
   */
  static readonly WIDGET_ADDED = (widgetName: string) => 
    `Added '${widgetName}' widget to dashboard`;
  
  /**
   * When user removes a widget from dashboard
   * @param widgetName - Widget name
   */
  static readonly WIDGET_REMOVED = (widgetName: string) => 
    `Removed '${widgetName}' widget from dashboard`;
  
  /**
   * When user browses widget library
   */
  static readonly WIDGET_LIBRARY_VIEWED = () => 
    `Browsed widget library`;

  // ==========================================
  // EXPORT MESSAGES
  // ==========================================
  
  /**
   * When user exports data
   * @param type - Data type (e.g., "Properties", "Bookings")
   * @param count - Number of records
   * @param format - Export format (e.g., "Excel", "PDF")
   */
  static readonly DATA_EXPORTED = (type: string, count: number, format: string) => 
    `Exported ${count} ${type} records to ${format}`;
  
  /**
   * When user exports data (simple)
   * @param type - Data type
   * @param count - Number of records
   */
  static readonly DATA_EXPORTED_SIMPLE = (type: string, count: number) => 
    `Exported ${count} ${type} records`;

  // ==========================================
  // SEARCH MESSAGES
  // ==========================================
  
  /**
   * When user performs a search
   * @param term - Search term
   */
  static readonly SEARCH_PERFORMED = (term: string) => 
    `Searched for '${term}'`;
  
  /**
   * When user searches in specific entity
   * @param term - Search term
   * @param entityType - Entity type (e.g., "Properties", "Rooms")
   */
  static readonly SEARCH_IN_ENTITY = (term: string, entityType: string) => 
    `Searched for '${term}' in ${entityType}`;

  // ==========================================
  // USER MANAGEMENT MESSAGES
  // ==========================================
  
  /**
   * When user creates a new user account
   * @param username - Username
   */
  static readonly USER_CREATED = (username: string) => 
    `Created user '${username}'`;
  
  /**
   * When user updates a user account
   * @param username - Username
   */
  static readonly USER_UPDATED = (username: string) => 
    `Updated user '${username}'`;
  
  /**
   * When user deletes a user account
   * @param username - Username
   */
  static readonly USER_DELETED = (username: string) => 
    `Deleted user '${username}'`;

  // ==========================================
  // SETTINGS MESSAGES
  // ==========================================
  
  /**
   * When user updates settings
   * @param settingName - Setting name
   */
  static readonly SETTINGS_UPDATED = (settingName: string) => 
    `Updated ${settingName} settings`;
  
  /**
   * When user views settings
   * @param settingName - Setting name
   */
  static readonly SETTINGS_VIEWED = (settingName: string) => 
    `Viewed ${settingName} settings`;

  // ==========================================
  // REPORT MESSAGES
  // ==========================================
  
  /**
   * When user generates a report
   * @param reportName - Report name
   */
  static readonly REPORT_GENERATED = (reportName: string) => 
    `Generated ${reportName} report`;
  
  /**
   * When user views a report
   * @param reportName - Report name
   */
  static readonly REPORT_VIEWED = (reportName: string) => 
    `Viewed ${reportName} report`;
}
