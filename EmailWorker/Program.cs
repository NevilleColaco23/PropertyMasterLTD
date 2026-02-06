using EmailWorker;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Resend;

//var builder = Host.CreateApplicationBuilder(args);
//builder.Services.AddHostedService<Worker>();

//var host = builder.Build();
//host.Run();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        // Mongo
        var mongoUri = config.GetConnectionString("MongoDb");
        if (string.IsNullOrWhiteSpace(mongoUri))
            throw new InvalidOperationException("Missing connection string 'ConnectionStrings:MongoDb' in appsettings.json.");

        var dbName = config["AppSettings:MongoDbDatabaseName"];
        if (string.IsNullOrWhiteSpace(dbName))
            throw new InvalidOperationException("Missing AppSettings:MongoDbDatabaseName in appsettings.json.");

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoUri));
        services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(dbName));

        // Resend options (singleton-friendly)
        services.Configure<ResendClientOptions>(options =>
        {
            options.ApiToken = config["Resend:ApiKey"] ?? config["RESEND_API_KEY"];
            if (string.IsNullOrWhiteSpace(options.ApiToken))
                throw new InvalidOperationException("Missing Resend:ApiKey (or RESEND_API_KEY).");
        });

        // Resend client (singleton)
        services.AddSingleton(sp =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();
            var apiKey = cfg["Resend:ApiKey"] ?? cfg["RESEND_API_KEY"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Missing Resend:ApiKey (or RESEND_API_KEY).");

            var optionsSnapshot = new StaticOptionsSnapshot<ResendClientOptions>(
                new ResendClientOptions { ApiToken = apiKey }
            );

            var httpClient = new HttpClient();

            return new ResendClient(optionsSnapshot, httpClient);
        });

        services.AddSingleton<IEmailSender, ResendEmailSender>();

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();