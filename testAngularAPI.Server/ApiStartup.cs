using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using MyWarehouse.Infrastructure.Filters;

namespace testAngularAPI.Server;

[ExcludeFromCodeCoverage]
internal static class ApiStartup
{
    public static void AddMyApi(this IServiceCollection services)
    {
        services.AddHealthChecks();

        // Register Activity Logging Filter as Scoped (important for DI)
        services.AddScoped<ActivityLoggingActionFilter>();

        services.AddControllers(options =>
        {
            // Add activity logging filter globally
            options.Filters.Add<ActivityLoggingActionFilter>();
        })
            .AddControllersAsServices()
            .AddJsonOptions(c =>
                c.JsonSerializerOptions.PropertyNamingPolicy
                    = JsonNamingPolicy.CamelCase);
    }

    /// <summary>
    /// Depends on UseRouting() being called before calling this method.
    /// </summary>
    public static void UseMyApi(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health");
        });
    }
}
