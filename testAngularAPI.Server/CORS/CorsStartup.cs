using MyWarehouse.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using testAngularAPI.Server.CORS.Settings;

namespace testAngularAPI.Server.CORS;

[ExcludeFromCodeCoverage]
internal static class CorsStartup
{
    public static void AddMyCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSettings = configuration.GetMyOptions<CorsSettings>();

        if (corsSettings == null)
            return;

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(builder =>
            {
                builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials()
                .SetIsOriginAllowedToAllowWildcardSubdomains()
                .WithOrigins(
                     "http://localhost:4200", // Note: http not https
                    "https://localhost:4200",
                    "https://testangularapidocker-production.up.railway.app")
                .Build();
            });
        });
    }

    public static void UseMyCorsConfiguration(this IApplicationBuilder app)
    {
        app.UseCors();
    }
}
