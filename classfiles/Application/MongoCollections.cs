namespace MyWarehouse.Application;

public static class MongoCollections
{
    public const string PropertyCollection = "Property";
    public const string UsersCollection = "Users";
    public const string UsersPropertyMapCollection = "UsersPropertyMap";
    public const string KeyCounterCollection = "KeyCounter";
    public const string RoomCollection = "Room";
    public const string RoomsCollection = "rooms"; // Plural version for room planner queries
    public const string AccessLogCollection = "AccessLog";
    public const string MenuCollection = "Menus";
    public const string MenuPermissionsCollection = "MenuPermissions";
    public const string BookingsCollection = "Bookings";
    public const string SystemMessagesCollection = "systemMessages";
    public const string EmailOutboxCollection = "EmailOutbox";
    public const string AuditLogsCollection = "AuditLogs";
    public const string PropertyAuditCollection = "PropertyAudit";
    public const string RoomAuditCollection = "RoomAudit";

    // Dashboard collections
    public const string DashboardConfigurationsCollection = "DashboardConfigurations";
    public const string WidgetLibraryCollection = "WidgetLibrary";
    public const string DashboardTemplatesCollection = "DashboardTemplates";

    // User Activity Tracking
    public const string UserActivityLogsCollection = "UserActivityLogs";
}