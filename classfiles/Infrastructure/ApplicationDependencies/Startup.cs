using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyWarehouse.Application.Common.Dependencies.DataAccess;
using MyWarehouse.Application.Common.Dependencies.DataAccess.Repositories;
using MyWarehouse.Application.Dependencies.Services;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories;
using MyWarehouse.Infrastructure.ApplicationDependencies.DataAccess.Repositories.Mongo;
using MyWarehouse.Infrastructure.ApplicationDependencies.Services;
using System.Diagnostics.CodeAnalysis;
using MyWarehouse.Application.Services;
using MyWarehouse.Infrastructure.Services;
using MyWarehouse.Application.Common.Audit;
using MyWarehouse.Application.UserActivity.Services;
using MyWarehouse.Application.Bookings.Services;

namespace MyWarehouse.Infrastructure.ApplicationDependencies;

[ExcludeFromCodeCoverage]
internal static class Startup
{
    public static void ConfigureServices(this IServiceCollection services, IConfiguration _)
    {
        //NOTE: Also update IUnitOfWork class constructor when adding new repositories
        services.AddScoped<ICounterService, CounterService>();
        services.AddScoped<IProductRepository, ProductRepositoryEf>();
        services.AddScoped<IPartnerRepository, PartnerRepositoryEf>();
        services.AddScoped<ITransactionRepository, TransactionRepositoryEf>();
        services.AddScoped<IPropertyRepository, PropertyRepositoryMongo>();
        services.AddScoped<IRoomRepository, RoomRepositoryMongo>();
        services.AddScoped<IPropertyAuditRepository, PropertyAuditRepositoryMongo>();
        services.AddScoped<IRoomAuditRepository, RoomAuditRepositoryMongo>();
        services.AddScoped<IUsersRepository, UsersRepositoryMongo>();
        services.AddScoped<IMenuRepository, MenuRepositoryMongo>();
        services.AddScoped<IMenuPermissionRepository, MenuPermissionRepositoryMongo>();
        services.AddScoped<IBookingsRepository, BookingsRepositoryMongo>();
        services.AddScoped<ISystemMessagesRepository, SystemMessagesRepositoryMongo>();
        services.AddScoped<IEmailOutboxRepository, EmailOutboxRepositoryMongo>();
        services.AddScoped<IAuditLogRepository, AuditLogRepositoryMongo>();
        services.AddScoped<IUserActivityRepository, UserActivityRepositoryMongo>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddTransient<IDateTime, DateTimeService>();
        services.AddTransient<IStockStatisticsService, StockStatisticsService>();
        services.AddScoped<IAuditService, AuditService>();
        services.AddScoped<UserActivityService>();
        services.AddScoped<ActivityDisplayMessageBuilder>();  // ⭐ Centralized message builder
        services.AddScoped<BookingActivityLogger>();  // ⭐ Booking operations activity logger
    }
}
