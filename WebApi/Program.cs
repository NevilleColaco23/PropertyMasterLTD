using Messaging.Shared;
using MyWarehouse.Infrastructure;
using MyWarehouse.Infrastructure.Logging;
using MyWarehouse.WebApi.Messaging_Queue;
using System.Reflection;

namespace MyWarehouse.Infrastructure;

[ExcludeFromCodeCoverage]
public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Console.WriteLine("=== PROGRAM.CS STARTING ===");
            Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");
            Console.WriteLine($"ASPNETCORE_URLS: {Environment.GetEnvironmentVariable("ASPNETCORE_URLS")}");

            Console.WriteLine("Starting application...");
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection("RabbitMq"));
            builder.Services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            builder.Services.AddControllers();

            Console.WriteLine("✅ WebApplication.CreateBuilder completed");

            builder.Host
                //.AddMySerilogLogging() // Temporarily disabled for Railway debugging
                .ConfigureAppConfiguration((context, config) =>
                {
                    // ConfigureWebHostDefaults only adds secrets if environment is Develop.
                    // This ensures they're always added, for local testing of Production setting.
                    config.AddUserSecrets(Assembly.GetEntryAssembly(), optional: true);

                    // Notice: Infrastructure hook.
                    config.AddMyInfrastructureConfiguration(context);
                });

            Console.WriteLine("✅ Services configured");
            Console.WriteLine("✅ Host configured");

            var startup = new Startup(builder.Configuration, builder.Environment);
            Console.WriteLine("✅ Startup instance created");

            startup.ConfigureServices(builder.Services);
            Console.WriteLine("✅ Startup.ConfigureServices completed");

            var app = builder.Build();
            Console.WriteLine("✅ App built");

            app.MapGet("/", () => "API running!");

            startup.Configure(app);
            Console.WriteLine("✅ Startup.Configure completed");

            app.Lifetime.ApplicationStarted.Register(() =>
            {
                Console.WriteLine("Application started.");
            });

            Console.WriteLine("App built, now running...");
            Console.WriteLine("✅✅✅ APPLICATION FULLY STARTED ✅✅✅");
            Console.WriteLine("Calling app.Run() now...");

            app.Run();

            Console.WriteLine("⚠️ app.Run() returned - this shouldn't happen!");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌❌❌ FATAL ERROR IN PROGRAM.CS ❌❌❌");
            Console.WriteLine($"Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"Message: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
            if (ex.InnerException != null)
            {
                Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
            }
            throw;
        }
    }
}
