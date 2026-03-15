using System;

namespace MyWarehouse.Application.UserActivity.Attributes
{
    /// <summary>
    /// Attribute to automatically log user activities on controller actions
    /// </summary>
    /// <example>
    /// Usage:
    /// <code>
    /// [ActivityLog(ActivityType.Create, "Property", "Created new property")]
    /// public async Task&lt;IActionResult&gt; CreateProperty(CreatePropertyCommand command)
    /// {
    ///     var result = await _mediator.Send(command);
    ///     return Ok(result);
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class ActivityLogAttribute : Attribute
    {
        /// <summary>
        /// Type of activity being performed (Create, Update, Delete, etc.)
        /// </summary>
        public string ActivityType { get; set; }

        /// <summary>
        /// Type of entity being acted upon (Property, Room, Booking, etc.)
        /// Optional - if not provided, will be inferred from controller name
        /// </summary>
        public string? EntityType { get; set; }

        /// <summary>
        /// Description template for the activity
        /// Supports placeholders: {entityType}, {entityId}, {action}
        /// Example: "Created {entityType} #{entityId}"
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Module/area of the application
        /// Optional - if not provided, will be inferred from controller name
        /// </summary>
        public string? Module { get; set; }

        /// <summary>
        /// Property name in the result object that contains the entity ID
        /// Default: "Id" or "{EntityType}Id"
        /// </summary>
        public string? EntityIdProperty { get; set; }

        /// <summary>
        /// Whether to log even if the action fails (returns non-success status)
        /// Default: false (only log successful operations)
        /// </summary>
        public bool LogOnFailure { get; set; }

        /// <summary>
        /// Creates an activity log attribute
        /// </summary>
        /// <param name="activityType">Type of activity (Create, Update, Delete, View, etc.)</param>
        /// <param name="entityType">Optional: Type of entity (Property, Room, Booking, etc.)</param>
        /// <param name="description">Optional: Description template</param>
        public ActivityLogAttribute(string activityType, string? entityType = null, string? description = null)
        {
            ActivityType = activityType;
            EntityType = entityType;
            Description = description;
            LogOnFailure = false;
        }

        /// <summary>
        /// Creates an activity log attribute with just activity type
        /// Entity type will be inferred from controller name
        /// </summary>
        /// <param name="activityType">Type of activity</param>
        public ActivityLogAttribute(string activityType)
        {
            ActivityType = activityType;
            LogOnFailure = false;
        }
    }

    /// <summary>
    /// Convenience attributes for common CRUD operations
    /// </summary>
    public class LogCreateAttribute : ActivityLogAttribute
    {
        public LogCreateAttribute(string? entityType = null, string? description = null)
            : base("Create", entityType, description ?? "Created {entityType} #{entityId}")
        {
        }
    }

    public class LogUpdateAttribute : ActivityLogAttribute
    {
        public LogUpdateAttribute(string? entityType = null, string? description = null)
            : base("Update", entityType, description ?? "Updated {entityType} #{entityId}")
        {
        }
    }

    public class LogDeleteAttribute : ActivityLogAttribute
    {
        public LogDeleteAttribute(string? entityType = null, string? description = null)
            : base("Delete", entityType, description ?? "Deleted {entityType} #{entityId}")
        {
            LogOnFailure = true; // Log delete attempts even if they fail
        }
    }

    public class LogViewAttribute : ActivityLogAttribute
    {
        public LogViewAttribute(string? entityType = null, string? description = null)
            : base("View", entityType, description ?? "Viewed {entityType} #{entityId}")
        {
        }
    }

    public class LogListAttribute : ActivityLogAttribute
    {
        public LogListAttribute(string? entityType = null, string? description = null)
            : base("List", entityType, description ?? "Listed {entityType} records")
        {
        }
    }

    public class LogExportAttribute : ActivityLogAttribute
    {
        public LogExportAttribute(string? entityType = null, string? description = null)
            : base("Export", entityType, description ?? "Exported {entityType} data")
        {
        }
    }

    public class LogSearchAttribute : ActivityLogAttribute
    {
        public LogSearchAttribute(string? entityType = null, string? description = null)
            : base("Search", entityType, description ?? "Searched {entityType} records")
        {
        }
    }
}
