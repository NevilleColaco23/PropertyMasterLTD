namespace MyWarehouse.Domain.UserActivity
{
    /// <summary>
    /// Enum representing different types of user activities tracked in the system
    /// </summary>
    public enum ActivityType
    {
        // Navigation & Access
        PageView = 1,
        Login = 2,
        Logout = 3,
        
        // CRUD Operations
        Create = 10,
        Update = 11,
        Delete = 12,
        View = 13,
        
        // Data Operations
        Export = 20,
        Import = 21,
        Download = 22,
        Upload = 23,
        Print = 24,
        
        // Search & Filter
        Search = 30,
        Filter = 31,
        Sort = 32,
        
        // Communication
        Email = 40,
        Notification = 41,
        
        // Status Changes
        StatusChange = 50,
        Approve = 51,
        Reject = 52,
        
        // Bulk Operations
        BulkCreate = 60,
        BulkUpdate = 61,
        BulkDelete = 62,
        
        // System Operations
        ConfigChange = 70,
        PermissionChange = 71,
        
        // Dashboard Operations
        DashboardView = 80,
        DashboardCreate = 81,
        DashboardUpdate = 82,
        DashboardDelete = 83,
        WidgetAdd = 84,
        WidgetRemove = 85,
        WidgetConfigure = 86,
        
        // Other
        Other = 99
    }
}
