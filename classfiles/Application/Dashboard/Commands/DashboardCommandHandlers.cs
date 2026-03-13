using MongoDB.Driver;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Dashboard.DTOs;
using MyWarehouse.Domain.Dashboard;

namespace MyWarehouse.Application.Dashboard.Commands
{
    /// <summary>
    /// Handler for SaveDashboardConfigurationCommand
    /// </summary>
    public class SaveDashboardConfigurationCommandHandler : IRequestHandler<SaveDashboardConfigurationCommand, string>
    {
        private readonly IMongoDatabase _database;

        public SaveDashboardConfigurationCommandHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<string> Handle(SaveDashboardConfigurationCommand request, CancellationToken cancellationToken)
        {
            var collection = _database.GetCollection<DashboardConfiguration>(MongoCollections.DashboardConfigurationsCollection);

            DashboardConfiguration dashboard;

            if (string.IsNullOrEmpty(request.Id))
            {
                // Create new dashboard
                dashboard = new DashboardConfiguration
                {
                    UserId = request.UserId,
                    DashboardName = request.DashboardName,
                    IsDefault = request.IsDefault,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }
            else
            {
                // Update existing dashboard
                var filter = Builders<DashboardConfiguration>.Filter.And(
                    Builders<DashboardConfiguration>.Filter.Eq(d => d.Id, request.Id),
                    Builders<DashboardConfiguration>.Filter.Eq(d => d.UserId, request.UserId)
                );

                dashboard = await collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
                
                if (dashboard == null)
                {
                    throw new UnauthorizedAccessException("Dashboard not found or access denied");
                }

                dashboard.DashboardName = request.DashboardName;
                dashboard.IsDefault = request.IsDefault;
                dashboard.UpdatedAt = DateTime.UtcNow;
            }

            // Map layout
            dashboard.Layout = new DashboardLayout
            {
                Columns = request.Layout.Columns,
                RowHeight = request.Layout.RowHeight,
                Widgets = request.Layout.Widgets.Select(w => new WidgetConfiguration
                {
                    WidgetId = w.WidgetId,
                    WidgetType = w.WidgetType,
                    Position = new WidgetPosition
                    {
                        X = w.Position.X,
                        Y = w.Position.Y,
                        Width = w.Position.Width,
                        Height = w.Position.Height
                    },
                    Settings = w.Settings
                }).ToList()
            };

            // If this dashboard is being set as default, unset any other default dashboards
            if (request.IsDefault)
            {
                var updateFilter = Builders<DashboardConfiguration>.Filter.And(
                    Builders<DashboardConfiguration>.Filter.Eq(d => d.UserId, request.UserId),
                    Builders<DashboardConfiguration>.Filter.Ne(d => d.Id, dashboard.Id)
                );

                var update = Builders<DashboardConfiguration>.Update.Set(d => d.IsDefault, false);
                await collection.UpdateManyAsync(updateFilter, update, cancellationToken: cancellationToken);
            }

            // Save or update
            if (string.IsNullOrEmpty(request.Id))
            {
                await collection.InsertOneAsync(dashboard, cancellationToken: cancellationToken);
            }
            else
            {
                await collection.ReplaceOneAsync(
                    d => d.Id == dashboard.Id,
                    dashboard,
                    cancellationToken: cancellationToken
                );
            }

            return dashboard.Id;
        }
    }

    /// <summary>
    /// Handler for DeleteDashboardConfigurationCommand
    /// </summary>
    public class DeleteDashboardConfigurationCommandHandler : IRequestHandler<DeleteDashboardConfigurationCommand, bool>
    {
        private readonly IMongoDatabase _database;

        public DeleteDashboardConfigurationCommandHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<bool> Handle(DeleteDashboardConfigurationCommand request, CancellationToken cancellationToken)
        {
            var collection = _database.GetCollection<DashboardConfiguration>(MongoCollections.DashboardConfigurationsCollection);

            var filter = Builders<DashboardConfiguration>.Filter.And(
                Builders<DashboardConfiguration>.Filter.Eq(d => d.Id, request.Id),
                Builders<DashboardConfiguration>.Filter.Eq(d => d.UserId, request.UserId)
            );

            var result = await collection.DeleteOneAsync(filter, cancellationToken);

            return result.DeletedCount > 0;
        }
    }

    /// <summary>
    /// Handler for SetDefaultDashboardCommand
    /// </summary>
    public class SetDefaultDashboardCommandHandler : IRequestHandler<SetDefaultDashboardCommand, bool>
    {
        private readonly IMongoDatabase _database;

        public SetDefaultDashboardCommandHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<bool> Handle(SetDefaultDashboardCommand request, CancellationToken cancellationToken)
        {
            var collection = _database.GetCollection<DashboardConfiguration>(MongoCollections.DashboardConfigurationsCollection);

            // First, unset all default dashboards for this user
            var unsetFilter = Builders<DashboardConfiguration>.Filter.Eq(d => d.UserId, request.UserId);
            var unsetUpdate = Builders<DashboardConfiguration>.Update.Set(d => d.IsDefault, false);
            await collection.UpdateManyAsync(unsetFilter, unsetUpdate, cancellationToken: cancellationToken);

            // Then set the specified dashboard as default
            var setFilter = Builders<DashboardConfiguration>.Filter.And(
                Builders<DashboardConfiguration>.Filter.Eq(d => d.Id, request.DashboardId),
                Builders<DashboardConfiguration>.Filter.Eq(d => d.UserId, request.UserId)
            );

            var setUpdate = Builders<DashboardConfiguration>.Update.Set(d => d.IsDefault, true);
            var result = await collection.UpdateOneAsync(setFilter, setUpdate, cancellationToken: cancellationToken);

            return result.ModifiedCount > 0;
        }
    }

    /// <summary>
    /// Handler for ResetDashboardToTemplateCommand
    /// </summary>
    public class ResetDashboardToTemplateCommandHandler : IRequestHandler<ResetDashboardToTemplateCommand, string>
    {
        private readonly IMongoDatabase _database;

        public ResetDashboardToTemplateCommandHandler(IMongoDatabase database)
        {
            _database = database;
        }

        public async Task<string> Handle(ResetDashboardToTemplateCommand request, CancellationToken cancellationToken)
        {
            var templatesCollection = _database.GetCollection<DashboardTemplate>(MongoCollections.DashboardTemplatesCollection);
            var dashboardsCollection = _database.GetCollection<DashboardConfiguration>(MongoCollections.DashboardConfigurationsCollection);

            // Get template
            var template = await templatesCollection.Find(t => t.Id == request.TemplateId).FirstOrDefaultAsync(cancellationToken);
            
            if (template == null)
            {
                throw new ArgumentException("Template not found");
            }

            // Find user's default dashboard or create new one
            var filter = Builders<DashboardConfiguration>.Filter.And(
                Builders<DashboardConfiguration>.Filter.Eq(d => d.UserId, request.UserId),
                Builders<DashboardConfiguration>.Filter.Eq(d => d.IsDefault, true)
            );

            var existingDashboard = await dashboardsCollection.Find(filter).FirstOrDefaultAsync(cancellationToken);

            if (existingDashboard != null)
            {
                // Update existing dashboard with template layout
                existingDashboard.DashboardName = $"{template.TemplateName} (Reset)";
                existingDashboard.Layout = new DashboardLayout
                {
                    Columns = template.Layout.Columns,
                    RowHeight = template.Layout.RowHeight,
                    Widgets = template.Layout.Widgets.Select(w => new WidgetConfiguration
                    {
                        WidgetId = w.WidgetId,
                        WidgetType = w.WidgetType,
                        Position = new WidgetPosition
                        {
                            X = w.Position.X,
                            Y = w.Position.Y,
                            Width = w.Position.Width,
                            Height = w.Position.Height
                        },
                        Settings = new Dictionary<string, object>(w.Settings)
                    }).ToList()
                };
                existingDashboard.UpdatedAt = DateTime.UtcNow;

                await dashboardsCollection.ReplaceOneAsync(d => d.Id == existingDashboard.Id, existingDashboard, cancellationToken: cancellationToken);
                
                return existingDashboard.Id;
            }
            else
            {
                // Create new dashboard from template
                var newDashboard = new DashboardConfiguration
                {
                    UserId = request.UserId,
                    DashboardName = template.TemplateName,
                    IsDefault = true,
                    Layout = new DashboardLayout
                    {
                        Columns = template.Layout.Columns,
                        RowHeight = template.Layout.RowHeight,
                        Widgets = template.Layout.Widgets.Select(w => new WidgetConfiguration
                        {
                            WidgetId = w.WidgetId,
                            WidgetType = w.WidgetType,
                            Position = new WidgetPosition
                            {
                                X = w.Position.X,
                                Y = w.Position.Y,
                                Width = w.Position.Width,
                                Height = w.Position.Height
                            },
                            Settings = new Dictionary<string, object>(w.Settings)
                        }).ToList()
                    },
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await dashboardsCollection.InsertOneAsync(newDashboard, cancellationToken: cancellationToken);
                
                return newDashboard.Id;
            }
        }
    }
}
