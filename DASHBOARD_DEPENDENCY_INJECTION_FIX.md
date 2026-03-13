# Dashboard Dependency Injection Fix

## Problem

The Dashboard command and query handlers were failing to compile with the error:

```
'IUnitOfWork' does not contain a definition for 'GetCollection' 
and no accessible extension method 'GetCollection' accepting a first 
argument of type 'IUnitOfWork' could be found
```

## Root Cause

The `IUnitOfWork` interface in this codebase uses the **Repository Pattern** with specific repository properties (e.g., `Properties`, `Rooms`, `MenuPermissions`), not a generic `GetCollection<T>()` method.

```csharp
public interface IUnitOfWork : IDisposable
{
    public IPropertyRepository? Properties { get; }
    public IRoomRepository? Rooms { get; }
    public IMenuPermissionRepository? MenuPermissions { get; }
    // ... other repositories
    
    // ❌ NO GetCollection<T>() method exists
}
```

## Solution

Changed all Dashboard handlers to inject `IMongoDatabase` directly instead of `IUnitOfWork`.

### Before (❌ Broken)
```csharp
public class SaveDashboardConfigurationCommandHandler : IRequestHandler<SaveDashboardConfigurationCommand, string>
{
    private readonly IUnitOfWork _unitOfWork;
    
    public SaveDashboardConfigurationCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<string> Handle(SaveDashboardConfigurationCommand request, CancellationToken cancellationToken)
    {
        var collection = _unitOfWork.GetCollection<DashboardConfiguration>(...); // ❌ Doesn't exist
    }
}
```

### After (✅ Fixed)
```csharp
public class SaveDashboardConfigurationCommandHandler : IRequestHandler<SaveDashboardConfigurationCommand, string>
{
    private readonly IMongoDatabase _database;
    
    public SaveDashboardConfigurationCommandHandler(IMongoDatabase database)
    {
        _database = database;
    }
    
    public async Task<string> Handle(SaveDashboardConfigurationCommand request, CancellationToken cancellationToken)
    {
        var collection = _database.GetCollection<DashboardConfiguration>(...); // ✅ Works!
    }
}
```

## Files Modified

### 1. `classfiles/Application/Dashboard/Commands/DashboardCommandHandlers.cs`
**Changed all 4 command handlers:**
- `SaveDashboardConfigurationCommandHandler`
- `DeleteDashboardConfigurationCommandHandler`
- `SetDefaultDashboardCommandHandler`
- `ResetDashboardToTemplateCommandHandler`

### 2. `classfiles/Application/Dashboard/Queries/DashboardQueryHandlers.cs`
**Changed all 4 query handlers:**
- `GetDashboardByUserIdQueryHandler`
- `GetUserDashboardsQueryHandler`
- `GetWidgetLibraryQueryHandler`
- `GetDashboardTemplatesQueryHandler`

## Why IMongoDatabase Works

`IMongoDatabase` is registered in your dependency injection container and provides:
```csharp
IMongoCollection<T> GetCollection<T>(string name)
```

This is the standard MongoDB driver interface used throughout the codebase in similar scenarios.

## Alternative Solution (More Complex)

You could create dedicated repositories for Dashboard entities:

```csharp
// 1. Create interface
public interface IDashboardConfigurationRepository : IRepository<DashboardConfiguration, string> { }

// 2. Create implementation
public class DashboardConfigurationRepositoryMongo : RepositoryBaseMongo<DashboardConfiguration, string>, IDashboardConfigurationRepository
{
    public DashboardConfigurationRepositoryMongo(IMongoDatabase database, IMapper mapper, ICounterService counterService)
        : base(database, mapper, MongoCollections.DashboardConfigurationsCollection, counterService)
    {
    }
}

// 3. Register in Startup.cs
services.AddScoped<IDashboardConfigurationRepository, DashboardConfigurationRepositoryMongo>();

// 4. Add to IUnitOfWork
public IDashboardConfigurationRepository? DashboardConfigurations { get; }
```

**However, this is overkill for now** since:
- Direct `IMongoDatabase` access is simpler
- Other parts of the codebase also use direct MongoDB access for specialized queries
- The Repository Pattern is better suited for standard CRUD operations, not complex dashboard customization

## Verification

✅ **Build Status**: Successful  
✅ **All 8 handlers fixed**: 4 commands + 4 queries  
✅ **No compilation errors**

## Next Steps

1. ✅ Database seeded with widgets (already done via MongoDB Compass)
2. ⏭️ Test API endpoints using Postman/Swagger
3. ⏭️ Implement Angular frontend for dashboard customization
