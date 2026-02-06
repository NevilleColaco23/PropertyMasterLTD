using EmailWorker;
using MongoDB.Driver;

//var builder = Host.CreateApplicationBuilder(args);
//builder.Services.AddHostedService<Worker>();

//var host = builder.Build();
//host.Run();


var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        // Connection string from appsettings.json -> ConnectionStrings:MongoDb
        var mongoUri = config.GetConnectionString("MongoDb");
        if (string.IsNullOrWhiteSpace(mongoUri))
            throw new InvalidOperationException("Missing connection string 'ConnectionStrings:MongoDb' in appsettings.json.");

        // Database name from appsettings.json -> AppSettings:MongoDbDatabaseName
        var dbName = config["AppSettings:MongoDbDatabaseName"];
        if (string.IsNullOrWhiteSpace(dbName))
            throw new InvalidOperationException("Missing AppSettings:MongoDbDatabaseName in appsettings.json.");

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoUri));
        services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(dbName));

        var smtpSection = config.GetSection("Smtp");
        var smtp = new SmtpSettings
        {
            Host = smtpSection["Host"] ?? "smtp.gmail.com",
            Port = 587,//int.TryParse(smtpSection["Port"], out var p) ? p : 587,
            Username = smtpSection["Username"] ?? throw new InvalidOperationException("Missing Smtp:Username in appsettings.json."),
            Password = smtpSection["Password"] ?? throw new InvalidOperationException("Missing Smtp:Password in appsettings.json."),
            From = smtpSection["From"] ?? throw new InvalidOperationException("Missing Smtp:From in appsettings.json.")
        };

        services.AddSingleton(smtp);
        services.AddSingleton<IEmailSender, SmtpEmailSender>();

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();