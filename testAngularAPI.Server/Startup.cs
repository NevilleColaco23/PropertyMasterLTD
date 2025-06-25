using MyWarehouse.Application;
using MyWarehouse.Infrastructure.Authentication;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Conventions;
using System.Diagnostics.CodeAnalysis;
using testAngularAPI.Server;
using testAngularAPI.Server.ErrorHandling;
using testAngularAPI.Server.Swagger;
using testAngularAPI.Server.CORS;
using testAngularAPI.Server.Logging;
using testAngularAPI.Server.SignalR;
using testAngularAPI.Server.Versioning;

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

        var serviceProvider = services.BuildServiceProvider();
        // Build the service provider
        ResourceLocator.RegisterServiceProvider(serviceProvider);

        services.AddMyApi();
        services.AddMyApiAuthDeps();
        services.AddMyErrorHandling();
        services.AddMySwagger(Configuration);
        services.AddMyVersioning();
        services.AddMyCorsConfiguration(Configuration);

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
            return client.GetDatabase("ListingDB");
        });
        services.AddMyInfrastructureDependencies(Configuration, Environment);
        services.AddMyApplicationDependencies();
        services.AddSignalR();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseDefaultFiles(); // this enables serving index.html by default
        // Enable serving static files from wwwroot
        app.UseStaticFiles();
        app.UseMyRequestLogging();
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseMyCorsConfiguration();
        app.UseMySwagger(Configuration);
        app.UseMyInfrastructure(Configuration, Environment);
        app.UseMyApi();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<ChatHub>("/chatHub");
        });

    }
}
