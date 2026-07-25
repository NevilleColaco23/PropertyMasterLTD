using Azure.Messaging.ServiceBus;
using EmailWorker;
using Messaging.Shared;
using MongoDB.Driver;
using Resend;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        var config = context.Configuration;

        // ── MongoDB Atlas ────────────────────────────────────────────────────
        var mongoUri = config.GetConnectionString("MongoDb")
            ?? throw new InvalidOperationException("Missing ConnectionStrings:MongoDb");

        var dbName = config["AppSettings:MongoDbDatabaseName"]
            ?? throw new InvalidOperationException("Missing AppSettings:MongoDbDatabaseName");

        services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoUri));
        services.AddSingleton(sp => sp.GetRequiredService<IMongoClient>().GetDatabase(dbName));

        // ── Resend email sender ──────────────────────────────────────────────
        var resendApiKey = config["Resend:ApiKey"]
            ?? throw new InvalidOperationException("Missing Resend:ApiKey");

        services.AddSingleton(_ =>
        {
            var opts        = new StaticOptionsSnapshot<ResendClientOptions>(new ResendClientOptions { ApiToken = resendApiKey });
            return new ResendClient(opts, new HttpClient());
        });

        services.AddSingleton<IEmailSender, ResendEmailSender>();

        // ── Azure Service Bus processor ──────────────────────────────────────
        var sbConnectionString = config["ServiceBus:ConnectionString"]
            ?? throw new InvalidOperationException("Missing ServiceBus:ConnectionString");

        var emailQueueName = config["ServiceBus:EmailQueueName"]
            ?? ServiceBusQueueNames.Email;

        services.AddSingleton(_ =>
        {
            var client = new ServiceBusClient(sbConnectionString);
            return client.CreateProcessor(emailQueueName, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls  = 1,   // one email at a time — safe for Resend rate limits
                AutoCompleteMessages = false
            });
        });

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
