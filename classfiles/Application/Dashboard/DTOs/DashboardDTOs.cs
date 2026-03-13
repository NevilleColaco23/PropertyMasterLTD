using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories.Common;
using MyWarehouse.Domain.Dashboard;

namespace MyWarehouse.Application.Dashboard.DTOs
{
    /// <summary>
    /// DTO for dashboard configuration
    /// </summary>
    public class DashboardConfigurationDTO
    {
        public string Id { get; set; } = string.Empty;
        public int UserId { get; set; }
        public string DashboardName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public DashboardLayoutDTO Layout { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class DashboardLayoutDTO
    {
        public int Columns { get; set; } = 12;
        public int RowHeight { get; set; } = 80;
        public List<WidgetConfigurationDTO> Widgets { get; set; } = new();
    }

    public class WidgetConfigurationDTO
    {
        public string WidgetId { get; set; } = string.Empty;
        public string WidgetType { get; set; } = string.Empty;
        public WidgetPositionDTO Position { get; set; } = new();
        public Dictionary<string, object> Settings { get; set; } = new();
    }

    public class WidgetPositionDTO
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    /// <summary>
    /// DTO for widget library item
    /// </summary>
    public class WidgetLibraryItemDTO
    {
        public string Id { get; set; } = string.Empty;
        public string WidgetId { get; set; } = string.Empty;
        public string WidgetType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public Dictionary<string, object> DefaultSettings { get; set; } = new();
        public WidgetSizeDTO DefaultSize { get; set; } = new();
        public WidgetSizeDTO MinSize { get; set; } = new();
        public WidgetSizeDTO MaxSize { get; set; } = new();
        public List<string> RequiredPermissions { get; set; } = new();
        public bool IsActive { get; set; }
    }

    public class WidgetSizeDTO
    {
        public int Width { get; set; }
        public int Height { get; set; }
    }

    /// <summary>
    /// DTO for dashboard template
    /// </summary>
    public class DashboardTemplateDTO
    {
        public string Id { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int? RoleId { get; set; }
        public DashboardLayoutDTO Layout { get; set; } = new();
        public bool IsPublic { get; set; }
        public string? PreviewImage { get; set; }
    }

    // ============================================
    // QUERIES
    // ============================================

    /// <summary>
    /// Get dashboard configuration by user ID
    /// </summary>
    public class GetDashboardByUserIdQuery : IRequest<DashboardConfigurationDTO?>
    {
        public int UserId { get; set; }
        public bool DefaultOnly { get; set; } = true;
    }

    /// <summary>
    /// Get all dashboards for a user
    /// </summary>
    public class GetUserDashboardsQuery : IRequest<List<DashboardConfigurationDTO>>
    {
        public int UserId { get; set; }
    }

    /// <summary>
    /// Get widget library
    /// </summary>
    public class GetWidgetLibraryQuery : IRequest<List<WidgetLibraryItemDTO>>
    {
        public string? Category { get; set; }
        public bool ActiveOnly { get; set; } = true;
    }

    /// <summary>
    /// Get dashboard templates
    /// </summary>
    public class GetDashboardTemplatesQuery : IRequest<List<DashboardTemplateDTO>>
    {
        public int? RoleId { get; set; }
        public bool PublicOnly { get; set; } = true;
    }

    // ============================================
    // COMMANDS
    // ============================================

    /// <summary>
    /// Create or update dashboard configuration
    /// </summary>
    public class SaveDashboardConfigurationCommand : IRequest<string>
    {
        public string? Id { get; set; } // Null for new, populated for update
        public int UserId { get; set; }
        public string DashboardName { get; set; } = string.Empty;
        public bool IsDefault { get; set; }
        public DashboardLayoutDTO Layout { get; set; } = new();
    }

    /// <summary>
    /// Delete dashboard configuration
    /// </summary>
    public class DeleteDashboardConfigurationCommand : IRequest<bool>
    {
        public string Id { get; set; } = string.Empty;
        public int UserId { get; set; } // For authorization check
    }

    /// <summary>
    /// Set dashboard as default
    /// </summary>
    public class SetDefaultDashboardCommand : IRequest<bool>
    {
        public string DashboardId { get; set; } = string.Empty;
        public int UserId { get; set; }
    }

    /// <summary>
    /// Reset dashboard to template
    /// </summary>
    public class ResetDashboardToTemplateCommand : IRequest<string>
    {
        public int UserId { get; set; }
        public string TemplateId { get; set; } = string.Empty;
    }
}
