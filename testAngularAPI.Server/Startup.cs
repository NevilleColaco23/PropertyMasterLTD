using MyWarehouse.Application;
using MyWarehouse.Infrastructure.Authentication;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Conventions;
using testAngularAPI.Server;
using testAngularAPI.Server.ErrorHandling;
using testAngularAPI.Server.Swagger;
using testAngularAPI.Server.Versioning;
using testAngularAPI.Server.CORS;
using testAngularAPI.Server.Logging;
using testAngularAPI.Server.SignalR;
using MyWarehouse.Application.Property.GetProperty;
using MyWarehouse.Application.Common.Behaviors;

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

     //   services.AddSingleton<IMailService, MailServiceImpl>();


     //   services
     //.AddFluentEmail("propertymaster193@yahoo.com", "property master")
     //.AddRazorRenderer()
     //.AddSmtpSender(new SmtpClient("smtp.mail.yahoo.com")
     //{
     //    UseDefaultCredentials = false,
     //    Credentials = new NetworkCredential("propertymaster193@yahoo.com", "thisismypropertyman"),
     //    EnableSsl = true,
     //    Port = 587
     //});


        //services.AddFluentEmail("youremail@example.com", "Your Name").AddRazorRenderer().AddSmtpSender(new SmtpClient("smtp.example.com") { UseDefaultCredentials = false, Credentials = new NetworkCredential("yourusername", "yourpassword"), EnableSsl = true, Port = 587 });
        //services.ConfigureServicesMailingService();
        var serviceProvider = services.BuildServiceProvider();
        // Build the service provider
        MyWarehouse.Infrastructure.ResourceLocator.RegisterServiceProvider(serviceProvider);

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
        app.UseStaticFiles();// Enable serving static files from wwwroot
        app.UseMyRequestLogging();

        // Only use HTTPS redirection in Development
        if (Environment.IsDevelopment())
        {
            app.UseHttpsRedirection();
        }

        app.UseRouting();

        // CORS must be after UseRouting and before UseAuthentication
        app.UseMyCorsConfiguration();

        app.UseMySwagger(Configuration);

        // Authentication and Authorization - handled by UseMyInfrastructure
        app.UseMyInfrastructure(Configuration, Environment);

        app.UseMyApi();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHub<ChatHub>("/chatHub");
        });

    }
}
