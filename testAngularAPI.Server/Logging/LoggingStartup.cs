using MyWarehouse.Infrastructure.Logging.Helper;
using MyWarehouse.Infrastructure.Logging.Settings;
using MyWarehouse.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using Serilog;
using Serilog.Events;

namespace testAngularAPI.Server.Logging;

[ExcludeFromCodeCoverage]
internal static class LoggingStartup
{
    public static IHostBuilder AddMySerilogLogging(this IHostBuilder webBuilder)
    {
        return webBuilder.UseSerilog((context, loggerCfg) =>
        {
            // Access the configuration from the context
            IConfiguration configuration = context.Configuration;

            loggerCfg
                .MinimumLevel.Information() // Default minimum level
                .Enrich.FromLogContext()
                .Enrich.WithProperty("EnvironmentName", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithMachineName();

            // *** Read file log settings from appsettings.json ***
            var fileLogPath = configuration["Serilog:FileLog:Path"] ?? "logs/api-log-.txt"; // Default fallback
            var fileMinLevelString = configuration["Serilog:FileLog:MinimumLevel"] ?? "Warning";
            Enum.TryParse(fileMinLevelString, out LogEventLevel fileMinLevel);


            // Unconditionally add a file sink
            loggerCfg.WriteTo.File(
                fileLogPath,
                rollingInterval: RollingInterval.Day,
                restrictedToMinimumLevel: fileMinLevel, // Use the configured minimum level
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            // formatter: new CompactJsonFormatter() // Uncomment for JSON format if you prefer
            );

            if (context.HostingEnvironment.IsDevelopment())
            {
                loggerCfg
                    .WriteTo.Console()
                    .WriteTo.Debug();
            }
            else
            {
                loggerCfg
                    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Error);
            }

            var logglySettings = configuration.GetMyOptions<LogglySettings>(); // Assuming GetMyOptions uses IConfiguration
            if (logglySettings.WriteToLoggly.GetValueOrDefault() == true)
            {
                loggerCfg.WriteTo.Loggly(
                    customerToken: logglySettings.CustomerToken);
            }

            loggerCfg
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning);
        });
    }

    public static IApplicationBuilder UseMyRequestLogging(this IApplicationBuilder appBuilder)
    {
        return appBuilder
            .UseSerilogRequestLogging(
                opts => opts.GetLevel = LogHelper.ExcludeHealthChecks);
    }
}