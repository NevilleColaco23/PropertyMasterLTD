using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MyWarehouse.Domain.Dashboard
{
    /// <summary>
    /// Dashboard configuration for a user
    /// </summary>
    public class DashboardConfiguration
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("UserId")]
        public int UserId { get; set; }

        [BsonElement("DashboardName")]
        public string DashboardName { get; set; } = string.Empty;

        [BsonElement("IsDefault")]
        public bool IsDefault { get; set; }

        [BsonElement("Layout")]
        public DashboardLayout Layout { get; set; } = new();

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("UpdatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Dashboard layout configuration
    /// </summary>
    public class DashboardLayout
    {
        [BsonElement("columns")]
        public int Columns { get; set; } = 12;

        [BsonElement("rowHeight")]
        public int RowHeight { get; set; } = 80;

        [BsonElement("widgets")]
        public List<WidgetConfiguration> Widgets { get; set; } = new();
    }

    /// <summary>
    /// Widget configuration within a dashboard
    /// </summary>
    public class WidgetConfiguration
    {
        [BsonElement("widgetId")]
        public string WidgetId { get; set; } = string.Empty;

        [BsonElement("widgetType")]
        public string WidgetType { get; set; } = string.Empty;

        [BsonElement("position")]
        public WidgetPosition Position { get; set; } = new();

        [BsonElement("settings")]
        public Dictionary<string, object> Settings { get; set; } = new();
    }

    /// <summary>
    /// Widget position in the grid
    /// </summary>
    public class WidgetPosition
    {
        [BsonElement("x")]
        public int X { get; set; }

        [BsonElement("y")]
        public int Y { get; set; }

        [BsonElement("width")]
        public int Width { get; set; }

        [BsonElement("height")]
        public int Height { get; set; }
    }

    /// <summary>
    /// Widget definition in the library
    /// </summary>
    public class WidgetLibraryItem
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("WidgetId")]
        public string WidgetId { get; set; } = string.Empty;

        [BsonElement("WidgetType")]
        public string WidgetType { get; set; } = string.Empty;

        [BsonElement("Name")]
        public string Name { get; set; } = string.Empty;

        [BsonElement("Description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("Icon")]
        public string Icon { get; set; } = string.Empty;

        [BsonElement("Category")]
        public string Category { get; set; } = string.Empty;

        [BsonElement("DefaultSettings")]
        public Dictionary<string, object> DefaultSettings { get; set; } = new();

        [BsonElement("DefaultSize")]
        public WidgetSize DefaultSize { get; set; } = new();

        [BsonElement("MinSize")]
        public WidgetSize MinSize { get; set; } = new();

        [BsonElement("MaxSize")]
        public WidgetSize MaxSize { get; set; } = new();

        [BsonElement("RequiredPermissions")]
        public List<string> RequiredPermissions { get; set; } = new();

        [BsonElement("IsActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Widget size constraints
    /// </summary>
    public class WidgetSize
    {
        [BsonElement("width")]
        public int Width { get; set; }

        [BsonElement("height")]
        public int Height { get; set; }
    }

    /// <summary>
    /// Dashboard template
    /// </summary>
    public class DashboardTemplate
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("TemplateName")]
        public string TemplateName { get; set; } = string.Empty;

        [BsonElement("Description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("RoleId")]
        public int? RoleId { get; set; }

        [BsonElement("Layout")]
        public DashboardLayout Layout { get; set; } = new();

        [BsonElement("IsPublic")]
        public bool IsPublic { get; set; }

        [BsonElement("PreviewImage")]
        public string? PreviewImage { get; set; }

        [BsonElement("CreatedAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
