namespace MyWarehouse.Application.UserActivity.Constants
{
    /// <summary>
    /// Centralized activity log message templates and patterns.
    /// MODIFY THIS FILE to change how activity messages appear in logs.
    /// 
    /// This is the single source of truth for all server-generated activity messages.
    /// New developers should refer to this file to understand message formats.
    /// </summary>
    public static class ActivityMessageTemplates
    {
        // ==========================================
        // VERB MAPPINGS (for dynamic messages)
        // ==========================================
        /// <summary>
        /// Action verbs used in activity messages
        /// </summary>
        public static class Verbs
        {
            public const string Created = "created";
            public const string Updated = "updated";
            public const string Deleted = "deleted";
            public const string Viewed = "viewed";
            public const string Searched = "searched";
            public const string Exported = "exported";
            public const string Imported = "imported";
            public const string Downloaded = "downloaded";
            public const string Uploaded = "uploaded";
            public const string LoggedIn = "logged into";
            public const string LoggedOut = "logged out of";
            public const string Approved = "approved";
            public const string Rejected = "rejected";
            public const string ChangedStatus = "changed status of";
            public const string BulkCreated = "bulk created";
            public const string BulkUpdated = "bulk updated";
            public const string BulkDeleted = "bulk deleted";
        }

        // ==========================================
        // GENERIC MESSAGE PATTERNS
        // ==========================================
        /// <summary>
        /// Pattern: "{user} {verb} {entity}"
        /// Example: "john.doe viewed Properties"
        /// </summary>
        public const string GenericAction = "{0} {1} {2}";
        
        /// <summary>
        /// Pattern: "{user} {verb} {entity} in {property}"
        /// Example: "john.doe created Room in Sunset Villa"
        /// </summary>
        public const string GenericActionWithProperty = "{0} {1} {2} in {3}";
        
        /// <summary>
        /// Pattern: "{user} {verb} {entity} #{id}"
        /// Example: "john.doe deleted Room #42"
        /// </summary>
        public const string GenericActionWithId = "{0} {1} {2} #{3}";
        
        /// <summary>
        /// Pattern: "{user} {verb} {entity} #{id} in {property}"
        /// Example: "john.doe updated Booking #123 in Grand Hotel"
        /// </summary>
        public const string GenericActionWithIdAndProperty = "{0} {1} {2} #{3} in {4}";

        // ==========================================
        // SPECIFIC MESSAGE TEMPLATES
        // Use these for important actions where exact wording matters
        // ==========================================
        
        // Property Messages
        public const string PropertySelected = "{0} selected '{1}' property";
        public const string PropertyCreated = "{0} created property '{1}'";
        public const string PropertyUpdated = "{0} updated property '{1}'";
        public const string PropertyDeleted = "{0} deleted property '{1}'";
        public const string PropertiesViewed = "{0} viewed property list";
        
        // Room Messages
        public const string RoomCreated = "{0} added '{1}' room";
        public const string RoomCreatedWithFloor = "{0} added '{1}' room to {2}th floor";
        public const string RoomUpdated = "{0} updated room '{1}'";
        public const string RoomDeleted = "{0} deleted room '{1}'";
        public const string RoomsViewed = "{0} viewed rooms";
        
        // Booking Messages
        public const string BookingCreated = "{0} created booking for {1}";
        public const string BookingUpdated = "{0} updated booking #{1}";
        public const string BookingCancelled = "{0} cancelled booking #{1}";
        public const string BookingDeleted = "{0} deleted booking #{1}";
        public const string BookingsViewed = "{0} viewed bookings";
        
        // Dashboard Messages
        public const string DashboardViewed = "{0} opened {1} dashboard";
        public const string DashboardsViewed = "{0} viewed all dashboards";
        
        // Export Messages
        public const string DataExported = "{0} exported {1} {2} records";
        public const string DataExportedToFormat = "{0} exported {1} records to {2}";
        
        // Search Messages
        public const string SearchPerformed = "{0} searched for '{1}'";
        public const string SearchPerformedInEntity = "{0} searched for '{1}' in {2}";
        
        // Authentication Messages
        public const string UserLoggedIn = "{0} logged in successfully";
        public const string UserLoggedOut = "{0} logged out";
        public const string UserLoginFailed = "Failed login attempt for {0}";
        public const string UserRegistered = "{0} registered new account";
        public const string EmailConfirmed = "{0} confirmed email address";
        
        // User Management Messages
        public const string UserCreated = "{0} created user '{1}'";
        public const string UserUpdated = "{0} updated user '{1}'";
        public const string UserDeleted = "{0} deleted user '{1}'";
        public const string UsersViewed = "{0} viewed user list";
        
        // Report Messages
        public const string ReportGenerated = "{0} generated {1} report";
        public const string ReportViewed = "{0} viewed {1} report";
        
        // Settings Messages
        public const string SettingsUpdated = "{0} updated {1} settings";
        public const string SettingsViewed = "{0} viewed {1} settings";
        
        // Widget Messages
        public const string WidgetAdded = "{0} added {1} widget to dashboard";
        public const string WidgetRemoved = "{0} removed {1} widget from dashboard";
        public const string WidgetLibraryViewed = "{0} browsed widget library";
        
        // ==========================================
        // PROPERTY CONTEXT SUFFIXES
        // ==========================================
        public const string InProperty = "in {0}";
        public const string WhileWorkingOn = "while working on '{0}' property";
    }
}
