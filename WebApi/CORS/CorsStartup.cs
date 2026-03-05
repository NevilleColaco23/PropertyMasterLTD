using MyWarehouse.Infrastructure;
using MyWarehouse.Infrastructure.CORS.Settings;
using System.Diagnostics.CodeAnalysis;

namespace MyWarehouse.Infrastructure.CORS;

[ExcludeFromCodeCoverage]
internal static class CorsStartup
{
    public static void AddMyCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var corsSettings = configuration.GetMyOptions<CorsSettings>();

        if (corsSettings == null || corsSettings.AllowedOrigins == null || corsSettings.AllowedOrigins.Length == 0)
        {
            Console.WriteLine("⚠️ WARNING: CorsSettings is NULL or empty - Using fallback CORS configuration!");
            Console.WriteLine("Check that appsettings.json has a 'CorsSettings' section or environment variables are set");

            // FALLBACK: Allow Vercel and localhost
            var fallbackOrigins = new[]
            {
                "http://localhost:4200",
                "https://localhost:4200",
                "https://property-master-silk.vercel.app"
            };

            Console.WriteLine("Using fallback origins:");
            foreach (var origin in fallbackOrigins)
            {
                Console.WriteLine($"   - {origin}");
            }

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithOrigins(fallbackOrigins);
                });
            });

            Console.WriteLine("✅ Fallback CORS Policy Configured");
            return;
        }

        Console.WriteLine($"✅ CORS Configuration Loading...");
        Console.WriteLine($"   Allowed Origins Count: {corsSettings.AllowedOrigins?.Length ?? 0}");
        if (corsSettings.AllowedOrigins != null)
        {
            foreach (var origin in corsSettings.AllowedOrigins)
            {
                Console.WriteLine($"   - {origin}");
            }
        }

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
                    corsSettings.AllowedOrigins)
                .Build();
            });
        });

        Console.WriteLine("✅ CORS Policy Configured Successfully");
    }

    public static void UseMyCorsConfiguration(this IApplicationBuilder app)
    {
        app.UseCors();
    }
}
