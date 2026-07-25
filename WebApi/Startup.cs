using MyWarehouse.Infrastructure.Logging;
using MyWarehouse.Infrastructure.ErrorHandling;
using MyWarehouse.Infrastructure.CORS;
using MyWarehouse.Infrastructure;
using MyWarehouse.Application;
using MyWarehouse.Infrastructure.Authentication;
using MyWarehouse.Infrastructure.Swagger;
using MyWarehouse.Infrastructure.Versioning;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Conventions;
using MyWarehouse.Infrastructure.SignalR;
using MyWarehouse.Infrastructure.Services;
using System.Net.Mail;
using System.Net;
 
namespace MyWarehouse.Infrastructure;

[ExcludeFromCodeCoverage]
public class Startup
{
    protected IConfiguration Configuration { get; }
    protected IWebHostEnvironment Environment { get; }

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
        Config.Init(configuration);     // Initialize our Configuration wrapper class
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(x => new AppEnvironment.AppEnvironment
        {
            EnvironmentName = Environment.EnvironmentName,
            ApplicationName = Environment.ApplicationName,
            ContentRootPath = Environment.ContentRootPath,
            ContentRootFileProvider = Environment.ContentRootFileProvider,

            IsWebHost = false,
            WebRootPath = Environment.WebRootPath,
            WebRootFileProvider = Environment.WebRootFileProvider
        });

        //services.AddFluentEmail("youremail@example.com", "Your Name").AddRazorRenderer().AddSmtpSender(new SmtpClient("smtp.example.com") { UseDefaultCredentials = false, Credentials = new NetworkCredential("yourusername", "yourpassword"), EnableSsl = true, Port = 587 });
        var serviceProvider = services.BuildServiceProvider();
        // Build the service provider
        MyWarehouse.Infrastructure.ResourceLocator.RegisterServiceProvider(serviceProvider);

        services.AddMyApi();
        services.AddMyApiAuthDeps();
        services.AddMyErrorHandling();
        services.AddMySwagger(Configuration);
        services.AddMyVersioning();
        services.AddMyCorsConfiguration(Configuration);

        services.AddHealthChecks();

        // Register the convention to ignore extra elements
        var conventionPack = new ConventionPack { new IgnoreExtraElementsConvention(true) };
        ConventionRegistry.Register("IgnoreExtraElements", conventionPack, type => true);

        // Register MongoDB clients
        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = MongoClientSettings.FromConnectionString(Config.MongoDbConnectionString);
            return new MongoClient(settings);
        });

        // Register MongoDB database
        services.AddScoped(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var dbName = Configuration["AppSettings:MongoDbDatabaseName"] ?? "ListingDB";
            return client.GetDatabase(dbName);
        });

        // Register Email Queue Service for sending emails via Resend
        services.AddScoped<IEmailQueueService, EmailQueueService>();

        // Register Demo Property Services for new user onboarding
        services.AddScoped<DemoPropertySeeder>();
        services.AddScoped<IDemoPropertyService, DemoPropertyService>();

        services.AddMyInfrastructureDependencies(Configuration, Environment);
        services.AddMyApplicationDependencies();
        services.AddSignalR();

        // Register Azure Service Bus configuration and publisher
        services.Configure<Messaging.Shared.ServiceBusOptions>(Configuration.GetSection("ServiceBus"));
        services.AddSingleton<Messaging.Shared.IServiceBusPublisher, MyWarehouse.WebApi.Messaging_Queue.ServiceBusPublisher>();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseDefaultFiles(); // this enables serving index.html by default
        // Enable serving static files from wwwroot
        app.UseStaticFiles();

        // TEMPORARY: Disable Serilog request logging in Production (missing DiagnosticContext)
        if (Environment.IsDevelopment())
        {
            app.UseMyRequestLogging();
        }

        // Only use HTTPS redirection in Development - Railway handles HTTPS at proxy level
        if (Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseRouting();
        app.UseMyCorsConfiguration();
        app.UseMySwagger(Configuration);
        app.UseMyInfrastructure(Configuration, Environment);
        app.UseMyApi();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<ChatHub>("/chatHub");
            endpoints.MapHealthChecks("/health");
        });

    }
}
