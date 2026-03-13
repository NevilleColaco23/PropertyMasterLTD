using MongoDB.Driver;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Dashboard.DTOs;
using MyWarehouse.Domain.Dashboard;

namespace MyWarehouse.Application.Dashboard.Queries
{
    /// <summary>
    /// Handler for GetDashboardByUserIdQuery
    /// </summary>
    public class GetDashboardByUserIdQueryHandler : IRequestHandler<GetDashboardByUserIdQuery, DashboardConfigurationDTO?>
    {
        private readonly IMongoDatabase _database;

        public GetDashboardByUserIdQueryHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<DashboardConfigurationDTO?> Handle(GetDashboardByUserIdQuery request, CancellationToken cancellationToken)
        {
            var collection = _database.GetCollection<DashboardConfiguration>(MongoCollections.DashboardConfigurationsCollection);

            var filter = Builders<DashboardConfiguration>.Filter.Eq(d => d.UserId, request.UserId);
            
            if (request.DefaultOnly)
            {
                filter &= Builders<DashboardConfiguration>.Filter.Eq(d => d.IsDefault, true);
            }

            var dashboard = await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);

            if (dashboard == null)
                return null;

            return MapToDTO(dashboard);
        }

        private static DashboardConfigurationDTO MapToDTO(DashboardConfiguration dashboard)
        {
            return new DashboardConfigurationDTO
            {
                Id = dashboard.Id,
                UserId = dashboard.UserId,
                DashboardName = dashboard.DashboardName,
                IsDefault = dashboard.IsDefault,
                Layout = new DashboardLayoutDTO
                {
                    Columns = dashboard.Layout.Columns,
                    RowHeight = dashboard.Layout.RowHeight,
                    Widgets = dashboard.Layout.Widgets.Select(w => new WidgetConfigurationDTO
                    {
                        WidgetId = w.WidgetId,
                        WidgetType = w.WidgetType,
                        Position = new WidgetPositionDTO
                        {
                            X = w.Position.X,
                            Y = w.Position.Y,
                            Width = w.Position.Width,
                            Height = w.Position.Height
                        },
                        Settings = w.Settings
                    }).ToList()
                },
                CreatedAt = dashboard.CreatedAt,
                UpdatedAt = dashboard.UpdatedAt
            };
        }
    }

    /// <summary>
    /// Handler for GetUserDashboardsQuery
    /// </summary>
    public class GetUserDashboardsQueryHandler : IRequestHandler<GetUserDashboardsQuery, List<DashboardConfigurationDTO>>
    {
        private readonly IMongoDatabase _database;

        public GetUserDashboardsQueryHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<List<DashboardConfigurationDTO>> Handle(GetUserDashboardsQuery request, CancellationToken cancellationToken)
        {
            var collection = _database.GetCollection<DashboardConfiguration>(MongoCollections.DashboardConfigurationsCollection);

            var filter = Builders<DashboardConfiguration>.Filter.Eq(d => d.UserId, request.UserId);
            var dashboards = await collection.Find(filter).ToListAsync(cancellationToken);

            return dashboards.Select(MapToDTO).ToList();
        }

        private static DashboardConfigurationDTO MapToDTO(DashboardConfiguration dashboard)
        {
            return new DashboardConfigurationDTO
            {
                Id = dashboard.Id,
                UserId = dashboard.UserId,
                DashboardName = dashboard.DashboardName,
                IsDefault = dashboard.IsDefault,
                Layout = new DashboardLayoutDTO
                {
                    Columns = dashboard.Layout.Columns,
                    RowHeight = dashboard.Layout.RowHeight,
                    Widgets = dashboard.Layout.Widgets.Select(w => new WidgetConfigurationDTO
                    {
                        WidgetId = w.WidgetId,
                        WidgetType = w.WidgetType,
                        Position = new WidgetPositionDTO
                        {
                            X = w.Position.X,
                            Y = w.Position.Y,
                            Width = w.Position.Width,
                            Height = w.Position.Height
                        },
                        Settings = w.Settings
                    }).ToList()
                },
                CreatedAt = dashboard.CreatedAt,
                UpdatedAt = dashboard.UpdatedAt
            };
        }
    }

    /// <summary>
    /// Handler for GetWidgetLibraryQuery
    /// </summary>
    public class GetWidgetLibraryQueryHandler : IRequestHandler<GetWidgetLibraryQuery, List<WidgetLibraryItemDTO>>
    {
        private readonly IMongoDatabase _database;

        public GetWidgetLibraryQueryHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<List<WidgetLibraryItemDTO>> Handle(GetWidgetLibraryQuery request, CancellationToken cancellationToken)
        {
            var collection = _database.GetCollection<WidgetLibraryItem>(MongoCollections.WidgetLibraryCollection);

            var filterBuilder = Builders<WidgetLibraryItem>.Filter;
            var filter = filterBuilder.Empty;

            if (request.ActiveOnly)
            {
                filter &= filterBuilder.Eq(w => w.IsActive, true);
            }

            if (!string.IsNullOrEmpty(request.Category))
            {
                filter &= filterBuilder.Eq(w => w.Category, request.Category);
            }

            var widgets = await collection.Find(filter).ToListAsync(cancellationToken);

            return widgets.Select(w => new WidgetLibraryItemDTO
            {
                Id = w.Id,
                WidgetId = w.WidgetId,
                WidgetType = w.WidgetType,
                Name = w.Name,
                Description = w.Description,
                Icon = w.Icon,
                Category = w.Category,
                DefaultSettings = w.DefaultSettings,
                DefaultSize = new WidgetSizeDTO
                {
                    Width = w.DefaultSize.Width,
                    Height = w.DefaultSize.Height
                },
                MinSize = new WidgetSizeDTO
                {
                    Width = w.MinSize.Width,
                    Height = w.MinSize.Height
                },
                MaxSize = new WidgetSizeDTO
                {
                    Width = w.MaxSize.Width,
                    Height = w.MaxSize.Height
                },
                RequiredPermissions = w.RequiredPermissions,
                IsActive = w.IsActive
            }).ToList();
        }
    }

    /// <summary>
    /// Handler for GetDashboardTemplatesQuery
    /// </summary>
    public class GetDashboardTemplatesQueryHandler : IRequestHandler<GetDashboardTemplatesQuery, List<DashboardTemplateDTO>>
    {
        private readonly IMongoDatabase _database;

        public GetDashboardTemplatesQueryHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<List<DashboardTemplateDTO>> Handle(GetDashboardTemplatesQuery request, CancellationToken cancellationToken)
        {
            var collection = _database.GetCollection<DashboardTemplate>(MongoCollections.DashboardTemplatesCollection);

            var filterBuilder = Builders<DashboardTemplate>.Filter;
            var filter = filterBuilder.Empty;

            if (request.PublicOnly)
            {
                filter &= filterBuilder.Eq(t => t.IsPublic, true);
            }

            if (request.RoleId.HasValue)
            {
                filter &= filterBuilder.Eq(t => t.RoleId, request.RoleId.Value);
            }

            var templates = await collection.Find(filter).ToListAsync(cancellationToken);

            return templates.Select(t => new DashboardTemplateDTO
            {
                Id = t.Id,
                TemplateName = t.TemplateName,
                Description = t.Description,
                RoleId = t.RoleId,
                Layout = new DashboardLayoutDTO
                {
                    Columns = t.Layout.Columns,
                    RowHeight = t.Layout.RowHeight,
                    Widgets = t.Layout.Widgets.Select(w => new WidgetConfigurationDTO
                    {
                        WidgetId = w.WidgetId,
                        WidgetType = w.WidgetType,
                        Position = new WidgetPositionDTO
                        {
                            X = w.Position.X,
                            Y = w.Position.Y,
                            Width = w.Position.Width,
                            Height = w.Position.Height
                        },
                        Settings = w.Settings
                    }).ToList()
                },
                IsPublic = t.IsPublic,
                PreviewImage = t.PreviewImage
            }).ToList();
        }
    }
}
